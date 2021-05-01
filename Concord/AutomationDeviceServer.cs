using Automation.Concord.InboundMessages;
using Automation.Concord.InboundMessages.Alerts;
using Automation.Concord.OutboundMessages;
using Automation.Concord.Panel;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Automation.Concord
{
    /// <summary>
    /// Concord 4 automation device server implementation. Currently configured for Serial-to-TCP-client endpoint device MOXA NPORT 5110
    /// </summary>

    public partial class AutomationDeviceServer
    {
        #region Fields  ************************************************************************
        
        readonly ICommunicationDevice device;
        readonly ILogger log;
        Panel.Panel panel;
        MessageProcessor processor;
        
        Timer minuteTimer;

        ConcordConfiguration config;

        string preArmingUsername = null;
        bool preArmingAutoArm = false;

        bool communicationsOnline = false;
        bool? connectionFlushed = false;
        bool isPanelConnected = false;
        bool running = false;

        KeyfobButton? lastKeyfobButton = null;
        Zone lastKeyfobPressed = null;

        object lockAllLightsControl = new object();
        object lockArming = new object();
        object lockBeginKeyfobButton = new object();
        object lockBypassToggle = new object();
        
        Thread threadPanelCommunications;
        
        long tickPreArmingStarted = 0;
        long tickCommunicationStartTime = 0;
        long tickDataRefreshTimeout = 0;
        long tickKeyfobButtonRepeatExpiry = 0;
        long tickSilentArmingTimeout = 0;
        long tickDownloadTimeout = 0;

        ManualResetEvent[] waitPostArmingResult = new ManualResetEvent[] { 
            null,
            new ManualResetEvent(false),
            new ManualResetEvent(false),
            new ManualResetEvent(false),
            new ManualResetEvent(false),
            new ManualResetEvent(false),
            new ManualResetEvent(false)
        };
        

        AutoResetEvent waitDisplayUpdate = new AutoResetEvent(false);
        AutoResetEvent waitKeyfobButton = new AutoResetEvent(false);
        AutoResetEvent waitRefreshComplete = new AutoResetEvent(false);

        const int PARTITION_PRIMARY_ID = 1;
        const int USERID_ALARMDOTCOM = 255;
        const string USERID_DURESS_DESCRIPTION = "Master";
        const int USERID_MASTER = 246;

        #endregion Fields

        #region Private Methods ****************************************************************


        private User HomeUser
        {
            get
            {
                return panel.Users[config.AutomationUserId];
            }
        }

        private Partition PrimaryPartition
        {
            get
            {
                return this.panel.Partitions[PARTITION_PRIMARY_ID];
            }
        }

        /// <summary>
        /// Bypass open zones and continue arming
        /// </summary>
        /// <param name="partitionId"></param>
        /// <returns></returns>
        private bool BypassProtest(int partitionId)
        {
            LogEvent("Bypassing open zones");
            Keypress message = new Keypress(partitionId, TouchpadKey.Pound);
            return SendMessage(message);
        }

        private bool CancelProtest(int partitionId)
        {
            Partition partition = this.panel.Partitions[partitionId];
            if (partition.ArmingProtest != false)
            {
                SendKeys(partitionId, TouchpadKey.Key1);

                // spin up to 2000 milliseconds for arming protest to end
                for (int i = 0; i < 20; i++)
                {
                    Thread.Sleep(100);
                    if (this.panel.Partitions[1].ArmingProtest == false)
                    {
                        return true;
                    }
                }
                return this.panel.Partitions[1].ArmingProtest == false;
            }
            else
                return true;
        }

        private string GetMasterCode(int partitionId)
        {
            return panel.Users[USERID_MASTER].Code;
        }

        /// <summary>
        /// Unbypass all zones for all partitions
        /// </summary>
        /// <returns></returns>
        private bool UnbypassAllZones()
        {
            bool success = true;

            List<Zone> bypassedZones = panel.GetZones(ZoneState.Bypassed);
            foreach (Zone zone in bypassedZones)
            {
                if (!UnbypassZone(zone.Partition, zone.Id)) success = false;
                Thread.Sleep(500);
            }
            return success;
        }

        /// <summary>
        /// Unbypass closed zones for all partitions
        /// </summary>
        /// <returns></returns>
        private bool UnbypassClosedZones()
        {
            bool success = true;
            lock (lockBypassToggle)
            {
                List<Zone> bypassedZones = panel.GetZones(ZoneState.Bypassed);
                foreach (Zone zone in bypassedZones)
                {
                    if ((zone.State & ZoneState.Opened) != ZoneState.Opened)
                    {
                        // zone is closed
                        if (!UnbypassZone(zone.Partition, zone.Id)) success = false;
                        Thread.Sleep(500);
                    }
                }
            }
            return success;
        }

        /// <summary>
        /// Waits for arming to complete. Returns false if timeout elapsed before arming level is set or arming was not complete
        /// </summary>
        /// <returns>True if target arming level completed, false if timed out</returns>
        private bool WaitForArmingLevel(int partitionId, ArmingLevel armingLevel, int millisecondsTimeout)
        {
            bool completed = false;
            completed = waitPostArmingResult[partitionId].WaitOne(millisecondsTimeout);

            if (completed)
            {
                return this.panel.Partitions[partitionId].ArmingLevel == armingLevel;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns true if display was updated, otherwise returns false to indicate that wait timed out
        /// </summary>
        /// <param name="partition"></param>
        /// <param name="millisecondsTimeout"></param>
        /// <param name="displayText"></param>
        /// <returns></returns>
        private bool WaitForDisplayUpdate(int partitionId, int millisecondsTimeout, out string displayText)
        {
            Partition partition = this.panel.Partitions[partitionId];
            string currentText = partition.DisplayText;

            bool signaled = false;
            while (partition.DisplayText == currentText)
            {
                signaled = waitDisplayUpdate.WaitOne(millisecondsTimeout);
                if (!signaled) break;
            }

            displayText = partition.DisplayText;

            return signaled;
        }

        /// <summary>
        /// Waits for zone state to change exact state (use existing state with bit AND for targetState). Returns false if timeout elasped before zone change
        /// </summary>
        /// <param name="zone"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        private bool WaitForZoneState(Zone zone, ZoneState targetState, int millisecondsTimeout)
        {
            // check if zone is already in desired state

            if (targetState == ZoneState.Normal)
            {
                if (zone.State == ZoneState.Normal)
                {
                    return true;
                }
            }
            else if (zone.State != null && (zone.State.Value & targetState) == targetState)
            {
                return true;
            }

            bool complete = false;
            DateTime timeout = DateTime.Now.AddMilliseconds(millisecondsTimeout);

            int timeLeft = (int)(timeout - DateTime.Now).TotalMilliseconds;

            while (timeLeft > 0)
            {
                ((IStateChangeWaitHandle)zone).StateChange.WaitOne(timeLeft);

                if (targetState == ZoneState.Normal)
                {
                    if (zone.State == ZoneState.Normal)
                    {
                        complete = true;
                        break;
                    }
                }
                else if (zone.State != null && zone.State.Value == targetState)
                {
                    complete = true;
                    break;
                }

                timeLeft = (int)(timeout - DateTime.Now).TotalMilliseconds;
            }

            return complete;
        }

        #endregion Private Methods


        #region Globals ************************************************************************

        static int lastTick = Environment.TickCount & Int32.MaxValue;
        static object lastTickLock = new object();
        static int lastTickRollover = 0;

        /// <summary>
        /// Milliseconds since system started (or used to be... todo: platform specific)
        /// </summary>
        private static long SystemTick
        {
            get
            {
                lock (lastTickLock)
                {
                    int currentTick = Environment.TickCount & Int32.MaxValue;
                    if (currentTick < lastTick)
                    {
                        lastTickRollover++;
                    }
                    lastTick = currentTick;
                    return currentTick + (Int32.MaxValue * lastTickRollover);
                }
            }
        }

        private static string ParseWordsInCaps(string compoundWord)
        {
            StringBuilder builder = new StringBuilder();
            foreach (char c in compoundWord)
            {
                if (Char.IsUpper(c) && builder.Length > 0)
                {
                    builder.Append(' ');
                }
                builder.Append(c);
            }
            return builder.ToString();
        }

        #endregion


        #region Communications *****************************************************************

        private delegate void ProcessMessageDelegate(string message);

        public void ShutDown()
        {
            running = false;
            LogEvent("Concord automation device server stopped", EventCode.Panel);

            if (threadPanelCommunications != null)
            {
                if (threadPanelCommunications.IsAlive)
                {
                    if (!threadPanelCommunications.Join(5000))
                        threadPanelCommunications.Interrupt();
                }

                threadPanelCommunications = null;
            }

            if (minuteTimer != null)
            {
                minuteTimer.Change(Timeout.Infinite, Timeout.Infinite);
                minuteTimer = null;
            }
        }

        public void StartUp()
        {
            if (running) return;

            long tick = SystemTick;

            tickDataRefreshTimeout = tick;
            LogEvent("System started", EventCode.Panel);

            LoadConcordConfiguration();
            panel.Reset();

            running = true;
            threadPanelCommunications = new Thread(() => StartPanelCommunications(device));
            threadPanelCommunications.IsBackground = false;
            threadPanelCommunications.Name = string.Format("HomeControlPanelCommunicationLink", SystemTick / 10000);
            threadPanelCommunications.Start();

            minuteTimer = new System.Threading.Timer(new System.Threading.TimerCallback(minuteTimerElapsed), null, new TimeSpan(0, 0, 0, 59 - DateTime.Now.Second, 1000 - DateTime.Now.Millisecond), new TimeSpan(0, 1, 0));

            LogEvent("Concord automation device server started");
        }

        private void ProcessMessage(string message)
        {
            if (!Message.IsMessageValid(message))
            {
                log.LogWarning(string.Format("Bad message data receieved '{0}'.", message));
                return;
            }

            MessageType command = MessageCodeMap.MapInboundProtocolMessage(message);

            try
            {
                switch (command)
                {
                    case MessageType.Unknown:
                        this.OnUnknown(new InboundMessages.Unknown(message));
                        break;

                    case MessageType.AlarmTrouble:
                        AlarmTrouble general = new InboundMessages.AlarmTrouble(message);

                        switch (general.GeneralEventType)
                        {
                            case AlertClass.Alarm:
                            case AlertClass.AlarmCancel:
                            case AlertClass.AlarmRestoral:
                                this.OnAlarm(new Automation.Concord.InboundMessages.Alerts.Alarm(message));
                                break;

                            case AlertClass.FireTrouble:
                            case AlertClass.FireTroubleRestoral:
                            case AlertClass.NonFireTrouble:
                            case AlertClass.NonFireTroubleRestoral:
                                this.OnTrouble(new Automation.Concord.InboundMessages.Alerts.Trouble(message));
                                break;

                            case AlertClass.Bypass:
                            case AlertClass.Unbypass:
                                this.OnBypass(new Automation.Concord.InboundMessages.Alerts.Bypass(message));
                                break;

                            case AlertClass.Opening:
                            case AlertClass.Closing:
                                this.OnOpeningClosing(new Automation.Concord.InboundMessages.Alerts.OpeningClosing(message));
                                break;

                            case AlertClass.PartitionEvent:
                                this.OnPartitionEvent(new Automation.Concord.InboundMessages.Alerts.PartitionEvent(message));
                                break;

                            case AlertClass.PartitionTest:
                                this.OnPartitionTest(new Automation.Concord.InboundMessages.Alerts.PartitionTest(message));
                                break;

                            case AlertClass.SystemEvent:
                                this.OnSystemEvent(new Automation.Concord.InboundMessages.Alerts.SystemEvent(message));
                                break;

                            case AlertClass.SystemTrouble:
                            case AlertClass.SystemTroubleRestoral:
                                this.OnSystemTrouble(new Automation.Concord.InboundMessages.Alerts.SystemTrouble(message));
                                break;

                            case AlertClass.SystemConfigurationChange:
                                this.OnSystemConfigurationChange(new Automation.Concord.InboundMessages.Alerts.SystemConfigurationChange(message));
                                break;
                        }
                        break;

                    case MessageType.ArmingLevel:
                        this.OnArmingLevelState(new InboundMessages.ArmingLevelState(message));
                        break;

                    case MessageType.AutomationEventLost:
                        this.OnAutomationEventLost(new InboundMessages.AutomationEventLost(message));
                        break;

                    case MessageType.ClearAutomationDynamicImage:
                        this.OnClearAutomationDynamicImage(new InboundMessages.ClearAutomationDynamicImage(message));
                        break;

                    case MessageType.EntryExitDelay:
                        this.OnEntryExitDelay(new InboundMessages.EntryExitDelay(message));
                        break;

                    case MessageType.EquipmentListComplete:
                        this.OnEquipmentListComplete(new InboundMessages.EquipmentListComplete(message));
                        break;

                    case MessageType.EquipmentListLightToSensor:
                        this.OnEquipmentListLightToSensor(new InboundMessages.EquipmentListLightToSensor(message));
                        break;

                    case MessageType.EquipmentListOutput:
                        this.OnEquipmentListOutput(new InboundMessages.EquipmentListOutput(message));
                        break;

                    case MessageType.EquipmentListPartition:
                        this.OnEquipmentListPartition(new InboundMessages.EquipmentListPartition(message));
                        break;

                    case MessageType.EquipmentListScheduledEvent:
                        this.OnEquipmentListScheduledEvent(new InboundMessages.EquipmentListScheduledEvent(message));
                        break;

                    case MessageType.EquipmentListSchedule:
                        this.OnEquipmentListSchedule(new InboundMessages.EquipmentListSchedule(message));
                        break;

                    case MessageType.EquipmentListSuperBusDevice:
                        this.OnEquipmentListSuperBusDevice(new InboundMessages.EquipmentListSuperBusDevice(message));
                        break;

                    case MessageType.EquipmentListSuperBusDeviceCapabilities:
                        this.OnEquipmentListSuperBusDeviceCapabilities(new InboundMessages.EquipmentListSuperBusDeviceCapabilities(message));
                        break;

                    case MessageType.EquipmentListUser:
                        this.OnEquipmentListUser(new InboundMessages.EquipmentListUser(message));
                        break;

                    case MessageType.EquipmentListZone:
                        this.OnEquipmentListZone(new InboundMessages.EquipmentListZone(message));
                        break;

                    case MessageType.FeatureState:
                        this.OnFeatureState(new InboundMessages.FeatureState(message));
                        break;

                    case MessageType.Keyfob:
                        this.OnKeyfob(new InboundMessages.Keyfob(message));
                        break;

                    case MessageType.LightsState:
                        this.OnLightsState(new InboundMessages.LightsState(message));
                        break;

                    case MessageType.PanelType:
                        this.OnPanelType(new InboundMessages.PanelType(message));
                        break;

                    case MessageType.Reserved:
                        this.OnReserved(new InboundMessages.Reserved(message));
                        break;

                    case MessageType.SirenGo:
                        this.OnSirenGo(new InboundMessages.SirenGo(message));
                        break;

                    case MessageType.SirenSetup:
                        this.OnSirenSetup(new InboundMessages.SirenSetup(message));
                        break;

                    case MessageType.SirenStop:
                        this.OnSirenStop(new InboundMessages.SirenStop(message));
                        break;

                    case MessageType.SirenSynchronize:
                        this.OnSirenSynchronize(new InboundMessages.SirenSynchronize(message));
                        break;

                    case MessageType.Temperature:
                        this.OnTemperature(new InboundMessages.Temperature(message));
                        break;

                    case MessageType.TimeAndDate:
                        this.OnTimeAndDate(new InboundMessages.TimeAndDate(message));
                        break;

                    case MessageType.TouchpadDisplay:
                        this.OnTouchpadDisplay(new InboundMessages.TouchpadDisplay(message));
                        break;

                    case MessageType.UserLights:
                        this.OnUserLights(new InboundMessages.UserLights(message));
                        break;

                    case MessageType.ZoneStatus:
                        this.OnZoneStatus(new InboundMessages.ZoneStatus(message));
                        break;
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, "An error occured processing concord messages");
            }
        }

        private void ProcessMessageAsync(string message)
        {
            ProcessMessageDelegate del = ProcessMessage;

            // Wait at most x milliseconds for completion in an attempt to maintain order
            if (!Task.Run(() => del.Invoke(message)).Wait(250))
            {
                // no signal
                log.LogDebug(string.Format("Inbound message processing overlap. {0} handler did not complete on time.", message));
            }
        }

        private void StartPanelCommunications(ICommunicationDevice device)
        {
            string message;
            processor = new Automation.Concord.MessageProcessor(log);
            

            while (running)
            {
                try
                {
                    tickCommunicationStartTime = SystemTick;
                    long tickCommunicationFlushTimeout = tickCommunicationStartTime + (Timing.Communications_Flush_Timeout);

                    if (communicationsOnline)
                    {
                        communicationsOnline = false;

                        message = "Panel communications link disconnected";
                        LogEvent(message, EventCode.Panel);

                        ThreadPool.QueueUserWorkItem(delegate
                        {
                            try
                            {
                                for (int i = 1; i <= 6; i++)
                                {
                                    Partition partition = this.panel.Partitions[i];
                                    partition.DisplayText = null;

                                    if (DisplayTextChange != null)
                                        DisplayTextChange(this, partition);
                                }
                            }
                            catch (Exception ex)
                            {
                                log.LogError(ex, "this sucks");
                            }
                        });

                        Thread.Sleep(device.GetReconnectDelay());
                    }

                    processor.Start(device);
                    LogEvent("Panel communications started", EventCode.Panel);

                    while (running)
                    {
                        bool timeout = false;

                        string nextMessage = processor.InboundQueue.Head(MessageProcessor.TIMEOUT_INBOUND_MESSAGE, out timeout);

                        if (timeout)
                        {
                            message = string.Format("Messages have not been recieved from message processor in {0} ms", MessageProcessor.TIMEOUT_INBOUND_MESSAGE);
                            LogEvent(message, EventCode.Panel);

                            log.LogDebug(message);

                            // restart communications
                            break;
                        }
                        else
                        {
                            if (!running) break;

                            if (!communicationsOnline)
                            {
                                communicationsOnline = true;

                                message = "Panel communications link established";
                                LogEvent(message, EventCode.Panel);

                                isPanelConnected = true;
                            }

                            if (connectionFlushed != true)
                            {
                                // if panel image clear or EquipmentListComplete
                                if (nextMessage == "022022" || nextMessage == "02080A")
                                {
                                    connectionFlushed = true;
                                }
                                else
                                {
                                    if (SystemTick < tickCommunicationFlushTimeout)
                                    {
                                        // flush

                                        processor.InboundQueue.Dequeue(0, out timeout);
                                        continue;
                                    }
                                    else
                                    {
                                        connectionFlushed = true;
                                        ResetPanelMemory(false);
                                    }
                                }
                            }

                            try
                            {
                                ProcessMessageAsync(nextMessage);
                            }
                            catch (Exception ex)
                            {
                                message = "Panel communications processing error: " + ex.Message;
                                LogEvent(message, EventCode.Panel);

                                log.LogError(ex, "Panel communications message processing error.");
                            }

                            processor.InboundQueue.Dequeue(0, out timeout);
                        }
                    }
                }
                catch (ThreadAbortException)
                {
                    return;
                }
                catch (ThreadInterruptedException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    connectionFlushed = false;

                    message = "Home automation error: \n" + ex.Message;
                    LogEvent(message, EventCode.Panel);

                    log.LogError(ex, message);

                    try
                    {
                        Thread.Sleep(MessageProcessor.TIMEOUT_INBOUND_MESSAGE);
                    }
                    catch (ThreadAbortException)
                    {
                        return;
                    }
                    catch (ThreadInterruptedException)
                    {
                        return;
                    }
                }

                try
                {
                    processor.Stop();
                    isPanelConnected = false;
                }
                catch { }
                continue;
            }
        }

        #endregion Communications


        #region Concord Automation Message Handlers ********************************************

        private void OnAlarm(Automation.Concord.InboundMessages.Alerts.Alarm message)
        {
            Partition partition = this.panel.Partitions[message.Partition];
            partition.SetAlarm(message.GeneralEventType, message.AlarmType);

            if (AlarmChange != null)
                AlarmChange(this, partition.Panel, partition);

            string alarmType = ParseWordsInCaps(Enum.GetName(typeof(AlarmType), partition.AlarmType));

            if (message.GeneralEventType == AlertClass.Alarm)
            {
                string shortMessage;

                if (message.Zone != 0)
                {
                    Zone alarmZone = this.panel.Zones[message.Zone];
                    shortMessage = string.Format("{0} {1} alarm", alarmZone.Text, alarmType);
                }
                else
                {
                    shortMessage = string.Format("{0} alarm", alarmType);
                }

                LogEvent(shortMessage, EventCode.Alarm);
            }
            else if (message.GeneralEventType == AlertClass.AlarmCancel)
            {
                string shortMessage;

                if (message.Zone != 0)
                {
                    Zone alarmZone = this.panel.Zones[message.Zone];
                    shortMessage = string.Format("{0} {1} alarm canceled", alarmZone.Text, alarmType);
                }
                else
                {
                    shortMessage = string.Format("{0} alarm canceled", alarmType);
                }

                LogEvent(shortMessage, EventCode.Alarm);
            }
            else if (message.GeneralEventType == AlertClass.AlarmRestoral)
            {
            }
        }

        private void OnArmingLevelState(Automation.Concord.InboundMessages.ArmingLevelState message)
        {
            Partition partition = this.panel.Partitions[message.Partition];

            lock (partition)
            {

                string loggedUsername = "";
                string pendingUsername = null;

                // based on the user id, this could be an automated arming
                if ((SystemTick - tickPreArmingStarted) < Timing.Silent_Arming_Period)
                {
                    //still inside the silent arming user prompt time period and user impersonation is therefore valid
                    pendingUsername = preArmingUsername;
                }
                else
                {
                    // impersonationed username is no longer valid for automation
                    pendingUsername = null;
                }

            
                if (message.UserClass == UserClass.System || message.UserClass == UserClass.Panel)
                {
                    // parition before state update
                    if ((IsDataRefreshing && partition.ArmingLevel == null) || message.ArmingLevel == partition.ArmingLevel)
                    {
                        // most likely a status refresh event, don't announce or log arming level 
                        if (partition.InitializeArmingLevel(message.ArmingLevel) == true)
                        {
                            if (PartitionArmingLevelChange != null)
                                PartitionArmingLevelChange(this, partition);
                        }

                        // consume pre-arming username  
                        PostArming(partition.Id, message.ArmingLevel);
                        return;
                    }
                }


                // determine effective user name

                if (message.Keyfob)
                {
                    if (pendingUsername != null)
                    {
                        loggedUsername = pendingUsername;
                    }
                    else if (panel.Zones.ContainsKey(message.Zone))
                    {
                        // arming user name will be keyfob text
                        loggedUsername = panel.Zones[message.Zone].Text;
                    }
                    else
                    {
                        // not a recognized zone id
                        loggedUsername = "Keyfob (zone " + message.Zone.ToString() + ")";
                    }
                }
                else
                {
                    int userId = message.User;
                    UserClass userClass = message.UserClass;

                    if (userId == HomeUser.Id)
                    {
                        if (pendingUsername != null)
                        {
                            // impersonation to log username issuing arming command using the home automation arming code
                            loggedUsername = pendingUsername;
                        }
                        else
                        {
                            loggedUsername = HomeUser.Name;
                        }
                    }
                    else if (!string.IsNullOrWhiteSpace(panel.Users[userId].Name))
                    {
                        // impersonation not allowed: received a named user id (all other user ids are the result of
                        // some type of programmatic behavior)

                        loggedUsername = panel.Users[userId].Name;
                    }
                    else
                    {
                        switch (userClass)
                        {
                            case UserClass.Regular:
                                if (pendingUsername != null)
                                {
                                    // impersonation allowed: not a recognized user id 
                                    loggedUsername = pendingUsername;
                                }
                                else
                                {
                                    loggedUsername = "User " + userId.ToString();
                                }
                                break;

                            case UserClass.PartitionDuress:
                                // override the duress name here
                                // todo: lookup from configuration
                                loggedUsername = USERID_DURESS_DESCRIPTION;
                                break;

                            case UserClass.AVM:
                            case UserClass.Dealer:
                            case UserClass.Installer:
                            case UserClass.KeySwitchArm:
                            case UserClass.Panel:
                            case UserClass.PartitionMaster:
                            case UserClass.QuickArm:
                            case UserClass.System:
                            case UserClass.SystemMaster:
                            default:
                                // specialized user class
                                loggedUsername = ParseWordsInCaps(userClass.ToString());
                                break;
                        }

                    }
                }

                if (partition.SilentArming == true && SystemTick > tickSilentArmingTimeout)
                {
                    // we are able to capture when silent mode begins, but it is more difficult to determine when it is off...
                    // the silent arming timeout window controls announcement behavior
                    partition.SilentArming = false;
                }

                // update partition state
                partition.SetArmingLevel(message.ArmingLevel, loggedUsername, message.User, message.UserClass, message.Keyfob, preArmingAutoArm);

                // consume pre-arming username  
                PostArming(partition.Id, message.ArmingLevel);

                if (PartitionArmingLevelChange != null)
                    PartitionArmingLevelChange(this, partition);

                string activity = GetArmingLevelDescription(partition, loggedUsername, true);
                LogEvent(activity, EventCode.Arming);

                

                if (message.ArmingLevel == ArmingLevel.Disarmed)
                {
                    ThreadPool.QueueUserWorkItem(delegate
                    {
                        //todo: disable this behavior if you like
                        try
                        {
                            // wait a sec
                            Thread.Sleep(Timing.Refresh_Timeout);

                            // and clear keypad display warning behavior annoyance 
                            SendKeys(message.Partition, TouchpadKey.Star);
                        }
                        catch (Exception ex)
                        {
                            log.LogError(ex, "Exception occured sending key to panel: " + ex.Message);
                        }
                    });
                }
            }
        }
        

        private void OnAutomationEventLost(AutomationEventLost message)
        {
            ResetPanelMemory(false);
        }

        private void OnBypass(Bypass message)
        {
            if (message.GeneralEventType == AlertClass.Bypass)
                return;

            ThreadPool.QueueUserWorkItem(delegate
            {
                // code below is of marginal value, but previously used for voice announcements

                try
                {
                    // wait for out of order zone update to complete (is this necessary?)
                    Thread.Sleep(Timing.Command_Clear);

                    Partition primaryPartition = PrimaryPartition;
                    Zone zone = panel.Zones[message.Zone];

                    if (primaryPartition.IsArmingPending != null || primaryPartition.ArmingLevel == ArmingLevel.Away || primaryPartition.ArmingLevel == ArmingLevel.Stay)
                    {
                        // log arming status

                        Dictionary<int, Zone> problemZones = panel.GetProblemZones(null);
                        if (problemZones.Count == 0 || (problemZones.Count == 1 && problemZones.ContainsKey(zone.Id)))
                        {
                            LogEvent("All zones secured", EventCode.Panel);
                        }
                        else
                        {
                            Dictionary<int, Zone> openZones = panel.GetOpenZones(null);

                            int perimeterZonesOpen = 0;
                            foreach (Zone openZone in openZones.Values)
                            {
                                if (openZone.IsPerimeter == true)
                                {
                                    perimeterZonesOpen++;
                                }
                            }

                            if (perimeterZonesOpen == 0)
                                LogEvent("Perimeter secured");
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.LogError(ex, "Exception occured in OnBypass: " + ex.Message);
                }
            });
        }

        private void OnClearAutomationDynamicImage(ClearAutomationDynamicImage message)
        {
            ResetPanelMemory(false);
        }

        private void OnEntryExitDelay(EntryExitDelay message)
        {
            Partition partition = this.panel.Partitions[message.Partition];

            if (message.Length == DelayDuration.Standard)
            {
                if (message.DelayPermission == DelayPermission.Entry)
                {
                    // alarm may be imminent

                    if (message.DelayState == DelayState.Start)
                    {
                        partition.IsAlarmPending = true;

                        int countdown = message.DelayTimeSeconds;
                        if (countdown > 0)
                        {
                            LogEvent("Alarm has been tripped");
                        }
                    }
                    else //if (message.DelayState == DelayState.End)
                    {
                        partition.IsAlarmPending = false;
                    }
                }
                else if (message.DelayPermission == DelayPermission.Exit)
                {
                    if (message.DelayState == DelayState.Start)
                    {
                        int countdown = message.DelayTimeSeconds;
                        if (countdown > 0)
                        {
                            partition.IsArmingPending = true;
                        }
                    }
                    else //if (message.DelayState == DelayState.End)
                    {
                        partition.IsArmingPending = false;

                        // arming complete
                        string action = string.Format("Partition {0} armed to {1}", partition.Id, partition.ArmingLevel);
                        LogEvent(action, EventCode.Arming);
                    }
                }
            }
            else if (message.Length == DelayDuration.Extended)
            {
                if (message.DelayState == DelayState.End)
                {
                }
                else if (message.DelayState == DelayState.Start)
                {
                }
            }
            else if (message.Length == DelayDuration.TwiceExtended)
            {
                if (message.DelayState == DelayState.End)
                {
                }
                else if (message.DelayState == DelayState.Start)
                {
                }
            }

            if (PartitionArmingLevelChange != null)
                PartitionArmingLevelChange(this, partition);
        }

        private void OnEquipmentListComplete(EquipmentListComplete message)
        {
            connectionFlushed = true;

            LogEvent("Equipment configuration download complete", EventCode.Panel);
            DynamicDataRefresh();
        }

        private void OnEquipmentListLightToSensor(EquipmentListLightToSensor message)
        {
            //todo: Not implemented
        }

        private void OnEquipmentListOutput(EquipmentListOutput message)
        {
            Output output = this.panel.Outputs[message.Output];
            output.OutputState = message.OutputState;

            if (!string.IsNullOrEmpty(message.Text))
            {
                output.Text = Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(message.Text.ToLower());
            }
        }

        private void OnEquipmentListPartition(EquipmentListPartition message)
        {
            Partition partition = this.panel.Partitions[message.Partition];
            
            if (partition.InitializeArmingLevel(message.ArmingLevelState) == true)
            {
                if (PartitionArmingLevelChange != null)
                    PartitionArmingLevelChange(this, partition);
            }
        }

        private void OnEquipmentListSchedule(EquipmentListSchedule message)
        {
            if (message.Partition < 1 || message.Partition > 6) return;

            Schedule schedule = this.panel.Partitions[message.Partition].Schedules[message.Schedule];
            schedule.Days = message.Days;
            schedule.StartHour = message.StartHour;
            schedule.StartMinute = message.StartMinute;
            schedule.StopHour = message.StopHour;
            schedule.StopMinute = message.StopMinute;
        }

        private void OnEquipmentListScheduledEvent(EquipmentListScheduledEvent message)
        {
            Partition partition = this.panel.Partitions[message.Partition];
            for (int i = 0; i < Partition.CAPABILITY_COUNT_SCHEDULES; i++)
            {
                ScheduledAction action = Schedule.MapScheduledEvent(message.ScheduledEvent);
                Schedule schedule = partition.Schedules[i];

                if (message.ScheduleAssignment[i])
                {
                    // set flag
                    schedule.Actions = schedule.Actions | action;
                }
                else
                {
                    // clear flag
                    schedule.Actions = schedule.Actions & ~action;
                }
            }
        }

        private void OnEquipmentListSuperBusDevice(EquipmentListSuperBusDevice message)
        {
            Device device;
            if (panel.Devices.ContainsKey(message.DeviceUnitId))
            {
                device = this.panel.Devices[message.DeviceUnitId];
            }
            else
            {
                device = new Device(message.DeviceUnitId);
                this.panel.Devices.Add(message.DeviceUnitId, device);
            }

            device.Status = message.DeviceStatus;
        }

        private void OnEquipmentListSuperBusDeviceCapabilities(EquipmentListSuperBusDeviceCapabilities message)
        {
            Device device;
            if (panel.Devices.ContainsKey(message.DeviceUnitId))
            {
                device = this.panel.Devices[message.DeviceUnitId];
            }
            else
            {
                device = new Device(message.DeviceUnitId);
                panel.Devices.Add(message.DeviceUnitId, device);
            }

            if (!device.Capabilities.Contains(message.Capability))
                device.Capabilities.Add(message.Capability);

            device.ExtendedCapabilityData = message.ExtendedCapabilityData;
        }

        private void OnEquipmentListUser(EquipmentListUser message)
        {
            Panel.User user = this.panel.Users[message.User];
            user.Code = message.Code;
        }

        private void OnEquipmentListZone(EquipmentListZone message)
        {
            Zone zone = panel.Zones[message.Zone];

            lock (zone)
            {
                //When sent in response to an equipment list request the bit 0: 1 = tripped will always be ‘0’
                ZoneState newZoneState = message.ZoneState;
                if (zone.State != null)
                {
                    if ((zone.State & ZoneState.Opened) == ZoneState.Opened)
                    {
                        // current zone shows open state (preserve bit 0 state)
                        newZoneState |= ZoneState.Opened;
                    }
                }

                zone.Partition = message.Partition;
                zone.Type = message.ZoneType;
                zone.Group = message.ZoneGroup;

                if (!string.IsNullOrEmpty(message.Text))
                {
                    zone.Text = Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(message.Text.ToLower());
                }
              
                if (panel.Zones[zone.Id].SetState(newZoneState) == true)
                {
                    if (ZoneChange != null)
                        ZoneChange(this, zone);
                }
            }
        }

        private void OnFeatureState(FeatureState message)
        {
            Partition partition = this.panel.Partitions[message.Partition];
            partition.Latchkey = (message.FeaturesOn & Feature.Latchkey) == Feature.Latchkey;
            partition.Chime = (message.FeaturesOn & Feature.Chime) == Feature.Chime;
            partition.EnergySaver = (message.FeaturesOn & Feature.EnergySaver) == Feature.EnergySaver;
            partition.NoDelay = (message.FeaturesOn & Feature.NoDelay) == Feature.NoDelay;
            partition.QuickArm = (message.FeaturesOn & Feature.QuickArm) == Feature.QuickArm;

            if (partition.Id == PARTITION_PRIMARY_ID)
            {
                // panel is not allowed to turn-off home automation enabled silent arming mode. timeout period will revert silent mode
                if ((message.FeaturesOn & Feature.SilentArming) == Feature.SilentArming)
                {
                    partition.SilentArming = true;
                    tickSilentArmingTimeout = Math.Max(SystemTick + Timing.Silent_Arming_Period, tickSilentArmingTimeout);
                }
                else
                {
                    partition.SilentArming = false;
                }
            }
            else
            {
                partition.SilentArming = (message.FeaturesOn & Feature.SilentArming) == Feature.SilentArming;
            }

            if (PartitionArmingLevelChange != null)
                PartitionArmingLevelChange(this, partition);

            LogEvent("Partition features received", EventCode.Panel);
        }

        private void OnKeyfob(Keyfob message)
        {
            long tick = SystemTick;

            Partition partition = this.panel.Partitions[message.Partition];
            ArmingLevel? armingLevelWhenPressed = partition.ArmingLevel;

            KeyfobButton? previousButton = lastKeyfobButton;
            Zone previousKeyfobZone = lastKeyfobPressed;

            lastKeyfobButton = message.KeyCode;
            lastKeyfobPressed = this.panel.Zones[message.Zone];

            // note: doesn't support concurrent keyfobs or revisited button repetition
            if (previousButton != lastKeyfobButton || previousKeyfobZone.Id != lastKeyfobPressed.Id || tick > tickKeyfobButtonRepeatExpiry)
            {
                // new button pressed, begin will block if any outstanding calls are active
                BeginKeyfobButton(armingLevelWhenPressed);

                // release any outstanding calls
                waitKeyfobButton.Set();
            }
            else
            {
                // increment button counter
                waitKeyfobButton.Set();
            }
        }

        private void OnLightsState(LightsState message)
        {
            Partition partition = this.panel.Partitions[message.Partition];

            if (partition.AllLights != message.AllLights)
            {
                partition.AllLights = message.AllLights;
                LogEvent("All lights toggle", EventCode.Lights);
            }

            for (int i = 0; i < Partition.CAPABILITY_COUNT_LIGHTS; i++)
            {
                Light light = partition.Lights[i + 1];
                if (light.Enabled != message.LightState[i])
                {
                    light.SetEnabled(message.LightState[i]);
                    LogEvent(string.Format("Light {0} {1}", i+1, (string)(light.Enabled == true ? "on" : "off")), EventCode.Lights);
                }

            }
        }

        private void OnOpeningClosing(OpeningClosing message)
        {
            LogEvent("Opening or closing event: " + message.ToString(), EventCode.Alarm);
        }

        private void OnPanelType(InboundMessages.PanelType message)
        {
            this.panel.PanelTypeId = message.PanelTypeId;
            this.panel.HardwareRevision = message.HardwareRevision;
            this.panel.SoftwareRevision = message.SoftwareRevision;
            this.panel.SerialNumber = message.SerialNumber;
        }

        private void OnPartitionEvent(Automation.Concord.InboundMessages.Alerts.PartitionEvent message)
        {
            Partition partition = this.panel.Partitions[message.Partition];

            switch (message.PartitionEventType)
            {
                case PartitionEventType.ScheduleOn:
                case PartitionEventType.ScheduleOff:
                case PartitionEventType.LatchkeyOn:
                case PartitionEventType.LatchkeyOff:
                case PartitionEventType.SmokeDetectorsReset:
                case PartitionEventType.PartitionRemoteAccess:
                case PartitionEventType.ManualForceArm:
                case PartitionEventType.AutoForceArm:
                    LogEvent(ParseWordsInCaps(message.PartitionEventType.ToString()), EventCode.Arming);
                    break;

                case PartitionEventType.ArmingProtestBegun:
                    partition.ArmingProtest = true;

                    if (PartitionArmingLevelChange != null)
                        PartitionArmingLevelChange(this, partition);

                    if (partition.Id != 1) return;

                    // cannot automatically bypass keyfob arming button due to siren bug in panel
                    if (lastKeyfobButton == KeyfobButton.Arm && SystemTick < tickKeyfobButtonRepeatExpiry)
                    {
                        return;
                    }
                    else
                    {
                        // auto bypass open zones and continue arming
                        ThreadPool.QueueUserWorkItem(delegate
                        {
                            //todo: make configuration based decision here to bypass protests
                            try
                            {
                                BypassProtest(1);
                            }
                            catch (Exception ex)
                            {
                                log.LogError(ex, "Exception occured bypassing protest: " + ex.Message);
                            }
                        });
                    }

                    LogEvent("Arming protest begun", EventCode.Arming);
                    break;

                case PartitionEventType.ArmingProtestEnded:

                    partition.ArmingProtest = false;

                    if (PartitionArmingLevelChange != null)
                        PartitionArmingLevelChange(this, partition);

                    LogEvent("Arming protest ended", EventCode.Arming);
                    break;
            }
        }

        private void OnPartitionTest(PartitionTest message)
        {
            LogEvent("Partition test", EventCode.Panel);
        }

        private void OnReserved(Reserved message)
        {
            log.LogWarning("Reserved message received.");
            LogEvent("Reserved message received");
        }

        private void OnSirenGo(SirenGo message)
        {
        }

        private void OnSirenSetup(SirenSetup message)
        {
            LogEvent("Siren setup: " + message.ToString(), EventCode.Panel);
        }

        private void OnSirenStop(SirenStop message)
        {
            LogEvent("Siren stopped", EventCode.Panel);
        }

        private void OnSirenSynchronize(SirenSynchronize message)
        {
        }

        private void OnSystemConfigurationChange(SystemConfigurationChange message)
        {
            switch (message.SystemConfigurationChangeType)
            {
                case SystemConfigurationChangeType.DateTimeChanged:
                    LogEvent("Panel date or time changed", EventCode.Panel);
                    break;

                case SystemConfigurationChangeType.ProgramModeEntry:
                    LogEvent("Program mode entered", EventCode.Panel);
                    break;

                case SystemConfigurationChangeType.ProgramModeExitWithChange:
                    LogEvent("Panel configuration changed", EventCode.Panel);
                    ResetPanelMemory(true);
                    break;

                case SystemConfigurationChangeType.PanelBackInService:
                    LogEvent("Panel service restored", EventCode.Panel);
                    break;
            }
        }

        private void OnSystemEvent(SystemEvent message)
        {
            if (message.SystemEventType == SystemEventType.OutputOn)
            {
                Output output = panel.Outputs[message.SpecificEventData + 64];
                output.OutputState = OutputState.On;

                string action = output.Text + " activated";

                if (message.SpecificEventData == 1 || message.SpecificEventData == 2)
                    LogEvent(action, EventCode.Output);
                else
                    LogEvent(action, EventCode.Output);
            }
            else if (message.SystemEventType == SystemEventType.OutputOff)
            {
                Output output = panel.Outputs[message.SpecificEventData + 64];
                output.OutputState = OutputState.Off;
            }
        }

        private void OnSystemTrouble(SystemTrouble message)
        {
            if (message.GeneralEventType == AlertClass.SystemTrouble)
            {
                string condition = ParseWordsInCaps(Enum.GetName(typeof(SystemTroubleType), message.SystemTroubleType));
                string source = "Unknown source";

                switch (message.Source)
                {
                    case SourceDeviceType.BusDevice:
                        source = "Bus Device " + message.SourceDeviceUnitId.ToString();
                        break;

                    case SourceDeviceType.LocalPhone:
                        source = "Local Phone";
                        break;

                    case SourceDeviceType.RemotePhone:
                        source = "Remote Phone";
                        break;

                    case SourceDeviceType.System:
                        source = "System";
                        break;

                    case SourceDeviceType.Zone:
                        source = this.panel.Zones[message.Zone].Text + " sensor";
                        break;
                }

                string notification = string.Format("{0} reports {1} condition", source, condition);

                LogEvent(notification, EventCode.Panel);
            }
        }

        private void OnTemperature(Temperature message)
        {
        }
        
        private void OnTimeAndDate(TimeAndDate message)
        {
            this.panel.DateTime = new DateTime(message.Year, message.Month, message.Day, message.Hour, message.Minute, 30);

            //optimistically assume this is the end of a data refresh
            waitRefreshComplete.Set();
        }

        private void OnTouchpadDisplay(TouchpadDisplay message)
        {
            string text = message.Text;

            if (text == null) return; // why?

            Partition partition;
            if (message.MessageType == DisplayTextType.Broadcast)
            {
                for (int i = 1; i <= 6; i++)
                {
                    partition = this.panel.Partitions[i];
                    partition.DisplayText = text;

                    if (DisplayTextChange != null)
                        DisplayTextChange(this, partition);
                }
            }
            else
            {
                partition = this.panel.Partitions[message.Partition];

                // does this code need to duplicated for broadcasts?

                if (text == "SILENT ARM ON")
                {
                    partition.SilentArming = true;
                    if (partition.Id == PARTITION_PRIMARY_ID)
                    {
                        tickSilentArmingTimeout = Math.Max(SystemTick + Timing.Silent_Arming_Period, tickSilentArmingTimeout);
                    }
                }

                partition.DisplayText = text;

                if (DisplayTextChange != null)
                    DisplayTextChange(this, partition);
            }

            waitDisplayUpdate.Set();
        }

        private void OnTrouble(Trouble message)
        {
            string condition = ParseWordsInCaps(Enum.GetName(typeof(TroubleType), message.TroubleType));
            string source = "Unknown source";

            switch (message.Source)
            {
                case SourceDeviceType.BusDevice:
                    source = "Bus Device " + message.SourceDeviceUnitId.ToString();
                    break;

                case SourceDeviceType.LocalPhone:
                    source = "Local Phone";
                    break;

                case SourceDeviceType.RemotePhone:
                    source = "Remote Phone";
                    break;

                case SourceDeviceType.System:
                    source = "System";
                    break;

                case SourceDeviceType.Zone:
                    source = this.panel.Zones[message.Zone].Text + " sensor";
                    break;
            }

            string notification = string.Format("{0} reports {1} condition", source, condition);

            LogEvent(notification, EventCode.Panel);
        }

        private void OnUnknown(Unknown message)
        {
            log.LogWarning("Unknown message received.");
            LogEvent("Unknown message: " + message.ToAsciiHex(), EventCode.Panel);
        }

        private void OnUserLights(UserLights message)
        {
            Partition partition = this.panel.Partitions[message.Partition];

            string action;
            if (message.LightCode == 0)
            {
                bool powered = message.Enabled;

                //todo: light event signal
                //Worker.Start(
                //    "HomeAssistant.AllLights",
                //    delegate { return HomeAssistant.Api.SetAllLights(powered); },
                //    delegate (object sender, EventArgs e) { LogActivity("HomeAssistant user set all lights " + (powered ? "on" : "off"), Activity.System); },
                //    delegate (object sender, EventArgs e) { ReportSystemActivityError("HomeAssistant error executing user all lights " + (powered ? "on" : "off")); },
                //    3
                //);

                action = string.Format("Lights {0}", (powered ? "turned on" : "turned off"));
            }
            else
            {
                //specific light
                Light light = partition.Lights[message.LightCode];
                action = string.Format("{0} {1}", GetLightDescription(light.Partition, light.Id), (message.Enabled ? "turned on" : "turned off"));
            }

            // since it's difficult to determine who made this request (home or user), just assume that it's the home
            LogEvent(action, EventCode.Lights);
        }

        private void OnZoneStatus(ZoneStatus message)
        {
            Zone zone = panel.Zones[message.Zone];
            zone.Partition = message.Partition;

            if (panel.Zones[zone.Id].SetState(message.ZoneState) == true)
            {
                if (ZoneChange != null)
                    ZoneChange(this, zone);
            }
        }

        #endregion Concord Automation Message Handlers


        #region External Events ****************************************************************


        public delegate void AlarmCallback(AutomationDeviceServer sender, Panel.Panel panel, Partition partition); 
        public delegate void DataRefreshedCallback(AutomationDeviceServer sender);
        public delegate void KeyfobButtonCallback(AutomationDeviceServer sender, ArmingLevel? armingLevelWhenPressed, Zone keyfob, KeyfobButton button, int presses);
        public delegate void PartitionArmingLevelCallback(AutomationDeviceServer sender, Panel.Partition partition);
        public delegate void PartitionCallback(AutomationDeviceServer sender, Partition partition);
        public delegate void ZoneCallback(AutomationDeviceServer sender, Zone zone);

        public event AlarmCallback AlarmChange;
        public event DataRefreshedCallback DataRefreshed;
        public event PartitionCallback DisplayTextChange;
        public event KeyfobButtonCallback KeyfobButtonChange;
        public event PartitionArmingLevelCallback PartitionArmingLevelChange;
        public event ZoneCallback ZoneChange;

        #endregion External Events


        #region Concord Protocol Actions *******************************************************

        public void DynamicDataRefresh()
        {

            bool result = SendMessage(new OutboundMessages.DynamicDataRefreshRequest());
            if (result)
            {
                LogEvent("Dynamic data refresh requested", EventCode.Panel);

                // concerned about the code below
                tickDataRefreshTimeout = SystemTick + Timing.Refresh_Timeout;

                ThreadPool.QueueUserWorkItem(delegate
                {
                    try
                    {
                        // wait for panel to send refresed data
                        waitRefreshComplete.WaitOne(Timing.Refresh_Timeout);

                        tickDataRefreshTimeout = 0;

                        if (DataRefreshed != null)
                            DataRefreshed(this);
                    }
                    catch (ThreadInterruptedException)
                    {
                        return;
                    }
                    catch (Exception ex)
                    {
                        log.LogError(ex, "Exception occured during dynamic data refresh: " + ex.Message);
                    }
                });
            }
            else
            {
                tickDownloadTimeout = 0;
            }
        }
        
        public void ResetPanelMemory(bool nuke)
        {
            // reset memory
            if (nuke)
            {
                this.panel.Reset();
                log.LogDebug("Nuked panel data");
            }

            tickDownloadTimeout = SystemTick + Timing.Equipment_Download_Timeout;

            LogEvent("Panel image reset", EventCode.Panel);
 
            ThreadPool.QueueUserWorkItem(delegate
            {
                try
                {
                    // retry up to three times
                    for (int retryCount = 1; retryCount <= 3; retryCount++)
                    {
                        if (SendMessage(new OutboundMessages.FullEquipmentListRequest()))
                            break;

                        log.LogWarning("FullEquipmentListRequest send failed");
                    }
                }
                catch (Exception ex)
                {
                    log.LogError(ex, "Exception occured sending full equipment list request: " + ex.Message);
                }
            });
        }

        /// <summary>
        /// Should be called prior to every arming action to capture intended arming user name, arming level and options
        /// </summary>
        /// <param name="userName"></param>
        private void PreArming(int partitionId, string userName, ArmingLevel armingLevel, bool nodelay, bool silent, bool autoArm)
        {
            preArmingUsername = userName;
            preArmingAutoArm = autoArm;
            tickPreArmingStarted = SystemTick;

            panel.Partitions[partitionId].NoDelay = nodelay;
            panel.Partitions[partitionId].SilentArming = silent;

            if (silent)
            {
                tickSilentArmingTimeout = Math.Max(SystemTick + Timing.Silent_Arming_Period + Timing.Command_Clear, tickSilentArmingTimeout);
            }
            else
            {
                // may introduce uncertainty in silent arming, effectively enables audible reports
                tickSilentArmingTimeout = 0;
            }

            waitPostArmingResult[partitionId].Reset();
        }

        /// <summary>
        /// Primary partition only: Releases specified arming wait handle and clears last arming request info
        /// </summary>
        /// <returns>User who initiated the arming action</returns>
        private void PostArming(int partitionId, ArmingLevel armingLevel)
        {
            waitPostArmingResult[partitionId].Set();

            preArmingUsername = null;
            preArmingAutoArm = false;
            tickPreArmingStarted = 0;
        }

        private void LoadConcordConfiguration()
        {
            if (config == null)
                throw new ApplicationException("Panel configuration is invalid");

            if (config.Users != null)
                foreach (ConcordConfiguration.PanelUser user in config.Users)
                {
                    // load user names from configuration since they are not stored in the panel memory
                    this.panel.Users[user.Id].Name = user.Name;
                }

            if (config.Partitions != null)
                foreach (ConcordConfiguration.PanelPartition partition in config.Partitions)
                {
                    // load user names from configuration since they are not stored in the panel memory
                    this.panel.Partitions[partition.Id].Name = partition.Name;
                }
        }

        private bool SendMessage(Message message)
        {
            bool success = false;

            try
            {
                MessageResponse response = processor.SendMessage(message);
                success = (response == MessageResponse.ACK);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "An error occured sending message to panel automation device");
            }

            return success;
        }

        #endregion Concord Protocol Actions


        #region Keyfob *************************************************************************

        private void BeginKeyfobButton(ArmingLevel? armingLevelWhenPressed)
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                try
                {
                    lock (lockBeginKeyfobButton)
                    {
                        KeyfobButton? repeatButton = lastKeyfobButton;
                        Zone repeatKeyfob = lastKeyfobPressed;
                        int keyfobButtonRepeatCount = 0;

                        tickKeyfobButtonRepeatExpiry = SystemTick + Timing.Keyfob_Repeat_Button_Timeout;

                        // loop while button is repeatedly pressed
                        while (true)
                        {
                            bool buttonPressed = waitKeyfobButton.WaitOne(Timing.Keyfob_Repeat_Button_Timeout);

                            //if (SystemTick < tickKeyfobStarButtonTimeout) break;

                            if (buttonPressed)
                            {
                                if (lastKeyfobButton != repeatButton || lastKeyfobPressed.Id != repeatKeyfob.Id)
                                {
                                    // new button pressed
                                    int presses = (int)Math.Ceiling((double)keyfobButtonRepeatCount / 2);
                                    KeyfobButtonUp(armingLevelWhenPressed, repeatKeyfob, repeatButton.Value, presses);

                                    keyfobButtonRepeatCount = 1;
                                    break; // e.g. return
                                }
                                else
                                {
                                    // repeat button
                                    keyfobButtonRepeatCount++;
                                    tickKeyfobButtonRepeatExpiry = SystemTick + Timing.Keyfob_Repeat_Button_Timeout;

                                    int presses = (int)Math.Ceiling((double)keyfobButtonRepeatCount / 2);
                                    if (keyfobButtonRepeatCount % 2 == 0)
                                    {
                                        KeyfobButtonDown(repeatKeyfob, repeatButton.Value, presses);
                                    }
                                    else
                                    {
                                        KeyfobButtonPreDown(repeatKeyfob, repeatButton.Value, presses);
                                    }
                                    continue;
                                }
                            }
                            else
                            {
                                // button not repeated
                                int presses = (int)Math.Ceiling((double)keyfobButtonRepeatCount / 2);
                                KeyfobButtonUp(armingLevelWhenPressed, repeatKeyfob, repeatButton.Value, presses);
                                break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.LogError(ex, "Exception occured in begin keyfob button: " + ex.Message);
                }
            });
        }

        private void KeyfobButtonDown(Zone keyfob, KeyfobButton button, int presses)
        {
            string action;
            if (presses == 1)
            {
                action = string.Format("{0} button on {1} pressed", button, keyfob.Text);
                LogEvent(action, EventCode.KeyfobButton);
            }
            else if (presses > 1)
            {
                // announce presses beyond the first press (that was already announced)
                action = string.Format("{0} button on {1} pressed {2}", button, keyfob.Text, (presses > 2) ? "repeatedly" : (presses > 1) ? "twice" : "");
                LogEvent(action, EventCode.KeyfobButton);
            }
            return;
        }

        private void KeyfobButtonPreDown(Zone keyfob, KeyfobButton button, int presses)
        {
            if (button == KeyfobButton.Disarm)
            {
                // clear all waiting conditions, panel action has been overridden
                PostArming(keyfob.Partition, ArmingLevel.Indeterminate);

                // set arming user
                PreArming(keyfob.Partition, keyfob.Text, ArmingLevel.Disarmed, false, false, false);
            }
            else if (button == KeyfobButton.Arm)
            {
                // clear all waiting conditions, panel action has been overridden
                PostArming(keyfob.Partition, ArmingLevel.Indeterminate);

                // set arming user
                PreArming(keyfob.Partition, keyfob.Text, ArmingLevel.Indeterminate, false, false, false);
            }
        }

        private void KeyfobButtonUp(ArmingLevel? armingLevelWhenPressed, Zone keyfob, KeyfobButton button, int presses)
        {
            if (KeyfobButtonChange != null)
                KeyfobButtonChange(this, armingLevelWhenPressed, keyfob, button, presses);
        }

        #endregion Keyfob


        #region Activity Tracking and Logging **************************************************

       
        private enum EventCode : int
        {
            Alarm = 1000,
            Arming = 1100,
            KeyfobButton = 1200,
            Lights = 1300,
            Output = 1400,
            Panel = 0
        }

        private bool IsDataRefreshing
        {
            get { return SystemTick < tickDataRefreshTimeout; }
        }

        private void LogEvent(string description)
        {
            //todo: check out IsOccupantActivityLogged
            LogEvent(description, EventCode.Panel);
        }

        private void LogEvent(string description, EventCode eventCode)
        {
            log.LogInformation((int)eventCode, eventCode.ToString() + ": " + description);
        }

        #endregion Activity Tracking and Logging


        #region Reporting and Notifications ****************************************************

        private static string GetArmingLevelDescription(Partition partition, string userName, bool levelChanged)
        {
            string announcement = "";

            string delayToken = "";
            if (partition.ArmingLevel == ArmingLevel.Away || partition.ArmingLevel == ArmingLevel.Stay)
            {
                if (partition.NoDelay == false)
                    delayToken = " with delay";
            }

            string silenceToken = "";
            if (partition.SilentArming == true)
            {
                silenceToken = "silently ";
            }

            if (levelChanged)
            {
                if (string.IsNullOrEmpty(userName))
                    userName = "Unknown";

                string partitionToken = partition.Name.ToLowerInvariant();

                switch (partition.ArmingLevel)
                {
                    case ArmingLevel.Away:
                        announcement = string.Format("{0} {1}armed {2} to away{3}", userName, silenceToken, partitionToken, delayToken);
                        break;

                    case ArmingLevel.Stay:
                        announcement = string.Format("{0} {1}armed {2} to stay{3}", userName, silenceToken, partitionToken, delayToken);
                        break;

                    case ArmingLevel.Disarmed:
                        announcement = string.Format("{0} {1}disarmed {2}", userName, silenceToken, partitionToken);
                        break;

                    case ArmingLevel.PhoneTest:
                        announcement = string.Format("{0} enabled {1} phone test", userName, partitionToken);
                        break;

                    case ArmingLevel.SensorTest:
                        announcement = string.Format("{0} enabled {1} sensor test", userName, partitionToken);
                        break;

                    case ArmingLevel.Indeterminate:
                        announcement = string.Format("Unknown {0} arming level", partitionToken);
                        break;
                }
            }
            else
            {
                string partitionToken = "";
                if (partition.Id == PARTITION_PRIMARY_ID)
                {
                    partitionToken = "Home";
                }
                else
                {
                    partitionToken = "Partition " + partition.Id.ToString();
                }

                switch (partition.ArmingLevel)
                {
                    case ArmingLevel.Away:
                        announcement = string.Format("{0} is armed to away{1}", partitionToken, delayToken);
                        break;

                    case ArmingLevel.Stay:
                        announcement = string.Format("{0} is armed to stay{1}", partitionToken, delayToken);
                        break;

                    case ArmingLevel.Disarmed:
                        announcement = string.Format("{0} is disarmed", partitionToken);
                        break;

                    case ArmingLevel.PhoneTest:
                        announcement = string.Format("{0} phone test is active", partitionToken);
                        break;

                    case ArmingLevel.SensorTest:
                        announcement = string.Format("{0} sensor test is active", partitionToken);
                        break;

                    case ArmingLevel.Indeterminate:
                        announcement = string.Format("{0} arming level is indeterminate", partitionToken);
                        break;
                }
            }

            return announcement;
        }

        private string GetLightDescription(int partitionId, int light)
        {
            string description;
            switch (light)
            {
                default:
                    description = "Light " + light.ToString();
                    break;
            }
            return description;
        }

        #endregion Reporting and Notifications


        #region Public Methods *****************************************************************

        public bool IsPanelConnected { get => isPanelConnected; }

        public bool CheckImageValidity()
        {
            // check for undesirable conditions reflective of failure to complete image download
            foreach (Partition partition in this.Panel.Partitions.Values)
            {
                if (partition.ArmingLevel == null)
                    return false;

                foreach (Zone zone in panel.GetZones(partition.Id))
                {
                    if (zone.State == null)
                        return false;
                }
            }

            return true;
        }

        public Panel.Panel Panel { get => panel; set => panel = value; }

        public AutomationDeviceServer(ConcordConfiguration config, ILogger logger, ICommunicationDevice device)
        {
            this.log = logger;
            this.device = device;
            this.config = config;

            this.panel = new Panel.Panel();
        }

        public bool ActivateOutput(int partitionId, int outputId)
        {
            if (outputId < 1 || outputId > 6) throw new ArgumentOutOfRangeException("outputId");

            string code = "77" + outputId.ToString();

            bool success;
            Keypress message = new Keypress(partitionId, TouchpadKeyCodeMap.GetKeypress(code));
            success = SendMessage(message);

            if (!success) return false;

            Output output = panel.Outputs[outputId + 64];

            success = output.OutputState == OutputState.On;
            for (int retry = 0; !success && retry < 3; retry++)
            {
                success = ((IStateChangeWaitHandle)output).StateChange.WaitOne(Math.Max(Timing.Command_Clear, Timing.Output_Activation_Time));
            }

            return success;
        }

        /// <summary>
        /// Uses preconfigured home automation user code for arming action
        /// </summary>
        /// <param name="username"></param>
        /// <param name="partitionId"></param>
        /// <param name="level"></param>
        /// <param name="silent"></param>
        /// <param name="nodelay"></param>
        /// <param name="autonomous"></param>
        /// <returns></returns>
        public bool Arm(string username, int partitionId, ArmingLevel level, bool silent, bool nodelay, bool autonomous)
        {
            lock (lockArming)
            {
                Partition partition = this.panel.Partitions[partitionId];

                if (level != ArmingLevel.Disarmed)
                {
                    // unfortunately I never found a means to determine if the panel is currently in alarm after a panel image reset ...
                    // sending home code keypresses in alarm will disarm system, and disarming hasn't been requested. this means that automous actions 
                    // could fail to run or far worse disable an active alarm

                    if (partition.IsAlarmPending != false || partition.InAlarm != false)
                    {

                        foreach (Zone zone in panel.GetProblemZones(null).Values)
                        {
                            // any problem zone during auto arming when alarm level isn't known will generate an exception

                            // todo: seek expert advice on how to retrieve current alarm and pending alarm states (probably never)
                            string errorMessage = string.Format("System fail-safe refused partition {0} arming due to alarm state", partitionId);

                            LogEvent(errorMessage, EventCode.Panel);
                            throw new ApplicationException(errorMessage);
                        }
                    }
                }

                // ignore nodelay request and set nodelay to avoid SIA behavior
                if (level == ArmingLevel.Away)
                {
                    // ANSI-SIA CP-01 requires Auto stay arming: if the system detects that no one opened and
                    // closed a delay door within the delay time. It assumes that someone is still inside and
                    //arms to 2—STAY to prevent a false alarm.

                    nodelay = true;
                }

                string code = "";

                if (silent)
                {
                    code += "5";
                }

                if (level == ArmingLevel.Disarmed)
                {
                    // FF delay, per the instruction manual on macros
                    code += "1FF";
                    code += HomeUser.Code;
                }
                else
                {
                    if (level == ArmingLevel.Stay)
                    {
                        code += "2";
                    }
                    else if (level == ArmingLevel.Away)
                    {
                        code += "3";
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException("level");
                    }

                    if (partition.QuickArm != true)
                    {
                        // FF delay, per the instruction manual on macros
                        code += "FF";
                        code += HomeUser.Code;
                    }

                    if (nodelay)
                        code += "4";
                }

                // retry up to three times
                bool armingSucceeded = false;
                for (int retryCount = 0; retryCount < 3; retryCount++)
                {
                    // override features
                    PreArming(partitionId, username, level, nodelay, silent, autonomous);

                    Keypress message = new Keypress(partitionId, TouchpadKeyCodeMap.GetKeypress(code));
                    if (!SendMessage(message)) continue;

                    armingSucceeded = WaitForArmingLevel(partitionId, level, Timing.Command_Clear);

                    if (armingSucceeded) break;
                    log.LogWarning("Set arming level timed out");
                }

                if (!armingSucceeded)
                {
                    log.LogWarning("Set arming level failed");
                }
                return armingSucceeded;
            }
        }

        /// <summary>
        /// Arms primary partition to specified level. Only to be used by autonomous home automation actions!
        /// </summary>
        /// <param name="level"></param>
        /// <param name="silent"></param>
        /// <param name="nodelay"></param>
        /// <returns></returns>
        public bool AutoArmHome(int partitionId, ArmingLevel level, bool nodelay)
        {
            return Arm(HomeUser.Name, partitionId, level, true, nodelay, true);
        }

        public bool BeginUserProgramming(int partitionId)
        {
            // FF delay, per the instruction manual on macros
            string keys = "9FF" + GetMasterCode(partitionId);
            Keypress message = new Keypress(partitionId, TouchpadKeyCodeMap.GetKeypress(keys));
            return SendMessage(message);
        }

        /// <summary>
        /// Bypasses a zone that currently has an unbypassed zone state
        /// </summary>
        /// <param name="partition"></param>
        /// <param name="zone"></param>
        /// <returns></returns>
        public bool BypassZone(int partitionId, int zoneId)
        {
            if (this.panel.Zones.ContainsKey(zoneId))
            {
                Zone zone = this.panel.Zones[zoneId];
                if (zone.Partition != partitionId) return false;

                if ((zone.State & ZoneState.Bypassed) != ZoneState.Bypassed)
                {
                    return ToggleZoneBypass(zone.Partition, zone.Id);
                }
                else
                {
                    return true;
                }
            }
            return false;
        }

        public bool EndSystemProgramming(int partitionId)
        {
            string keys = "*****A#";
            Keypress message = new Keypress(partitionId, TouchpadKeyCodeMap.GetKeypress(keys));
            return SendMessage(message);
        }

        public bool EndUserProgramming(int partitionId)
        {
            Keypress message = new Keypress(partitionId, "***00#");
            return SendMessage(message);
        }

        public List<Zone> GetPartitionZones(int partitionId)
        {
            return panel.GetZones(partitionId);
        }

        public bool KeyfobArmAway(string username, int partitionId)
        {
            lock (lockArming)
            {
                if (panel.Partitions[partitionId].ArmingLevel == ArmingLevel.Away) return true;

                // first press is for stay
                PreArming(partitionId, username, ArmingLevel.Away, false, false, false);
                Keypress message = new Keypress(partitionId, TouchpadKey.KeyfobArm);
                if (!SendMessage(message)) return false;

                //second press is for away
                PreArming(partitionId, username, ArmingLevel.Away, false, false, false);
                // send button press twice per concord behavior
                if (!SendMessage(message)) return false;

                return WaitForArmingLevel(partitionId, ArmingLevel.Away, Timing.Exit_Delay);
            }
        }

        public bool KeyfobArmStay(string username, int partitionId)
        {
            lock (lockArming)
            {
                PreArming(partitionId, username, ArmingLevel.Stay, false, false, false);

                Keypress message = new Keypress(partitionId, TouchpadKey.KeyfobArm);
                if (!SendMessage(message)) return false;

                return WaitForArmingLevel(partitionId, ArmingLevel.Stay, Timing.Exit_Delay);
            }
        }

        public bool KeyfobDisarm(string username, int partitionId)
        {
            lock (lockArming)
            {
                PreArming(partitionId, username, ArmingLevel.Disarmed, false, false, false);

                Keypress message = new Keypress(partitionId, TouchpadKey.KeyfobDisarm);
                if (!SendMessage(message)) return false;

                return WaitForArmingLevel(partitionId, ArmingLevel.Disarmed, Timing.Command_Clear);
            }
        }

        public bool SendKeys(int partitionId, params TouchpadKey[] keys)
        {
            Automation.Concord.OutboundMessages.Keypress message = new Automation.Concord.OutboundMessages.Keypress(partitionId, keys);
            return SendMessage(message);
        }

        public bool SendKeyString(int partitionId, string numbersPoundStar)
        {
            TouchpadKey[] keys = TouchpadKeyCodeMap.GetKeypress(numbersPoundStar);
            return SendKeys(partitionId, keys);
        }

        public bool SetDateAndTime(DateTime newTime)
        {
            if (!BeginUserProgramming(6)) return false;
            Thread.Sleep(500);

            Keypress message;

            string changeTimeString = string.Format("020{0:00}{1:00}#", newTime.Hour, newTime.Minute);
            string changeDateString = string.Format("*021{0:MMddyy}#", newTime);

            message = new Keypress(6, TouchpadKeyCodeMap.GetKeypress(changeDateString));
            if (!SendMessage(message)) return false;
            Thread.Sleep(250);

            message = new Keypress(6, TouchpadKeyCodeMap.GetKeypress(changeTimeString));
            if (!SendMessage(message)) return false;
            Thread.Sleep(250);

            return EndUserProgramming(6);
        }

        public void SetPanelAllLights(int partitionId, bool enabled)
        {
            lock (lockAllLightsControl)
            {
                Partition partition = this.panel.Partitions[partitionId];

                if (enabled)
                {
                    if (partition.AllLights == false)
                    {
                        // toggle all lights mode on
                        TogglePanelAllLights(partition.Id);
                    }
                    else
                    {
                        foreach (Light light in partition.Lights.Values)
                        {
                            if (light.Enabled == false)
                                SetPanelLight(partitionId, light.Id, true);
                        }
                    }
                }
                else
                {
                    if (partition.AllLights == true)
                    {
                        // toggle all lights mode off
                        TogglePanelAllLights(partition.Id);
                    }
                    else
                    {
                        foreach (Light light in partition.Lights.Values)
                        {
                            if (light.Enabled == true)
                                SetPanelLight(partitionId, light.Id, false);
                        }
                    }
                }
            }
        }

        public void SetPanelLight(int partitionId, int lightId, bool enabled)
        {
            Partition partition = this.panel.Partitions[partitionId];

            if (enabled)
            {
                if (partition.Lights[lightId].Enabled == false)
                {
                    TogglePanelLight(partitionId, lightId);
                }
            }
            else
            {
                if (partition.Lights[lightId].Enabled == true)
                {
                    TogglePanelLight(partitionId, lightId);
                }
            }
        }

        public bool ToggleChime(int partitionId)
        {
            Keypress message = new Keypress(partitionId, TouchpadKeyCodeMap.GetKeypress("71"));
            return SendMessage(message);
        }

        public bool TogglePanelAllLights(int partitionId)
        {
            Keypress message;
            message = new Keypress(partitionId, TouchpadKey.LightsToggle);
            return SendMessage(message);
        }

        public bool TogglePanelLight(int partitionId, int light)
        {
            Keypress message = new Keypress(partitionId, TouchpadKeyCodeMap.GetKeypress("0" + light.ToString()));
            return SendMessage(message);
        }

        public bool ToggleZoneBypass(int partitionId, int zoneId)
        {
            if (!panel.Partitions.ContainsKey(partitionId) || !panel.Zones.ContainsKey(zoneId)) return false;

            Partition partition = panel.Partitions[partitionId];

            if (partition.IsAlarmPending == true || partition.InAlarm == true)
            {
                //string errorMessage = string.Format("Partition {0} zone {0} bypass toggle refused", partitionId, zoneId);

                //LogActivity(errorMessage, Activity.Automation);
                //Log(errorMessage, NoticeDelivery.Maximum);
                return false;
            }

            Zone zone = panel.Zones[zoneId];
            ZoneState? priorState = zone.State;
            ZoneState toggledState;

            if (zone.Behavior == null)
            {
                // not sure how to proceed, fail...
                return false;
            }
            else if (partition.ArmingLevel != null && !zone.Behavior.Value.IsActive(partition.ArmingLevel.Value))
            {
                // zone bypass is not applicable, success?
                return true;
            }

            // FF delay, per the instruction manual on macros
            string keys = string.Format("#{0}FF{1:00}", HomeUser.Code, zoneId);
            Keypress message = new Keypress(partitionId, TouchpadKeyCodeMap.GetKeypress(keys));
            if (!SendMessage(message)) return false;

            if ((priorState & ZoneState.Bypassed) == ZoneState.Bypassed)
            {
                // toggled state is unbypassed
                toggledState = priorState != null ? (priorState.Value & ZoneState.Bypassed) ^ ZoneState.Bypassed : ZoneState.Normal;
            }
            else
            {
                // toggled state is bypassed
                toggledState = priorState != null ? priorState.Value & ZoneState.Bypassed : ZoneState.Bypassed;
            }

            return WaitForZoneState(zone, toggledState, Timing.Command_Clear);
        }

        /// <summary>
        /// Unbypasses a zone that currently has a bypassed zone state
        /// </summary>
        /// <param name="partition"></param>
        /// <param name="zone"></param>
        /// <returns></returns>
        public bool UnbypassZone(int partitionId, int zoneId)
        {
            if (this.panel.Zones.ContainsKey(zoneId))
            {
                Zone zone = this.panel.Zones[zoneId];
                if (zone.Partition != partitionId) return false;

                if ((zone.State & ZoneState.Bypassed) == ZoneState.Bypassed)
                {
                    return ToggleZoneBypass(zone.Partition, zone.Id);
                }
                else
                {
                    return true;
                }
            }
            return false;
        }
        public bool ViewHistory(int partitionId)
        {
            // FF delay, per the instruction manual on macros
            string keys = "8FF" + GetMasterCode(partitionId) + "8";
            Keypress message = new Keypress(partitionId, TouchpadKeyCodeMap.GetKeypress(keys));
            return SendMessage(message);
        }

        #endregion Public Methods


        #region Panel Specific Timings *********************************************************
        public static class Timing
        {
            /// <summary>
            /// Time it takes a partial command to be cleared from touchpad
            /// </summary>
            public const int Command_Clear = 5 * Milliseconds_In_Second + 2 * Milliseconds_In_Second;

            public const int Communications_Flush_Timeout = 15 * Milliseconds_In_Second;

            /// <summary>
            /// Configured entry delay
            /// </summary>
            public const int Entry_Delay = 30 * Milliseconds_In_Second;

            /// <summary>
            /// Maximum time allowed for an equipment download to complete
            /// </summary>
            public const int Equipment_Download_Timeout = 30 * Milliseconds_In_Second;

            public const int Keyfob_Repeat_Button_Timeout = 4000;
            public const int Milliseconds_In_Hour = Milliseconds_In_Minute * 60;
            public const int Milliseconds_In_Minute = Milliseconds_In_Second * 60;
            public const int Milliseconds_In_Second = 1000;

            /// <summary>
            /// Configured output relay activation time
            /// </summary>
            public const int Output_Activation_Time = 1 * Milliseconds_In_Second + 250;

            /// <summary>
            /// Exit delay programmed into panel (shouldn't be a staticant)
            /// </summary>
            public const int Exit_Delay = 45 * Milliseconds_In_Second;

            // configured output time plus a 250 ms buffer
            /// <summary>
            /// Maximum time allowed for a data refresh to complete
            /// </summary>
            public const int Refresh_Timeout = 20 * Milliseconds_In_Second;

            /// <summary>
            /// Specifies how long silent arming is effective
            /// </summary>
            public const int Silent_Arming_Period = Exit_Delay;
        }
        #endregion

        #region Watchdog

        private void minuteTimerElapsed(object state)
        {
            // data watchdog
            try
            {
                if (CheckImageValidity() == false)
                {
                    if (SystemTick > tickDownloadTimeout)
                    {
                        ResetPanelMemory(true);
                    }
                }

                minuteTimer.Change(new TimeSpan(0, 0, 0, 59 - DateTime.Now.Second, 1000 - DateTime.Now.Millisecond), new TimeSpan(0, 1, 0));
            }
            catch (Exception ex)
            {
                log.LogError(ex, "An exception occured: " + ex.Message);
            }
        }
        #endregion
    }
}
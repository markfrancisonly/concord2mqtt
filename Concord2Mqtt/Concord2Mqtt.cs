using Automation.Concord;
using Automation.Concord.Panel;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.ManagedClient;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Automation.Concord2Mqtt
{
    public class Concord2Mqtt
    {
        private const string timestampLocalFormat = "yyyy-MM-dd HH:mm:ss";
        private const string timestampUtcFormat = "yyyy-MM-ddTHH:mm:ssZ";
        private static IManagedMqttClient mqtt = null;
        private AutomationDeviceServer concord;
        private Concord2MqttConfiguration config;
        private ICommunicationDevice device;
        private bool initialized = false;
        private ILogger log;
        private Timer minuteTimer;

        public Concord2Mqtt(ILogger logger, Concord2MqttConfiguration settings, Concord.ConcordConfiguration concordSettings)
        {
            this.log = logger;
            this.config = settings;

            switch (concordSettings.Connection)
            {
                case ConcordConfiguration.ConnectionMethod.SerialPort:
                    device = new SerialCommunicationDevice(concordSettings.SerialPort);
                    break;
                case ConcordConfiguration.ConnectionMethod.TcpClient:
                    device = new TcpClientCommunicationDevice(concordSettings.TcpAddress, concordSettings.TcpPort);
                    break;
                case ConcordConfiguration.ConnectionMethod.TcpServer:
                    device = new TcpServerCommunicationDevice(log, concordSettings.TcpAddress, concordSettings.TcpPort);
                    break;
            }
            
            concord = new AutomationDeviceServer(concordSettings, log, device);
        }

        public void Start()
        {
            if (minuteTimer != null) return;

            concord.AlarmChange += OnAlarmChange;
            concord.DataRefreshed += OnDataRefreshed;
            concord.DisplayTextChange += OnDisplayTextChange;
            concord.KeyfobButtonChange += OnKeyfobButtonChange;
            concord.PartitionArmingLevelChange += OnPartitionArmingLevelChange;
            concord.ZoneChange += OnZoneChange;
            concord.StartUp();

            MqttConnect(config.MQTT);

            minuteTimer = new System.Threading.Timer(new System.Threading.TimerCallback(minuteTimerElapsed), null, new TimeSpan(0, 0, 0, 59 - DateTime.Now.Second, 1000 - DateTime.Now.Millisecond), new TimeSpan(0, 1, 0));
        }

        public void Stop()
        {
            if (concord == null) return;

            concord.ShutDown();
            concord.DisplayTextChange -= OnDisplayTextChange;
            concord.KeyfobButtonChange -= OnKeyfobButtonChange;
            concord.ZoneChange -= OnZoneChange;
            concord.AlarmChange -= OnAlarmChange;
            concord.PartitionArmingLevelChange -= OnPartitionArmingLevelChange;
            concord.DataRefreshed -= OnDataRefreshed;

            if (minuteTimer != null)
            {
                minuteTimer.Change(Timeout.Infinite, Timeout.Infinite);
                minuteTimer = null;
            }
        }

        private string DeriveDeviceClass(string text)
        {
            string lowerText = text.ToLowerInvariant();
            if (lowerText.Contains("window"))
            {
                return "window";
            }
            else if (lowerText.Contains("garage door"))
            {
                return "garage_door";
            }
            else if (lowerText.Contains("door"))
            {
                return "door";
            }
            else if (lowerText.Contains("motion"))
            {
                return "motion";
            }
            else if (lowerText.Contains("smoke"))
            {
                return "smoke";
            }
            else            {
                return null;
            }
        }

        private string DeriveNameByDeviceClass(string text, string device_class)
        {
            if (device_class != null && text.EndsWith(device_class, StringComparison.InvariantCultureIgnoreCase))
            {
                string device_class_title = device_class[0].ToString().ToUpperInvariant() + device_class.Substring(1);
                return text.Replace(device_class_title, device_class);
            }
            return text;
        }

        private void ExecuteCommand(int partitionId, string command, string source)
        {
            if (string.IsNullOrWhiteSpace(command)) return;

            try
            {
                string lowerCaseCommand = command.Trim().Replace("  ", " ").ToLowerInvariant();

                // tokenize string on whitespace and keep quoted text together
                string[] tokens = System.Text.RegularExpressions.Regex.Split(lowerCaseCommand, "(?<=^[^\"]*(?:\"[^\"]*\"[^\"]*)*) (?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");

                log.LogInformation("Mqtt command '{0}' received for partition {1}{2}", lowerCaseCommand, partitionId, string.IsNullOrWhiteSpace(source) ? "" : " from " + source);

                switch (tokens[0])
                {
                    case Concord2MqttCommand.Arm:
                        ArmingLevel level = (ArmingLevel)Enum.Parse(typeof(ArmingLevel), tokens[1], true);
                        concord.Arm(source, partitionId, level, lowerCaseCommand.Contains("silent"), lowerCaseCommand.Contains("no delay"), false);
                        break;

                    case Concord2MqttCommand.Arm_Away:
                        concord.Arm(source, partitionId, ArmingLevel.Away, false, true, false);
                        break;

                    case Concord2MqttCommand.Arm_Night:
                        concord.Arm(source, partitionId, ArmingLevel.Stay, false, true, false);
                        break;

                    case Concord2MqttCommand.Arm_Home:
                        concord.Arm(source, partitionId, ArmingLevel.Stay, false, false, false);
                        break;

                    case Concord2MqttCommand.Arm_Custom_Bypass:
                        break;

                    case Concord2MqttCommand.AutoArm:
                        ArmingLevel autoArmingLevel = (ArmingLevel)Enum.Parse(typeof(ArmingLevel), tokens[1], true);
                        concord.AutoArmHome(partitionId, autoArmingLevel, lowerCaseCommand.Contains("no delay"));
                        break;

                    case Concord2MqttCommand.Bypass:
                        if (tokens.Length < 3)
                        {
                            concord.BypassZone(partitionId, int.Parse(tokens[1]));
                        }
                        else if (tokens[2] == "toggle")
                        {
                            concord.ToggleZoneBypass(partitionId, int.Parse(tokens[1]));
                        }
                        break;

                    case Concord2MqttCommand.Disarm:
                        if (config.DirectDisarm)
                            concord.Arm(source, partitionId, ArmingLevel.Disarmed, false, true, false);
                        else
                            concord.SendKeys(partitionId, TouchpadKey.Key1);
                        break;

                    case Concord2MqttCommand.History:
                        concord.ViewHistory(partitionId);
                        break;

                    case Concord2MqttCommand.Key:
                        concord.SendKeyString(partitionId, tokens[1]);
                        break;

                    case Concord2MqttCommand.Keypress:
                        concord.SendKeys(partitionId, (TouchpadKey)int.Parse(tokens[1]));
                        break;

                    case Concord2MqttCommand.Light:

                        int light;

                        if (tokens.Length > 2)
                        {
                            lowerCaseCommand = lowerCaseCommand.Substring(Concord2MqttCommand.Light.Length + 1).Trim('\"');
                            if (lowerCaseCommand.EndsWith(" on"))
                            {
                                light = int.Parse(lowerCaseCommand.Substring(0, lowerCaseCommand.Length - " on".Length));

                                concord.SetPanelLight(partitionId, light, true);
                            }
                            else if (lowerCaseCommand.EndsWith(" off"))
                            {
                                light = int.Parse(lowerCaseCommand.Substring(0, lowerCaseCommand.Length - " off".Length));

                                concord.SetPanelLight(partitionId, light, false);
                            }
                        }
                        else
                        {
                            light = int.Parse(lowerCaseCommand.Substring(Concord2MqttCommand.Light.Length + 1).Trim('\"'));
                            concord.TogglePanelLight(partitionId, light);
                        }
                        break;

                    case Concord2MqttCommand.Lights:

                        if (tokens.Length > 1)
                        {
                            switch (tokens[1])
                            {
                                case "on":
                                    concord.SetPanelAllLights(partitionId, true);
                                    break;

                                case "off":
                                    concord.SetPanelAllLights(partitionId, false);
                                    break;

                                case "toggle":
                                    concord.TogglePanelAllLights(partitionId);
                                    break;
                            }
                        }
                        break;

                    case Concord2MqttCommand.Output:
                        concord.ActivateOutput(partitionId, int.Parse(tokens[1]));
                        break;

                    case Concord2MqttCommand.Refresh:
                        concord.DynamicDataRefresh();
                        break;

                    case Concord2MqttCommand.Reset:
                        concord.ResetPanelMemory(true);
                        break;

                    case Concord2MqttCommand.Unbypass:
                        concord.UnbypassZone(partitionId, int.Parse(tokens[1]));
                        break;

                    default:
                        log.LogInformation("Unrecognized command: " + command);
                        break;
                }
            }
            catch (Exception ex)
            {
                // command failed
                log.LogError(ex, "Invalid command syntax: " + ex.Message);
            }
        }

        private void HandleApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                try
                {
                    string topic = e.ApplicationMessage.Topic;

                    if (string.IsNullOrWhiteSpace(topic) == false)
                    {
                        string payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

                        log.LogInformation($"Topic: {topic}. Message Received: {payload}");

                        if (topic.StartsWith("homeassistant/alarm_control_panel/panel_partition_"))
                        {
                            int partitionId = int.Parse(topic.Substring("homeassistant/alarm_control_panel/panel_partition_".Length, 1));

                            if (topic.EndsWith("command"))
                            {
                                ExecuteCommand(partitionId, payload, null);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.LogError(ex, ex.Message);
                }
            });
        }

        private async void MqttConnect(Concord2MqttConfiguration.MqttSettings config)
        {
            var messageBuilder = new MqttClientOptionsBuilder()
               .WithClientId(config.ClientId + Guid.NewGuid().ToString())
               .WithCredentials(config.Username, config.Password)
               .WithTcpServer(config.Host, config.Port)
               //.WithWebSocketServer(config.WebSocketUri)
               .WithCleanSession();

            var options = config.Secure ? messageBuilder.WithTls().Build() : messageBuilder.Build();

            var managedOptions = new ManagedMqttClientOptionsBuilder()
              .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
              .WithClientOptions(options)
              .Build();

            mqtt = new MqttFactory().CreateManagedMqttClient();

            await mqtt.StartAsync(managedOptions);

            mqtt.UseConnectedHandler(e =>
            {
                log.LogInformation("MQTT client connected to {host}:{port}", config.Host, config.Port);
            });

            mqtt.UseDisconnectedHandler(e =>
            {
                log.LogInformation("MQTT client disconnected");
            });

            mqtt.UseApplicationMessageReceivedHandler(e => HandleApplicationMessageReceivedAsync(e));

            await SubscribeAsync("homeassistant/alarm_control_panel/panel_partition_1/command");
            await SubscribeAsync("homeassistant/alarm_control_panel/panel_partition_2/command");
            await SubscribeAsync("homeassistant/alarm_control_panel/panel_partition_3/command");
            await SubscribeAsync("homeassistant/alarm_control_panel/panel_partition_4/command");
            await SubscribeAsync("homeassistant/alarm_control_panel/panel_partition_5/command");
            await SubscribeAsync("homeassistant/alarm_control_panel/panel_partition_6/command");


        }

        /// <summary>
        /// Subscribe topic.
        /// </summary>
        /// <param name="topic">Topic.</param>
        /// <param name="qos">Quality of Service.</param>
        /// <returns>Task.</returns>
        private static async Task SubscribeAsync(string topic, int qos = 1) =>
          await mqtt.SubscribeAsync(new MqttTopicFilterBuilder()
            .WithTopic(topic)
            .WithQualityOfServiceLevel((MQTTnet.Protocol.MqttQualityOfServiceLevel)qos)
            .Build());

        private void OnAlarmChange(AutomationDeviceServer sender, Panel panel, Partition partition)
        {
            try
            {
                PublishPartition(partition, false);
                PublishPanel(panel, false);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "An exception occured: " + ex.Message);
            }
        }

        private void OnDataRefreshed(AutomationDeviceServer sender)
        {
            try
            {
                PublishAll(true);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "An exception occured: " + ex.Message);
            }
        }

        private void OnDisplayTextChange(AutomationDeviceServer sender, Partition partition)
        {
            try
            {
                PublishPartition(partition, false);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "An exception occured: " + ex.Message);
            }
        }

        private void OnKeyfobButtonChange(AutomationDeviceServer sender, ArmingLevel? armingLevelWhenPressed, Zone keyfob, KeyfobButton button, int presses)
        {
            try
            {
                PublishKeyfobButtonPress(armingLevelWhenPressed, keyfob, button, presses, true);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "An exception occured: " + ex.Message);
            }
        }

        private void OnPartitionArmingLevelChange(AutomationDeviceServer sender, Partition partition)
        {
            try
            {
                PublishPartition(partition, false);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "An exception occured: " + ex.Message);
            }
        }

        private void OnZoneChange(AutomationDeviceServer sender, Zone zone)
        {
            try
            {
                PublishZone(zone, false);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "An exception occured: " + ex.Message);
            }
        }

        private async Task<bool> PublishAlarmControlPanel(string partition_unique_id, string state, string name, bool discovery)
        {
            string domain = "alarm_control_panel";
            string unique_id = partition_unique_id + "_alarm_control_panel";

            if (discovery && config.Discovery)
            {
                string configJson;

                var config = new
                {
                    name,
                    uniq_id = unique_id,
                    cmd_t = string.Format("homeassistant/{0}/{1}/command", domain, partition_unique_id),
                    stat_t = string.Format("homeassistant/{0}/{1}/state", domain, partition_unique_id)
                };

                configJson = JsonConvert.SerializeObject(config);

                var configMessage = new MqttApplicationMessageBuilder()

                  .WithTopic(string.Format("homeassistant/{0}/{1}/config", domain, partition_unique_id))
                  .WithPayload(configJson)
                  .WithAtLeastOnceQoS()
                  .WithRetainFlag()
                  .Build();

                await mqtt.PublishAsync(configMessage, CancellationToken.None);
            }

            var stateMessage = new MqttApplicationMessageBuilder()

                .WithTopic(string.Format("homeassistant/{0}/{1}/state", domain, partition_unique_id))
                .WithPayload(state)
                .WithAtLeastOnceQoS()
                .WithRetainFlag()
                .Build();
            await mqtt.PublishAsync(stateMessage, CancellationToken.None);

            return true;
        }

        private void PublishAll(bool discovery)
        {
            try
            {
                PublishPanel(concord.Panel, discovery);

                foreach (Partition partition in concord.Panel.Partitions.Values)
                {
                    PublishPartition(partition, discovery);

                    foreach (Light light in partition.Lights.Values)
                    {
                        // todo: pub light
                    }
                }

                foreach (Zone zone in concord.Panel.Zones.Values)
                {
                    PublishZone(zone, true);
                }

                foreach (Output output in concord.Panel.Outputs.Values)
                {
                    // todo: pub output
                }

                foreach (Device device in concord.Panel.Devices.Values)
                {
                    // todo: pub device
                }

                PublishHearbeat(concord.IsPanelConnected, discovery);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "An exception occured: " + ex.Message);
            }

            initialized = true;
        }

        private async Task<bool> PublishGeneric(string unique_id, string domain, string device_class, string state, string name, string attributesJson, bool discovery)
        {
            if (discovery && config.Discovery)
            {
                string configJson;
                if (device_class != null)
                {
                    var config = new
                    {
                        name,
                        uniq_id = unique_id,
                        exp_aft = 70,
                        dev_cla = device_class,
                        stat_t = string.Format("homeassistant/{0}/{1}/state", domain, unique_id),
                        json_attr_t = string.Format("homeassistant/{0}/{1}/attributes", domain, unique_id)
                    };

                    configJson = JsonConvert.SerializeObject(config);
                }
                else
                {
                    var config = new
                    {
                        name,
                        uniq_id = unique_id,
                        exp_aft = 70,
                        stat_t = string.Format("homeassistant/{0}/{1}/state", domain, unique_id),
                        json_attr_t = string.Format("homeassistant/{0}/{1}/attributes", domain, unique_id)
                    };

                    configJson = JsonConvert.SerializeObject(config);
                }

                var configMessage = new MqttApplicationMessageBuilder()

                  .WithTopic(string.Format("homeassistant/{0}/{1}/config", domain, unique_id))
                  .WithPayload(configJson)
                  .WithAtLeastOnceQoS()
                  .WithRetainFlag()
                  .Build();

                await mqtt.PublishAsync(configMessage, CancellationToken.None);
            }

            var stateMessage = new MqttApplicationMessageBuilder()

                .WithTopic(string.Format("homeassistant/{0}/{1}/state", domain, unique_id))
                .WithPayload(state)
                .WithAtLeastOnceQoS()
                .WithRetainFlag()
                .Build();
            await mqtt.PublishAsync(stateMessage, CancellationToken.None);

            var attributesMessage = new MqttApplicationMessageBuilder()

                .WithTopic(string.Format("homeassistant/{0}/{1}/attributes", domain, unique_id))
                .WithPayload(attributesJson)
                .WithAtLeastOnceQoS()
                .WithRetainFlag()
                .Build();
            await mqtt.PublishAsync(attributesMessage, CancellationToken.None);

            return true;
        }

        private void PublishHearbeat(bool online, bool discovery)
        {
            var attributes = new
            {
                version = typeof(AutomationDeviceServer).Assembly.GetName().Version.ToString(),
                online,
                build_datetime_local = (new FileInfo(Assembly.GetExecutingAssembly().Location).CreationTime).ToString(timestampLocalFormat), // works for docker build?
                mqtt_client = config.MQTT.ClientId
                //timestamp_local = DateTime.Now.ToString(timestampLocalFormat)
            };

            string unique_id = "panel_heartbeat";
            string state = DateTimeOffset.UtcNow.ToString(timestampUtcFormat);
            string device_class = "timestamp";
            string integration = "sensor";
            string name = "Panel Heartbeat";
            string attributesJson = JsonConvert.SerializeObject(attributes);

            _ = PublishGeneric(unique_id, integration, device_class, state, name, attributesJson, discovery);
        }

        private void PublishKeyfobButtonPress(ArmingLevel? armingLevelWhenPressed, Concord.Panel.Zone keyfob, KeyfobButton button, int presses, bool discovery)
        {
            if (keyfob == null || string.IsNullOrWhiteSpace(keyfob.Text)) return;

            var attributes = new
            {
                zone = keyfob.Id,
                button = button.ToString(),
                presses,
                arming_level_when_pressed = armingLevelWhenPressed
                //timestamp_local = DateTime.Now.ToString(timestampLocalFormat)
            };

            string unique_id = string.Format("panel_zone_{0}_keyfob", keyfob.Id);
            string state = "ON";
            string device_class = null;
            string integration = "binar_sensor";
            string name = keyfob.Text;
            string attributesJson = JsonConvert.SerializeObject(attributes);

            _ = PublishGeneric(unique_id, integration, device_class, state, name, attributesJson, discovery);

            // immediately turn it off
            state = "OFF";
            _ = PublishGeneric(unique_id, integration, device_class, state, name, attributesJson, false);
        }

        private void PublishPanel(Panel panel, bool discovery)
        {
            if (panel == null || panel.InAlarm == null || string.IsNullOrWhiteSpace(panel.SerialNumber)) return;

            var attributes = new
            {
                internal_date_time = panel.DateTime,
                serial_number = panel.SerialNumber,
                software_revision = panel.SoftwareRevision,
                panel_type_id = panel.PanelTypeId,
                hardware_revision = panel.HardwareRevision,
                alarm = panel.InAlarm,
                alarm_pending = panel.IsAlarmPending
                //timestamp_local = DateTime.Now.ToString(timestampLocalFormat)
            };

            string unique_id = string.Format("panel_{0}", panel.SerialNumber);
            string state = panel.InAlarm == true ? "ON" : "OFF";
            string device_class = "safety";
            string integration = "binary_sensor";
            string name = "Panel " + panel.PanelType.ToString().Replace("_", " ");
            string attributesJson = JsonConvert.SerializeObject(attributes);

            _ = PublishGeneric(unique_id, integration, device_class, state, name, attributesJson, discovery);
        }

        private void PublishPartition(Partition partition, bool discovery)
        {
            if (partition == null || partition.ArmingLevel == null || string.IsNullOrEmpty(partition.Name) || string.IsNullOrEmpty(partition.DisplayText)) return;

            var attributes = new
            {
                partition = partition.Id,
                arming_level = partition.ArmingLevel.ToString(),
                alarm = partition.InAlarm == null ? null : partition.InAlarm.Value ? "on" : "off",
                alarm_type = partition.AlarmType,
                alarm_pending = partition.IsAlarmPending,
                nodelay = partition.NoDelay,
                silent = partition.SilentArming,
                last_arming_autonomous = partition.LastArming == null || partition.LastArming.Autonomous == null ? null : partition.LastArming.Autonomous.Value.ToString(),
                last_arming_level = partition.LastArming == null || partition.LastArming.ArmingLevel == null ? null : partition.LastArming.ArmingLevel.ToString(),
                last_arming_keyfob = partition.LastArming == null ? null : partition.LastArming.Keyfob,
                last_arming_user_name = partition.LastArming == null ? null : partition.LastArming.Name,
                last_arming_user_id = partition.LastArming == null ? null : partition.LastArming.UserId,
                last_arming_user_class = partition.LastArming == null ? null : partition.LastArming.UserClass,
                last_arming_timestamp = partition.LastArming == null || partition.LastArming.Timestamp == null ? null : partition.LastArming.Timestamp.Value.ToString(timestampUtcFormat),
                timestamp = DateTime.Now.ToString(timestampLocalFormat)
            };

            string entity_id = string.Format("panel_partition_{0}", partition.Id);
            string device_class = null;
            string integration = "sensor";
            string state = partition.DisplayText.Replace("\r", " ");
            string name = "Panel " + partition.Name;
            string attributesJson = JsonConvert.SerializeObject(attributes);

            _ = PublishGeneric(entity_id, integration, device_class, state, name, attributesJson, discovery);

            state = null;
            if (partition.InAlarm == true)
            {
                state = "triggered";
            }
            else if (partition.IsAlarmPending == true)
            {
                state = "pending";
            }
            else if (partition.IsArmingPending == true)
            {
                state = "arming";
            }
            else
            {
                switch (partition.ArmingLevel.Value)
                {
                    case ArmingLevel.Disarmed:
                        state = "disarmed";
                        break;

                    case ArmingLevel.Away:
                        state = "armed_away";
                        break;

                    case ArmingLevel.Stay:
                        if (partition.NoDelay == true)
                            state = "armed_night";
                        else
                            state = "armed_home";
                        break;
                }
            }

            _ = PublishAlarmControlPanel(entity_id, state, name, discovery);
        }

        private void PublishZone(Zone zone, bool discovery)
        {
            if (zone == null || zone.State == null || zone.Group == null || zone.Type == null || string.IsNullOrWhiteSpace(zone.Text)) return;

            var attributes = new
            {
                zone = zone.Id,
                opened = (zone.State & ZoneState.Opened) == ZoneState.Opened ? "on" : "off",
                bypassed = (zone.State & ZoneState.Bypassed) == ZoneState.Bypassed ? "on" : "off",
                alarm = (zone.State & ZoneState.Alarm) == ZoneState.Alarm ? "on" : "off",
                faulted = (zone.State & ZoneState.Faulted) == ZoneState.Faulted ? "on" : "off",
                trouble = (zone.State & ZoneState.Trouble) == ZoneState.Trouble ? "on" : "off",
                partition = zone.Partition,
                configured_type = zone.Type.ToString(),
                configured_group = zone.Group.ToString(),
                configured_motion = zone.IsMotion,
                configured_perimeter = zone.IsPerimeter,
                configured_interior = zone.IsInterior,
                configured_specialized = zone.IsSpecialized,
                zone_state = zone.State.ToString(),
                timestamp = DateTime.Now.ToString(timestampLocalFormat)
            };

            string unique_id = string.Format("panel_zone_{0}", zone.Id);
            string state = zone.State == ZoneState.Normal ? "OFF" : "ON";
            string device_class = DeriveDeviceClass(zone.Text);
            string integration = "binary_sensor";
            string name = DeriveNameByDeviceClass(zone.Text, device_class);
            string attributesJson = JsonConvert.SerializeObject(attributes);

            _ = PublishGeneric(unique_id, integration, device_class, state, name, attributesJson, discovery);
        }

        private void minuteTimerElapsed(object state)
        {
            if (!initialized) return;

            try
            {
                try
                {
                    PublishHearbeat(concord.IsPanelConnected, false);
                    if (concord.IsPanelConnected == true)
                    {
                        PublishAll(true);
                    }
                }
                catch (Exception ex)
                {
                    log.LogError(ex, "An exception occured: " + ex.Message);
                }

                minuteTimer.Change(new TimeSpan(0, 0, 0, 59 - DateTime.Now.Second, 1000 - DateTime.Now.Millisecond), new TimeSpan(0, 1, 0));
            }
            catch
            {
            }
        }

    }
}
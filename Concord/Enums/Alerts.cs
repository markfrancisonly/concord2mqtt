using System;

namespace Automation.Concord
{
    /// <summary>
    /// General alert type codes
    /// </summary>
   
    public enum AlertClass
    {
        None = 0,
        Alarm = 1,
        AlarmCancel = 2,
        AlarmRestoral = 3,
        FireTrouble = 4,
        FireTroubleRestoral = 5,
        NonFireTrouble = 6,
        NonFireTroubleRestoral = 7,
        Bypass = 8,
        Unbypass = 9,
        Opening = 10,
        Closing = 11,
        //AdventPartitionConfigurationChange = 12,
        PartitionEvent = 13,
        PartitionTest = 14,
        SystemTrouble = 15,
        SystemTroubleRestoral = 16,
        SystemConfigurationChange = 17,
        SystemEvent = 18
    }

    /// <summary>
    /// Used for Alarm (General Type = 1), Alarm Cancel (General Type = 2), and Alarm Restoral (General Type = 3)
    /// </summary>
   
    public enum AlarmType
    {
        Unspecified = 0,
        Fire = 1,
        FirePanic = 2,
        Police = 3,
        PolicePanic = 4,
        //AdventMedical              =5,
        //AdventMedicalPanic         =6,
        Auxiliary = 7,
        AuxiliaryPanic = 8,
        //AdventTamper               =9,
        NoActivity = 10,
        //AdventSuspicion            =11,
        NotUsed = 12,
        LowTemperature = 13,
        //AdventHighTemperature      =14,
        KeystrokeViolation = 15,
        Duress = 16,
        ExitFault = 17,
        //AdventExplosiveGas         =18,
        CarbonMonoxide = 19,
        //AdventEnvironmental        =20,
        Latchkey = 21,
        //AdventEquipmentTamper      =22,
        //AdventHoldup               =23,
        //AdventSprinkler            =24,
        //AdventHeat                 =25,
        SirenTamper = 26,
        //AdventSmoke                =27,
        //AdventRepeaterTamper       =28,
        //AdventFirePumpActivated    =29,
        //AdventFirePumpFailure      =30,
        //AdventFireGateValve        =31,
        //AdventLowCO2Pressure       =32,
        //AdventLowLiquidPressure    =33,
        //AdventLowLiquidLevel       =34,
        EntryExit = 35,
        Perimeter = 36,
        Interior = 37,
        Near = 38,
        Water = 39
    }

    /// <summary>
    /// Used for Fire Trouble (General Type = 4), Fire Trouble Restoral (General Type = 5), 
    /// Non-Fire Trouble (General Type = 6), and Non-Fire Trouble (General Type = 6)
    /// </summary>
   
    public enum TroubleType
    {
        Unspecified = 0,
        Hardwire = 1,
        //AdventGroundFault                 =2,
        //AdventDevice                      =3,
        Supervisory = 4,
        LowBattery = 5,
        Tamper = 6,
        //AdventSAM                         =7,
        PartialObscurity = 8,
        //AdventJam                         =9,
        //AdventZoneACFail                  =10,
        nu = 11	 //  n/u??
        //AdventNACTrouble                  =12,
        //AdventAnalogZoneTrouble           =13,
        //AdventFireSupervisory             =14,
        //AdventPumpFail                    =15,
        //AdventFireGateValveClosed         =16,
        //AdventCO2PressureTrouble          =17,
        //AdventLiquidPressureTrouble       =18,
        //AdventLiquidLevelTrouble          =19	  
    }

    /// <summary>
    /// Used for Bypass (General Type = 8) and Unbypass (General Type = 9)
    /// </summary>
   
    public enum BypassType
    {
        Direct = 0,
        Indirect = 1,
        Swinger = 2,
        //AdventInhibit	        =3		//Inhibit	ES = user number
    }

    /// <summary>
    /// Used for Opening (General Type = 10) and Closing (General Type = 11)
    /// </summary>
   
    public enum OpeningClosingType
    {
        //AdventNormal                  =0,
        Early = 1,
        Late = 2,
        Fail = 3,
        //AdventException	            =4,
        //AdventExtension	            =5,
        //AdventUsingKeyfobOrKeyswitch  =6,
        //AdventScheduled               =7,
        //AdventRemote	                =8,
        Recent = 9	//Recent Close (Concord only)	ES = user number
    }

    //public enum PartitionConfigurationChange 
    //{
    //    //(General Type = 12)
    //    AdventUserAccessCodeAdded = 0,
    //    AdventUserAccessCodeDeleted = 1,
    //    AdventUserAccessCodeChanged = 2,
    //    AdventUserAccessCodeExpired = 3,
    //    AdventUserCodeAuthorityChanged = 4,
    //    AdventAuthorityLevelsChanged = 5,
    //    AdventScheduleChanged = 6,
    //    AdventArmingorOCScheduleChanged = 7,
    //    AdventZoneAdded = 8,
    //    AdventZoneDeleted = 9	 //Adventone Deleted
    //}

    /// <summary>
    /// Used for Partition Event (General Type = 13)
    /// </summary>
   
    public enum PartitionEventType
    {
        ScheduleOn = 0,
        ScheduleOff = 1,
        LatchkeyOn = 2,
        LatchkeyOff = 3,
        SmokeDetectorsReset = 4,
        //AdventValidUserAccessCodeEntered = 5,
        //AdventArmingLevelChanged = 6,
        //AdventAlarmReported = 7,
        //AdventAgentRelease = 8,
        //AdventAgentReleaseRestoral = 9,
        PartitionRemoteAccess = 10,
        //AdventKeystrokeViolationinPartition   =11,
        ManualForceArm = 12,
        AutoForceArm = 13,
        //AdventAutoForceArmFailed              =14,
        ArmingProtestBegun = 15,
        ArmingProtestEnded = 16
    }

    /// <summary>
    /// Use for Partition Test (General Type = 14)
    /// </summary>
   
    public enum PartitionTestType
    {
        ManualPhoneTest = 0,
        AutoPhoneTest = 1,
        //AdventAutoPhoneTestwithexistingtrouble    =2,
        PhoneTestOK = 3,
        PhoneTestFailed = 4,
        UserSensorTestStarted = 5,
        UserSensorTestEnded = 6,
        //AdventUserSensorTestCompleted	            =7,
        //AdventUserSensorTestIncomplete            =8,
        //AdventuserSensorTestTrip                  =9,
        InstallerSensorTestStarted = 10,
        InstallerSensorTestEnded = 11
        //AdventInstallerSensorTestCompleted        =12,
        //AdventInstallerSensorTestIncomplete       =13,
        //AdventInstallerSensorTestTrip             =14,
        //AdventFireDrillStarted                    =15	
    }

    /// <summary>
    /// Used for System Trouble (General Type = 15), and System Restoral General Type = 16)
    /// </summary>
   
    public enum SystemTroubleType
    {
        BusReceiverFailure = 0,
        //AdventBusAntennaTamper                    =1,
        MainLowBattery = 2,
        //AdventSnapCardLowBattery                  =3,
        //AdventModuleLowBattery                    =4,
        MainACFailure = 5,
        //AdventSnapCardACFailure                   =6,
        //AdventModuleACFailure                     =7,
        //AdventAuxPowerFailure                     =8 ,
        BusShutdown = 9,
        BusLowPowerMode = 10,
        PhoneLine1Failure = 11,
        //AdventPhoneLine2Failure                   =12,
        //AdventRemotePhoneTamper                   =13,
        WatchdogReset = 14,
        //AdventRAMFailure                          =15,
        //AdventFlashFailure                        =16,
        //AdventPrinterError                        =17,
        HistoryBufferAlmostFull = 18,
        HistoryBufferOverflow = 19,
        ReportBufferOverflow = 20,
        BusDeviceFailure = 21,
        FailureToCommunicate = 22
        //AdventLongRangeRadioTrouble               =23,
        //AdventModuleTamperTrouble                 =24,
        //AdventUnenrolledModuleTrouble             =25,
        //AdventAudioOutputTrouble                  =26,
        //AdventAnalogModuleTrouble                 =27,
        //AdventCellModuleTrouble                   =28,
        //AdventBuddy1Failure                       =29,
        //AdventBuddy2Failure                       =30,
        //AdventBuddy3Failure                       =31,
        //AdventBuddy4Failure                       =32,
        //AdventSnapCardTrouble                     =33,
        //AdventAnalogLoopShort                     =34,
        //AdventAnalogLoopBreak                     =35,
        //AdventAnalogAddress0                      =36,
        //AdventUnenrolledAnalogHead                =35,
        //AdventDuplicateAnalogHead                 =38,
        //AdventAnalogModuleInitializing            =39,
        //AdventMicrophoneSwitchTrouble             =40,
        //AdventMicrophoneTrouble                   =41,
        //AdventMicrophoneWiringTrouble             =42,
        //AdventJTECHPremisePagingTrouble           =43,
        //AdventVoiceSirenTamperTrouble             =44,
        //AdventMicroburstTransmitFailure           =45,
        //AdventMicroburstTransmitDisabled          =46,
        //AdventMicroburstModuleFailure             =47,
        //AdventMicroburstNotInService              =48,
        //AdventAutomationSupervisoryTrouble        =49,
        //AdventMicroburstModuleInitializing        =50,
        //AdventPrinterPaperOutTrouble              =51	 
    }

    /// <summary>
    /// Used for System Configuration Change (General Type = 17)
    /// </summary>
   
    public enum SystemConfigurationChangeType
    {
        ProgramModeEntry = 0,
        //AdventProgramModeExitWithoutChange            =1,
        ProgramModeExitWithChange = 2,
        DownloaderSessionStart = 3,
        //AdventDownloaderSessionEndWithoutChange       =4,
        DownloaderSessionEndWithChange = 5,
        DownloaderError = 6,
        DownloaderConnectionDenied = 7,
        DateTimeChanged = 8,
        //AdventModuleAdded                             =9,
        //AdventModuleDeleted                           =10,
        //AdventSpeechTokensChanged                     =11,
        //AdventCodeChanged                             =12,
        //AdventPanelFirstService                       =13,
        PanelBackInService = 14
        //AdventInstallerCodeChanged                    =15	
    }

    /// <summary>
    /// Used for System Event (General Type = 18)
    /// </summary>
   
    public enum SystemEventType
    {
        CallbackRequested = 0,
        //OutputActivity                        =1,
        //AdventBuddyReception                        =2,
        //AdventBuddyTransmissionRequest              =3,
        //AdventHistoryBufferCleared                  =4,
        OutputOn = 5,
        OutputOff = 6
    }


}

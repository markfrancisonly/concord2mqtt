using System;

namespace Automation.Concord
{
    public enum Setting
    {
        AccountNo,
        AlarmVerify,
        AuxiliaryPanic,
        ClosingReports,
        DuressCode,
        DuressOption,
        EntryDelay,
        ExitDelay,
        ExitExtension,
        ExtendedDelay,
        FirePanic,
        ForceArmed,
        FreezeAlarm,
        FreezeTemp,
        KeyfobArming,
        KeyswitchSensor,
        KeyswitchStyleTransition,
        LatchkeyFormat,
        LocalPhoneControl,
        NoActivity,
        OpeningReports,
        PhoneAccessKeyCode,
        PhonePanic,
        PolicePanic,
        QuickArm,
        QuickExit,
        RingHangRing,
        SirenTimeout,
        SleepTime,
        StarIsNoDelay,
        SystemTamper,
        TollSaver
    }

    /*
"00{0}0" = Setting.AccountNo
"00{0}1" = Setting.QuickArm
"00{0}2" = Setting.QuickExit
"00{0}3" = Setting.ExitExtension
"00{0}4" = Setting.KeyswitchSensor
"00{0}5" = Setting.KeyswitchStyleTransition
"00{0}6" = Setting.DuressCode
"02{0}0" = Setting.LocalPhoneControl
"02{0}2" = Setting.RingHangRing
"02{0}4" = Setting.TollSaver
"02{0}5" = Setting.PhonePanic
"02{0}6" = Setting.PhoneAccessKeyCode
"03{0}0" = Setting.EntryDelay
"03{0}1" = Setting.ExitDelay
"03{0}2" = Setting.ExtendedDelay
"03{0}3" = Setting.SirenTimeout
"03{0}4" = Setting.SleepTime
"05{0}0" = Setting.FirePanic
"05{0}1" = Setting.AuxiliaryPanic
"05{0}2" = Setting.PolicePanic
"05{0}3" = Setting.KeyfobArming
"05{0}4" = Setting.StarIsNoDelay
"06{0}00" = Setting.OpeningReports
"06{0}01" = Setting.ClosingReports
"06{0}02" = Setting.NoActivity
"06{0}03" = Setting.DuressOption
"06{0}04" = Setting.ForceArmed
"06{0}05" = Setting.LatchkeyFormat
"06{0}06" = Setting.FreezeAlarm
"06{0}07" = Setting.FreezeTemp
"06{0}08" = Setting.AlarmVerify
"06{0}09" = Setting.SystemTamper
    */

}

using System;

namespace Automation.Concord
{
   
    public enum SourceDeviceType
    {
        BusDevice = 0,
        LocalPhone = 1,
        Zone = 2,
        System = 3,

        /// <summary>
        /// Not used in user light message
        /// </summary>
        RemotePhone = 4
    }

}

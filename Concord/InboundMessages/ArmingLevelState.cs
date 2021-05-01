using System;

namespace Automation.Concord.InboundMessages
{
    /// <summary>
    /// This command is sent whenever there is a change in the arming level.  Also, if the 
    /// Automation Module requests a Dynamic Data Refresh Request this command will be sent 
    /// for each partition that is enabled.
    /// </summary>
   
    public class ArmingLevelState : Message
    {
        //Format: 08h 22h 01h [PN] [AN] [UNh] [UNl] [AL] [CS]
        public ArmingLevelState(string message) : base(message)
        { }

        /// <summary>
        /// Partition 1-6
        /// </summary>
        public int Partition
        {
            get
            {
                string token = this[2];
                return ToInt(token);
            }
        }

        /// <summary>
        /// Keyfob 
        /// </summary>
        public bool Keyfob
        {
            get
            {
                string token = this[5];
                int source = ToInt(token);

                if (source > 96)
                    return false;
                else
                {
                    token = this[4];
                    return ToInt(token) == 1 ? true : false;
                }
            }
        }

        public int User
        {
            get
            {
                if (Keyfob) return 0;
                string token = this[5];
                return ToInt(token);
            }
        }

        public UserClass UserClass
        {
            get
            {
                int user = this.User;
                if (Enum.IsDefined(typeof(UserClass), user))
                {
                    return (UserClass)user;
                }
                else
                {
                    //check range
                    if (user >= 0 && user <= (int)UserClass.Regular)
                    {
                        return UserClass.Regular;
                    }
                    else if (user > (int)UserClass.Regular && user <= (int)UserClass.PartitionMaster)
                    {
                        return UserClass.PartitionMaster;
                    }
                    else if (user > (int)UserClass.PartitionMaster && user <= (int)UserClass.PartitionDuress)
                    {
                        return UserClass.PartitionDuress;
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException("Invalid user identifier encountered.");
                    }

                }
            }
        }

        public int Zone
        {
            get
            {
                if (!Keyfob) return 0;
                string token = this[5];
                return ToInt(token);
            }
        }

        public ArmingLevel ArmingLevel
        {
            get
            {
                string token = this[6];
                return (ArmingLevel)ToInt(token);
            }
        }
    }


}

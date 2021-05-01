using System;

namespace Automation.Concord.InboundMessages
{
    /// <summary>
    /// This command is sent for each user in response to an equipment list request.
    /// </summary>
   
    public class EquipmentListUser : Message
    {
        //Format: 04 09h [UNh] [UNl] [text] [CS]
        //	
        // or
        //
        //Format: 07 09h [UNh] [UNl] [UCh] [UCm] [UCl] [text] [CS]
        // if the ACCESS CODE LOCK option is OFF.

        public EquipmentListUser(string message) : base(message)
        { }

        /// <summary>
        /// 0-252
        /// </summary>
        public int User
        {
            get
            {
                string token = this[2];
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

        public string Code
        {
            get
            {
                if (LastIndex == "07")
                {
                    string token = string.Concat(this[4], this[5]);
                    return token;
                }
                else
                {
                    return null;
                }
            }
        }
    }
}

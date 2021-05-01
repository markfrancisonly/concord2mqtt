using System;

namespace Automation.Concord.Panel
{
   
    public class User
    {
        public User()
        {
        }

        public User(int id)
        {
            this.Id = id;
        }

        /// <summary>
        /// Must be loaded from configuration settings since user names are not implemented on Concord 4 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 0-252 but system sends userid 255....
        /// </summary>
        public int Id { get; set; }

        public string Code { get; set; }
    }
}

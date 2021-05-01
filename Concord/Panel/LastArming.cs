using System;

namespace Automation.Concord.Panel
{
    public class LastArming
    {
        public LastArming() { }

        public LastArming(ArmingLevel armingLevel, string arming_user_name, int? arming_user_id, UserClass? arming_user_class, bool? keyfob, bool autoArmed)
        {
            this.ArmingLevel = armingLevel;
            this.Name = arming_user_name;
            this.UserClass = arming_user_class;
            this.UserId = arming_user_id;
            this.Keyfob = keyfob;
            this.Autonomous = autoArmed;
            this.Timestamp = DateTimeOffset.UtcNow;
        }

        public ArmingLevel? ArmingLevel { get; set; }
        public string Name { get; set; }
        public int? UserId { get; set; }
        public UserClass? UserClass { get; set; }
        public bool? Keyfob { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public bool? Autonomous { get; set; }

    }
}

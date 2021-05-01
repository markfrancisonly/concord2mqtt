
using System.Collections.Generic;

namespace Automation.Concord2Mqtt
{

    public class Concord2MqttConfiguration
    {
        public MqttSettings MQTT { get; set; }

        public class MqttSettings
        {
            public string ClientId { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public string Host { get; set; }
            public int Port { get; set; }
            public bool Secure { get; set; }
        }

        public bool Discovery { get; set; }
        public bool DirectDisarm { get; set; }
    }
}
  
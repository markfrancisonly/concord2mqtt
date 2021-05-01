using System.Collections.Generic;

namespace Automation.Concord
{
    public class ConcordConfiguration
    {
        public enum ConnectionMethod : int
        {
            SerialPort=0, TcpServer=1, TcpClient=2
        }

        public int AutomationUserId { get; set; }
        public ConnectionMethod Connection { get; set; }
        public List<PanelPartition> Partitions { get; set; }
        public string SerialPort { get; set; }
        public string TcpAddress { get; set; }
        public int TcpPort { get; set; }
        public List<PanelUser> Users { get; set; }
        public class PanelPartition
        {
            public PanelPartition()
            { }

            public PanelPartition(int id, string name)
            {
                this.Name = name;
                this.Id = id;
            }

            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class PanelUser
        {
            public PanelUser()
            { }

            public PanelUser(int id, string name)
            {
                this.Name = name;
                this.Id = id;
            }

            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
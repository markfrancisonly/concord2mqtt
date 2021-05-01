using System.Threading;

namespace Automation.Concord.Panel
{

    public class Output : IStateChangeWaitHandle
    {
        private AutoResetEvent outputStateSignal = new AutoResetEvent(false);

        /// <summary>
        /// 1-70
        /// </summary>
        public int Id { get; set; }

        WaitHandle IStateChangeWaitHandle.StateChange
        {
            get
            {
                return outputStateSignal;
            }
        }

        private OutputState? outputState;
        public OutputState? OutputState
        {
            get { return outputState; }
            set
            {
                outputState = value;
                outputStateSignal.Set();
            }
        }

        public string Text
        {
            get;
            set;
        }

        public Output(int id)
        {
            this.Id = id;
            this.Text = "Output " + id.ToString();
        }
    }
}

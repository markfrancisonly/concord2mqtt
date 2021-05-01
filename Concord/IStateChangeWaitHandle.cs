using System.Threading;

namespace Automation.Concord
{
    public interface IStateChangeWaitHandle
    {
        WaitHandle StateChange { get; }
    }
}

using TheSwarm.Components.Listener;
using TheSwarm.Common;
using TheSwarm.Interfaces;

namespace TheSwarm.Components.Executors; 

/// <summary>
/// Default task executor class. Used to run one-time task and then expire.
/// </summary>
public class OneShotTaskExecutor : TaskExecutor {
    public OneShotTaskExecutor(ResultsListener resultsListener) : base(resultsListener) {}

    internal override void TaskLoop(TaskSet taskSet) {
        
    }
}

using TheSwarm.Components.Listener;
using TheSwarm.Common;
using TheSwarm.Interfaces;
using TheSwarm.Attributes;

namespace TheSwarm.Components.Executors; 

/// <summary>
/// Default task executor class. Used to run one-time task and then expire.
/// </summary>
public class OneShotTaskExecutor : TaskExecutor {
    public OneShotTaskExecutor(ResultsListener resultsListener) : base(resultsListener) {}

    internal override void TaskLoop(TaskSet taskSet) {
        if (taskSet.Setup is not null)
            taskSet.Setup.Execute();

        foreach(SwarmTask task in taskSet.GetAllTasks()) {
            if (taskSet.BeforeTask is not null)
                taskSet.BeforeTask.Execute();

            task.Execute();

            if (taskSet.AfterTask is not null)
                taskSet.AfterTask.Execute();
        }
            
        if (taskSet.Teardown is not null)
            taskSet.Teardown.Execute();

        Console.WriteLine("Task loop exited");
    }
}

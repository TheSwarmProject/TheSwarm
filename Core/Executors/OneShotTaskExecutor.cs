using TheSwarm.Components.Listener;
using TheSwarm.Extendables;
using TheSwarm.Interfaces;
using TheSwarm.Attributes;

namespace TheSwarm.Components.Executors; 

/// <summary>
/// Default task executor class. Used to run one-time task and then expire.
/// </summary>
public class OneShotTaskExecutor : TaskExecutor {
    public OneShotTaskExecutor(ResultsListener resultsListener) : base(resultsListener) {}

    internal override void TaskLoop(ExecutorThread executor) {
        TaskSet taskSet = executor.TaskSet;

        if (taskSet.Setup is not null)
            if (executor.IsRunning)
                taskSet.Setup.Execute();
            else
                return;

        foreach(SwarmTask task in taskSet.GetAllTasks()) {
            if (taskSet.BeforeTask is not null)
                if (executor.IsRunning)
                    taskSet.BeforeTask.Execute();
                else
                    break;

            if (executor.IsRunning)
                task.Execute();
            else
                break;

            if (taskSet.AfterTask is not null)
                if (executor.IsRunning)
                    taskSet.AfterTask.Execute();
                else
                    break;
        }
            
        if (taskSet.Teardown is not null)
            taskSet.Teardown.Execute();
    }
}

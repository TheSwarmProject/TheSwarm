using TheSwarm.Components.Listener;
using TheSwarm.Common;
using TheSwarm.Interfaces;
using TheSwarm.Attributes;

namespace TheSwarm.Components.Executors; 

/// <summary>
/// Looped task executor. Picks weight-random tasks from the pool and keeps working until the command to shut-down is given.
/// </summary>
public class LoopedTaskExecutor : TaskExecutor {
    public LoopedTaskExecutor(ResultsListener resultsListener) : base(resultsListener) {}

    internal override void TaskLoop(ExecutorThread executor) {
        TaskSet taskSet = executor.TaskSet;

        if (taskSet.Setup is not null)
            if (executor.IsRunning)
                taskSet.Setup.Execute();
            else
                return;

        while (executor.IsRunning)
            taskSet.PickRandomTask().Execute();
            
        if (taskSet.Teardown is not null)
            taskSet.Teardown.Execute();

        Console.WriteLine("Task loop exited");
    }
}
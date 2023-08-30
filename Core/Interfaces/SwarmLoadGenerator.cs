using TheSwarm.Extendables;
using TheSwarm.Components.Executors;

namespace TheSwarm.Components.LoadGenerator;

/// <summary>
/// A wrapper type - encapsulates task executor and execution sequence into a single entity.
/// This also allows user to store load generator patterns, and use them by ID instead of defining them with each builder invocation
/// </summary>
public class SwarmLoadGenerator
{
    public Thread?              ScenarioRunner      { get; set; }
    public TaskExecutor?        TaskExecutor        { get; set; }

    public SwarmLoadGenerator SetTaskExecutor(TaskExecutor executor) { TaskExecutor = executor; return this; }
    public SwarmLoadGenerator UseLoopedTaskExecutor() { TaskExecutor = new LoopedTaskExecutor(); return this; }
    public SwarmLoadGenerator UseOneShotTaskExecutor() { TaskExecutor = new OneShotTaskExecutor(); return this; }

    public SwarmLoadGenerator SetExecutorSequence(Action<TaskExecutor> action)
    {
        if (TaskExecutor is null)
            throw new Exception("Task executor must be set prior to executor sequence initialization");

        this.ScenarioRunner = new Thread(() =>
        {
            action(TaskExecutor);
            TaskExecutor.Finish();
        });

        return this;
    }
}
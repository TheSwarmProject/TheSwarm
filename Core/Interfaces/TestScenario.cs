using System.Reflection;
using TheSwarm.Components.Listener;
using TheSwarm.Components.Executors;
using TheSwarm.Attributes;

namespace TheSwarm.Interfaces;

public class SwarmPreparedTestScenario {
    private Thread? scenarioRunner {get; set;}
    public TaskExecutor? TaskExecutor {get; private set;}
    public ResultsListener? ResultsListener {get; private set;}

    internal SwarmPreparedTestScenario() {}

    internal SwarmPreparedTestScenario SetTaskExecutor(TaskExecutor executor) { TaskExecutor = executor; return this; }
    internal SwarmPreparedTestScenario SetResultsListener(ResultsListener listener) { ResultsListener = listener; return this; }

    internal SwarmPreparedTestScenario SetExecutorSequence(Action<TaskExecutor> action) {
        this.scenarioRunner = new Thread(() => {
            action(TaskExecutor);
            TaskExecutor.Finish();
            });

        return this;
    }

    internal SwarmPreparedTestScenario SetExecutorTaskSet(string taskSetID) {
        Type result = AppDomain.CurrentDomain.GetAssemblies()
	        .SelectMany(a => a.GetTypes()
                .Where(t => t.IsDefined(typeof(SwarmTaskSet))))
                .Where(t => ((SwarmTaskSet) t.GetCustomAttribute(typeof(SwarmTaskSet))).TaskSetID == taskSetID)
            .FirstOrDefault();

        if (result is not null)
            if (TaskExecutor is not null)
                TaskExecutor.TaskSet = result;
            else
                throw new Exception("Task executor is not initialized");
        else
            throw new Exception($"Couldn't find a task set type with TaskSetID of {taskSetID}");

        return this;
    }

    public void RunScenario() {
        if (scenarioRunner is null)
            throw new Exception("Executor sequence was not set. Aborting");
        if (TaskExecutor is null)
            throw new Exception("Task executor was not set. Aborting");
        if (ResultsListener is null)
            throw new Exception("Results listener was not set. Aborting");
            
        TaskExecutor.IsGreen = true;
        ResultsListener.Start();
        scenarioRunner.Start();
    }
}
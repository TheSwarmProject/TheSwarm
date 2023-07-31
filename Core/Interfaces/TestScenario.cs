using System.Reflection;
using TheSwarm.Components.Listener;
using TheSwarm.Components.Executors;
using TheSwarm.Attributes;

namespace TheSwarm.Interfaces;

public class TestScenario {
    private Thread? scenarioRunner {get; set;}
    public TaskExecutor TaskExecutor {get; private set;}
    public ResultsListener ResultsListener {get; private set;}

    public TestScenario(ExecutorType executorType, SwarmMode mode = SwarmMode.Local) {
        this.ResultsListener = new ResultsListener();
        switch (executorType) {
            case ExecutorType.OneShotExecutor:
                this.TaskExecutor = new OneShotTaskExecutor(ResultsListener);
                break;
            default:
                this.TaskExecutor = new OneShotTaskExecutor(ResultsListener);
                break;
        }
    }

    public TestScenario SetExecutorSequence(Action<TaskExecutor> action) {
        this.scenarioRunner = new Thread(() => {action(TaskExecutor);});

        return this;
    }

    public TestScenario SetExecutorTaskSet(string taskSetID) {
        Type result = AppDomain.CurrentDomain.GetAssemblies()
	        .SelectMany(a => a.GetTypes()
                .Where(t => t.IsDefined(typeof(SwarmTaskSet))))
                .Where(t => ((SwarmTaskSet) t.GetCustomAttribute(typeof(SwarmTaskSet))).TaskSetID == taskSetID)
            .FirstOrDefault();

        if (result is not null)
            this.TaskExecutor.TaskSet = result;
        else
            throw new Exception($"Couldn't find a task set type with TaskSetID of {taskSetID}");

        return this;
    }

    public void RunScenario() {
        if (scenarioRunner is null) {
            throw new Exception("Executor sequence was not specified. Aborting");
        }
        TaskExecutor.IsGreen = true;
        scenarioRunner.Start();
    }
}
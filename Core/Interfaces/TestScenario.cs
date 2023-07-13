using TheSwarm.Components.Listener;
using TheSwarm.Components.Executors;
using TheSwarm.Attributes;

namespace TheSwarm.Interfaces;

public class TestScenario {
    private Thread? scenarioRunner {get; set;}
    public TaskExecutor TaskExecutor {get; private set;}
    public ResultsListener ResultsListener {get; private set;}

    public TestScenario(ExecutorType executorType) {
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

    public TestScenario SetExecutorTaskSet(Type taskSetType) {
        if (taskSetType.IsSubclassOf(typeof(TaskSet)))
            this.TaskExecutor.TaskSet = taskSetType;
        else
            throw new Exception($"Provided task set was of type {taskSetType.GetType()}. Subclass of TaskSet is required.");

        return this;
    }

    public void RunScenario() {
        if (scenarioRunner is null) {
            throw new Exception("Executor sequence was not specified. Aborting");
        }
        scenarioRunner.Start();
    }
}
using System.Reflection;
using TheSwarm.Components.Listener;
using TheSwarm.Components.LoadGenerator;
using TheSwarm.Extendables;
using TheSwarm.Attributes;

namespace TheSwarm;

/// <summary>
/// A container type for test scenario - used as an output of SwarmBuilder initializer.
/// </summary>
public class SwarmPreparedTestScenario
{
    public SwarmLoadGenerator   LoadGenerator       { get; private set; } = new SwarmLoadGenerator();
    public ResultsListener?     ResultsListener     { get; private set; }

    internal SwarmPreparedTestScenario SetTaskExecutor(TaskExecutor executor)       { LoadGenerator.TaskExecutor = executor; return this; }
    internal SwarmPreparedTestScenario SetResultsListener(ResultsListener listener) { ResultsListener = listener; return this; }

    internal SwarmPreparedTestScenario SetExecutorSequence(Action<TaskExecutor> action)
    {
        if (LoadGenerator.TaskExecutor is null)
            throw new Exception("Task executor must be set prior to executor sequence initialization");
        
        LoadGenerator.ScenarioRunner = new Thread(() =>
        {
            action(LoadGenerator.TaskExecutor);
            LoadGenerator.TaskExecutor.Finish();
        });

        return this;
    }

    internal SwarmPreparedTestScenario SetExecutorTaskSet(string taskSetID)
    {
        Type result = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes() 
                            .Where(t => t.IsDefined(typeof(SwarmTaskSet))))
                .Where(t => ((SwarmTaskSet)t.GetCustomAttribute(typeof(SwarmTaskSet))).TaskSetID == taskSetID)
            .FirstOrDefault();

        if (result is not null)
            if (LoadGenerator.TaskExecutor is not null)
                LoadGenerator.TaskExecutor.TaskSet = result;
            else
                throw new Exception("Task executor is not initialized");
        else
            throw new Exception($"Couldn't find a task set type with TaskSetID of {taskSetID}");

        return this;
    }

    internal SwarmPreparedTestScenario SetLoadScenario(string scenarioID)
    {
        if (ResultsListener is null)
            throw new Exception("Results listener needs to be initialized priot to setting executor task set");
        
        FieldInfo result = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes()
                            .Where(t => t.GetCustomAttributes(typeof(SwarmLoadScenariosRepository)).Count() > 0)
                            .SelectMany(t => t.GetFields().
                                Where(p => p.IsStatic == true))
                        .Where(m => ((SwarmLoadScenario)m.GetCustomAttribute(typeof(SwarmLoadScenario))).ScenarioID == scenarioID))
            .FirstOrDefault();

        if (result is not null)
            if (result.GetValue(null) is SwarmLoadGenerator)
                LoadGenerator = (SwarmLoadGenerator) result.GetValue(null);
            else
                throw new Exception($"SwarmLoadScenario property must be of type 'SwarmLoadGenerator'. Type provided was {result.GetValue(null).GetType()}");
        else
            throw new Exception($"Couldn't find scenario with ID {scenarioID}");

        LoadGenerator.TaskExecutor.ResultsListener = ResultsListener;
        return this;
    }

    public void RunScenario()
    {
        if (LoadGenerator.ScenarioRunner is null)
            throw new Exception("Executor sequence was not set. Aborting");
        if (LoadGenerator.TaskExecutor is null)
            throw new Exception("Task executor was not set. Aborting");
        if (ResultsListener is null)
            throw new Exception("Results listener was not set. Aborting");

        LoadGenerator.TaskExecutor.IsGreen = true;
        ResultsListener.Start();
        LoadGenerator.ScenarioRunner.Start();
    }
}
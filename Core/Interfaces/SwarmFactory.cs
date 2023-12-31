using TheSwarm.Attributes;
using TheSwarm.Extendables;
using TheSwarm.Components.Listener;
using TheSwarm.Components.Executors;
using TheSwarm.Common;

namespace TheSwarm;

/// <summary>
/// /// Main entry point for a user - this factory class is used to initialize instances of results listener and task executor.
/// </summary>
public static class SwarmBuilder
{
    public static Builder InitializeScenario() => new Builder();

    /// <summary>
    /// Convenience builder type, used to assemble and return and instance of SwarmPreparedTestScenario.
    /// </summary>
    public class Builder
    {
        private SwarmPreparedTestScenario scenario { get; init; }

        internal Builder() { scenario = new SwarmPreparedTestScenario(); }

        public Builder InitializeResultsListener(SwarmListenerMode mode, bool printStats = true, LoggingLevel loggingLevel = LoggingLevel.INFO)
        {
            scenario.SetResultsListener(new ResultsListener(mode, printStats));
            return this;
        }

        public Builder InitializeOneShotTaskExecutor()
        {
            if (scenario.ResultsListener is null)
                throw new Exception("Task executor requires results listener to be initialized first.");
            OneShotTaskExecutor executor = new OneShotTaskExecutor();
            executor.SetResultsListener(scenario.ResultsListener);
            scenario.SetTaskExecutor(executor);

            return this;
        }

        public Builder InitializeLoopedTaskExecutor()
        {
            if (scenario.ResultsListener is null)
                throw new Exception("Task executor requires results listener to be initialized first.");
            LoopedTaskExecutor executor = new LoopedTaskExecutor();
            executor.SetResultsListener(scenario.ResultsListener);
            scenario.SetTaskExecutor(executor);

            return this;
        }

        public Builder UseCustomTaskExecutor(TaskExecutor executor)
        {
            if (scenario.ResultsListener is null)
                throw new Exception("Task executor requires results listener to be initialized first.");
            executor.SetResultsListener(scenario.ResultsListener);
            scenario.SetTaskExecutor(executor);
            
            return this;
        }

        public Builder SetExecutorTaskSet(string taskSetID)
        {
            scenario.SetExecutorTaskSet(taskSetID);
            return this;
        }

        public Builder SetLoadScenario(string scenarioID)
        {
            scenario.SetLoadScenario(scenarioID);
            return this;
        }

        public Builder SetExecutorSequence(Action<TaskExecutor> action)
        {
            scenario.SetExecutorSequence(action);
            return this;
        }

        public SwarmPreparedTestScenario Build() => scenario;

        public ResultsListener GetResultsListener()
        {
            if (scenario.ResultsListener is not null)
                return scenario.ResultsListener;
            else
                throw new Exception("Results listener was not initialized");
        }
    }
}


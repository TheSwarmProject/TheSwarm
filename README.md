# The Swarm
The Swarm client is a light-weight, flexible and scalable performance testing tool. Write your test scenarios and define user behavior using plain C# code, store task sets/sequences/scenarios and re-use them, execute the tests any way you like - whether integrated into existing testing solutions or executed on it's own.

## Downloads
The latest releases are available on [NuGet](https://www.nuget.org/packages/TheSwarmClient) or [GitHub](https://github.com/TheSwarmProject/TheSwarmClient/releases).

## Getting started
To prepare our first scenario we need to prepare several components:
- Task set, which will define user's behavior
- Executor sequence - the scenario itself. This will define the test flow itself - how much users to use at any given moment, how long to run it for, etc

Task sets are created by marking the type with **SwarmTaskSet** annotation. Task set type usually consists of following:
- The client, decorated with **RegisterSwarmClient** attribute - this points the client to results listener and makes sure the results are reported.
- The task methods themselves, decorated with **SwarmTask** attribute
- Optionally, task set can have pre/after test methods (**SwarmTaskSetSetup** and **SwarmTaskSetTeardown**) and pre/after task methods (**SwarmBeforeTask** and **SwarmAfterTask**) - former are executed once in the beginning and end of the test, and the latter are executed before and after each task.
```cs
[SwarmTaskSet(TaskSetID = "TC-1", Description = "Test taskset")]
public class TestClass
{
    // Client needs to be registered to make sure it will report the results to results listener
    [RegisterSwarmClient]
    SwarmHttpClient client { get; } = new SwarmHttpClient();

    // Methods marked as SwarmTaskSetSetup will be executed once before the task loop begins
    [SwarmTaskSetSetup]
    public void Setup()
    {
        client.BaseURI = "https://google.com/";
    }

    // Tasks can have weight (more weight - higher chance to be executed) and set delay after the execution in milliseconds
    [SwarmTask(Weight = 15, DelayAfterExecution = 50)]
    public void OpenIndex()
    {
        client.Get(name: "Open Index", "/");
    }

    [SwarmTask(Weight = 5, DelayAfterExecution = 50)]
    public void SearchSwarm()
    {
        client.Get(name: "Search \"Swarm\"", uri:"/search?q=swarm");
    }
}
```

Once task set is defined, we can create the scenario itself:
```cs
SwarmPreparedTestScenario scenario = TheSwarmClient.SwarmBuilder
    .InitializeScenario()
        .InitializeResultsListener(mode: SwarmListenerMode.Local, printStats: true)     // Initialize local results listener and enable current stats printing in console
        .InitializeLoopedTaskExecutor()                                                 // Initialize looped task executor
        .SetExecutorTaskSet("TC-1")                                                     // Use the task set TC-1 we've created earlier
        .SetExecutorSequence((executor) =>                                              // Define the executor sequence - the scenario itself
            executor
                .CreateUsers(5)                                                         // Create 5 users at once and start execution
                .WaitSeconds(10)                                                        // Keep them working for 10 seconds
                )
        .Build();

scenario.RunScenario();                                                                 // Start execution
```

## Links
- [GitHub repository](https://github.com/TheSwarmProject/TheSwarmClient)
- [Issues tracker](https://github.com/TheSwarmProject/TheSwarmClient/issues)
- [NuGet page](https://www.nuget.org/packages/TheSwarmClient)
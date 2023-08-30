# Load generation scenarios
The Swarm allows user to define a load profile on the fly (by manually selecting the task executor type and setting execution sequence) and create and store these scenarios in the codebase.

## Setting load profile on the fly
You can specify "anonymous" load profile using SwarmBuilder methods
```cs
SwarmPreparedTestScenario scenario = TheSwarm.SwarmBuilder
    .InitializeScenario()
        .InitializeResultsListener(mode: SwarmListenerMode.Local, printStats: true)
        .UseLoopedTaskExecutor()
        .SetExecutorTaskSet("TC-3")
        .SetExecutorSequence((executor) =>
            executor
                .CreateUsersOverTime(quantity: 5, periodInSeconds: 10)
                .WaitSeconds(5)
                .CreateUsersOverTime(quantity: 5, periodInSeconds: 5)
                .WaitSeconds(5)
                .RemoveUsersOverTime(quantity: 5, periodInSeconds: 5)
                )
        .Build();
```
**NOTE**: Results listener should be the first to initialize, since it is used by other components

## Storing load scenarios in the codebase
In case we would like to store the load profile scenarios in the codebase and re-use them - user can create "repository" types, which will contain said scenarios.
There are few simple rules to follow:
- Type must be marked with **SwarmLoadScenariosRepository** annotation - it will include the type in field scanning
- Load profile field should be:
    - **static**
    - Marked with **SwarmLoadScenario** annotation, which in turn should have at least **ScenarioID** property set
```cs
[SwarmLoadScenariosRepository]
public class ScenariosRepo
{
    [SwarmLoadScenario(ScenarioID = "SCN-1", Description = "Basic test scenario")]
    public static SwarmLoadGenerator StandardGenerator = 
        new SwarmLoadGenerator()
            .UseLoopedTaskExecutor()
            .SetExecutorSequence((executor) =>
                    executor
                        .CreateUsersOverTime(quantity: 5, periodInSeconds: 10)
                        .WaitSeconds(5)
                        .CreateUsersOverTime(quantity: 5, periodInSeconds: 5)
                        .WaitSeconds(5)
                        .RemoveUsersOverTime(quantity: 5, periodInSeconds: 5));
}
```
Once created, we can set the load profile using SwarmBuilder **SetLoadScenario** method
```cs
SwarmPreparedTestScenario scenario = TheSwarm.SwarmBuilder.
    InitializeScenario().
        InitializeResultsListener(mode: SwarmListenerMode.Local, printStats: true).
        SetLoadScenario("SCN-1").
        SetExecutorTaskSet("TC-3").
    Build();
```
**NOTE**: Results listener should be the first to initialize, since it is used by other components
# Task Executor
## What is it?
Performance test typically consists of 2 parts:
- Task Set - a set of instructions that define the behavior of virtual user. I.e. **WHAT** user is going to do
- Execution Sequence - a set of instructions that will orchestrate the number of virtual users, scenario length, requests amount limits and so on. I.e. **HOW** the scenario is going to run

## Task executor components
Task executor in The Swarm client serves a dual purpose:
- It defines the user task loop upon client's creation (more on task loop further)
- It defines the execution sequence itself - a chain of pre-set actions that define the load profile of the scenario.

## Task loop
Since task executor is responsible for handling the creation and disposal of virtual users, it needs to define the action loop each client is going to perform.
Out-of-the-box, The Swarm client offers 2 types of executors:
- **One-Shot executor** - Task loop of this executor performs a single execution of every task in the set and then expires
- **Looped executor** - All tasks are being executed in a closed loop, until the client is explicitly told to finish

### Defining custom task executor behavior
Let's say we need a custom task loop, and none of existing executors cover it.
The Swarm client allows us to define a custom executor and use it for running our scenario.
To do that - we need to inherit our executor type from base **TaskExecutor** class and override **TaskLoop** method:
```cs
using TheSwarmClient.Extendables;
using TheSwarmClient.Components.Listener;
using TheSwarmClient.Interfaces;

public class CustomExecutor : TaskExecutor
{
    public override void TaskLoop(ExecutorThread executor)
    {
        // First, we get a task set
        TaskSet taskSet = executor.TaskSet;

        // Let's check if setup was set. And if yes - execute it.
        if (taskSet.Setup is not null)
            if (executor.IsRunning)
                taskSet.Setup.Execute();
            else
                return;
        
        // Let's pick a random task and execute it.
        taskSet.PickRandomTask().Execute();

        // Once finished - let's see whether we have a teardown set. If yes - execute it.
        if (taskSet.Teardown is not null)
            taskSet.Teardown.Execute();
    }
}
```
Once done, you can use scenario builder's **UseCustomTaskExecutor** method to set it
```cs
SwarmPreparedTestScenario scenario = TheSwarmClient.SwarmBuilder.
    InitializeScenario().
        InitializeResultsListener(mode: SwarmListenerMode.Local, printStats: true).
        UseCustomTaskExecutor(new CustomExecutor())
```

## Execution Sequence
Execution sequence defines the flow of the scenario - how executor will orchestrate the virtual users.
For the time being, these are the available controls:
- **CreateUsers** - Creates given amount of users at once and immideately starts their taskset execution.
- **CreateUsersOverTime** - Creates given amount of users over specified period of time. Once user is created, task-set execution is started immideately.
- **RemoveUsers** - Stops task set execution for given amount of currently active users.
- **RemoveUsersOverTime** - Stops the task execution for given amount of currently active users over specified period of time.
- **SetRPSLimit** - Sets the Requests Per Second limit - once the limit is reached, requests stop from being executed.
- **GetToRPSLimitOverTime** - Gradually increases/decreases the Requests per second limit over specified period of time.
- **WaitSeconds** - Wait for given amount of seconds without making any changes to virtual users/limits
- **WaitMilliseconds** - Same, but measured in milliseconds
- **Finish** - Marks the end of test scenario, stops remaining virtual users and generates a report. **OPTIONAL** - executor will trigger the finish method automatically once execution sequence is finished.
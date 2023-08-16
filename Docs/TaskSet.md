# Task Set
## What is it?
Performance test typically consists of 2 parts:
- Task Set - a set of instructions that define the behavior of virtual user. I.e. **WHAT** user is going to do
- Execution Sequence - a set of instructions that will orchestrate the number of virtual users, scenario length, requests amount limits and so on. I.e. **HOW** the scenario is going to run

## Rules of composition
- Any type with available no-argument constructor can be turned into a task set by marking it with **SwarmTaskSet** annotation. This annotation has 2 parameters:
    - **TaskSetID** - ID of the task set. **Mandatory parameter**
    - **Description** - Short description. **Optional**, not yet used.
```cs
[SwarmTaskSet(TaskSetID = "TC-1", Description = "Test taskset")]
public class TestClass
```
- Any task-set must contain at least 1 task - **no-arguments, void-type method**, marked with **SwarmTask** annotation. This annotation has 2 optional parameters:
    - **Weight** - Weight of the task. The higher the value - the higher the chances of this task to be executed.
    - **DelayAfterExecution** - the amount of delay in milliseconds after the task execution.
```cs
[SwarmTask(Weight = 15, DelayAfterExecution = 50)]
public void OpenIndex()
{
    client.Get(name: "Open Index", "/");
}
```
- Task set can have 1 of each of optional flow control steps - **no-arguments, void-type methods**, marked with following annotations:
    - **SwarmTaskSetSetup** - Method is executed once during the taskset initialization
    - **SwarmTaskSetTeardown** - Method is execuoted once in the end of taskset lifetime
    - **SwarmBeforeTask** - Method is executed before each task
    - **SwarmAfterTask** - Method is executed after each task
```cs
[SwarmTaskSetSetup]
public void Setup()
{
    client.BaseURL = "https://google.com/";
}
```
- **SwarmTaskSetSetup**, **SwarmTaskSetTeardown**, **SwarmBeforeTask** and **SwarmAfterTask** have **DelayAfterExecution** as optional parameter
- Task-set can have multiple clients. However, in order for those clients to report the data to Results Listener - they need to be properly initialized. To make sure this initialization is done - annotate the client property with **RegisterSwarmClient** attribute.
```cs
[RegisterSwarmClient]
SwarmHttpClient client { get; } = new SwarmHttpClient();
```
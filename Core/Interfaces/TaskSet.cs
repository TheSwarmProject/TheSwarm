using System.Reflection;
using TheSwarm.Attributes;

namespace TheSwarm.Interfaces;

/// <summary>
/// Internal wrapper, used to initialize and process the SwarmTaskSet type.
/// </summary>
internal class TaskSet {
    private object taskSetInstance {get; init;}
    private SwarmTaskSet taskSetParams {get; set;}
    protected TaskExecutor? Executor {get; set;}
    protected void SetTaskExecutor(TaskExecutor executor) => Executor = executor;
    private Random rnd {get; init;} = new Random();
    private Action? setup {get; init;}
    private Action? teardown {get; init;}
    private Action? beforeTask {get; init;}
    private Action? afterTask {get; init;}
    private List<SwarmTask> tasks {get; init;} = new List<SwarmTask>();
    private int totalTasksWeight {get; init;} = 0;

    public TaskSet(Type type) {
        if (type.GetCustomAttribute(typeof(SwarmTaskSet)) is not null) {
            this.taskSetInstance = Activator.CreateInstance(type);
            this.taskSetParams = (SwarmTaskSet) taskSetInstance.GetType().GetCustomAttribute(typeof(SwarmTaskSet));
        } else
            throw new Exception($"{type.ToString()} is not marked as SwarmTaskSet. Make sure it has SwarmTaskSet annotation added and try again");

        MethodInfo? m = this.taskSetInstance.GetType().GetMethods().Where(m => m.GetCustomAttributes(typeof(SwarmTaskSetSetup), false).Length > 0).FirstOrDefault();
        if (m is not null)
            this.setup = (Action)Delegate.CreateDelegate(typeof(Action), this.taskSetInstance, m);
        m = this.taskSetInstance.GetType().GetMethods().Where(m => m.GetCustomAttributes(typeof(SwarmTaskSetTeardown), false).Length > 0).FirstOrDefault();
        if (m is not null)
            this.teardown = (Action)Delegate.CreateDelegate(typeof(Action), this.taskSetInstance, m);
        m = this.taskSetInstance.GetType().GetMethods().Where(m => m.GetCustomAttributes(typeof(SwarmBeforeTask), false).Length > 0).FirstOrDefault();
        if (m is not null)
            this.beforeTask = (Action)Delegate.CreateDelegate(typeof(Action), this.taskSetInstance, m);
        m = this.taskSetInstance.GetType().GetMethods().Where(m => m.GetCustomAttributes(typeof(SwarmAfterTask), false).Length > 0).FirstOrDefault();
        if (m is not null)
            this.afterTask = (Action)Delegate.CreateDelegate(typeof(Action), this.taskSetInstance, m);

        List<MethodInfo> tasks = this.taskSetInstance.GetType().GetMethods().Where(m => m.GetCustomAttributes(typeof(SwarmTask), false).Length > 0).ToList();

        if (tasks.Count == 0)
            throw new Exception("No tasks were defined in taskset. Use SwarmTask annotation to mark a void method as a task");
        foreach(MethodInfo method in tasks) 
            if(!method.IsStatic) {
                SwarmTask task = (SwarmTask) method.GetCustomAttribute(typeof(SwarmTask));
                task.SetMethod((Action)Delegate.CreateDelegate(typeof(Action), this.taskSetInstance, method));
                this.tasks.Add(task);
                if (task.Weight != 0)
                    totalTasksWeight += task.Weight;
            }
                

        Console.WriteLine($"Tasks: {this.tasks.ToArray()}");
    }

    public void ExecuteRandomTask() {
        SwarmTask task = PickRandomTask();
        if (task.Method is not null)
            task.Method();
        else
            throw new Exception($"Task method was not initialized");
    }

    private SwarmTask PickRandomTask() {
        if (totalTasksWeight > 0)
            return PickRandomTaskWithWeight();
        else
            return tasks[rnd.Next(tasks.Count)];
    }

    private SwarmTask PickRandomTaskWithWeight() {
        int randomInt = rnd.Next(0, totalTasksWeight);

        SwarmTask task = null;
        foreach (SwarmTask swarmTask in tasks){
            if (randomInt < swarmTask.Weight){
                task = swarmTask;
                break;
            }

            randomInt = randomInt - swarmTask.Weight;
        }

        if (task is not null)
            return task;
        else
            return tasks[rnd.Next(tasks.Count)];
    }
}
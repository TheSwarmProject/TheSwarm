using System.Reflection;

namespace TheSwarm.Interfaces;
using TheSwarm.Attributes;

public abstract class TaskSet {
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

    public TaskSet() {
        if (this.GetType().GetCustomAttribute(typeof(SwarmTaskSet)) is not null)
            this.taskSetParams = (SwarmTaskSet) this.GetType().GetCustomAttribute(typeof(SwarmTaskSet));
        else
            this.taskSetParams = new SwarmTaskSet();

        MethodInfo m = this.GetType().GetMethods().Where(m => m.GetCustomAttributes(typeof(SwarmTaskSetSetup), false).Length > 0).First();
        if (m is not null)
            this.setup = (Action)Delegate.CreateDelegate(typeof(Action), this, m);
        m = this.GetType().GetMethods().Where(m => m.GetCustomAttributes(typeof(SwarmTaskSetTeardown), false).Length > 0).First();
        if (m is not null)
            this.teardown = (Action)Delegate.CreateDelegate(typeof(Action), this, m);
        m = this.GetType().GetMethods().Where(m => m.GetCustomAttributes(typeof(SwarmBeforeTask), false).Length > 0).First();
        if (m is not null)
            this.beforeTask = (Action)Delegate.CreateDelegate(typeof(Action), this, m);
        m = this.GetType().GetMethods().Where(m => m.GetCustomAttributes(typeof(SwarmAfterTask), false).Length > 0).First();
        if (m is not null)
            this.afterTask = (Action)Delegate.CreateDelegate(typeof(Action), this, m);

        List<MethodInfo> tasks = this.GetType().GetMethods().Where(m => m.GetCustomAttributes(typeof(SwarmTask), false).Length > 0).ToList();

        if (tasks.Count == 0)
            throw new Exception("No tasks were defined in taskset. Use SwarmTask annotation to mark a void method as a task");
        foreach(MethodInfo method in tasks) 
            if(!method.IsStatic) {
                SwarmTask task = (SwarmTask) method.GetType().GetCustomAttribute(typeof(SwarmTask));
                task.SetMethod((Action)Delegate.CreateDelegate(typeof(Action), this, method));
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
        return tasks[rnd.Next(tasks.Count)];
    }
}
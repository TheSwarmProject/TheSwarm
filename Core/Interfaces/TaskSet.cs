using System.Reflection;
using TheSwarmClient.Attributes;
using TheSwarmClient.Extendables;

namespace TheSwarmClient.Interfaces;

/// <summary>
/// Internal wrapper, used to initialize and process the SwarmTaskSet type.
/// </summary>
internal class TaskSet {
    private object taskSetInstance {get; init;}
    private SwarmTaskSet taskSetParams {get; set;}
    protected TaskExecutor Executor {get; set;}
    private Random rnd {get; init;} = new Random();
    public SwarmTaskSetSetup? Setup {get; init;}
    public SwarmTaskSetTeardown? Teardown {get; init;}
    public SwarmBeforeTask? BeforeTask {get; init;}
    public SwarmAfterTask? AfterTask {get; init;}
    private List<SwarmTask> Tasks {get; init;} = new List<SwarmTask>();
    private int totalTasksWeight {get; init;} = 0;

    public TaskSet(TaskExecutor executor) {
        this.Executor = executor;
        if (executor.TaskSet.GetCustomAttribute(typeof(SwarmTaskSet)) is not null) {
            this.taskSetInstance = Activator.CreateInstance(executor.TaskSet);
            this.taskSetParams = (SwarmTaskSet) taskSetInstance.GetType().GetCustomAttribute(typeof(SwarmTaskSet));
        } else
            throw new Exception($"{executor.TaskSet.ToString()} is not marked as SwarmTaskSet. Make sure it has SwarmTaskSet annotation added and try again");

        MethodInfo? m = this.taskSetInstance.GetType().GetMethods().Where(m => m.GetCustomAttributes(typeof(SwarmTaskSetSetup), false).Length > 0).FirstOrDefault();
        if (m is not null) {
            this.Setup = ((SwarmTaskSetSetup) m.GetCustomAttribute(typeof(SwarmTaskSetSetup)));
            this.Setup.SetMethod((Action)Delegate.CreateDelegate(typeof(Action), this.taskSetInstance, m));
        }
        m = this.taskSetInstance.GetType().GetMethods().Where(m => m.GetCustomAttributes(typeof(SwarmTaskSetTeardown), false).Length > 0).FirstOrDefault();
        if (m is not null) {
            this.Teardown = ((SwarmTaskSetTeardown) m.GetCustomAttribute(typeof(SwarmTaskSetTeardown)));
            this.Teardown.SetMethod((Action)Delegate.CreateDelegate(typeof(Action), this.taskSetInstance, m));
        }
        m = this.taskSetInstance.GetType().GetMethods().Where(m => m.GetCustomAttributes(typeof(SwarmBeforeTask), false).Length > 0).FirstOrDefault();
        if (m is not null) {
            this.BeforeTask = ((SwarmBeforeTask) m.GetCustomAttribute(typeof(SwarmBeforeTask)));
            this.BeforeTask.SetMethod((Action)Delegate.CreateDelegate(typeof(Action), this.taskSetInstance, m));
        }
        m = this.taskSetInstance.GetType().GetMethods().Where(m => m.GetCustomAttributes(typeof(SwarmAfterTask), false).Length > 0).FirstOrDefault();
        if (m is not null) {
            this.AfterTask = ((SwarmAfterTask) m.GetCustomAttribute(typeof(SwarmAfterTask)));
            this.AfterTask.SetMethod((Action)Delegate.CreateDelegate(typeof(Action), this.taskSetInstance, m));
        }

        List<MethodInfo> tasks = this.taskSetInstance.GetType().GetMethods().Where(m => m.GetCustomAttributes(typeof(SwarmTask), false).Length > 0).ToList();

        if (tasks.Count == 0)
            throw new Exception("No tasks were defined in taskset. Use SwarmTask annotation to mark a public void method as a task");
        foreach(MethodInfo method in tasks) 
            if(!method.IsStatic) {
                SwarmTask task = (SwarmTask) method.GetCustomAttribute(typeof(SwarmTask));
                task.SetMethod((Action)Delegate.CreateDelegate(typeof(Action), this.taskSetInstance, method));
                this.Tasks.Add(task);
                if (task.Weight != 0)
                    totalTasksWeight += task.Weight;
            }
        
        // Since taskset is created as object during runtime, we look for RegisterSwarmClient in runtime properties.
        PropertyInfo? prop = this.taskSetInstance.GetType().GetRuntimeProperties()
            .Where(p => p.GetCustomAttributes(typeof(RegisterSwarmClient), false).Length > 0)
            .FirstOrDefault();
        if(prop is not null)
            if (prop.GetValue(this.taskSetInstance).GetType().IsSubclassOf(typeof(SwarmClient)))
                ((SwarmClient) prop.GetValue(this.taskSetInstance)).SetTaskExecutor(executor);
            else
                throw new Exception($"Type {prop.GetType().Name} is not a sub-class of SwarmClient and thus cannot be registered as one");
    }

    /// <summary>
    /// Picks random task from the pool.
    /// NOTE: If task weights were set - it will pick it using PickRandomTaskWithWeight method (i.e. using weight as factor). If not - it will pick one at random.
    /// </summary>
    /// <returns>SwarmTask to execute</returns>
    public SwarmTask PickRandomTask() {
        if (totalTasksWeight > 0)
            return PickRandomTaskWithWeight();
        else
            return Tasks[rnd.Next(Tasks.Count)];
    }

    public List<SwarmTask> GetAllTasks() => Tasks;

    /// <summary>
    /// Picks random task, based on their weight.
    /// If no tasks were picked by an algorithm - a random task will be picked.
    /// </summary>
    /// <returns>SwarmTask to execute</returns>
    private SwarmTask PickRandomTaskWithWeight() {
        int randomInt = rnd.Next(0, totalTasksWeight);

        SwarmTask task = null;
        foreach (SwarmTask swarmTask in Tasks){
            if (randomInt < swarmTask.Weight){
                task = swarmTask;
                break;
            }

            randomInt = randomInt - swarmTask.Weight;
        }

        if (task is not null)
            return task;
        else
            return Tasks[rnd.Next(Tasks.Count)];
    }
}
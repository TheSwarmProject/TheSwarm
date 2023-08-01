namespace TheSwarm.Interfaces; 

/// <summary>
/// A representation of an executor thread - a container for client's Thread with extra bits and pieces for fine-grained control.
/// </summary>
public class ExecutorThread {
    private Thread thread {get; init;}
    /// <summary>
    /// Given the fact that each client using the task-set must be unique (1 client per executor thread) - we initialize an instance of task set for each executor thread
    /// </summary>
    internal TaskSet TaskSet {get; init;}
    public bool IsRunning {get; set;}
    public bool IsAlive => thread.IsAlive;

    public ExecutorThread(TaskExecutor executor) {
        this.TaskSet = new TaskSet(executor);
        this.thread = new Thread(() => { executor.TaskLoop(this); });
    }

    public void Start() {
        IsRunning = true;
        thread.Start();
    }

    public void Finish() => IsRunning = false;
    public void Terminate() => thread.Interrupt();
}
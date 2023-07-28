using TheSwarm.Common;
using TheSwarm.Components.Listener;

namespace TheSwarm.Interfaces; 

/// <summary>
/// Task Executor template class - defines the basic attributes and methods to be used by all task executor types.
/// </summary>
public abstract class TaskExecutor {
    protected ResultsListener resultsListener {get; set;}
    protected List<Thread> executorThreads {get;} = new List<Thread>();

    internal TaskSet? TaskSet {get; set;}
    internal abstract void TaskLoop(TaskSet taskSet);
    
    protected int CurrentRequestsPerSecond {get; set;}
    protected int RequestsPerSecondLimit {get; set;}
    public bool isGreen {get; private set;}

    public TaskExecutor(ResultsListener resultsListener) {
        this.resultsListener = resultsListener;            
    }

    /// <summary>
    /// This method is merely a proxy between client and results listener - it simply routes Response
    /// object to listener instance.
    /// </summary>
    /// <param name="response">Response instance</param>
    public void LogEntry(Response response) {
        resultsListener.LogEntry(response);
    }

    public void ReportRequestExecuted() {
        CurrentRequestsPerSecond += 1;
        if (CurrentRequestsPerSecond >= RequestsPerSecondLimit) {
            isGreen = false;
        }
    }

    public TaskExecutor CreateUsers(int quantity) {
        if (TaskSet is null)
            throw new Exception("TaskSet was not initialized. Assign it using SetExecutorTaskSet call first");
        
        for (int index = 0; index < quantity; index++){
            Thread thread = new Thread(() => { TaskLoop(TaskSet); });
            executorThreads.Add(thread);
            thread.Start();
        }
            

        return this;
    }

    public TaskExecutor SetRPSLimit(int limit) {
        RequestsPerSecondLimit = limit;

        return this;
    }

    public TaskExecutor WaitSeconds(int amount) {
        Thread.Sleep(amount * 1000);

        return this;
    }

    public TaskExecutor WaitMilliseconds(int amount) {
        Thread.Sleep(amount);

        return this;
    }
}
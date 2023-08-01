using TheSwarm.Common;
using TheSwarm.Components.Listener;
using System.Diagnostics;

namespace TheSwarm.Interfaces; 

/// <summary>
/// Task Executor template class - defines the basic attributes and methods to be used by all task executor types.
/// </summary>
public abstract class TaskExecutor {
    protected ResultsListener resultsListener {get; set;}
    protected List<ExecutorThread> executorThreads {get;} = new List<ExecutorThread>();

    internal Type? TaskSet {get; set;}
    internal abstract void TaskLoop(ExecutorThread executor);
    
    protected int CurrentRequestsPerSecond {get; set;}
    protected int RequestsPerSecondLimit {get; set;}
    /// <summary>
    /// RPS limiter flag - prevents requests execution in case we've exceeded the RPS limit
    /// </summary>
    public bool IsGreen {get; set;}
    /// <summary>
    /// Indicator flag - set by FInish() method and used to prevent multiple finish calls at the same time
    /// </summary>
    public bool IsFinishing {get; private set;}

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
        if (RequestsPerSecondLimit > 0 && CurrentRequestsPerSecond >= RequestsPerSecondLimit) {
            IsGreen = false;
        }
    }

    public TaskExecutor CreateUsers(int quantity) {
        if (TaskSet is null)
            throw new Exception("TaskSet was not initialized. Assign it using SetExecutorTaskSet call first");
        
        for (int index = 0; index < quantity; index++){
            ExecutorThread thread = new ExecutorThread(this);
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

    public void Finish() {
        if (!IsFinishing) {
            IsFinishing = true;
            lock (executorThreads) {
                foreach (ExecutorThread thread in executorThreads)
                    if (thread.IsRunning)
                        thread.Finish();
                
                Stopwatch stopwatch = new Stopwatch();
                TimeSpan timeout = TimeSpan.FromSeconds(60);
                stopwatch.Start();       // We wait 60 seconds for teardown to kick in. If it doesn't happen - we terminate threads by force
                while (executorThreads.Where((thread) => thread.IsRunning).Count() > 0) {
                    if (stopwatch.ElapsedMilliseconds > timeout.Milliseconds)
                        foreach (ExecutorThread thread in executorThreads)
                            thread.Terminate();
                }
            }

            resultsListener.Stop();
            resultsListener.GenerateReport();
        }    
    }
}
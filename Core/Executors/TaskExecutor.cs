using TheSwarm.Common;
using TheSwarm.Interfaces;
using TheSwarm.Components.Listener;
using System.Diagnostics;

namespace TheSwarm.Extendables;

/// <summary>
/// Task Executor template class - defines the basic attributes and methods to be used by all task executor types.
/// </summary>
public abstract class TaskExecutor
{
    protected LoggingChannel        log             { get; init; }
    protected ResultsListener?      resultsListener { get; set; }
    protected List<ExecutorThread>  executorThreads { get; } = new List<ExecutorThread>();

    internal Type? TaskSet { get; set; }
    public abstract void TaskLoop(ExecutorThread executor);

    protected int CurrentRequestsPerSecond { get; set; }
    protected int RequestsPerSecondLimit { get; set; }
    /// <summary>
    /// RPS limiter flag - prevents requests execution in case we've exceeded the RPS limit
    /// </summary>
    public bool IsGreen { get; set; }
    /// <summary>
    /// Indicator flag - set by FInish() method and used to prevent multiple finish calls at the same time
    /// </summary>
    public bool IsFinishing { get; private set; }

    public TaskExecutor()
    {
        this.log = Logger.CreateChannel(this.GetType().Name);
    }

    public void SetResultsListener(ResultsListener resultsListener) => this.resultsListener = resultsListener;

    /// <summary>
    /// This method is merely a proxy between client and results listener - it simply routes Response
    /// object to listener instance.
    /// </summary>
    /// <param name="response">Response instance</param>
    public void LogEntry(Response response)
    {
        resultsListener.LogEntry(response);
    }

    public void ReportRequestExecuted()
    {
        CurrentRequestsPerSecond += 1;
        if (RequestsPerSecondLimit > 0 && CurrentRequestsPerSecond >= RequestsPerSecondLimit)
        {
            IsGreen = false;
        }
    }

    /// <summary>
    /// Creates given amount of users at once and immideately starts their taskset execution.
    /// </summary>
    /// <param name="quantity">Number of users to create</param>
    /// <returns></returns>
    public TaskExecutor CreateUsers(int quantity)
    {
        if (TaskSet is null)
            throw new Exception("TaskSet was not initialized. Assign it using SetExecutorTaskSet call first");

        log.Info($"Creating {quantity} users");
        for (int index = 0; index < quantity; index++)
        {
            ExecutorThread thread = new ExecutorThread(this);
            executorThreads.Add(thread);
            thread.Start();
        }

        return this;
    }

    /// <summary>
    /// Creates given amount of users over specified period of time. Once user is created, task-set execution is started immideately.
    /// </summary>
    /// <param name="quantity">Number of users to create</param>
    /// <param name="periodInSeconds">Amount of time in seconds during which to create said users</param>
    /// <returns></returns>
    public TaskExecutor CreateUsersOverTime(int quantity, int periodInSeconds)
    {
        if (TaskSet is null)
            throw new Exception("TaskSet was not initialized. Assign it using SetExecutorTaskSet call first");

        double sleepTime = (double)quantity / periodInSeconds;
        if (sleepTime <= 0)
            sleepTime = 0.05;

        log.Info($"Creating {quantity} users over {periodInSeconds} - 1 user every {sleepTime} seconds");
        for (int index = 0; index < quantity; index++)
        {
            ExecutorThread thread = new ExecutorThread(this);
            executorThreads.Add(thread);
            thread.Start();

            WaitMilliseconds((int)(sleepTime * 1000));
        }

        return this;
    }

    /// <summary>
    /// Stops task set execution for given amount of currently active users.
    /// </summary>
    /// <param name="quantity">Number of users to stop</param>
    /// <returns></returns>
    public TaskExecutor RemoveUsers(int quantity)
    {
        if (TaskSet is null)
            throw new Exception("TaskSet was not initialized. Assign it using SetExecutorTaskSet call first");

        log.Info($"Removing {quantity} users");
        lock (executorThreads)
        {
            for (int index = 0; index < quantity; index++)
            {
                if (executorThreads.Count > 0)
                {
                    ExecutorThread thread = executorThreads[0];
                    thread.Finish();
                    executorThreads.Remove(thread);
                }
                else
                {
                    log.Warning("No more active users to stop.");
                }
            }
        }

        return this;
    }

    /// <summary>
    /// Stops the task execution for given amount of currently active users over specified period of time.
    /// </summary>
    /// <param name="quantity">NUmber of users to stop</param>
    /// <param name="periodInSeconds">Amount of time in seconds during which to stop said users</param>
    /// <returns></returns>
    public TaskExecutor RemoveUsersOverTime(int quantity, int periodInSeconds)
    {
        if (TaskSet is null)
            throw new Exception("TaskSet was not initialized. Assign it using SetExecutorTaskSet call first");

        double sleepTime = (double)quantity / periodInSeconds;
        if (sleepTime <= 0)
            sleepTime = 0.05;

        log.Info($"Removing {quantity} users over {periodInSeconds} - 1 user every {sleepTime} seconds");
        lock (executorThreads)
        {
            for (int index = 0; index < quantity; index++)
            {
                if (executorThreads.Count > 0)
                {
                    ExecutorThread thread = executorThreads[0];
                    thread.Finish();
                    executorThreads.Remove(thread);

                    WaitMilliseconds((int)(sleepTime * 1000));
                }
            }
        }

        return this;
    }

    /// <summary>
    /// Sets the Requests Per Second limit - once the limit is reached, requests stop from being executed.
    /// Executions counter is reset every tick (1 second), so until the counter is reset - clients will be waiting for it.
    /// </summary>
    /// <param name="limit">Requests per second limit</param>
    /// <returns></returns>
    public TaskExecutor SetRPSLimit(int limit)
    {
        log.Info($"Setting RPS to {limit}");
        RequestsPerSecondLimit = limit;

        return this;
    }

    /// <summary>
    /// Gradually increases/decreases the Requests per second limit over specified period of time.
    /// Executions counter is reset every tick (1 second), so until the counter is reset - clients will be waiting for it.
    /// </summary>
    /// <param name="limit">Requests per second limit</param>
    /// <param name="periodInSeconds">Amount of time in seconds during which to stop said users</param>
    /// <returns></returns>
    public TaskExecutor GetToRPSLimitOverTime(int limit, int periodInSeconds)
    {
        double sleepTime = 0;
        int ticks = 0;
        bool increase = false;
        if (limit > RequestsPerSecondLimit)
        {
            ticks = limit - RequestsPerSecondLimit;
            increase = true;
        }
        else
        {
            ticks = RequestsPerSecondLimit - limit;
        }
        sleepTime = (float)ticks / periodInSeconds;
        if (sleepTime <= 0)
            sleepTime = 0.05;

        log.Info($"Getting to {limit} RPS over {periodInSeconds} seconds - {(increase ? "Increment" : "Decrement")} of 1 every {sleepTime} seconds");
        for (int index = 0; index < ticks; index++)
        {
            if (increase)
                RequestsPerSecondLimit += 1;
            else
                RequestsPerSecondLimit -= 1;

            WaitMilliseconds((int)(sleepTime * 1000));
        }

        return this;
    }

    public TaskExecutor WaitSeconds(int amount)
    {
        log.Info($"Waiting {amount} seconds");
        Thread.Sleep(amount * 1000);

        return this;
    }

    public TaskExecutor WaitMilliseconds(int amount)
    {
        log.Info($"Waiting {amount} milliseconds");
        Thread.Sleep(amount);

        return this;
    }

    public void Finish()
    {
        log.Info("Finish reached. Wrapping up...");
        if (!IsFinishing)
        {
            IsFinishing = true;
            lock (executorThreads)
            {
                foreach (ExecutorThread thread in executorThreads)
                    if (thread.IsRunning)
                        thread.Finish();

                Stopwatch stopwatch = new Stopwatch();
                TimeSpan timeout = TimeSpan.FromSeconds(60);
                stopwatch.Start();       // We wait 60 seconds for teardown to kick in. If it doesn't happen - we terminate threads by force
                while (executorThreads.Where((thread) => thread.IsRunning).Count() > 0)
                {
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
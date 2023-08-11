using TheSwarmClient.Common;
using TheSwarmClient.Extendables;

namespace TheSwarmClient.Interfaces;

/// <summary>
/// Transaction is a set of procedures that yields a certain result. For instance - opening a web page triggers requests the page itself, then it pulls all the resources
/// and runs AJAX requests to populate the data. Timespan between requesting a page itself and having a fully-loaded page is how long it took for all of them to open.
/// 
/// This particular handler provides sequential control - all requested procedures are executed one-by-one.
/// </summary>
public class Transaction
{
    private string name { get; init; }
    private Action action { get; init; }
    private TaskExecutor executor {get; init;}

    public Transaction(string name, Action sequenceToExecute, TaskExecutor executor)
    {
        this.name = name;
        this.action = sequenceToExecute;
        this.executor = executor;
    }

    public void Execute()
    {
        if (action is not null)
        {
            DateTime startTime = DateTime.Now;
            action();
            DateTime endTime = DateTime.Now;

            Response res = new Response(name, "(TRANSACTION)", (int)(endTime - startTime).TotalMilliseconds, null);
            executor.LogEntry(res);
        } else
            throw new Exception("Transaction directive was not set. Aborting");
    }
}

/// <summary>
/// Transaction is a set of procedures that yields a certain result. For instance - opening a web page triggers requests the page itself, then it pulls all the resources
/// and runs AJAX requests to populate the data. Timespan between requesting a page itself and having a fully-loaded page is how long it took for all of them to open.
/// 
/// This particular handler provides parallel control - all actions are stored and then executed concurrently.
/// </summary>
public class ParallelTransaction
{
    private string name { get; init; }
    private List<Action> actions {get; init;} = new List<Action>();
    private TaskExecutor executor {get; init;}

    public ParallelTransaction(string name, TaskExecutor executor) {
        this.name = name;
        this.executor = executor;
    }

    public ParallelTransaction AddAction(Action action) { actions.Add(action); return this; }

    public void Execute()
    {
        if (actions.Count > 0)
        {
            
            DateTime startTime = DateTime.Now;
            List<Thread> runners = new List<Thread>();
            foreach (Action action in actions) {
                Thread runner = new Thread(() => action());
                runners.Add(runner);
                runner.Start();
            }
            foreach(Thread runner in runners)
                runner.Join();
            DateTime endTime = DateTime.Now;

            Response res = new Response(name, "(TRANSACTION)", (int)(endTime - startTime).TotalMilliseconds, null);
            executor.LogEntry(res);
        } else
            throw new Exception("No actions were provided. Aborting");
    }
}
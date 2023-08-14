using TheSwarmClient.Interfaces;

namespace TheSwarmClient.Extendables;

public abstract class SwarmClient
{
    protected TaskExecutor TaskExecutor { get; set; }

    public void SetTaskExecutor(TaskExecutor executor) => TaskExecutor = executor;

    private bool LightIsGreen() => TaskExecutor.IsGreen;

    protected void WaitForGreenLight()
    {
        // TODO: There MUST be a more elegant way to stall the execution until we're not over the limit, but for the time being this will do.
        while (!LightIsGreen())
        {
            Thread.Sleep(10);
        }
    }

    public Transaction Transaction(string name, Action action) => new Transaction(name, action, TaskExecutor);
    public ParallelTransaction ParallelTransaction(string name) => new ParallelTransaction(name, TaskExecutor);

    /// <summary>
    /// Main measuring utility - executes given action and returns the amount of milliseconds it took.
    /// </summary>
    /// <param name="action">Action to measure</param>
    /// <returns>Time taken in milliseconds</returns>
    protected int MeasureExecutionTime(Action action)
    {
        DateTime startTime = DateTime.Now;
        action();
        DateTime endTime = DateTime.Now;

        return (int)(endTime - startTime).TotalMilliseconds;
    }
}

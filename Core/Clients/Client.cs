namespace TheSwarm.Extendables; 

public abstract class SwarmClient {
    protected TaskExecutor TaskExecutor {get; set;}

    public void SetTaskExecutor(TaskExecutor executor) => TaskExecutor = executor;

    private bool LightIsGreen() => TaskExecutor.IsGreen;

    protected void WaitForGreenLight() {
        // TODO: There MUST be a more elegant way to stall the execution until we're not over the limit, but for the time being this will do.
        while (!LightIsGreen()) {
            Thread.Sleep(10);
        }
    }
}

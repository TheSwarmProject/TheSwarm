namespace TheSwarm.Interfaces; 

public abstract class SwarmClient {
    protected TaskExecutor TaskExecutor {get; set;}

    protected void SetTaskExecutor(TaskExecutor executor) => TaskExecutor = executor;
}

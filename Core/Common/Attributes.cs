using System.Reflection;

namespace TheSwarm.Attributes;

[System.AttributeUsage(System.AttributeTargets.Class)]
public class SwarmTaskSet : System.Attribute {
    public string TaskSetID {get; set;}
    public string Description {get; set;}

    public SwarmTaskSet() {
        this.TaskSetID = "";
        this.Description = "";
    }
}

[System.AttributeUsage(System.AttributeTargets.Method)]
public class SwarmTaskSetSetup: System.Attribute {}

[System.AttributeUsage(System.AttributeTargets.Method)]
public class SwarmTaskSetTeardown: System.Attribute {}

[System.AttributeUsage(System.AttributeTargets.Method)]
public class SwarmBeforeTask: System.Attribute {}

[System.AttributeUsage(System.AttributeTargets.Method)]
public class SwarmAfterTask: System.Attribute {}

/// <summary>
/// This attribute serves as both meta-data container and a container for method itself
/// It is used to encapsulate the target method for TaskSet to use later
/// </summary>
[System.AttributeUsage(System.AttributeTargets.Method)]
public class SwarmTask : System.Attribute {
    public int Weight {get; set;}
    public int DelayAfterTaskMs {get; set;}
    public Action? Method {get; private set;}

    public SwarmTask() {
        this.Weight = 0;
        this.DelayAfterTaskMs = 0;
    }

    public void SetMethod(Action method) => this.Method = method;
}

[System.AttributeUsage(System.AttributeTargets.Class)]
public class ExecutorParameters : System.Attribute {
    public int TaskLoops {get; set;}
    public int IterationsPerCycle {get; set;}

    public ExecutorParameters() {
        this.TaskLoops = 1;
        this.IterationsPerCycle = 1;
    }
}

[System.AttributeUsage(System.AttributeTargets.Property)]
public class RegisterSwarmClient: System.Attribute {}

[System.AttributeUsage(System.AttributeTargets.Class)]
public class SwarmTestScenariosRepository: System.Attribute {}

[System.AttributeUsage(System.AttributeTargets.Method)]
public class SwarmTestScenario : System.Attribute {
    public string ScenarioID {get; set;}
    public string Description {get; set;}

    public SwarmTestScenario() {
        this.ScenarioID = "";
        this.Description = "";
    }
}

public enum ExecutorType {
    OneShotExecutor,
    LoopedExecutor,
    SequentialExecutor
}

public enum SwarmMode {
    Local,
    Hub,
    Node
}
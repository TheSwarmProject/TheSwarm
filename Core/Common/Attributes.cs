namespace TheSwarm.Attributes 
{
    using TheSwarm.Extendables;

    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class SwarmTaskSet : System.Attribute {
        public string TaskSetID {get; set;}
        public string Description {get; set;}

        public SwarmTaskSet() {
            this.TaskSetID = "";
            this.Description = "";
        }
    }

    /// <summary>
    /// This attribute serves as both meta-data container and a container for method itself
    /// It is used to encapsulate the target method for TaskSet to use later
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class SwarmTaskSetSetup: MethodContainer {}

    /// <summary>
    /// This attribute serves as both meta-data container and a container for method itself
    /// It is used to encapsulate the target method for TaskSet to use later
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class SwarmTaskSetTeardown: MethodContainer {}

    /// <summary>
    /// This attribute serves as both meta-data container and a container for method itself
    /// It is used to encapsulate the target method for TaskSet to use later
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class SwarmBeforeTask: MethodContainer {}

    /// <summary>
    /// This attribute serves as both meta-data container and a container for method itself
    /// It is used to encapsulate the target method for TaskSet to use later
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class SwarmAfterTask: MethodContainer {}

    /// <summary>
    /// This attribute serves as both meta-data container and a container for method itself
    /// It is used to encapsulate the target method for TaskSet to use later
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class SwarmTask : MethodContainer {
        public int Weight {get; set;} = 0;
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

    /// <summary>
    /// This attibute is used as a marker - it tells executor to initialize task executor instance for a client, marked with this attribute.
    /// If it is not registered - results will not be submitted to the listener and thus not added to the report.
    /// </summary>
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

    public enum SwarmListenerMode {
        Local,
        Hub,
        Node
    }
}

namespace TheSwarm.Extendables
{
    /// <summary>
    /// For convenience - we use Task set attributes as both attribute annotations AND method containers (method is to be assigned during TaskSet initialization).
    /// With that in mind, we create a base type for containing said method and handling it's initialization. The rest is type-unique.
    /// </summary>
    public class MethodContainer : System.Attribute {
        public int DelayAfterExecution {get; set;} = 0;
        public Action? Method {get; private set;}

        public void SetMethod(Action method) => this.Method = method;
        public void Execute() {
            if (Method is not null) {
                Method();

                if (DelayAfterExecution > 0)
                    Thread.Sleep(DelayAfterExecution);
            }
        }
    }
}
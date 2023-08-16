# Configuration manager
The Swarm uses internal configuration manager for key components of the client.
If default values are not satisfactory - user can override them by creating a config file and providing desired values

## How it works
On start-up, client checks-up project root folder (i.e. where the process is launched from) for file named **SwarmConfig.xml**.
- If the file is found - it is being parsed and values from the file are used to override the default values.
- If the file is not found - client proceeds with using default values

## File format and current supported entries
```xml
<TheSwarmClient>
    <General>
        <ResultsFolder>SwarmResults</ResultsFolder>
    </General>

    <Logger>
        <File WriteToFile="true" LogsDirectory="SwarmLogs" LogfilePrefix="SwarmClient" MaximumLogSizeKB="4120"></File>
        <LoggingChannels>
            <!--
                public enum LoggingLevel {
                    NONE = 0,
                    ERROR = 1,
                    WARNING = 2,
                    INFO = 3,
                    DEBUG = 4,
                    TRACE = 4
                }
            -->
            <Channel LogLevel="4">LoopedTaskExecutor</Channel>
            <Channel LogLevel="4">OneShotTaskExecutor</Channel>
            <Channel LogLevel="4">ResultsListener</Channel>
        </LoggingChannels>
    </Logger>
</TheSwarmClient>
```
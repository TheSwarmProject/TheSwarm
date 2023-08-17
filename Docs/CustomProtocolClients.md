# Custom protocol clients
The Swarm does not limit user to just the set of bundled clients - if some protocol is not covered, it can be implemented manually.

## Create custom protocol client
Custom protocol client is created by inheriting **SwarmClient** class and implementing custom logic.
Base class will already provide us with required boilerplate (task executor container, transaction executors, semaphore and measuring utilities)

In this particular example - we'll create a simple implementation of raw Socket client.
```cs
using System.Text;
using System.Net;
using System.Net.Sockets;
using TheSwarm.Common;
using TheSwarm.Extendables;

public class SocketClient : SwarmClient
{
    public Socket? client {get; private set;}

    public SocketClient() {}

    /// <summary>
    /// Initializer method - initializes the socket object and opens the connection
    /// </summary>
    /// <param name="address">Server address</param>
    /// <param name="port">Server port</param>
    public void InitializeSocket(string address, int port) {
        IPAddress ipAddress = IPAddress.Parse(address);
        IPEndPoint iPEndPoint = new IPEndPoint(ipAddress, port); 

        client = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        client.Connect(iPEndPoint);  
    }

    public Response Send(string name, byte[] data) => SendData(name, data);
    public Response Send(string name, string data) => SendData(name, Encoding.UTF8.GetBytes(data));

    /// <summary>
    /// Common sender for both methods
    /// </summary>
    /// <param name="name">Request name - needed for report</param>
    /// <param name="data">Byte array to transmit</param>
    /// <returns>Response object</returns>
    private Response SendData(string name, byte[] data)
    {
        int responseTime = 0;
        Response res;
        try
        {
            responseTime = MeasureExecutionTime( () => client.Send(data) );
            res = new Response(name, "SEND", responseTime, null, null, false);
        }
        catch (Exception e)
        {
            res = new Response(name, "SEND", responseTime, null, null, false, true, e.ToString());
        }
        
        // Once processed - we log the call to results listener
        TaskExecutor.LogEntry(res);
        return res;
    }

    /// <summary>
    /// Finisher - disposes of connection and closes the socket
    /// </summary>
    public void Close() 
    {
        client.Shutdown(SocketShutdown.Both);
        client.Close();
    }
}
```
**NOTE:** Use of **Response** type as return values is not mandatory, but it is required to pass the results to ResultsListener via **TaskExecutor.LogEntry(Response)** method.

## Custom protocol client usage
Now that we've created our custom client - we can freely use it in our tasksets.
It follows the same rule as any other client - property must be marked with **RegisterSwarmClient** annotation. Otherwise, it won't be reporting the data to the results listener
```cs
[SwarmTaskSet(TaskSetID = "TC-4", Description = "Test taskset with custom socket client")]
public class CustomClass
{
    [RegisterSwarmClient]
    public SocketClient client {get; } = new SocketClient();

    [SwarmTaskSetSetup]
    public void SetUp() => client.InitializeSocket("127.0.0.1", 11000);
    
    [SwarmTask]
    public void SendData() => client.Send("Send data msg", "Data<ACK>");

    [SwarmTaskSetTeardown]
    public void Teardown() => client.Close();
}
```
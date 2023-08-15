using System.Text;

namespace TheSwarmClient.Common;

/// <summary>
/// Object, used to provide ResultsListener with information on request's execution.
/// We use separate type to encapsulate all required fields into it, instead of providing a bunch of scatterred values.
/// </summary>
public class Response
{
    public string   Name                { get; }
    public string   Method              { get; }
    public int      ResponseTimeMs      { get; }
    /// <summary>
    /// This property contains the client return object. Since types might vary from client to client, we leave it a plain object for now.
    /// TODO: I think it might straightened out by using generics. Get back to it and look for more elegant solution.
    /// </summary>
    public object   ResponseObject      { get; }
    public byte[]   ByteContent         { get; }
    public string   StringContent       { get; } = "";
    public long     ContentLengthBytes  { get; }
    public bool     IsFailed            { get; }
    public string   FailureMessage      { get; }

    public Response(string name, string method, int responseTimeMs, object responseObject, byte[] byteContent, bool isString = true, bool isFailed = false, string failureMessage = "")
    {
        Name            = name;
        Method          = method;
        ResponseTimeMs  = responseTimeMs;
        ResponseObject  = responseObject;
        ByteContent     = byteContent;
        if (isString && ByteContent is not null)
            StringContent = Encoding.UTF8.GetString(ByteContent, 0, ByteContent.Length);
        if (ByteContent is not null)
            ContentLengthBytes = byteContent.Length;
        IsFailed        = isFailed;
        FailureMessage  = failureMessage;
    }
}
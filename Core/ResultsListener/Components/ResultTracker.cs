namespace TheSwarm.Components;

/// <summary>
/// Type, used to store current results for particular request. It is created once during test lifetime and it used as value in ResultsListener dict
/// </summary>
public class ResultTracker
{
    internal string                     name                        { get; }
    internal string                     method                      { get; }
    internal int                        failuresCount               { get; set; } = 0;
    internal Dictionary<String, int>    failuresOccurrences         { get; } = new Dictionary<string, int>();
    internal double                     failRate                    { get; set; } = 0;
    internal int                        requestsCount               { get; set; } = 0;
    /*
        requestsCount is a counter, reqCount is a container - it's value is used to print value in the console.
        TODO: It's hacky as hell - gonna need some better way of preserving it.
    */
    internal int                        reqCount                    { get; set; } = 0;
    internal long                       totalResponseTime           { get; set; } = 0;
    internal long                       totalFailedResponseTime     { get; set; } = 0;
    internal long                       averageResponseTime         { get; set; } = 0;
    internal long                       averageFailedResponseTime   { get; set; } = 0;
    internal long                       minResponseTime             { get; set; } = 0;
    internal long                       maxResponseTime             { get; set; } = 0;
    internal double                     requestsPerSecond           { get; set; } = 0;
    internal double                     averageRequestsPerSecond    { get; set; } = 0;
    internal long                       averageContentLength        { get; set; } = 0;
    internal long                       totalContentLength          { get; set; } = 0;


    internal DateTime                   lastUpdateTick              { get; set; }

    public Results                      results                     { get; set; }

    public ResultTracker(string name, string method)
    {
        this.name = name;
        this.method = method;

        lastUpdateTick = DateTime.Now;

        results = new Results(name, method);
    }

    /// <summary>
    /// Logs executed request's results. 
    /// NOTE: this method also logs failed requests. Failure details and counting is done by logFailure method, 
    /// that should be called AFTER this one
    /// </summary>
    /// <param name="responseTime">Response time in milliseconds</param>
    /// <param name="contentLength">Response size in bytes</param>
    public void LogEntry(int responseTimeMs, long contentLengthBytes)
    {
        requestsCount += 1;
        totalResponseTime += responseTimeMs;
        totalContentLength += contentLengthBytes;
        if (minResponseTime == 0)
            minResponseTime = responseTimeMs;
        else if (minResponseTime > responseTimeMs)
            minResponseTime = responseTimeMs;
        if (maxResponseTime < responseTimeMs)
            maxResponseTime = responseTimeMs;
    }

    /// <summary>
    /// Logs occured failure.
    /// NOTE: It only registers an occurrence of the failure, not it's response time - this is done by calling logEntry method
    /// prior to calling this one
    /// </summary>
    /// <param name="failureMessage">Failure message</param>
    public void LogFailure(string failureMessage)
    {
        failuresCount += 1;
        if (failuresOccurrences.ContainsKey(failureMessage))
            failuresOccurrences[failureMessage] += 1;
        else
            if (failureMessage is null)
            failuresOccurrences["null"] = 1;
        else
            failuresOccurrences[failureMessage] = 1;
    }

    /// <summary>
    /// Kicks-off averages calculation, triggers ResultEntry creation in internal results object and then optionally resets the data.
    /// NOTE: To make sure we don't get any incoming values in between, this object should be locked before running this method.
    /// Otherwise, quality of numbers is not guaranteed.
    /// </summary>
    /// <param name="timestamp">Timestamp to apply to result entry</param>
    public void GenerateResultEntry(DateTime timestamp)
    {
        CalculateAverages();
        results.GenerateAndAddResultEntry(timestamp, this);
    }

    // ###########################################################################################
    // Service methods
    // ###########################################################################################

    private void CalculateAverages()
    {
        // TODO: This section is pretty-much copy-pasted from Howitzer framework. It needs to be re-worked for better readability
        reqCount = requestsCount;
        try
        {
            averageResponseTime = totalResponseTime / requestsCount;
        }
        catch (DivideByZeroException)
        {
            averageResponseTime = 0;
        }
        try
        {
            averageFailedResponseTime = totalFailedResponseTime / failuresCount;
        }
        catch (DivideByZeroException)
        {
            averageFailedResponseTime = 0;
        }
        try
        {
            // TODO: Decompose this calculation. The level of inception here is off the charts =\
            averageRequestsPerSecond = (requestsCount / Math.Round((double)((((DateTimeOffset)DateTime.Now).ToUnixTimeMilliseconds() - ((DateTimeOffset)lastUpdateTick).ToUnixTimeMilliseconds()) / 1000), 2));
            // Usually happens on first tick. Just put requests count in there, since tick is already 1 second
            if (Double.IsInfinity(averageRequestsPerSecond))
                averageRequestsPerSecond = requestsCount;
        }
        catch (DivideByZeroException)
        {
            averageRequestsPerSecond = 0;
        }
        try
        {
            if (totalContentLength != 0 && requestsCount != 0)
            {
                averageContentLength = totalContentLength / requestsCount;
            }
            else
            {
                averageContentLength = 0;
            }
        }
        catch (DivideByZeroException)
        {
            averageContentLength = 0;
        }
        try
        {
            if (requestsCount != 0 && failuresCount != 0)
            {
                failRate = (requestsCount / failuresCount) * 100;
            }
        }
        catch (DivideByZeroException)
        {
            failRate = 0;
        }
    }

    public void Reset()
    {
        requestsCount = 0; failuresCount = 0;
        totalResponseTime = 0; totalFailedResponseTime = 0;
        requestsPerSecond = 0; totalContentLength = 0; failRate = 0;

        foreach (string key in failuresOccurrences.Keys) { failuresOccurrences[key] = 0; }

        lastUpdateTick = DateTime.Now; ;
    }
}


/// <summary>
/// Container for result entries - later used for serialization
/// </summary>
public class Results
{
    /// <summary>
    /// Current data structure used to pass the results to a reporter uses header-value approach
    /// {
    ///     Data: {
    ///         Headers: <headers dict>    
    ///     },
    ///     Values: [
    ///         [array with values]    
    ///     ]
    /// }
    /// Since the header is the same for every result set, we keep it static and use it as a basis for filling up the values array
    /// </summary>
    public static Dictionary<string, int> DataHeaders = new Dictionary<string, int>()
    {
        ["timestamp"] = 0,
        ["failuresCount"] = 1,
        ["failuresOccurrences"] = 2,
        ["failRate"] = 3,
        ["requestsCount"] = 4,
        ["averageResponseTime"] = 5,
        ["averageFailedResponseTime"] = 6,
        ["minResponseTime"] = 7,
        ["maxResponseTime"] = 8,
        ["averageRequestsPerSecond"] = 9,
        ["averageContentLength"] = 10,
        ["totalContentLength"] = 11
    };
    private string name { get; set; }
    private string method { get; set; }
    /// <summary>
    /// Since all result entries are anonymous objects - we store them in a simple list
    /// </summary>
    public List<object[]> ResultsList { get; init; } = new List<object[]>();

    public Results(string name, string method)
    {
        this.name = name;
        this.method = method;
    }

    /// <summary>
    /// Main workhorse method - uses provided ResultTracker as data source to generate a values entry,
    /// and then appends it into results list
    /// </summary>
    /// <param name="timestamp">DateTime timestamp. For uniformity purposes - it is generated by ResultsListener before
    /// iterating over all result trackers, and then fed as parameter to each of those.</param>
    /// <param name="tracker">ResultTracker instance to take values from</param>
    internal void GenerateAndAddResultEntry(DateTime timestamp, ResultTracker tracker)
    {
        ResultsList.Add(new object[] {
            timestamp,
            tracker.failuresCount,
            new Dictionary<string, int>(tracker.failuresOccurrences),
            tracker.failRate,
            tracker.requestsCount,
            tracker.averageResponseTime,
            tracker.averageFailedResponseTime,
            tracker.minResponseTime,
            tracker.maxResponseTime,
            tracker.averageRequestsPerSecond,
            tracker.averageContentLength,
            tracker.totalContentLength
        });
    }
}


/// <summary>
/// Container object - once every tick, we get the average values, bind them to particular timestamp and save resulting
/// object into ResultListener's list.
/// 
/// TODO: Not used anymore. Do we need to keep it around?
/// </summary>
public class ResultEntry
{
    public int                      failuresCount               { get; set; } = 0;
    public Dictionary<String, int>  failuresOccurrences         { get; }
    public double                   failRate                    { get; set; } = 0;
    public int                      requestsCount               { get; set; } = 0;
    public long                     averageResponseTime         { get; set; } = 0;
    public long                     averageFailedResponseTime   { get; set; } = 0;
    public long                     minResponseTime             { get; set; } = 0;
    public long                     maxResponseTime             { get; set; } = 0;
    public double                   averageRequestsPerSecond    { get; set; } = 0;
    public long                     averageContentLength        { get; set; } = 0;
    public long                     totalContentLength          { get; set; } = 0;

    public ResultEntry(ResultTracker tracker)
    {
        this.failuresCount              = tracker.failuresCount;
        this.failuresOccurrences        = new Dictionary<string, int>(tracker.failuresOccurrences);
        this.failRate                   = tracker.failRate;
        this.requestsCount              = tracker.requestsCount;
        this.averageResponseTime        = tracker.averageResponseTime;
        this.averageFailedResponseTime  = tracker.averageFailedResponseTime;
        this.minResponseTime            = tracker.minResponseTime;
        this.maxResponseTime            = tracker.maxResponseTime;
        this.averageRequestsPerSecond   = tracker.averageRequestsPerSecond;
        this.averageContentLength       = tracker.averageContentLength;
        this.totalContentLength         = tracker.totalContentLength;
    }
}
using System.Collections.Generic;

public class SignalManager
{
    private static SignalManager instance;
    public static SignalManager Instance => instance ??= new SignalManager();

    private Dictionary<string, object> signals = new();

    public ResponseSignal<TRequest, TResponse> GetResponseSignal<TRequest, TResponse>(string key)
    {
        if (!signals.ContainsKey(key))
        {
            signals[key] = new ResponseSignal<TRequest, TResponse>();
        }
        return (ResponseSignal<TRequest, TResponse>)signals[key];
    }
    
    public Signal<TRequest> GetSignal<TRequest>(string key)
    {
        if (!signals.ContainsKey(key))
        {
            signals[key] = new Signal<TRequest>();
        }
        return (Signal<TRequest>)signals[key];
    }
}

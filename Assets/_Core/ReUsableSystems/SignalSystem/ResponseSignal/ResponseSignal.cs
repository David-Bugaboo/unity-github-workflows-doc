using System;

public class ResponseSignal<TRequest, TResponse>
{
    private event Action<TRequest, Action<TResponse>> listeners;

    public void AddListener(Action<TRequest, Action<TResponse>> listener)
    {
        listeners += listener;
    }

    public void RemoveListener(Action<TRequest, Action<TResponse>> listener)
    {
        listeners -= listener;
    }

    public void Dispatch(TRequest request, Action<TResponse> callback)
    {
        if (listeners != null)
        {
            foreach (var @delegate in listeners.GetInvocationList())
            {
                var listener = (Action<TRequest, Action<TResponse>>)@delegate;
                listener.Invoke(request, callback);
            }
        }
    }
}

public class Signal<TRequest>
{
    private event Action<TRequest> listeners;
    
    public void AddListener(Action<TRequest> listener)
    {
        listeners += listener;
    }

    public void RemoveListener(Action<TRequest> listener)
    {
        listeners -= listener;
    }

    public void Dispatch(TRequest request)
    {
        if (listeners != null)
        {
            foreach (var @delegate in listeners.GetInvocationList())
            {
                var listener = (Action<TRequest>)@delegate;
                listener.Invoke(request);
            }
        }
    }
}

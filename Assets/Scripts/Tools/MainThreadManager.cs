using System;
using System.Threading;

public class MainThreadManager
{
    public static MainThreadManager Instance
    {
        get { return _instance; }
    }
    private static MainThreadManager _instance;
    static MainThreadManager()
    {
        _instance = new MainThreadManager();
    }
    private SynchronizationContext m_mainThreadContext = null;

    public void Init()
    {
        m_mainThreadContext = SynchronizationContext.Current;
    }

    public void RunOnMainThread(Action runOnMain)
    {
        m_mainThreadContext.Send(new SendOrPostCallback((needless) => {
            runOnMain?.Invoke();
        }), null);
    }
}

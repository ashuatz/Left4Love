using UnityEngine;
using System.Collections;

public static class Logger
{
    public static void Log(LogType logType, string name, object message)
    {
#if (UNITY_EDITOR || DEVELOPMENT_BUILD) && ENABLE_LOG
        switch (logType)
        {
            case LogType.Log:
                Debug.LogFormat("[{0}] {1}", name, message);
                break;
            case LogType.Assert:
                Debug.LogAssertionFormat("[{0}] {1}", name, message);
                break;
            case LogType.Warning:
                Debug.LogWarningFormat("[{0}] {1}", name, message);
                break;
            case LogType.Error:
            case LogType.Exception:
                Debug.LogErrorFormat("[{0}] {1}", name, message);
                break;
            default:
                Debug.LogFormat("[{0}] {1}", name, message);
                break;
        }
#endif

    }

    public static void LogFormat(LogType logType, string name, string format, params object[] args)
    {
        Log(logType, name, string.Format(format, args));
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DebugHelper
{
    /// <summary>
    /// Logs a message to the console.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public static void Log(string message)
    {
        Debug.Log(message);
    }

    /// <summary>
    /// Logs an error message to the console.
    /// </summary>
    /// <param name="message">The error message to log.</param>
    public static void LogError(string message)
    {
        //DebugHelper.LogError(message);
    }

    /// <summary>
    /// Logs a warning message to the console.
    /// </summary>
    public static void LogWarning(string message)
    {
       // DebugHelper.LogWarning(message);
    }

    /// <summary>
    /// Logs a formatted message to the console.
    /// </summary>
    /// <param name="format">The format string.</param>
    /// <param name="args">The arguments to format the string with.</param>
    public static void LogFormat(string format, params object[] args)
    {
        //Debug.LogFormat(format, args);
    }
}

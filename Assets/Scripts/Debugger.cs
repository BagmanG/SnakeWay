using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class Debugger : MonoBehaviour
{
    public static Debugger Instance;
    private StringBuilder log = new StringBuilder();
    public bool enableLogging = true;
    private string logFilePath;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            logFilePath = Path.Combine(Application.persistentDataPath, "debug_log.txt");
            File.WriteAllText(logFilePath, "=== Debug Log Started ===\n");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Log(string message)
    {
        if (!enableLogging) return;

        string logEntry = $"[{Time.time:F2}] {message}";
        Debug.Log(logEntry);
        log.AppendLine(logEntry);
        File.AppendAllText(logFilePath, logEntry + "\n");
    }

    public void Clear()
    {
        log.Clear();
        File.WriteAllText(logFilePath, "=== Debug Log Cleared ===\n");
    }
}

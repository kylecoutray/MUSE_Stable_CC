using System;
using System.IO;
using System.IO.Ports;
using UnityEngine;
using System.Collections.Generic;


/// <summary>
/// Sends TTL event codes via serial to Arduino for event marking.
/// All event codes are exponentiated: SendTTL(3) sends 2^(3-1) = 4.
/// Logs all events to a file as well as the Unity console.
/// </summary>
public class ArduinoTTLManager : MonoBehaviour
{
    public string portName = "COM3";
    public int baudRate = 115200;
    public bool testMode = false;
    private SerialPort serialPort;
    private static ArduinoTTLManager instance;

    private static readonly string[] eventLabels = {
        "TrialOn",           // 1 TTL
        "SampleOn",           // 2 TTL
        "SampleOff",           // 3 
        "DistractorOn",          // 4 TTL 
        "DistractorOff",          // 5
        "TargetOn",         // 6 TTL 
        "Choice",     // 7 TTL 
        "StartEndBlock", // 8 TTL
        "Success",    // 9 LOG ONLY. Will not work with Arduino script.
        "Fail",         // 10 LOG ONLY. Will not work with Arduino script.
        "AudioPlaying", //11 LOG ONLY 
    };

    private static readonly HashSet<string> ttlEnabledEvents = new HashSet<string> //only the events that TTL will send for
    {
        "TrialOn", "SampleOn", "DistractorOn", "TargetOn", "Choice", "StartEndBlock"
    };

    private string logFilePath;

    void Awake()
    {

        // Prevent duplicate instances
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);  // Keeps it alive


        Debug.Log("[TTL] Awake called by: " + gameObject.name);

        // Start from the directory where this script runs (Assets/_USE_Tasks/WorkingMemory)
        string scriptPath = Application.dataPath;  // This gives you: .../USE_CORE/Assets

        // Go up four levels: Assets → USE_CORE → stableMUSE
        string stableMusePath = Directory.GetParent(scriptPath)  // Assets
                                        .Parent                 // USE_CORE                
                                        .FullName;             // stableMUSE

        // Create LOGS path inside stableMUSE
        string logsFolder = Path.Combine(stableMusePath, "LOGS");
        Directory.CreateDirectory(logsFolder); // Ensure it exists

        // Create log file with timestamp
        logFilePath = Path.Combine(logsFolder, $"TTL_log_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
       
        // Nice log file header :)
        LogToFile("==== TTL LOG HEADER ====");
        LogToFile($"Port: {portName}");
        LogToFile($"Start Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
        LogToFile($"Test Mode: {testMode}");
        LogToFile("Columns: Timestamp | Status | Event | Byte Sent");
        LogToFile("=========================");

        Debug.Log("[TTL] Target folder: " + logsFolder);

        // Log first entry
        LogToFile($"[TTL][INIT] Log started at {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
        
        if (testMode)
        {
            LogToBoth("TTL Manager in TEST MODE (no serial port will be opened).");
            return;
        }


        

        serialPort = new SerialPort(portName, baudRate);
        try
        {
            serialPort.Open();
            LogToBoth($"Serial port {portName} opened for TTL output.");
        }
        catch (Exception e)
        {
            LogToBoth($"Failed to open serial port: {e}", isError: true);
        }
        
        


    }

    /// <summary>
    /// Sends a TTL pulse for the given event code (1-indexed).
    /// Logs all attempts with context.
    /// </summary>
    public void SendTTL(int eventCode)
    {
        int codeToSend = 1 << (eventCode - 1); // 2^(eventCode-1)
        string label = (eventCode >= 1 && eventCode <= eventLabels.Length)
            ? eventLabels[eventCode - 1]
            : $"Event{eventCode}";

        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

        string status;

        if (ttlEnabledEvents.Contains(label))
        {

            if (testMode)
            {
                status = "TEST_ONLY";
            }

            else if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.Write(new byte[] { (byte)codeToSend }, 0, 1);
                status = "SENT";
            }

            else
            {
                status = "FAILED";
            }

        }

        else
        {
            status = "LOG_ONLY";
        }

        string logLine;
        if (status == "SENT")
        {
            logLine = $"{timestamp} | {status} | event={label} | byte={codeToSend}";
        }
        else
        {
            logLine = $"{timestamp} | {status} | event={label} | No Byte Sent";
        }

        LogToBoth(logLine, isError: status == "FAILED");
    }


    void OnApplicationQuit()
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.Close();
            LogToFile($"[TTL][SHUTDOWN] Serial port {portName} closed at {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
        }
    }

    // --- Logging Helpers ---

    private void LogToBoth(string message, bool isError = false)
    {
        if (isError)
            Debug.LogWarning(message);
        else
            Debug.Log(message);
        LogToFile(message);
    }

    private void LogToFile(string message)
    {
        try
        {
            File.AppendAllText(logFilePath, message + Environment.NewLine);
        }
        catch (Exception e)
        {
            Debug.LogError($"[TTL][LOG ERROR] Could not write to log file: {e}");
        }
    }
}
using System;
using System.IO.Ports;
using System.Threading;
using UnityEngine;

public class ArduinoInput : MonoBehaviour
{
    [Header("Serial Settings")]
    public string portName = "COM3";
    public int baudRate = 9600;

    [Header("Sensor Data")]
    public int force0, force1, force2, force3;
    public bool buttonPressed;

    [HideInInspector] public bool force0Held, force1Held, force2Held, force3Held;

    private SerialPort serialPort;
    private Thread readThread;
    private bool isRunning;
    private string latestMessage;

    void Start()
    {
        try
        {
            serialPort = new SerialPort(portName, baudRate);
            serialPort.ReadTimeout = 50;
            serialPort.Open();

            isRunning = true;
            readThread = new Thread(ReadFromPort);
            readThread.Start();

            Debug.Log($"✅ Connected to {portName}");
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Could not open {portName}: {e.Message}");
        }
    }

    private void ReadFromPort()
    {
        while (isRunning && serialPort.IsOpen)
        {
            try
            {
                string message = serialPort.ReadLine().Trim();
                lock (this)
                {
                    latestMessage = message;
                }
            }
            catch (TimeoutException) { }
        }
    }

    void Update()
    {
        string message = null;
        lock (this)
        {
            if (latestMessage != null)
            {
                message = latestMessage;
                latestMessage = null;
            }
        }

        if (message != null)
        {
            ParseMessage(message);
        }
    }

    private void ParseMessage(string message)
    {
        if (message.StartsWith("BTN1_PRESSED"))
        {
            buttonPressed = true;
            Debug.Log("🔘 Button pressed!");
        }
        else if (message.StartsWith("FORCE0_"))
        {
            if (int.TryParse(message.Substring(7), out int val))
            {
                force0 = val;
                force0Held = val > 500;
            }
        }
        else if (message.StartsWith("FORCE1_"))
        {
            if (int.TryParse(message.Substring(7), out int val))
            {
                force1 = val;
                force1Held = val > 500;
            }
        }
        else if (message.StartsWith("FORCE2_"))
        {
            if (int.TryParse(message.Substring(7), out int val))
            {
                force2 = val;
                force2Held = val > 500;
            }
        }
        else if (message.StartsWith("FORCE3_"))
        {
            if (int.TryParse(message.Substring(7), out int val))
            {
                force3 = val;
                force3Held = val > 500;
            }
        }
    }

    private void OnApplicationQuit()
    {
        isRunning = false;

        if (readThread != null && readThread.IsAlive)
            readThread.Join();

        if (serialPort != null && serialPort.IsOpen)
            serialPort.Close();
    }
}

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
    public int force0;
    public int force1;
    public bool buttonPressed;

    private SerialPort serialPort;
    private Thread readThread;
    private bool isRunning;
    private string latestMessage;

    [HideInInspector] public bool force0Held = false;
    [HideInInspector] public bool force1Held = false;


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
            string val = message.Substring(7);
            if (int.TryParse(val, out int f0))
            {
                force0 = f0;
                force0Held = f0 > 500; // same threshold as InputManager
            }
        }
        else if (message.StartsWith("FORCE1_"))
        {
            string val = message.Substring(7);
            if (int.TryParse(val, out int f1))
            {
                force1 = f1;
                force1Held = f1 > 500;
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

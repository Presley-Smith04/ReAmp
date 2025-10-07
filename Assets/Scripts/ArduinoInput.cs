using System;
using System.IO.Ports;
using System.Threading;
using UnityEngine;

public class ArduinoInput : MonoBehaviour
{
    public string portName = "COM3"; // <-- CHANGE THIS to match your Arduino's port
    public int baudRate = 9600;

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

            Debug.Log("✅ Serial port connected on " + portName);
        }
        catch (Exception e)
        {
            Debug.LogError("❌ Error opening port: " + e.Message);
        }
    }

    private void ReadFromPort()
    {
        while (isRunning && serialPort.IsOpen)
        {
            try
            {
                string message = serialPort.ReadLine();
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
            Debug.Log("Arduino: " + message);
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

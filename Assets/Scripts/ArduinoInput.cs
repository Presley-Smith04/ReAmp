using System;
using System.IO.Ports;
using System.Threading;
using UnityEngine;

public class ArduinoInput : MonoBehaviour
{
    [Header("Serial Settings")]
    public string portName = "COM3";
    public int baudRate = 9600;

    [Header("Button States")]
    public bool button1Pressed;
    public bool button2Pressed;
    public bool button1Held;
    public bool button2Held;

    private SerialPort serialPort;
    private Thread readThread;
    private bool isRunning;
    private string latestMessage;

    // Hold tracking
    private float button1HoldTime = 0f;
    private float button2HoldTime = 0f;
    public float holdThreshold = 0.1f;

    // Previous frame
    private bool prevButton1 = false;
    private bool prevButton2 = false;

    void Start()
    {
        try
        {
            //connect to port and output 
            serialPort = new SerialPort(portName, baudRate);
            serialPort.ReadTimeout = 50;
            serialPort.Open();

            isRunning = true;
            readThread = new Thread(ReadFromPort);
            readThread.Start();

            Debug.Log($"Connected to {portName}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Could not open {portName}: {e.Message}");
        }
    }

    private void ReadFromPort()
    {
        //read from the port
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

        //hold detection
        button1HoldTime = button1Pressed ? button1HoldTime + Time.deltaTime : 0f;
        button2HoldTime = button2Pressed ? button2HoldTime + Time.deltaTime : 0f;

        button1Held = button1HoldTime >= holdThreshold;
        button2Held = button2HoldTime >= holdThreshold;

        //reset pressed for one frame events
        button1Pressed = !prevButton1 && button1Held;
        button2Pressed = !prevButton2 && button2Held;

        prevButton1 = button1Held;
        prevButton2 = button2Held;
    }

    private void ParseMessage(string message)
    {
        //expecting Arduino to send: BTN1,BTN2 (0=pressed, 1=released)
        string[] vals = message.Split(',');
        if (vals.Length >= 2)
        {
            if (int.TryParse(vals[0], out int b1))
                button1Pressed = b1 == 0;

            if (int.TryParse(vals[1], out int b2))
                button2Pressed = b2 == 0;
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

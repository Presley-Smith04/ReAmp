using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System;

public class ArduinoInput : MonoBehaviour
{
    SerialPort serial;
    public string portName = "COM3"; // Change this to match your Arduino port
    public int baudRate = 9600;

    void Start()
    {
        try
        {
            serial = new SerialPort(portName, baudRate);
            serial.Open();
            serial.ReadTimeout = 50;
            Debug.Log("Serial port opened successfully.");
        }
        catch (Exception e)
        {
            Debug.LogError("Error opening serial port: " + e.Message);
        }
    }

    void Update()
    {
        if (serial != null && serial.IsOpen)
        {
            try
            {
                string line = serial.ReadLine();
                line = line.Trim();
                HandleSerialInput(line);
            }
            catch (TimeoutException)
            {
                // ignore timeout
            }
        }
    }

    void HandleSerialInput(string input)
    {
        // Example inputs: "BTN1_PRESSED", "FORCE0_512", "FORCE1_800"
        if (input == "BTN1_PRESSED")
        {
            Debug.Log("Button pressed (Arduino)!");
            // Call your existing input logic
            FindObjectOfType<InputManager>().CheckHit(Direction.UpLeft); // Example: map button 1 to a lane
        }
        else if (input.StartsWith("FORCE"))
        {
            // parse force value
            string[] parts = input.Split('_');
            if (parts.Length == 2 && int.TryParse(parts[1], out int val))
            {
                int sensorIndex = int.Parse(parts[0].Substring(5)); // e.g., FORCE0 -> 0
                Debug.Log($"Force sensor {sensorIndex} value: {val}");

                // If pressed above threshold, send note hit
                if (val > 300) // tweak threshold
                {
                    switch (sensorIndex)
                    {
                        case 0: FindObjectOfType<InputManager>().CheckHit(Direction.DownLeft); break;
                        case 1: FindObjectOfType<InputManager>().CheckHit(Direction.DownRight); break;
                    }
                }
            }
        }
    }

    void OnApplicationQuit()
    {
        if (serial != null && serial.IsOpen)
            serial.Close();
    }
}
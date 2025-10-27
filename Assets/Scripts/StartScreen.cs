using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO.Ports;
using TMPro;

public class StartScreen : MonoBehaviour
{
    [Header("Serial Settings")]
    public string portName = "COM3";
    public int baudRate = 9600;

    [Header("UI Elements")]
    public Slider chargeBar;
    public Image lightningImg;
    public TextMeshProUGUI statusText;
    public Image flashOverlay;


    [Header("Charging Settings")]
    public float chargeSpeed = 0.01f;
    public float dischargeSpeed = 0.005f;
    public float chargeValue = 0f;
    public float chargeThreshold = 1f;
    public float touchThreshold = 100f;

    private SerialPort serialPort;
    private bool readyToStart = false;
    private bool fullyCharged = false;
    private Color baseLightningColor;
    private float glowPulse = 0f;


    private void Start()
    {
        serialPort = new SerialPort(portName, baudRate);
        try
        {
            serialPort.Open();
            serialPort.ReadTimeout = 100;
        }
        catch
        {
            Debug.LogWarning("couldnt open port or something ");
        }
        baseLightningColor = lightningImg.color;
        flashOverlay.color = new Color(1, 1, 1, 0);
        SetStatus("START CHARGING", Color.cyan);
    }

    void Update()
    {
        if (serialPort.IsOpen)
        {
            try
            {
                string data = serialPort.ReadLine();
                string[] values = data.Split(',');

                if (values.Length == 3)
                {
                    float touch = float.Parse(values[0]);
                    int button1 = int.Parse(values[1]);
                    int button2 = int.Parse(values[2]);

                    bool isTouching = touch > touchThreshold;


                    //capacative sensor
                    if (isTouching)
                        chargeValue += Time.deltaTime * chargeSpeed;
                    else
                        chargeValue -= Time.deltaTime * dischargeSpeed;

                    chargeValue = Mathf.Clamp01(chargeValue);
                    chargeBar.value = chargeValue;


                    //bar effects
                    glowPulse += Time.deltaTime * 4f;
                    float glow = Mathf.Abs(Mathf.Sin(glowPulse));
                    lightningImg.color = Color.Lerp(baseLightningColor, Color.white, glow * 0.6f);


                    //handle states
                    if (!fullyCharged)
                    {
                        if (isTouching && chargeValue < chargeThreshold)
                            SetStatus("CHARGING...", Color.yellow);
                        else if (chargeValue <= 0.01f)
                            SetStatus("START CHARGING", Color.cyan);
                    }

                    //charging finished
                    if (chargeValue >= chargeThreshold && !fullyCharged)
                    {
                        fullyCharged = true;
                        readyToStart = true;
                        StartCoroutine(FlashEffect());
                        SetStatus("CLEAR!", Color.green);
                    }

                    //start game when charged and both buttons pressed
                    if (readyToStart && button1 == 0 && button2 == 0)
                        LoadGameScene();
                }
            }
            catch (System.Exception)
            {
                //ignore read timeout
            }
        }
    }

    void SetStatus(string message, Color color)
    {
        //changes text and status
        if (statusText.text != message)
        {
            statusText.text = message;
            statusText.color = color;
            statusText.transform.localScale = Vector3.one * 1.1f;
            //Lean Tween is a unity animation package
            LeanTween.scale(statusText.gameObject, Vector3.one, 0.3f).setEaseOutBack();
        }
    }

    System.Collections.IEnumerator FlashEffect()
    {
        //flashing effect
        flashOverlay.color = new Color(1, 1, 1, 0.8f);
        yield return new WaitForSeconds(0.1f);
        flashOverlay.color = new Color(1, 1, 1, 0f);
    }

    void LoadGameScene()
    {
        serialPort.Close();
        SceneManager.LoadScene("GameScene");
    }


    void OnApplicationQuit()
    {
        if (serialPort.IsOpen)
            serialPort.Close();
    }
}
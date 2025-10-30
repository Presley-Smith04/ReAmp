using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Text scoreText;
    public Text comboText;
    public Text timingText;

    void Update()
    {
        scoreText.text = "Score: " + GameManager.Instance.score;
        comboText.text = "Combo: " + GameManager.Instance.combo;
        if (
            GameManager.Instance.timing == "Perfect"
            || GameManager.Instance.timing == "Perfect Hold"
        )
        {
            Color perfectColor = new Color(1f, 0.9f, 0.1f);
            timingText.color = perfectColor;
        }
        else if (GameManager.Instance.timing == "Miss")
        {
            Color missColor = new Color(1f, 0.2f, 0.1f);
            timingText.color = missColor;
        }
        else
        {
            Color baseColor = new Color(0f, 0f, 0f);
            timingText.color = baseColor;
        }
        timingText.text = GameManager.Instance.timing;
    }
}

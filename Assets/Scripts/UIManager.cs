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
        timingText.text = GameManager.Instance.timing;
    }
}

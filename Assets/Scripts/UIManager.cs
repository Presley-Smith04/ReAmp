using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public TMP_Text scoreText;
    public TMP_Text comboText;
    public TMP_Text timingText;

    void Update()
    {
        scoreText.text = "Score: " + GameManager.Instance.score;
        comboText.text = "Combo: " + GameManager.Instance.combo;
        timingText.text = GameManager.Instance.timing;
    }
}

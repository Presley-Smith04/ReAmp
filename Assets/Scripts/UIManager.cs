using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public TMP_Text scoreText;
    public TMP_Text comboText;

    void Update()
    {
        scoreText.text = "Score: " + GameManager.Instance.score;
        comboText.text = "Combo: " + GameManager.Instance.combo;
    }
}

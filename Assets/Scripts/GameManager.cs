using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // singleton for easy access
    public int score = 0;
    public int combo = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddScore(string result)
    {
        switch(result)
        {
            case "Perfect":
                score += 300;
                combo++;
                break;
            case "Good":
                score += 200;
                combo++;
                break;
            case "Bad":
                score += 100;
                combo++;
                break;
            case "Miss":
                combo = 0;
                break;
        }

        Debug.Log($"Result: {result} | Score: {score} | Combo: {combo}");
    }
}

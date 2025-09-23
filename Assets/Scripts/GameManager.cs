using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // singleton for easy access
    public int score = 0;
    public int combo = 0;
    public string timing = "";

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddScore(string result)
    {
        switch (result)
        {
            case "Perfect":
                score += 100 + (10 * combo);
                combo++;
                timing = "Perfect";
                break;
            case "Good":
                score += 50 + (5 * combo);
                combo++;
                timing = "Good";
                break;
            case "Bad":
                score += 25 + combo;
                combo++;
                timing = "Bad";
                break;
            case "Miss":
                combo = 0;
                timing = "Miss";
                break;
            case "Clear":
                combo++;
                break;
        }

        Debug.Log($"Result: {result} | Score: {score} | Combo: {combo}");
    }
}

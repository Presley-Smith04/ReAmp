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

    // Existing tap note scoring
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

    // NEW: Hold note scoring
    public void AddHoldScore(float heldTime, float fullDuration)
    {
        float ratio = Mathf.Clamp01(heldTime / fullDuration);
        int points = Mathf.RoundToInt(300 * ratio); // max 300 points for perfect hold

        score += points;

        // Add combo ticks every 0.5 seconds
        int comboTicks = Mathf.FloorToInt(heldTime / 0.5f);
        combo += comboTicks;

        timing = ratio >= 0.95f ? "Perfect Hold" :
                 ratio >= 0.7f ? "Good Hold" : "Bad Hold";

        Debug.Log($"Hold Score: {points} | Held: {heldTime:F2}s / {fullDuration}s | Combo: {combo} | Timing: {timing}");
    }

    // Reset combo if head missed
    public void ResetCombo()
    {
        combo = 0;
        Debug.Log("Combo Reset!");
    }
}

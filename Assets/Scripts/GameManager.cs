using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // singleton for easy access
    public int score = 0;
    public int combo = 0;
    public string timing = "";
    public int winScore = 3000; //NEW: required score to trigger win scene

    //check notes for level end
    public int totalNotes = 0;
    public int notesRemaining = 0;
    public bool allNotesSpawned = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Update() { }

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

        timing =
            ratio >= 0.95f ? "Perfect Hold"
            : ratio >= 0.7f ? "Good Hold"
            : "Bad Hold";

        Debug.Log(
            $"Hold Score: {points} | Held: {heldTime:F2}s / {fullDuration}s | Combo: {combo} | Timing: {timing}"
        );
    }

    // Reset combo if head missed
    public void ResetCombo()
    {
        combo = 0;
        Debug.Log("Combo Reset!");
    }

    // ------------------------------
    // OBSTACLE SYSTEM INTEGRATION
    // ------------------------------

    // Called when an obstacle reaches the center
    public void LosePoints(int amount)
    {
        score -= amount;
        if (score < 0)
            score = 0;
        combo = 0; // optional: reset combo on obstacle hit
        Debug.Log($"Obstacle hit! Lost {amount} points. Score: {score} | Combo reset to {combo}");
    }

    //NEW : note registration to end lvl
    public void RegisterNote()
    {
        totalNotes++;
        notesRemaining++;
    }
    
    public void NoteDestroyed()
    {
        notesRemaining--;

        if (allNotesSpawned && notesRemaining <= 0)
            CheckEndOfSong();
    }

    // ---------- WIN / LOSS ----------
    public void CheckEndOfSong()
    {
        if (score >= winScore)
            WinGame();
        else
            LoseGame();
    }

    public void WinGame()
    {
        Debug.Log("YOU WIN!");
        SceneManager.LoadScene("WinScene");
    }

    public void LoseGame()
    {
        Debug.Log("YOU LOSE!");
        SceneManager.LoadScene("LoseScene");
    }

}

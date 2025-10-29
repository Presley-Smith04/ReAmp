using UnityEngine;
using System.Collections.Generic;

public class InputManager : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource music;

    [Header("Scoring Windows")]
    public float perfectWindow = 0.1f;
    public float goodWindow = 0.3f;
    public float badWindow = 0.5f;

    [Header("Arduino Input")]
    public ArduinoInput arduinoInput;
    public int forceThreshold = 500;

    private bool force0Triggered = false;
    private bool force1Triggered = false;
    private bool forceComboTriggered = false;

    void Update()
    {
        // === Keyboard input ===
        if (Input.GetKeyDown(KeyCode.E)) CheckHit(Direction.UpRight);
        if (Input.GetKeyDown(KeyCode.Z)) CheckHit(Direction.DownLeft);
        if (Input.GetKeyDown(KeyCode.Q)) CheckHit(Direction.UpLeft);
        if (Input.GetKeyDown(KeyCode.C)) CheckHit(Direction.DownRight);
        if (Input.GetKeyDown(KeyCode.Space)) ClearNearestObstacle();

        // === Arduino input ===
        if (arduinoInput != null)
        {
            // Button 1 → UpLeft (Q)
            if (arduinoInput.button1Pressed)
            {
                CheckHit(Direction.UpLeft);
            }
            // Button 2 → DownLeft (Z)
            if (arduinoInput.button2Pressed)
            {
                CheckHit(Direction.DownLeft);
            }
            // Both buttons held → DownRight (C)
            if (arduinoInput.button1Held && arduinoInput.button2Held)
            {
                CheckHit(Direction.DownRight);
            }
        }

    }

    public void CheckHit(Direction dir)
    {
        Note[] notes = FindObjectsOfType<Note>();
        float closestTimeDiff = float.MaxValue;
        Note closestNote = null;

        foreach (Note note in notes)
        {
            if (note.direction == dir)
            {
                float noteHitTime = note.spawnTime + note.travelTime;
                float diff = Mathf.Abs(noteHitTime - music.time);

                if (diff < closestTimeDiff)
                {
                    closestTimeDiff = diff;
                    closestNote = note;
                }
            }
        }

        if (closestNote != null)
        {
            if (closestNote.isHold)
            {
                if (closestTimeDiff <= perfectWindow)
                {
                    closestNote.OnHeadHit();
                    GameManager.Instance.AddScore("Perfect");
                }
                else if (closestTimeDiff <= goodWindow)
                {
                    closestNote.OnHeadHit();
                    GameManager.Instance.AddScore("Good");
                }
                else if (closestTimeDiff <= badWindow)
                {
                    closestNote.OnHeadHit();
                    GameManager.Instance.AddScore("Bad");
                }
                else
                {
                    GameManager.Instance.ResetCombo();
                }
            }
            else
            {
                if (closestTimeDiff <= perfectWindow)
                {
                    GameManager.Instance.AddScore("Perfect");
                    Destroy(closestNote.gameObject);
                }
                else if (closestTimeDiff <= goodWindow)
                {
                    GameManager.Instance.AddScore("Good");
                    Destroy(closestNote.gameObject);
                }
                else if (closestTimeDiff <= badWindow)
                {
                    GameManager.Instance.AddScore("Bad");
                    Destroy(closestNote.gameObject);
                }
                else
                {
                    GameManager.Instance.AddScore("Miss");
                }
            }
        }
        else
        {
            GameManager.Instance.AddScore("Miss");
        }
    }

    private void ClearNearestObstacle()
    {
        Obstacle[] obstacles = FindObjectsOfType<Obstacle>();
        if (obstacles.Length == 0) return;

        GameObject centerObj = GameObject.FindGameObjectWithTag("Center");
        if (centerObj == null) return;

        Transform center = centerObj.transform;
        Obstacle nearest = null;
        float closestDist = float.MaxValue;

        foreach (Obstacle o in obstacles)
        {
            float dist = Vector3.Distance(o.transform.position, center.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                nearest = o;
            }
        }

        if (nearest != null)
        {
            nearest.ClearObstacle();
            Debug.Log("Cleared nearest obstacle!");
        }
    }
}

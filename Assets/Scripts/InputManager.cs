using UnityEngine;

public class InputManager : MonoBehaviour
{
    public AudioSource music;
    public float perfectWindow = 0.1f;
    public float goodWindow = 0.3f;
    public float badWindow = 0.5f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W)) CheckHit(Direction.Up);
        if (Input.GetKeyDown(KeyCode.S)) CheckHit(Direction.Down);
        if (Input.GetKeyDown(KeyCode.A)) CheckHit(Direction.Left);
        if (Input.GetKeyDown(KeyCode.D)) CheckHit(Direction.Right);
    }
    
   void CheckHit(Direction dir)
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
        else
        {
            GameManager.Instance.AddScore("Miss"); // No note to hit
        }
    }
}

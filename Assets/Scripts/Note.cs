using System;
using UnityEngine;

public enum Direction { UpRight, DownRight, DownLeft, UpLeft } // 0 1 2 3

public class Note : MonoBehaviour
{
    public Direction direction;
    public float travelTime;   // time it takes to reach center
    public float spawnTime;    // time at which it was spawned

    [HideInInspector]
    public Transform target;   // hit zone center
    [HideInInspector]
    public Vector2 startPos;   // starting position of note

    private AudioSource music;

    void Start()
    {
        //Debug.Log($"Note spawned at {startPos}, target is {target.position}");

        music = GameObject.FindGameObjectWithTag("Music")?.GetComponent<AudioSource>();

        if (target == null)
        {
            Debug.LogError("Target not assigned on Note!");
        }

        startPos = transform.position; // record spawn position
    }

    void Update()
    {
        if (music != null && target != null)
        {
            // Calculate how far along the note should be based on song time
            float elapsed = music.time - spawnTime;
            float t = Mathf.Clamp01(elapsed / travelTime);

            // ---- ARC MOVEMENT (Bezier curve) ----
            // Control point: halfway between start and target, offset to create an arc
            Vector2 controlPoint = (startPos + (Vector2)target.position) / 2f + new Vector2(0, 2f);

            // Quadratic Bezier interpolation
            Vector2 p0 = Vector2.Lerp(startPos, controlPoint, t);
            Vector2 p1 = Vector2.Lerp(controlPoint, target.position, t);
            transform.position = Vector2.Lerp(p0, p1, t);

            // ---- SCALING (start small -> full size) ----
            float scale = Mathf.Lerp(0.5f, 1f, t);
            transform.localScale = new Vector3(scale, scale, 1f);

            // Destroy note if it passes hit window
            if (elapsed > travelTime + 0.7f)
            {
                GameManager.Instance.AddScore("Miss");
                Debug.Log("Note Passed By");
                Destroy(gameObject);
            }
        }
    }
}

using System;
using UnityEngine;

public enum Direction { Up, Down, Left, Right }

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

            // Interpolate position from start to target
            transform.position = Vector2.Lerp(startPos, target.position, t);

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

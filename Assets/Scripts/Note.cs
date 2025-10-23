using System;
using UnityEngine;

public enum Direction { Right, Left }

public class Note : MonoBehaviour
{
    public Direction direction;
    public float travelTime;
    public float spawnTime;
    public bool isHold = false;
    public float holdDuration = 0f;

    [HideInInspector]
    public Transform target;
    [HideInInspector]
    public Vector2 startPos;

    public LineRenderer lineRenderer;
    public Material greyMaterial;

    private AudioSource music;
    private bool headHit = false;
    private float holdTimer = 0f;
    private float comboTick = 0.5f;
    private float tickCounter = 0f;

    void Start()
    {
        music = GameObject.FindGameObjectWithTag("Music")?.GetComponent<AudioSource>();
        startPos = transform.position;

        // Move the start position off-screen based on direction
        float xOffset = 10f; // adjust to match your screen width
        if (direction == Direction.Right)
            startPos = new Vector2(target.position.x + xOffset, target.position.y);
        else if (direction == Direction.Left)
            startPos = new Vector2(target.position.x - xOffset, target.position.y);

        transform.position = startPos;
    }

    void Update()
    {
        if (music != null && target != null)
        {
            float elapsed = music.time - spawnTime;
            float t = Mathf.Clamp01(elapsed / travelTime);

            // --- 7-PATTERN MOVEMENT ---
            // Define turning point (above the target)
            Vector2 turnPoint = new Vector2(target.position.x, target.position.y + 10); // 2 units above target
            Vector2 newPos;

            if (t < 0.5f)
            {
                // First half: move horizontally toward the turn point
                float p = t / 0.5f; // normalize [0–0.5] to [0–1]
                newPos = Vector2.Lerp(startPos, turnPoint, p);
            }
            else
            {
                // Second half: move vertically downward to target
                float p = (t - 0.5f) / 0.5f; // normalize [0.5–1] to [0–1]
                newPos = Vector2.Lerp(turnPoint, target.position, p);
            }

            transform.position = newPos;

            // ---- HOLD LINE ----
            if (isHold && lineRenderer != null)
            {
                UpdateHoldLine(t);
            }

            // ---- MISS LOGIC ----
            if (elapsed > travelTime + 0.7f && !headHit)
            {
                if (isHold) GameManager.Instance.ResetCombo();
                else GameManager.Instance.AddScore("Miss");

                Destroy(gameObject);
            }
        }

        if (isHold && headHit)
        {
            HandleHoldLogic();
        }
    }


    public void SetupHold(LineRenderer lr, float duration, Material greyMat)
    {
        lineRenderer = lr;
        holdDuration = duration;
        greyMaterial = greyMat;
        isHold = true;

        lineRenderer.startColor = Color.white;
        lineRenderer.endColor = Color.white;
    }

    public void OnHeadHit()
    {
        headHit = true;
    }

    private void UpdateHoldLine(float progress)
    {
        if (lineRenderer == null || target == null) return;

        int resolution = lineRenderer.positionCount;
        float amplitude = 1.5f;
        float frequency = 3f;
        float verticalOffset = 15f;
        float directionSign = (direction == Direction.Right) ? 1f : -1f;


// Starting point for the wave (above the target)
    Vector2 waveStart = (Vector2)target.position + new Vector2(0, verticalOffset);
    Vector2 waveEnd = target.position;
        for (int i = 0; i < resolution; i++)
        {
           float t = i / (float)(resolution - 1);

        // Vertical lerp downward
        Vector2 basePos = Vector2.Lerp(waveStart, waveEnd, t);

        // Horizontal sine offset
        float sineOffset = Mathf.Sin(t * Mathf.PI * frequency + progress * Mathf.PI * 2f) * amplitude * directionSign;

        Vector2 wavePos = basePos + new Vector2(sineOffset, 0f);

        lineRenderer.SetPosition(i, wavePos);
        }
    }


    private void HandleHoldLogic()
    {
        bool holding = false;

        // Keyboard input
        if (Input.GetKey(KeyForDirection(direction)))
            holding = true;

        // Arduino input
        ArduinoInput arduino = FindObjectOfType<ArduinoInput>(); // cache this if needed
        if (arduino != null)
        {
            switch (direction)
            {
                case Direction.Right:
                    holding |= arduino.force0Held;
                    break;
                case Direction.Left:
                    holding |= arduino.force1Held;
                    break;
            }
        }

        // If still holding, continue tracking time
        if (holding)
        {
            holdTimer += Time.deltaTime;
            tickCounter += Time.deltaTime;

            if (tickCounter >= comboTick)
            {
                GameManager.Instance.combo++;
                tickCounter -= comboTick;
            }

            if (holdTimer >= holdDuration)
            {
                GameManager.Instance.AddHoldScore(holdDuration, holdDuration);
                Destroy(gameObject);
            }
        }
        else
        {
            EndHoldEarly();
        }
    }


    private void EndHoldEarly()
    {
        if (lineRenderer != null) lineRenderer.material = greyMaterial;
        GameManager.Instance.AddHoldScore(holdTimer, holdDuration);
        Destroy(gameObject, 0.5f);
    }

    private KeyCode KeyForDirection(Direction dir)
    {
        switch (dir)
        {
            case Direction.Right: return KeyCode.J;
            case Direction.Left: return KeyCode.D;
        }
        return KeyCode.Space;
    }
}

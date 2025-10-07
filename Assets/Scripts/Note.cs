using System;
using UnityEngine;

public enum Direction { UpRight, DownRight, DownLeft, UpLeft } // 0 1 2 3

public class Note : MonoBehaviour
{
    public Direction direction;
    public float travelTime;
    public float spawnTime;
    public bool isHold = false;         // New
    public float holdDuration = 0f;     // New

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
    }

    void Update()
    {
        if (music != null && target != null)
        {
            // ---- ARC MOVEMENT ----
            float elapsed = music.time - spawnTime;
            float t = Mathf.Clamp01(elapsed / travelTime);

            Vector2 controlPoint = (startPos + (Vector2)target.position) / 2f + new Vector2(0, 2f);
            Vector2 p0 = Vector2.Lerp(startPos, controlPoint, t);
            Vector2 p1 = Vector2.Lerp(controlPoint, target.position, t);
            transform.position = Vector2.Lerp(p0, p1, t);

            // ---- SCALING ----
            float scale = Mathf.Lerp(0.5f, 1f, t);
            transform.localScale = new Vector3(scale, scale, 1f);

            if (isHold && lineRenderer != null)
            {
                UpdateHoldLine(t);
            }

            if (elapsed > travelTime + 0.7f && !headHit)
            {
                // Missed note
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
        Vector2 controlPoint = (startPos + (Vector2)target.position) / 2f + new Vector2(0, 2f);

        for (int i = 0; i < resolution; i++)
        {
            // Spread points behind the head along the curve
            float t = Mathf.Clamp01(progress - (i / (float)(resolution - 1)) * (holdDuration / travelTime));

            // Bezier curve
            Vector2 p0 = Vector2.Lerp(startPos, controlPoint, t);
            Vector2 p1 = Vector2.Lerp(controlPoint, target.position, t);
            Vector2 curvePos = Vector2.Lerp(p0, p1, t);

            // Offset slightly *away from center* (to draw behind)
            Vector2 dirFromCenter = ((Vector2)target.position - curvePos).normalized;
            curvePos -= dirFromCenter * 0.3f;

            lineRenderer.SetPosition(i, curvePos);
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
                case Direction.UpRight:
                    holding |= arduino.force0Held;
                    break;
                case Direction.DownLeft:
                    holding |= arduino.force1Held;
                    break;
                case Direction.UpLeft:
                    holding |= arduino.buttonPressed; // adjust if needed
                    break;
                case Direction.DownRight:
                    holding |= arduino.force0Held && arduino.force1Held;
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
            case Direction.UpRight: return KeyCode.E;
            case Direction.UpLeft: return KeyCode.Q;
            case Direction.DownRight: return KeyCode.C;
            case Direction.DownLeft: return KeyCode.Z;
        }
        return KeyCode.Space;
    }
}

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

    // Control point same as movement, but line goes outward
    Vector2 controlPoint = (startPos + (Vector2)target.position) / 2f + new Vector2(0, 2f);

    for (int i = 0; i < resolution; i++)
    {
        // Instead of going forward toward the target,
        // we extend *backward* past the note
        float t = progress - (i / (float)(resolution - 1)) * (holdDuration / travelTime);

        // Clamp to [0,1] so it doesn’t explode
        t = Mathf.Clamp01(t);

        // Use the same quadratic Bezier formula
        Vector2 p0 = Vector2.Lerp(startPos, controlPoint, t);
        Vector2 p1 = Vector2.Lerp(controlPoint, target.position, t);
        Vector2 curvePos = Vector2.Lerp(p0, p1, t);

        lineRenderer.SetPosition(i, curvePos);
    }
}



    private void HandleHoldLogic()
    {
        if (Input.GetKey(KeyForDirection(direction)))
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

using System;
using UnityEngine;

public enum Direction
{
    Right,
    Left,
}

public enum NoteType
{
    Tap,
    Hold,
}

public class Note : MonoBehaviour
{
    [Header("Note Data")]
    public NoteType noteType = NoteType.Tap;
    public Direction direction;
    public float travelTime;
    public float spawnTime;
    public bool isHold = false;
    public float holdDuration = 0f;

    [HideInInspector] public Transform target;
    [HideInInspector] public Transform despawnPoint; // ✅ Added for NoteSpawner
    [HideInInspector] public Vector2 startPos;

    public LineRenderer lineRenderer;
    public Material greyMaterial;

    private AudioSource music;
    private Animator animator;
    private bool headHit = false;
    private float holdTimer = 0f;
    private float comboTick = 0.5f;
    private float tickCounter = 0f;

    void Start()
    {
        music = GameObject.FindGameObjectWithTag("Music")?.GetComponent<AudioSource>();
        startPos = transform.position;
    }

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (music != null && target != null)
        {
            float elapsed = music.time - spawnTime;
            float t = Mathf.Clamp01(elapsed / travelTime);

            Vector2 controlPoint = (startPos + (Vector2)target.position) / 2f + new Vector2(0, 2f);
            Vector2 p0 = Vector2.Lerp(startPos, controlPoint, t);
            Vector2 p1 = Vector2.Lerp(controlPoint, target.position, t);
            transform.position = Vector2.Lerp(p0, p1, t);

            if (isHold && lineRenderer != null)
                UpdateHoldLine(t);

            if (elapsed > travelTime + 0.7f && !headHit)
            {
                if (isHold)
                    GameManager.Instance.ResetCombo();
                else
                    GameManager.Instance.AddScore("Miss");

                Destroy(gameObject);
            }
        }

        if (isHold && headHit)
        {
            HandleHoldLogic();
        }
    }

    public void OnReachTarget()
    {
        if (!isHold)
        {
            bool pressed = false;

            // --- Keyboard Input ---
            if (Input.GetKeyDown(KeyForDirection(direction)))
                pressed = true;

            // --- Arduino Input ---
            ArduinoInput arduino = FindObjectOfType<ArduinoInput>();
            if (arduino != null)
            {
                switch (direction)
                {
                    case Direction.Right:
                        pressed |= arduino.button2Pressed;
                        break;
                    case Direction.Left:
                        pressed |= arduino.button1Pressed;
                        break;
                }
            }

            if (pressed)
            {
                GameManager.Instance.AddScore("Perfect");
                headHit = true;
                Destroy(gameObject);
            }
        }
        else
        {
            // Hold note - waiting for hold input handled in HandleHoldLogic
            headHit = true;
        }
    }


    private void UpdateHoldLine(float progress)
    {
        if (lineRenderer == null || target == null) return;

        int resolution = lineRenderer.positionCount;
        Vector2 controlPoint = (startPos + (Vector2)target.position) / 2f + new Vector2(0, 2f);

        for (int i = 0; i < resolution; i++)
        {
            float t = Mathf.Clamp01(progress - (i / (float)(resolution - 1)) * (holdDuration / travelTime));

            Vector2 p0 = Vector2.Lerp(startPos, controlPoint, t);
            Vector2 p1 = Vector2.Lerp(controlPoint, target.position, t);
            Vector2 curvePos = Vector2.Lerp(p0, p1, t);

            Vector2 dirFromCenter = ((Vector2)target.position - curvePos).normalized;
            curvePos -= dirFromCenter * 0.3f;

            lineRenderer.SetPosition(i, curvePos);
        }
    }

    private void HandleHoldLogic()
    {
        bool holding = false;

        // --- Keyboard Input (fallback) ---
        if (Input.GetKey(KeyForDirection(direction)))
            holding = true;

        // --- Arduino Input ---
        ArduinoInput arduino = FindObjectOfType<ArduinoInput>();
        if (arduino != null)
        {
            // Match directions to Arduino buttons
            switch (direction)
            {
                case Direction.Right:
                    holding |= arduino.button2Held;
                    break;
                case Direction.Left:
                    holding |= arduino.button1Held;
                    break;
            }
        }

        // --- Holding Logic ---
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
        if (lineRenderer != null)
            lineRenderer.material = greyMaterial;

        GameManager.Instance.AddHoldScore(holdTimer, holdDuration);
        Destroy(gameObject, 0.5f);
    }

    private KeyCode KeyForDirection(Direction dir)
    {
        return dir == Direction.Right ? KeyCode.J : KeyCode.D;
    }

    public void OnHeadHit()
    {
        headHit = true;
    }
}

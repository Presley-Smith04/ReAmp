using System;
using UnityEngine;

public enum Direction { UpRight, DownRight, DownLeft, UpLeft } // 0 1 2 3

public enum Direction
{
    Right,
    Left,
}

public class Note : MonoBehaviour
{
    [Header("Note Data")]
    public NoteType noteType = NoteType.Tap;
    public Direction direction;
    public float travelTime;
    public float spawnTime;
    public bool isHold = false;         // New
    public float holdDuration = 0f;     // New

    [HideInInspector]
    public Transform target;
    [HideInInspector]
    public Vector2 startPos;

    private AudioSource music;
    private bool headHit = false;
    private float holdTimer = 0f; //how long you have been holding a note
    private float comboTick = 0.5f; //tick between combos
    private float tickCounter = 0f;

    void Start()
    {
        music = GameObject.FindGameObjectWithTag("Music")?.GetComponent<AudioSource>();
        startPos = transform.position;
    }

    void Awake()
    {
        animator = GetComponent<Animator>();

            Vector2 controlPoint = (startPos + (Vector2)target.position) / 2f + new Vector2(0, 2f);
            Vector2 p0 = Vector2.Lerp(startPos, controlPoint, t);
            Vector2 p1 = Vector2.Lerp(controlPoint, target.position, t);
            transform.position = Vector2.Lerp(p0, p1, t);

            // ---- SCALING ----
            //float scale = Mathf.Lerp(0.5f, 1f, t);
            //transform.localScale = new Vector3(scale, scale, 1f);

            if (isHold && lineRenderer != null)
            {
                UpdateHoldLine(t);
            }
            if (isHold && lineRenderer != null)
            if (elapsed > travelTime + 0.7f && !headHit)
            {
                // Missed note
                if (isHold) GameManager.Instance.ResetCombo();
                else GameManager.Instance.AddScore("Miss");

                Destroy(gameObject);
            }
        }

        if (isHold && headHit)
            }
        }

        if (isHold && headHit)
            }
        }

        if (isHold && headHit)
        {
            animator.Play("Hold", 0, 0f); // Play HoldNote at start
        }
    }

    // Called by BezierFollow when movement ends
    public void OnReachTarget()
    {
        // wait for player input within short window
        if (!isHold)
        {
            StartCoroutine(WaitForHit());
        }
        else
        {
            StartCoroutine(WaitForHold());
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
            // Offset slightly *away from center* (to draw behind)

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
                    break;
        // If still holding, continue tracking time
        if (holding)
        {
            holdTimer += Time.deltaTime;
            tickCounter += Time.deltaTime;
                    break;
            }
        }
                    break;
        // If still holding, continue tracking time
        if (holding)
        {
            holdTimer += Time.deltaTime;
            tickCounter += Time.deltaTime;
                    break;
            }
        }

        // If still holding, continue tracking time
        if (holding)
        {
            holdTimer += Time.deltaTime;
            tickCounter += Time.deltaTime;

                // Update combo ticks
                if (tickCounter >= comboTick)
                {
                    GameManager.Instance.combo++;
                    tickCounter -= comboTick;
                }
            }
            else
            {
                // Player released early -> partial hold score
                GameManager.Instance.AddHoldScore(holdTimer, holdDuration);
                Destroy(gameObject);
                yield break;
            }

            yield return null;
        }

        // Player held the note fully -> full hold score
        GameManager.Instance.AddHoldScore(holdDuration, holdDuration);
        Destroy(gameObject);

    private void EndHoldEarly()
    {
        if (lineRenderer != null) lineRenderer.material = greyMaterial;
        GameManager.Instance.AddHoldScore(holdTimer, holdDuration);
        Destroy(gameObject, 0.5f);

        while (Time.time < endTime)
        {

    private void EndHoldEarly()
    {
        if (lineRenderer != null) lineRenderer.material = greyMaterial;
        GameManager.Instance.AddHoldScore(holdTimer, holdDuration);
        Destroy(gameObject, 0.5f);
            yield return null;
        }


    private void EndHoldEarly()
    {
        if (lineRenderer != null) lineRenderer.material = greyMaterial;
        GameManager.Instance.AddHoldScore(holdTimer, holdDuration);
        Destroy(gameObject, 0.5f);
    }

    private KeyCode KeyForDirection(Direction dir)
    {
        return dir == Direction.Right ? KeyCode.J : KeyCode.D;
    }

    //Called in Input Manager
    public void OnHeadHit()
    {
        headHit = true;
    }
}

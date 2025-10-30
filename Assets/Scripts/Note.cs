using System;
using UnityEngine;

public enum NoteType
{
    Tap,
    Hold,
}

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
    public bool isHold;
    public float holdDuration; //how long a note lasts

    [Header("Note Settings")]
    public Transform despawnPoint; // assign in inspector
    public float hitWindow = 0.3f; // how long player can hit after reaching scoring zone
    private bool hasPassedScoringZone = false;
    private float timeSinceScoringZone = 0f;

    [HideInInspector]
    public Transform target;

    [HideInInspector]
    public Vector2 startPos;

    private AudioSource music;

    [SerializeField]
    private bool headHit = false;
    private float holdTimer = 0f; //how long you have been holding a note
    private float comboTick = 0.5f; //tick between combos
    private float tickCounter = 0f;

    [Header("Hold Note Visuals")]
    private Animator animator;

    private ArduinoInput arduinoInput;

    void Awake()
    {
        animator = GetComponent<Animator>();

        if (isHold)
            animator.SetBool("IsHold", true);

        // Find ArduinoInput in scene
        arduinoInput = FindObjectOfType<ArduinoInput>();
    }

    void Start()
    {
        music = GameObject.FindGameObjectWithTag("Music")?.GetComponent<AudioSource>();
        if (isHold)
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
        }
    }

    private System.Collections.IEnumerator WaitForHold()
    {
        float maxMissTime = 0.3f;
        float missTimer = 0f;

        // Wait until the player hits the head of the note
        while (!headHit)
        {
            missTimer += Time.deltaTime;
            if (missTimer >= maxMissTime)
            {
                GameManager.Instance.AddScore("Miss");
                Destroy(gameObject);
                yield break;
            }
            yield return null;
        }

        holdTimer = 0f;
        tickCounter = 0f;

        // Keep updating while the player hasn't completed the hold
        while (holdTimer < holdDuration)
        {
            bool holding = false;
            if (arduinoInput != null)
            {
                holding = direction == Direction.Right ? arduinoInput.button1Held : arduinoInput.button2Held;
            }

            if (holding)
            {
                // Increment timers
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
    }

    private System.Collections.IEnumerator WaitForHit()
    {
        float window = 0.3f;
        float endTime = Time.time + window;

        while (Time.time < endTime)
        {
            bool pressed = false;
            if (arduinoInput != null)
            {
                pressed = direction == Direction.Right ? arduinoInput.button1Pressed : arduinoInput.button2Pressed;
            }

            if (pressed)
            {
                GameManager.Instance.AddScore("Perfect");
                Destroy(gameObject);
                yield break;
            }
            yield return null;
        }

        GameManager.Instance.AddScore("Miss");
        Destroy(gameObject);
    }

    //Called in Input Manager
    public void OnHeadHit()
    {
        headHit = true;
    }
}

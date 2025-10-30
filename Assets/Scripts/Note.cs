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
    public float holdDuration;

    [Header("Note Settings")]
    public Transform despawnPoint;
    public float hitWindow = 0.3f;
    private bool hasPassedScoringZone = false;
    private float timeSinceScoringZone = 0f;

    [HideInInspector]
    public Transform target;

    [HideInInspector]
    public Vector2 startPos;

    private AudioSource music;

    [SerializeField]
    private bool headHit = false;
    private float holdTimer = 0f;
    private float comboTick = 0.5f;
    private float tickCounter = 0f;

    [Header("Hold Note Visuals")]
    private Animator animator;

    // Reference to Arduino input
    private ArduinoInput arduinoInput;

    void Awake()
    {
        animator = GetComponent<Animator>();

        if (isHold)
            animator.SetBool("IsHold", true);
    }

    void Start()
    {
        music = GameObject.FindGameObjectWithTag("Music")?.GetComponent<AudioSource>();
        arduinoInput = FindObjectOfType<ArduinoInput>();

        if (isHold)
        {
            animator.Play("Hold", 0, 0f);
        }
    }

    public void OnReachTarget()
    {
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

        while (holdTimer < holdDuration)
        {
            bool holding = IsDirectionPressed(direction);

            if (holding)
            {
                holdTimer += Time.deltaTime;
                tickCounter += Time.deltaTime;

                if (tickCounter >= comboTick)
                {
                    GameManager.Instance.combo++;
                    tickCounter -= comboTick;
                }
            }
            else
            {
                GameManager.Instance.AddHoldScore(holdTimer, holdDuration);
                Destroy(gameObject);
                yield break;
            }

            yield return null;
        }

        GameManager.Instance.AddHoldScore(holdDuration, holdDuration);
        Destroy(gameObject);
    }

    private System.Collections.IEnumerator WaitForHit()
    {
        float window = 0.3f;
        float endTime = Time.time + window;

        while (Time.time < endTime)
        {
            if (IsDirectionPressedDown(direction))
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

    private bool IsDirectionPressed(Direction dir)
    {
        if (dir == Direction.Right)
            return Input.GetKey(KeyCode.J) || (arduinoInput != null && arduinoInput.button2Held);
        else
            return Input.GetKey(KeyCode.D) || (arduinoInput != null && arduinoInput.button1Held);
    }

    private bool IsDirectionPressedDown(Direction dir)
    {
        if (dir == Direction.Right)
            return Input.GetKeyDown(KeyCode.J) || (arduinoInput != null && arduinoInput.button2Pressed);
        else
            return Input.GetKeyDown(KeyCode.D) || (arduinoInput != null && arduinoInput.button1Pressed);
    }

    public void OnHeadHit()
    {
        headHit = true;
    }
}

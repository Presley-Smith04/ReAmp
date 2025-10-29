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

    [HideInInspector]
    public Transform target;

    [HideInInspector]
    public Vector2 startPos;

    private AudioSource music;
    private bool headHit = false;
    private float holdTimer = 0f;
    private float comboTick = 0.5f;
    private float tickCounter = 0f;

    [Header("Hold Note Visuals")]
    public Sprite holdNoteSprite;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        music = GameObject.FindGameObjectWithTag("Music")?.GetComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (isHold)
            spriteRenderer.sprite = holdNoteSprite;
    }

    void Update()
    {
        // Remove notes that have gone past the hit window
        if (!headHit && music.time > spawnTime + travelTime + 0.5f)
        {
            GameManager.Instance.AddScore("Miss");
            Destroy(gameObject);
        }

        if (isHold && headHit)
            HandleHoldLogic();
    }

    public void OnHeadHit()
    {
        headHit = true;
    }

    public void OnReachTarget()
    {
        // Called by BezierFollow when movement ends
        if (!isHold)
        {
            // wait for player input within short window
            StartCoroutine(WaitForHit());
        }
    }

    private System.Collections.IEnumerator WaitForHit()
    {
        float window = 0.3f;
        float endTime = Time.time + window;

        while (Time.time < endTime)
        {
            if (Input.GetKeyDown(KeyForDirection(direction)))
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

    private void HandleHoldLogic()
    {
        bool holding = Input.GetKey(KeyForDirection(direction));

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
        GameManager.Instance.AddHoldScore(holdTimer, holdDuration);
        Destroy(gameObject);
    }

    private KeyCode KeyForDirection(Direction dir)
    {
        return dir == Direction.Right ? KeyCode.J : KeyCode.D;
    }
}

using UnityEngine;
using System.IO;
[System.Serializable]
public class BeatMapWrapper
{
    public Beat[] beats;
}

[System.Serializable]
public class Beat
{
    public float time;
    public Direction direction;
}

public class NoteSpawner : MonoBehaviour
{
    public AudioSource music;
    public GameObject notePrefab;
    public Transform upZone;
    public Transform downZone;
    public Transform leftZone;
    public Transform rightZone;
    public Color upColor = Color.red;
    public Color downColor = Color.blue;
    public Color leftColor = Color.green;
    public Color rightColor = Color.yellow;

    public float leadTime = 2f; // seconds before beat to spawn note

    [Header("Beatmap Settings")]
    public string beatmapFile = "easy.json"; // set in Inspector or dynamically

    private Beat[] beatMap;
    private int nextNoteIndex = 0;

    void Start()
    {
        LoadBeatMap(beatmapFile);
    }

    void Update()
    {
        if (beatMap == null || nextNoteIndex >= beatMap.Length) return;

        float songTime = music.time;
        if (songTime >= beatMap[nextNoteIndex].time - leadTime)
        {
            SpawnNote(beatMap[nextNoteIndex]);
            nextNoteIndex++;
        }
    }

    public Transform centerPoint;  // UI object at the middle
    public RectTransform canvasParent; // usually the Canvas

    void SpawnNote(Beat beat)
    {
        float spawnOffset = 10f; // distance outside the play area
        Vector3 startPos = Vector3.zero;
        Transform targetZone = null;
        Color noteColor = Color.white;

        switch (beat.direction)
        {
            case Direction.Up:
                targetZone = upZone;
                startPos = upZone.position + Vector3.up * spawnOffset;
                noteColor = upColor;
                break;
            case Direction.Down:
                targetZone = downZone;
                startPos = downZone.position + Vector3.down * spawnOffset;
                noteColor = downColor;
                break;
            case Direction.Left:
                targetZone = leftZone;
                startPos = leftZone.position + Vector3.left * spawnOffset;
                noteColor = leftColor;
                break;
            case Direction.Right:
                targetZone = rightZone;
                startPos = rightZone.position + Vector3.right * spawnOffset;
                noteColor = rightColor;
                break;
        }

        GameObject noteObj = Instantiate(notePrefab, startPos, Quaternion.identity);
        Note noteScript = noteObj.GetComponent<Note>();
        noteScript.direction = beat.direction;
        noteScript.travelTime = leadTime;
        noteScript.spawnTime = music.time;
        noteScript.target = targetZone;
        noteScript.startPos = startPos;

        // Apply zone color
        SpriteRenderer sr = noteObj.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = noteColor;
    }


    void LoadBeatMap(string fileName)
    {
        string path = Path.Combine(Application.streamingAssetsPath, "Beatmaps/" + fileName);

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            BeatMapWrapper wrapper = JsonUtility.FromJson<BeatMapWrapper>(json);
            beatMap = wrapper.beats;
            nextNoteIndex = 0;

            Debug.Log("Loaded beatmap " + fileName + " with " + beatMap.Length + " notes");
        }
        else
        {
            Debug.LogError("Beatmap file not found at: " + path);
        }
    }
}

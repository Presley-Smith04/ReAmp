using System.IO;
using UnityEngine;

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
    public float duration;
}

public class NoteSpawner : MonoBehaviour
{
    public AudioSource music;
    public GameObject rightNotePrefab;
    public GameObject leftNotePrefab;

    public Transform rightZone;
    public Transform leftZone;

    // 🌈 Add these
    public Transform rightRouteParent;
    public Transform leftRouteParent;

    public float leadTime = 2f;
    public string beatmapFile = "";

    private Beat[] beatMap;
    private int nextNoteIndex = 0;

    void Start()
    {
        LoadBeatMap(beatmapFile);
    }

    void Update()
    {
        if (beatMap == null || nextNoteIndex >= beatMap.Length)
            return;

        float songTime = music.time;
        if (songTime >= beatMap[nextNoteIndex].time - leadTime)
        {
            SpawnNote(beatMap[nextNoteIndex]);
            nextNoteIndex++;
        }
    }

    void SpawnNote(Beat beat)
    {
        GameObject prefab = (beat.direction == Direction.Right) ? rightNotePrefab : leftNotePrefab;
        Transform zone = (beat.direction == Direction.Right) ? rightZone : leftZone;
        Transform routeParent =
            (beat.direction == Direction.Right) ? rightRouteParent : leftRouteParent;

        GameObject noteObj = Instantiate(
            prefab,
            routeParent.GetChild(0).position,
            Quaternion.identity
        );
        Note note = noteObj.GetComponent<Note>();
        BezierFollow follow = noteObj.GetComponent<BezierFollow>();

        // Assign route dynamically
        Transform[] routePoints = new Transform[4];
        for (int i = 0; i < 4; i++)
            routePoints[i] = routeParent.GetChild(i);
        follow.SetRoute(routePoints);

        // Assign note data
        note.direction = beat.direction;
        note.travelTime = leadTime;
        note.spawnTime = music.time;
        note.target = zone;
        note.isHold = beat.duration > 0;
        note.holdDuration = beat.duration;
    }

    void LoadBeatMap(string fileName)
    {
        string path = Path.Combine(Application.streamingAssetsPath, "Beatmaps/" + fileName);
        if (!File.Exists(path))
        {
            Debug.LogError("Beatmap not found: " + path);
            return;
        }

        string json = File.ReadAllText(path);
        BeatMapWrapper wrapper = JsonUtility.FromJson<BeatMapWrapper>(json);
        beatMap = wrapper.beats;
        nextNoteIndex = 0;

        Debug.Log($"Loaded beatmap {fileName} ({beatMap.Length} notes)");
    }
}

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
    public Direction direction; // now uses diagonal enum
}

public class NoteSpawner : MonoBehaviour
{
    public AudioSource music;

    public GameObject upRightNotePrefab;
    public GameObject downRightNotePrefab;
    public GameObject downLeftNotePrefab;
    public GameObject upLeftNotePrefab;

    public Transform upRightZone;   // set at (1,1)
    public Transform downRightZone; // set at (1,-1)
    public Transform downLeftZone;  // set at (-1,-1)
    public Transform upLeftZone;    // set at (-1,1)

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

    void SpawnNote(Beat beat)
    {
        GameObject notePrefab = null;
        Transform targetZone = null;
        Vector3 startPos = Vector3.zero;
        float spawnOffset = 5f; // distance from center along diagonal

        switch (beat.direction)
        {
            case Direction.UpRight:
                targetZone = upRightZone;
                startPos = targetZone.position + new Vector3(spawnOffset, spawnOffset, 0);
                notePrefab = upRightNotePrefab;
                break;
            case Direction.DownRight:
                targetZone = downRightZone;
                startPos = targetZone.position + new Vector3(spawnOffset, -spawnOffset, 0);
                notePrefab = downRightNotePrefab;
                break;
            case Direction.DownLeft:
                targetZone = downLeftZone;
                startPos = targetZone.position + new Vector3(-spawnOffset, -spawnOffset, 0);
                notePrefab = downLeftNotePrefab;
                break;
            case Direction.UpLeft:
                targetZone = upLeftZone;
                startPos = targetZone.position + new Vector3(-spawnOffset, spawnOffset, 0);
                notePrefab = upLeftNotePrefab;
                break;
        }

        GameObject noteObj = Instantiate(notePrefab, startPos, Quaternion.identity);
        Note noteScript = noteObj.GetComponent<Note>();
        noteScript.direction = beat.direction;
        noteScript.travelTime = leadTime;
        noteScript.spawnTime = music.time;
        noteScript.target = targetZone;
        noteScript.startPos = startPos;
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

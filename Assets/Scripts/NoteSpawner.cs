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
    public float duration; // 0 = tap note, >0 = hold note

}

public class NoteSpawner : MonoBehaviour
{
    public AudioSource music;

    public GameObject upRightNotePrefab;
    public GameObject downRightNotePrefab;
    public GameObject downLeftNotePrefab;
    public GameObject upLeftNotePrefab;

    public Transform upRightZone;
    public Transform downRightZone;
    public Transform downLeftZone;
    public Transform upLeftZone;

    public Material holdMaterial;
    public Material greyMaterial;


    public float leadTime = 2f;
    public string beatmapFile = " ";

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
        float spawnOffset = 5f;

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

        if (beat.duration > 0)
        {
            LineRenderer lr = noteObj.GetComponent<LineRenderer>();
            lr.positionCount = 50;
            lr.material = holdMaterial;
            lr.widthMultiplier = 0.15f;
            noteScript.SetupHold(lr, beat.duration, greyMaterial);
        }
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

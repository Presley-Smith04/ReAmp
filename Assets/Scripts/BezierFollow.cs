using System.Collections;
using UnityEngine;

public class BezierFollow : MonoBehaviour
{
    [SerializeField]
    private Transform[] routes;
    private int routeToGo;
    private float tParam;
    private float speedModifier;

    private Note note; // reference to note info

    public void SetRoute(Transform[] routePoints)
    {
        routes = routePoints;
    }

    void Start()
    {
        note = GetComponent<Note>();
        if (routes == null || routes.Length < 4)
        {
            Debug.LogError($"{gameObject.name} has no Bezier route assigned!");
            return;
        }

        speedModifier = 1f / note.travelTime;
        StartCoroutine(GoByTheRoute());
    }

    private IEnumerator GoByTheRoute()
    {
        Vector2 p0 = routes[0].position;
        Vector2 p1 = routes[1].position;
        Vector2 p2 = routes[2].position;
        Vector2 p3 = routes[3].position;

        while (tParam < 1)
        {
            tParam += Time.deltaTime * speedModifier;

            Vector2 newPos =
                Mathf.Pow(1 - tParam, 3) * p0
                + 3 * Mathf.Pow(1 - tParam, 2) * tParam * p1
                + 3 * (1 - tParam) * Mathf.Pow(tParam, 2) * p2
                + Mathf.Pow(tParam, 3) * p3;

            transform.position = newPos;
            yield return null;
        }

        note.OnReachTarget();
    }
}

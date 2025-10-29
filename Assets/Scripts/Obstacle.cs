using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public Transform target; // Center position
    public float travelTime = 2f; // How long it takes to reach the center (slower than notes)
    public float spawnTime;
    public bool isActive = true;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        float t = (Time.time - spawnTime) / travelTime;
        transform.position = Vector3.Lerp(startPos, target.position, t);

        // If it reaches the center
        if (t >= 1f && isActive)
        {
            OnReachCenter();
        }
    }

    public void ClearObstacle()
    {
        if (!isActive)
            return;
        isActive = false;
        Destroy(gameObject);
    }

    private void OnReachCenter()
    {
        isActive = false;
        GameManager.Instance.LosePoints(100); // adjust penalty
        Destroy(gameObject);
    }
}

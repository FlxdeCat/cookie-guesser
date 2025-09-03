using UnityEngine;

public class CookieSpawner : MonoBehaviour
{
    public GameObject cookiePrefab; // Assign your cookie prefab
    public float spawnInterval = 0.5f; // Time between spawns
    public float minX = -50f;
    public float maxX = 50f;
    public float spawnY = 8f; // Y position to spawn
    public float spawnZ = 0f; // Z position (if needed)

    public Vector2 scaleRange = new Vector2(5f, 15f); // Min and max scale

    private void Start()
    {
        InvokeRepeating(nameof(SpawnCookie), 0f, spawnInterval);
    }

    void SpawnCookie()
    {
        float randomX = Random.Range(minX, maxX);
        Vector3 spawnPos = new Vector3(randomX, spawnY, spawnZ);

        GameObject cookie = Instantiate(cookiePrefab, spawnPos, cookiePrefab.transform.rotation);

        float randomScale = Random.Range(scaleRange.x, scaleRange.y);
        cookie.transform.localScale = Vector3.one * randomScale;
    }
}

using UnityEngine;

public class CookieController : MonoBehaviour
{
    public float minFallSpeed = 2f;
    public float maxFallSpeed = 4f;
    public float minRotationSpeed = 50f;
    public float maxRotationSpeed = 150f;

    private float fallSpeed;
    private float rotationSpeed;
    private Vector3 rotationDirection;

    void Start()
    {
        fallSpeed = Random.Range(minFallSpeed, maxFallSpeed);
        rotationSpeed = Random.Range(minRotationSpeed, maxRotationSpeed);
    }

    void Update()
    {
        // Move down
        transform.position += Vector3.down * fallSpeed * Time.deltaTime;

        // Rotate
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);

        if(transform.position.y < -10f)
        {
            Destroy(gameObject);
        }
    }
}

using UnityEngine;

public class FloatTitle : MonoBehaviour
{
    [Header("Float")]
    [SerializeField] float stepInterval = 0.15f;
    [SerializeField] float travelDistance = 16f;
    [SerializeField] float pixelStep = 4f;

    Vector3 startPos;
    float currentOffset = 0f;
    float timer = 0f;
    int direction = 1;

    void Start()
    {
        startPos = transform.localPosition;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer < stepInterval)
            return;

        timer = 0f;

        currentOffset += pixelStep * direction;

        if (currentOffset >= travelDistance)
        {
            currentOffset = travelDistance;
            direction = -1;
        }
        else if (currentOffset <= -travelDistance)
        {
            currentOffset = -travelDistance;
            direction = 1;
        }

        transform.localPosition =
            startPos + Vector3.up * currentOffset;
    }
}
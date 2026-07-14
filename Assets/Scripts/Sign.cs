using UnityEngine;

public class Sign : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float hoverHeight = 0.1f;
    [SerializeField] float hoverSpeed = 1.5f;

    [SerializeField] float swayDistance = 0.03f;
    [SerializeField] float swaySpeed = 0.9f;

    [Header("Rotation")]
    [SerializeField] float pitchAmount = 3f;
    [SerializeField] float rollAmount = 4f;
    [SerializeField] float rotationSpeed = 1.2f;

    Vector3 startPosition;
    Quaternion startRotation;

    void Start()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    void Update()
    {
        float t = Time.time;

        Vector3 offset = new Vector3(
            Mathf.Sin(t * swaySpeed) * swayDistance,
            Mathf.Sin(t * hoverSpeed) * hoverHeight,
            0f);

        Quaternion rotationOffset = Quaternion.Euler(
            Mathf.Sin(t * rotationSpeed) * pitchAmount,
            0f,
            Mathf.Cos(t * rotationSpeed * 1.3f) * rollAmount);

        transform.position = startPosition + offset;
        transform.rotation = startRotation * rotationOffset;
    }
}
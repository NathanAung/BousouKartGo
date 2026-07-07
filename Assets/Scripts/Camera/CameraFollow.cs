using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 1f, -2f);
    public float positionSmooth = 10f;
    public float rotationSmooth = 10f;

    void LateUpdate()
    {
        if (target == null) return;

        // Calculate the ideal position relative to target's rotation
        Vector3 desiredPosition = target.position + (target.rotation * offset);

        // Frame-rate independent smoothing factors
        float posT = 1f - Mathf.Exp(-positionSmooth * Time.deltaTime);
        float rotT = 1f - Mathf.Exp(-rotationSmooth * Time.deltaTime);

        // Smoothly position the camera
        transform.position = Vector3.Lerp(transform.position, desiredPosition, posT);

        // Smoothly rotate the camera to look at the target
        Quaternion desiredRotation = Quaternion.LookRotation(target.position - transform.position, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotT);
    }
}
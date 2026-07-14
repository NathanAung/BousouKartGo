using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 1f, -2f);
    public float positionSmooth = 10f;
    public float rotationSmooth = 10f;

    [SerializeField] Vector3 boostOffset = new Vector3(0f, 1f, -3.5f);
    [SerializeField] float boostOffsetSmooth = 0.3f;
    [SerializeField] float boostReturnSmooth = 4f;

    playerController player;
    Vector3 currentOffset;


    void Start()
    {
        currentOffset = offset;
        player = target.GetComponent<playerController>();
    }
    void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetOffset = offset;

        if (player != null && player.IsBoosting)
        {
            targetOffset = boostOffset;
        }

        float currentSmooth;

        if (player != null && player.IsBoosting)
        {
            currentSmooth = boostOffsetSmooth;
        }
        else
        {
            currentSmooth = boostReturnSmooth;
        }

        float offsetT = 1f - Mathf.Exp(-currentSmooth * Time.deltaTime);
        currentOffset = Vector3.Lerp(currentOffset, targetOffset, offsetT);

        // Calculate the ideal position relative to target's rotation
        Vector3 desiredPosition = target.position + (target.rotation * currentOffset);

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
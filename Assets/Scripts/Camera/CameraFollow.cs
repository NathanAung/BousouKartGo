using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] Transform target;

    [Header("Normal Camera")]
    [SerializeField] Vector3 offset = new Vector3(0f, 0.35f, -0.65f);
    [SerializeField] float positionSmooth = 10f;
    [SerializeField] float rotationSmooth = 10f;

    [Header("Boost Camera")]
    [SerializeField] Vector3 boostOffset = new Vector3(0f, 0.5f, -1.3f);
    [SerializeField] float boostOffsetSmooth = 0.8f;

    [Header("Drift Camera")]
    [SerializeField] float driftSideOffset = 0.35f;
    [SerializeField] float driftDistance = -0.4f;
    [SerializeField] float driftHeight = 0.35f;
    [SerializeField] float driftOffsetSmooth = 8f;

    [Header("Drift Rotation")]
    [SerializeField] float driftYaw = 8f;    // Turn toward inside of drift
    [SerializeField] float driftPitch = -5f; // Look slightly upward
    [SerializeField] float driftRoll = 3f;   // Camera tilt

    [Header("Boosting While Drifting")]
    [SerializeField] float boostDriftDistance = -0.75f;
    [SerializeField] float boostDriftHeight = 0.4f;

    [Header("Return")]
    [SerializeField] float returnSmooth = 8f;

    playerController player;
    Vector3 currentOffset;

    void Start()
    {
        currentOffset = offset;

        if (target != null)
        {
            player = target.GetComponent<playerController>();
        }
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        bool isBoosting = player != null && player.IsBoosting;
        bool isDriftingRight = player != null && player.IsDriftingRight;
        bool isDriftingLeft = player != null && player.IsDriftingLeft;
        bool isDrifting = isDriftingRight || isDriftingLeft;

        Vector3 targetOffset = offset;
        float currentSmooth = returnSmooth;

        // Boosting and drifting:
        // stay near the normal distance, but move toward the drift side.
        if (isBoosting && isDrifting)
        {
            float sideDirection = isDriftingRight ? 1f : -1f;

            targetOffset = new Vector3(
                driftSideOffset * sideDirection,
                boostDriftHeight,
                boostDriftDistance);

            currentSmooth = driftOffsetSmooth;
        }
        // Drifting without boosting:
        // quickly move closer and toward the drift side.
        else if (isDrifting)
        {
            float sideDirection = isDriftingRight ? 1f : -1f;

            targetOffset = new Vector3(
                driftSideOffset * sideDirection,
                driftHeight,
                driftDistance);

            currentSmooth = driftOffsetSmooth;
        }
        // Boosting without drifting:
        // gradually move farther away.
        else if (isBoosting)
        {
            targetOffset = boostOffset;
            currentSmooth = boostOffsetSmooth;
        }
        // Neither boosting nor drifting:
        // quickly return to the normal camera offset.
        else
        {
            targetOffset = offset;
            currentSmooth = returnSmooth;
        }

        float offsetT = 1f - Mathf.Exp(-currentSmooth * Time.deltaTime);

        currentOffset = Vector3.Lerp(
            currentOffset,
            targetOffset,
            offsetT);

        Vector3 desiredPosition =
            target.position + target.rotation * currentOffset;

        float positionT =
            1f - Mathf.Exp(-positionSmooth * Time.deltaTime);

        float rotationT =
            1f - Mathf.Exp(-rotationSmooth * Time.deltaTime);

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            positionT);

        Quaternion desiredRotation =
            Quaternion.LookRotation(target.position - transform.position, Vector3.up);

        Quaternion rotationOffset = Quaternion.identity;

        if (isDriftingRight)
        {
            rotationOffset = Quaternion.Euler(
                driftPitch,
                driftYaw,
                driftRoll);
        }
        else if (isDriftingLeft)
        {
            rotationOffset = Quaternion.Euler(
                driftPitch,
                -driftYaw,
                -driftRoll);
        }

        desiredRotation *= rotationOffset;

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            desiredRotation,
            rotationT);
    }
}
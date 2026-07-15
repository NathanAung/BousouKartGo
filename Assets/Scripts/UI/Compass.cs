using UnityEngine;

public class Compass : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] RectTransform compassNeedle;

    [SerializeField] float northOffset = 0f;
    [SerializeField] float rotationSmooth = 10f;

    void Update()
    {
        if (player == null || compassNeedle == null)
            return;

        float targetAngle = player.eulerAngles.y + northOffset;

        Quaternion targetRotation = Quaternion.Euler(
            0f,
            0f,
            targetAngle
        );

        float smoothAmount =
            1f - Mathf.Exp(-rotationSmooth * Time.deltaTime);

        compassNeedle.localRotation = Quaternion.Slerp(
            compassNeedle.localRotation,
            targetRotation,
            smoothAmount
        );
    }
}
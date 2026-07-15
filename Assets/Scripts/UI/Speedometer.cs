using UnityEngine;

public class Speedometer : MonoBehaviour
{
    [SerializeField] playerController player;
    [SerializeField] RectTransform needle;

    [SerializeField] float minAngle = 120f;
    [SerializeField] float maxAngle = -25f;
    [SerializeField] float maxBoostedAngle = -100f;

    void Update()
    {
        if (player == null || needle == null)
            return;

        float angle;

        if (player.CurrentSpeed <= player.MaxSpeed)
        {
            float speedPercent = player.CurrentSpeed / player.MaxSpeed;
            speedPercent = Mathf.Clamp01(speedPercent);

            angle = Mathf.Lerp(minAngle, maxAngle, speedPercent);
        }
        else
        {
            float boostedSpeedPercent = Mathf.InverseLerp(
                player.MaxSpeed,
                player.BoostedMaxSpeed,
                player.CurrentSpeed
            );

            angle = Mathf.Lerp(
                maxAngle,
                maxBoostedAngle,
                boostedSpeedPercent
            );
        }

        float shakeAmount = Mathf.Lerp(0.5f, 1f, player.CurrentSpeed / player.MaxSpeed);

        if (player.CurrentSpeed > player.MaxSpeed)
        {
            shakeAmount += Mathf.Lerp(
                0f,
                2f,
                Mathf.InverseLerp(player.MaxSpeed, player.BoostedMaxSpeed, player.CurrentSpeed)
            );
        }

        float shake = (Mathf.PerlinNoise(Time.time * 30f, 0f) - 0.5f) * 2f * shakeAmount;

        needle.localRotation = Quaternion.Euler(0f, 0f, angle + shake);
    }
}
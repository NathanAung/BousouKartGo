using UnityEngine;

public class BoostMeter : MonoBehaviour
{
    [SerializeField] playerController player;
    [SerializeField] RectTransform fill;

    [SerializeField] float fillHeight = 680f;
    [SerializeField] float pixelStep = 4f;

    float fullPositionY;

    void Start()
    {
        fullPositionY = fill.anchoredPosition.y;
    }

    void Update()
    {
        if (player == null || fill == null)
            return;

        float boostPercent = Mathf.Clamp01(player.BoostPercent);

        float hiddenAmount = (1f - boostPercent) * fillHeight;
        hiddenAmount = Mathf.Round(hiddenAmount / pixelStep) * pixelStep;

        float targetY = fullPositionY - hiddenAmount;

        fill.anchoredPosition = new Vector2(
            fill.anchoredPosition.x,
            targetY
        );
    }
}
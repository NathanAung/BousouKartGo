using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WinScreenUI : MonoBehaviour
{
    [Header("Player 1 Images")]
    [SerializeField] private Image p1Color1;
    [SerializeField] private Image p1Color2;

    [Header("Player 2 Images")]
    [SerializeField] private Image p2Color1;
    [SerializeField] private Image p2Color2;

    [Header("Flash Settings")]
    [SerializeField] private float flashInterval = 0.5f;

    private Coroutine flashCoroutine;

    private void Awake()
    {
        HideAllImages();
        gameObject.SetActive(false);
    }

    public void ShowWinner(int playerId)
    {
        gameObject.SetActive(true);
        HideAllImages();

        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        if (playerId == 0)
        {
            flashCoroutine = StartCoroutine(
                FlashImages(p1Color1, p1Color2)
            );
        }
        else if (playerId == 1)
        {
            flashCoroutine = StartCoroutine(
                FlashImages(p2Color1, p2Color2)
            );
        }
    }

    private IEnumerator FlashImages(Image firstImage, Image secondImage)
    {
        bool showFirst = true;

        while (true)
        {
            firstImage.enabled = showFirst;
            secondImage.enabled = !showFirst;

            showFirst = !showFirst;

            yield return new WaitForSecondsRealtime(flashInterval);
        }
    }

    private void HideAllImages()
    {
        p1Color1.enabled = false;
        p1Color2.enabled = false;
        p2Color1.enabled = false;
        p2Color2.enabled = false;
    }
}
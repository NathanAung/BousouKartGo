using UnityEngine;
using UnityEngine.UI;

public class Paolo : MonoBehaviour
{
    public Image uiImage;

    public Sprite sprite1;
    public Sprite sprite2;
    public Sprite sprite3;

    [Tooltip("Time in seconds between sprite changes")]
    public float speed = 0.3f;

    private Sprite[] frames;
    private int currentFrame;
    private float timer;

    void Start()
    {
        frames = new Sprite[]
        {
            sprite1,
            sprite2,
            sprite3,
            sprite2
        };

        uiImage.sprite = frames[0];
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= speed)
        {
            timer = 0f;

            currentFrame++;
            if (currentFrame >= frames.Length)
                currentFrame = 0;

            uiImage.sprite = frames[currentFrame];
        }
    }
}
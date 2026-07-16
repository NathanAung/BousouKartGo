using UnityEngine;

public class Blink : MonoBehaviour
{
    [SerializeField] GameObject target;
    [SerializeField] float blinkInterval = 0.5f;

    float timer;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= blinkInterval)
        {
            timer = 0f;
            target.SetActive(!target.activeSelf);
        }
    }
}
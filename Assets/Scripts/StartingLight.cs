using System.Collections;
using UnityEngine;

public class StartingLight : MonoBehaviour
{
    [Header("Light")]
    [SerializeField] private Renderer[] lightRenderers;
    [SerializeField] private Material offMaterial;
    [SerializeField] private Material onMaterial;

    [Header("Timing")]
    [SerializeField] private float turnOnTime = 1f;
    [SerializeField] private float turnOffTime = 4f;

    [Header("Floating")]
    [SerializeField] private float floatHeight = 0.08f;
    [SerializeField] private float floatSpeed = 2f;
    [SerializeField] private float sidewaysAmount = 0.03f;
    [SerializeField] private float sidewaysSpeed = 1.3f;
    [SerializeField] private float rotationAmount = 2f;
    [SerializeField] private float rotationSpeed = 1.5f;

    [Header("Fly Away")]
    [SerializeField] private Vector3 flyDirection = Vector3.up;
    [SerializeField] private float flySpeed = 15f;
    [SerializeField] private float flyAcceleration = 25f;
    [SerializeField] private float disappearDelay = 2f;

    private Vector3 startLocalPosition;
    private Quaternion startLocalRotation;

    private bool flyingAway = false;
    private float currentFlySpeed;
    private float randomOffset;

    private void Awake()
    {
        startLocalPosition = transform.localPosition;
        startLocalRotation = transform.localRotation;

        randomOffset = Random.Range(0f, 100f);

        SetOff();
    }

    private void Update()
    {
        if (flyingAway)
        {
            currentFlySpeed +=
                flyAcceleration * Time.deltaTime;

            transform.position +=
                transform.TransformDirection(flyDirection.normalized) *
                currentFlySpeed *
                Time.deltaTime;

            return;
        }

        float y =
            Mathf.Sin(
                Time.time * floatSpeed + randomOffset
            ) * floatHeight;

        float x =
            Mathf.Sin(
                Time.time * sidewaysSpeed + randomOffset
            ) * sidewaysAmount;

        transform.localPosition =
            startLocalPosition +
            new Vector3(x, y, 0f);

        float zRot =
            Mathf.Sin(
                Time.time * rotationSpeed + randomOffset
            ) * rotationAmount;

        transform.localRotation =
            startLocalRotation *
            Quaternion.Euler(0f, 0f, zRot);
    }

    public void BeginSequence()
    {
        StopAllCoroutines();
        StartCoroutine(SequenceRoutine());
    }

    private IEnumerator SequenceRoutine()
    {
        SetOff();

        yield return new WaitForSeconds(turnOnTime);

        SetOn();

        yield return new WaitForSeconds(
            Mathf.Max(0f, turnOffTime - turnOnTime)
        );

        SetOff();
    }

    public void FlyAway()
    {
        if (flyingAway)
            return;

        flyingAway = true;
        currentFlySpeed = flySpeed;

        StartCoroutine(DisappearRoutine());
    }

    private IEnumerator DisappearRoutine()
    {
        yield return new WaitForSeconds(disappearDelay);

        gameObject.SetActive(false);
    }

    private void SetOn()
    {
        foreach (Renderer r in lightRenderers)
        {
            r.material = onMaterial;
        }
    }

    private void SetOff()
    {
        foreach (Renderer r in lightRenderers)
        {
            r.material = offMaterial;
        }
    }
}
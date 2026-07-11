using UnityEngine;
using UnityEngine.InputSystem;

public class playerController : MonoBehaviour
{
    float horizontalInput = 0f;
    Rigidbody rb;

    bool isGrounded = false;

    CapsuleCollider capsuleCollider;
    [SerializeField] Transform orientation;
    [SerializeField] LayerMask layerMask;
    [SerializeField] float maxSpeed = 200f;
    [SerializeField] float acceleration = 8f;
    float currentSpeed = 0f;
    [SerializeField] Transform wheel1;
    [SerializeField] Transform wheel2;
    [SerializeField] Transform steeringWheel;

    Vector3 prevSafePosition;
    Vector3 latestSafePosition;

    Quaternion prevSafeRotation;
    Quaternion latestSafeRotation;

    bool hasFirstSafePosition = false;
    bool hasSecondSafePosition = false;

    [SerializeField] float safePositionSaveInterval = 0.8f;
    float safePositionTimer = 0f;


    RaycastHit hit;
    public InputAction move;
    float maxSteerAngle = 20f;
    public InputAction drift;
    bool isDrifting = false;

    [SerializeField] ParticleSystem driftSmokeLeftTurn1;
    [SerializeField] ParticleSystem driftSmokeLeftTurn2;
    [SerializeField] ParticleSystem driftSmokeRightTurn1;
    [SerializeField] ParticleSystem driftSmokeRightTurn2;

    private void OnEnable()
    {
        move.Enable();
        drift.Enable();
    }

    private void OnDisable()
    {
        move.Disable();
        drift.Disable();
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        InitSmoke(driftSmokeLeftTurn1);
        InitSmoke(driftSmokeLeftTurn2);
        InitSmoke(driftSmokeRightTurn1);
        InitSmoke(driftSmokeRightTurn2);
    }
    // Update is called once per frame
    void Update()
    {
        GetInput();
        CheckPosition();
        UpdateEffects();
    }

    void FixedUpdate()
    {
        MoveCar();
        
    }

    private void LateUpdate()
    {
        UpdateWheels();
    }

    void GetInput()
    {
        horizontalInput = move.ReadValue<float>();

        //verticalInput = Input.GetAxis("Vertical");
        Debug.Log($"{horizontalInput}");

        isDrifting = drift.IsPressed();
    }

    void MoveCar()
    {
        float speedPercent = currentSpeed / maxSpeed;

        float accelMultiplier = Mathf.Pow(1f - speedPercent, 2f);

        currentSpeed += acceleration * accelMultiplier * Time.fixedDeltaTime;
        currentSpeed = Mathf.Min(currentSpeed, maxSpeed);

        Vector3 moveVelocity = orientation.forward * currentSpeed;

        rb.linearVelocity = new Vector3(
            moveVelocity.x,
            rb.linearVelocity.y,
            moveVelocity.z
        );

        float yaw = rb.rotation.eulerAngles.y;

        if (isDrifting)
        {
            yaw += horizontalInput * 100f * Time.fixedDeltaTime;
        }
        else
        {
            yaw += horizontalInput * 50f * Time.fixedDeltaTime;
        }

        Quaternion uprightRotation = Quaternion.Euler(0f, yaw, 0f);
        rb.MoveRotation(uprightRotation);

        if (transform.position.y < 5.96f)
        {
            ResetCar();
        }
    }

    void ResetCar()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        currentSpeed = 0f;

        Vector3 respawnPosition;
        Quaternion respawnRotation;

        if (hasSecondSafePosition)
        {
            respawnPosition = Vector3.Lerp(
                prevSafePosition,
                latestSafePosition,
                0.5f
            );

            respawnRotation = Quaternion.Slerp(
                prevSafeRotation,
                latestSafeRotation,
                0.5f
            );

            if (Physics.Raycast(respawnPosition, Vector3.down, out hit, 10f, layerMask))
            {
                if (!hit.collider.CompareTag("Track"))
                {
                    respawnPosition = prevSafePosition;
                    respawnRotation = prevSafeRotation;
                }
            }


        }
        else if (hasFirstSafePosition)
        {
            respawnPosition = latestSafePosition;
            respawnRotation = latestSafeRotation;
        }
        else
        {
            Debug.LogWarning("No safe respawn position has been saved.");
            return;
        }

        rb.position = respawnPosition;
        rb.rotation = respawnRotation;

        rb.Sleep();
        rb.WakeUp();
    }

    void CheckPosition()
    {
        safePositionTimer += Time.deltaTime;

        if (safePositionTimer < safePositionSaveInterval || !isGrounded)
            return;

        safePositionTimer = 0f;

        if (Physics.Raycast(transform.position, Vector3.down, out hit, 10f, layerMask))
        {
            if (hit.collider.CompareTag("Track"))
            {
                SaveSafePosition(hit.point);
            }
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.collider.CompareTag("Track"))
        {
            isGrounded = true;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.collider.CompareTag("Track"))
        {
            isGrounded = false;
        }
    }

    void SaveSafePosition(Vector3 position)
    {
        Vector3 newSafePosition = position + Vector3.up * 1.5f;

        Quaternion newSafeRotation = Quaternion.Euler(
            0f,
            rb.rotation.eulerAngles.y,
            0f
        );

        if (!hasFirstSafePosition)
        {
            latestSafePosition = newSafePosition;
            latestSafeRotation = newSafeRotation;

            hasFirstSafePosition = true;
            return;
        }

        prevSafePosition = latestSafePosition;
        prevSafeRotation = latestSafeRotation;

        latestSafePosition = newSafePosition;
        latestSafeRotation = newSafeRotation;

        hasSecondSafePosition = true;
    }

    void UpdateWheels()
    {
        float steer;

        if (isDrifting)
        {
            steer = -horizontalInput * maxSteerAngle * 1.5f;
        }
        else 
        {
            steer = horizontalInput * maxSteerAngle;
        }




        wheel1.localRotation = Quaternion.Euler(0f, steer, 0f);
        wheel2.localRotation = Quaternion.Euler(0f, steer, 0f);
        steeringWheel.localRotation = Quaternion.Euler(steer, 0f, 0f);
    }


    

    void InitSmoke(ParticleSystem smoke)
    {
        if (!smoke) return;

        var emission = smoke.emission;
        emission.enabled = false;

        if (!smoke.isPlaying)
        {
            smoke.Play();
        }
    }

    void UpdateEffects()
    {
        bool driftingRight = isDrifting && horizontalInput > 0.1f;
        bool driftingLeft = isDrifting && horizontalInput < -0.1f;

        SetSmoke(driftSmokeRightTurn1, driftingRight);
        SetSmoke(driftSmokeRightTurn2, driftingRight);

        SetSmoke(driftSmokeLeftTurn1, driftingLeft);
        SetSmoke(driftSmokeLeftTurn2, driftingLeft);
    }

    void SetSmoke(ParticleSystem smoke, bool shouldPlay)
    {
        if (!smoke) return;

        var emission = smoke.emission;
        emission.enabled = shouldPlay;
    }

    //used to slow down player when encoutering obstacles
    public void MultiplySpeed(float multiplier)
    {
        currentSpeed *= multiplier;

        // Clamp just in case
        currentSpeed = Mathf.Clamp(currentSpeed, 0f, maxSpeed);
    }
}

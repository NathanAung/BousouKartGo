using UnityEngine;
using UnityEngine.InputSystem;

public class playerController : MonoBehaviour
{
    float horizontalInput = 0f;
    Rigidbody rb;

    bool isGrounded = false;
    bool isTouchingWall = false;
    float resetTimer = 0f;
    [SerializeField] float resetDelay = 2f;

    CapsuleCollider capsuleCollider;
    [SerializeField] Transform orientation;
    [SerializeField] LayerMask layerMask;
    [SerializeField] LayerMask safeLayer;
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

    [SerializeField] ParticleSystem windParticles;
    [SerializeField] float windMinimumSpeed = 50f;

    RaycastHit hit;
    public InputAction move;
    float maxSteerAngle = 20f;
    public InputAction drift;
    bool isDrifting = false;

    [SerializeField] ParticleSystem driftSmokeLeftTurn1;
    [SerializeField] ParticleSystem driftSmokeLeftTurn2;
    [SerializeField] ParticleSystem driftSmokeRightTurn1;
    [SerializeField] ParticleSystem driftSmokeRightTurn2;

    public InputAction boost;

    [SerializeField] float boostedMaxSpeed = 8f;
    [SerializeField] float boostAcceleration = 2f;
    [SerializeField] float boostSpeedDecay = 1.2f;

    public bool IsBoosting;

    [SerializeField] ParticleSystem booster1;
    [SerializeField] ParticleSystem booster2;
    [SerializeField] ParticleSystem booster3;
    [SerializeField] ParticleSystem booster4;

    private void OnEnable()
    {
        move.Enable();
        drift.Enable();
        boost.Enable();
    }

    private void OnDisable()
    {
        move.Disable();
        drift.Disable();
        boost.Disable();
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        InitEffect(driftSmokeLeftTurn1);
        InitEffect(driftSmokeLeftTurn2);
        InitEffect(driftSmokeRightTurn1);
        InitEffect(driftSmokeRightTurn2);
        InitEffect(windParticles);
        booster1.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        booster2.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        booster3.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        booster4.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
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
        IsBoosting = boost.IsPressed();
    }

    void MoveCar()
    {
        if (isGrounded)
        {
            rb.constraints =
                RigidbodyConstraints.FreezeRotationX |
                RigidbodyConstraints.FreezeRotationZ;
        }
        else
        {
            rb.constraints = RigidbodyConstraints.None;
        }

        if (IsBoosting)
        {
            float boostSpeedPercent = currentSpeed / boostedMaxSpeed;
            float boostAccelMultiplier = Mathf.Pow(
                Mathf.Clamp01(1f - boostSpeedPercent),
                2f
            );

            currentSpeed +=
                boostAcceleration *
                boostAccelMultiplier *
                Time.fixedDeltaTime;

            currentSpeed = Mathf.Min(currentSpeed, boostedMaxSpeed);
        }
        else if (currentSpeed > maxSpeed)
        {
            currentSpeed = Mathf.MoveTowards(
                currentSpeed,
                maxSpeed,
                boostSpeedDecay * Time.fixedDeltaTime
            );
        }
        else
        {
            float speedPercent = currentSpeed / maxSpeed;

            float accelMultiplier = Mathf.Pow(
                Mathf.Clamp01(1f - speedPercent),
                2f
            );

            currentSpeed += acceleration * accelMultiplier * Time.fixedDeltaTime;

            currentSpeed = Mathf.Min(currentSpeed, maxSpeed);
        }

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

        if (isTouchingWall)
        {
            resetTimer += Time.fixedDeltaTime;
            if (resetTimer >= resetDelay)
            {
                ResetCar();
                resetTimer = 0f;
            }
        }
        else
        {
            resetTimer = 0f;
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
            respawnPosition = prevSafePosition;
            respawnRotation = prevSafeRotation;
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

        if (Physics.Raycast(rb.position, Vector3.down, out hit, 10f, safeLayer))
        {
            SaveSafePosition(hit.point);

        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (!collision.collider.CompareTag("Track"))
            return;

        isGrounded = false;
        isTouchingWall = false;

        foreach (ContactPoint contact in collision.contacts)
        {
            Debug.DrawRay(contact.point, contact.normal, Color.red);

            // Upward-facing contact = ground
            if (contact.normal.y > 0.5f)
            {
                isGrounded = true;
            }

            // Mostly horizontal contact = wall, pillar, or track side
            if (Mathf.Abs(contact.normal.y) < 0.5f)
            {
                isTouchingWall = true;
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (!collision.collider.CompareTag("Track"))
            return;

        isGrounded = false;
        isTouchingWall = false;
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




    void InitEffect(ParticleSystem smoke)
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
        bool driftingRight =
            isGrounded &&
            isDrifting &&
            horizontalInput > 0.1f;

        bool driftingLeft =
            isGrounded &&
            isDrifting &&
            horizontalInput < -0.1f;

        SetEffect(driftSmokeRightTurn1, driftingRight);
        SetEffect(driftSmokeRightTurn2, driftingRight);

        SetEffect(driftSmokeLeftTurn1, driftingLeft);
        SetEffect(driftSmokeLeftTurn2, driftingLeft);

        bool shouldShowWind = currentSpeed >= windMinimumSpeed;

        SetEffect(windParticles, shouldShowWind);

        SetBoosterEffect(booster1, IsBoosting);
        SetBoosterEffect(booster2, IsBoosting);
        SetBoosterEffect(booster3, IsBoosting);
        SetBoosterEffect(booster4, IsBoosting);
    }

    void SetEffect(ParticleSystem smoke, bool shouldPlay)
    {
        if (!smoke) return;

        var emission = smoke.emission;
        emission.enabled = shouldPlay;
    }

    void SetBoosterEffect(ParticleSystem effect, bool shouldPlay)
    {
        if (!effect) return;

        if (shouldPlay)
        {
            if (!effect.isPlaying)
                effect.Play(true);
        }
        else
        {
            if (effect.isPlaying)
            {
                effect.Stop(
                    true,
                    ParticleSystemStopBehavior.StopEmittingAndClear
                );
            }
        }
    }

    public void MultiplySpeed(float multiplier)
    {
        currentSpeed *= multiplier;

        currentSpeed = Mathf.Clamp(
            currentSpeed,
            0f,
            boostedMaxSpeed
        );
    }
}

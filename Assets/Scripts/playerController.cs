
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class playerController : MonoBehaviour
{
    [Header("Core References")]
    [SerializeField] Transform orientation;

    Rigidbody rb;
    CapsuleCollider capsuleCollider;
    RaycastHit hit;


    [Header("Input")]
    private PlayerInput playerInput;
    private InputAction move;
    private InputAction drift;
    private InputAction boost;

    float horizontalInput = 0f;
    bool isDrifting = false;
    bool driftingRight = false;
    bool driftingLeft = false;

    public bool IsDriftingRight => driftingRight;
    public bool IsDriftingLeft => driftingLeft;

    [Header("Boost")]
    [SerializeField] float maxBoost = 100f;
    [SerializeField] float boostDrainRate = 30f;
    [SerializeField] float boostRechargeRate = 15f;
    [SerializeField] float boostRechargeDelay = 1f;

    float currentBoost;
    float boostRechargeTimer;
    bool boostPressed;
    bool isBoosting;
    bool boostDepleted;
    public float CurrentBoost => currentBoost;
    public float MaxBoost => maxBoost;
    public float BoostPercent => currentBoost / maxBoost;
    public bool IsBoosting => isBoosting;


    [Header("Movement")]
    [SerializeField] float maxSpeed = 4f;
    [SerializeField] float acceleration = 0.8f;
    [SerializeField] float boostedMaxSpeed = 6f;
    [SerializeField] float boostAcceleration = 1f;
    [SerializeField] float boostSpeedDecay = 1.2f;

    float currentSpeed = 0f;
    public float CurrentSpeed => currentSpeed;
    public float MaxSpeed => maxSpeed;
    public float BoostedMaxSpeed => boostedMaxSpeed;


    [Header("Steering Visuals")]
    [SerializeField] Transform wheel1;
    [SerializeField] Transform wheel2;
    [SerializeField] Transform steeringWheel;

    float maxSteerAngle = 20f;


    [Header("Ground and Wall Detection")]
    [SerializeField] LayerMask layerMask;
    [SerializeField] float driftGroundGraceTime = 0.15f;

    bool isGrounded = false;
    bool isTouchingWall = false;

    float lastGroundedTime;


    [Header("Reset Settings")]
    [SerializeField] float resetDelay = 2f;

    float resetTimer = 0f;


    [Header("Safe Position Saving")]
    [SerializeField] LayerMask safeLayer;
    [SerializeField] float safePositionSaveInterval = 0.8f;

    Vector3 prevSafePosition;
    Vector3 latestSafePosition;

    Quaternion prevSafeRotation;
    Quaternion latestSafeRotation;

    bool hasFirstSafePosition = false;
    bool hasSecondSafePosition = false;

    float safePositionTimer = 0f;


    [Header("Wind Effect")]
    [SerializeField] ParticleSystem windParticles;
    [SerializeField] float windMinimumSpeed = 50f;


    [Header("Drift Smoke Effects")]
    [SerializeField] ParticleSystem driftSmokeLeftTurn1;
    [SerializeField] ParticleSystem driftSmokeLeftTurn2;
    [SerializeField] ParticleSystem driftSmokeRightTurn1;
    [SerializeField] ParticleSystem driftSmokeRightTurn2;


    [Header("Booster Effects")]
    [SerializeField] ParticleSystem booster1;
    [SerializeField] ParticleSystem booster2;
    [SerializeField] ParticleSystem booster3;
    [SerializeField] ParticleSystem booster4;
    [SerializeField] ParticleSystem booster5;

    [Header("Engine Audio")]
    [SerializeField] AudioSource engineAudio;
    [SerializeField] float minimumEngineVolume = 0.1f;
    [SerializeField] float maximumEngineVolume = 0.5f;
    [SerializeField] float engineVolumeSmooth = 5f;

    [Header("Boost Audio")]
    [SerializeField] AudioSource boostAudio;
    bool wasBoosting = false;

    [Header("Drift Audio")]
    [SerializeField] AudioSource driftAudio;
    [SerializeField] float maximumDriftVolume = 0.8f;
    [SerializeField] float driftVolumeSmooth = 5f;
    bool wasDrifting = false;

    [Header("Victory Donut")]
    [SerializeField] float victoryDonutSpeed = 1.2f;
    [SerializeField] float victoryDonutTurnSpeed = 140f;
    bool controlsLocked = false;
    bool isDoingVictoryDonut = false;
    float victoryDonutDirection = 1f;
    [SerializeField] Transform victoryRespawnPoint;
    bool useVictoryRespawn = false;


    [Header("Loser Sequence")]
    [SerializeField] GameObject loserExplosionPrefab;
    [SerializeField] Transform loserExplosionPoint;
    [SerializeField] ParticleSystem loserSmoke;
    [SerializeField] float loserSlowdownDuration = 1f;

    bool isLosing = false;




    // Gets this player's individually assigned input actions.
    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        move = playerInput.actions["Move"];
        drift = playerInput.actions["Drift"];
        boost = playerInput.actions["Boost"];
    }

    // Gets required components and prepares all particle effects.
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        InitEffect(driftSmokeLeftTurn1);
        InitEffect(driftSmokeLeftTurn2);
        InitEffect(driftSmokeRightTurn1);
        InitEffect(driftSmokeRightTurn2);
        InitEffect(windParticles);

        InitEffect(loserSmoke);

        booster1.Stop(
            true,
            ParticleSystemStopBehavior.StopEmittingAndClear
        );

        booster2.Stop(
            true,
            ParticleSystemStopBehavior.StopEmittingAndClear
        );

        booster3.Stop(
            true,
            ParticleSystemStopBehavior.StopEmittingAndClear
        );

        booster4.Stop(
            true,
            ParticleSystemStopBehavior.StopEmittingAndClear
        );

        booster5.Stop(
            true,
            ParticleSystemStopBehavior.StopEmittingAndClear
        );

        StartEngineAudio();

        currentBoost = maxBoost;

        //loserExplosion.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        //loserSmoke

       
    }


    // Reads input, saves safe positions, and updates visual and sound effects each frame.
    void Update()
    {
        if (isGrounded)
        {
            lastGroundedTime = Time.time;
        }
        GetInput();
        CheckPosition();
        UpdateBoost();
        UpdateEffects();
        UpdateSFX();
    }


    // Handles physics-based movement at a fixed interval.
    void FixedUpdate()
    {
        MoveCar();
    }


    // Updates steering visuals after the car has moved.
    private void LateUpdate()
    {
        UpdateWheels();
    }


    // Reads steering, drifting, and boosting input states.
    void GetInput()
    {
        if (isDoingVictoryDonut)
        {
            horizontalInput = victoryDonutDirection;
            isDrifting = true;
            boostPressed = false;
            return;
        }

        if (controlsLocked)
        {
            horizontalInput = 0f;
            isDrifting = false;
            boostPressed = false;
            return;
        }

        horizontalInput = move.ReadValue<float>();
        isDrifting = drift.IsPressed();
        boostPressed = boost.IsPressed();
    }

    public void LockControls()
    {
        controlsLocked = true;

        horizontalInput = 0f;
        isDrifting = false;
        boostPressed = false;
        isBoosting = false;
    }

    // Controls acceleration, velocity, steering, rotation, and reset conditions.
    void MoveCar()
    {
        // Lock the car upright while grounded and allow free rotation in the air.
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

        // Accelerate toward the boosted maximum speed while boosting.
        if (isLosing)
        {
            // Speed is controlled by the loser slowdown coroutine.
        }
        else if (isDoingVictoryDonut)
        {
            currentSpeed = victoryDonutSpeed;
        }
        else if (isBoosting)
        {
            float boostSpeedPercent = currentSpeed / boostedMaxSpeed;

            float boostAccelMultiplier = Mathf.Pow(
                Mathf.Clamp01(1f - boostSpeedPercent),
                1.1f
            );

            currentSpeed +=
                boostAcceleration *
                boostAccelMultiplier *
                Time.fixedDeltaTime;

            currentSpeed = Mathf.Min(
                currentSpeed,
                boostedMaxSpeed
            );
        }
        else if (currentSpeed > maxSpeed)
        {
            currentSpeed = Mathf.MoveTowards(
                currentSpeed,
                maxSpeed,
                boostSpeedDecay * Time.fixedDeltaTime
            );
        }
        // Apply normal acceleration with reduced acceleration near maximum speed.
        else
        {
            float speedPercent = currentSpeed / maxSpeed;

            float accelMultiplier = Mathf.Pow(
                Mathf.Clamp01(1f - speedPercent),
                2f
            );

            currentSpeed +=
                acceleration *
                accelMultiplier *
                Time.fixedDeltaTime;

            currentSpeed = Mathf.Min(
                currentSpeed,
                maxSpeed
            );
        }

        // Apply forward movement while preserving vertical velocity.
        Vector3 moveVelocity =
            orientation.forward * currentSpeed;

        rb.linearVelocity = new Vector3(
            moveVelocity.x,
            rb.linearVelocity.y,
            moveVelocity.z
        );

        // Rotate the car horizontally using a faster turning speed while drifting.
        float yaw = rb.rotation.eulerAngles.y;

        if (isDoingVictoryDonut)
        {
            yaw +=
                victoryDonutDirection *
                victoryDonutTurnSpeed *
                Time.fixedDeltaTime;
        }
        else if (driftingRight || driftingLeft)
        {
            yaw +=
                horizontalInput *
                100f *
                Time.fixedDeltaTime;
        }
        else
        {
            yaw +=
                horizontalInput *
                50f *
                Time.fixedDeltaTime;
        }

        Quaternion uprightRotation =
            Quaternion.Euler(0f, yaw, 0f);

        rb.MoveRotation(uprightRotation);

        // Reset the car if it falls below the track.
        if (transform.position.y < 5.96f)
        {
            ResetCar();
        }

        // Reset the car after it remains against a wall for too long.
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

    // Drains boost while active and recharges it after a short delay.
    void UpdateBoost()
    {
        // Require the player to release boost after fully depleting it.
        if (!boostPressed)
        {
            boostDepleted = false;
        }

        isBoosting =
            boostPressed &&
            !boostDepleted &&
            currentBoost > 0f;

        if (isBoosting)
        {
            currentBoost -= boostDrainRate * Time.deltaTime;
            boostRechargeTimer = boostRechargeDelay;

            if (currentBoost <= 0f)
            {
                currentBoost = 0f;
                isBoosting = false;
                boostDepleted = true;
            }
        }
        else
        {
            boostRechargeTimer -= Time.deltaTime;

            if (boostRechargeTimer <= 0f)
            {
                currentBoost += boostRechargeRate * Time.deltaTime;
                currentBoost = Mathf.Min(currentBoost, maxBoost);
            }
        }
    }


    // Restores the car to a previously saved safe position.
    void ResetCar()
    {
        StopEngineAudio();

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        currentSpeed = 0f;

        Vector3 respawnPosition;
        Quaternion respawnRotation;

        if (useVictoryRespawn && victoryRespawnPoint != null)
        {
            respawnPosition = victoryRespawnPoint.position;
            respawnRotation = victoryRespawnPoint.rotation;
        }
        else if (hasSecondSafePosition)
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
            Debug.LogWarning(
                "No safe respawn position has been saved."
            );

            return;
        }

        rb.position = respawnPosition;
        rb.rotation = respawnRotation;

        rb.Sleep();
        rb.WakeUp();

        StartEngineAudio();

        //reset boost audio
        boostAudio.Stop();
        boostAudio.time = 0f;
        wasBoosting = false;
        //reset drift audio
        driftAudio.Stop();
        driftAudio.time = 0f;
        driftAudio.volume = 0f;
        wasDrifting = false;
    }


    // Periodically checks for valid ground beneath the car and saves its position.
    void CheckPosition()
    {
        if (useVictoryRespawn)
        {
            return;
        }

        safePositionTimer += Time.deltaTime;

        if (
            safePositionTimer < safePositionSaveInterval ||
            !isGrounded
        )
        {
            return;
        }

        safePositionTimer = 0f;

        if (
            Physics.Raycast(
                rb.position,
                Vector3.down,
                out hit,
                10f,
                safeLayer
            )
        )
        {
            SaveSafePosition(hit.point);
        }
    }


    // Stores the latest safe position while retaining the previous one.
    void SaveSafePosition(Vector3 position)
    {
        Vector3 newSafePosition =
            position + Vector3.up * 1.5f;

        Quaternion newSafeRotation =
            Quaternion.Euler(
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

    // Allows drifting to continue briefly after losing ground contact.
    bool CanDriftOnGround()
    {
        return isGrounded ||
               Time.time - lastGroundedTime <= driftGroundGraceTime;
    }


    // Detects whether the car is touching the ground or a track wall.
    void OnCollisionStay(Collision collision)
    {
        if (!collision.collider.CompareTag("Track"))
        {
            return;
        }

        isGrounded = false;
        isTouchingWall = false;

        foreach (ContactPoint contact in collision.contacts)
        {
            Debug.DrawRay(
                contact.point,
                contact.normal,
                Color.red
            );

            if (contact.normal.y > 0.5f)
            {
                isGrounded = true;
            }

            if (Mathf.Abs(contact.normal.y) < 0.5f)
            {
                isTouchingWall = true;
            }
        }
    }


    // Clears ground and wall contact states after leaving the track.
    void OnCollisionExit(Collision collision)
    {
        if (!collision.collider.CompareTag("Track"))
        {
            return;
        }

        isGrounded = false;
        isTouchingWall = false;
    }


    // Rotates the wheel and steering-wheel visuals to match steering input.
    void UpdateWheels()
    {
        float steer;

        if (driftingRight || driftingLeft)
        {
            steer =
                -horizontalInput *
                maxSteerAngle *
                1.5f;
        }
        else
        {
            steer =
                horizontalInput *
                maxSteerAngle;
        }

        wheel1.localRotation =
            Quaternion.Euler(0f, steer, 0f);

        wheel2.localRotation =
            Quaternion.Euler(0f, steer, 0f);

        steeringWheel.localRotation =
            Quaternion.Euler(0f, -90f, -90f + steer);
    }


    // Initializes a looping particle effect with its emission disabled.
    void InitEffect(ParticleSystem smoke)
    {
        if (!smoke)
        {
            return;
        }

        var emission = smoke.emission;
        emission.enabled = false;

        if (!smoke.isPlaying)
        {
            smoke.Play();
        }
    }


    // Updates drift smoke, wind, and booster effects based on the car's state.
    void UpdateEffects()
    {
        bool hasDriftGroundContact = CanDriftOnGround();

        driftingRight =
            hasDriftGroundContact &&
            isDrifting &&
            horizontalInput > 0.1f;

        driftingLeft =
            hasDriftGroundContact &&
            isDrifting &&
            horizontalInput < -0.1f;

        SetEffect(
            driftSmokeRightTurn1,
            driftingRight
        );

        SetEffect(
            driftSmokeRightTurn2,
            driftingRight
        );

        SetEffect(
            driftSmokeLeftTurn1,
            driftingLeft
        );

        SetEffect(
            driftSmokeLeftTurn2,
            driftingLeft
        );

        bool shouldShowWind =
            currentSpeed >= windMinimumSpeed;

        SetEffect(
            windParticles,
            shouldShowWind
        );

        SetBoosterEffect(
            booster1,
            isBoosting
        );

        SetBoosterEffect(
            booster2,
            isBoosting
        );

        SetBoosterEffect(
            booster3,
            isBoosting
        );

        SetBoosterEffect(
            booster4,
            isBoosting
        );

        SetBoosterEffect(
            booster5,
            isBoosting
        );
    }


    // Enables or disables emission for a looping particle effect.
    void SetEffect(
        ParticleSystem smoke,
        bool shouldPlay
    )
    {
        if (!smoke)
        {
            return;
        }

        var emission = smoke.emission;
        emission.enabled = shouldPlay;
    }


    // Starts or completely clears a booster particle effect.
    void SetBoosterEffect(
        ParticleSystem effect,
        bool shouldPlay
    )
    {
        if (!effect)
        {
            return;
        }

        if (shouldPlay)
        {
            if (!effect.isPlaying)
            {
                effect.Play(true);
            }
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


    // Multiplies the current speed while keeping it within the allowed range.
    public void MultiplySpeed(float multiplier)
    {
        currentSpeed *= multiplier;

        currentSpeed = Mathf.Clamp(
            currentSpeed,
            0f,
            boostedMaxSpeed
        );
    }

    // Updates the SFX
    void UpdateSFX()
    {
        UpdateEngineAudio();
        UpdateBoostAudio();
        UpdateDriftAudio();
    }

    // Starts the looping engine sound.
    void StartEngineAudio()
    {
        if (engineAudio == null || engineAudio.isPlaying)
            return;

        engineAudio.volume = minimumEngineVolume;
        engineAudio.Play();
    }

    // Adjusts engine volume based on the car's current speed.
    void UpdateEngineAudio()
    {
        if (engineAudio == null || !engineAudio.isPlaying)
            return;

        float speedPercent = Mathf.Clamp01(currentSpeed / maxSpeed);

        float targetVolume = Mathf.Lerp(
            minimumEngineVolume,
            maximumEngineVolume,
            speedPercent
        );

        engineAudio.volume = Mathf.Lerp(
            engineAudio.volume,
            targetVolume,
            engineVolumeSmooth * Time.deltaTime
        );
    }

    // Stops the engine sound and rewinds it to the beginning.
    void StopEngineAudio()
    {
        if (engineAudio == null)
            return;

        engineAudio.Stop();
        engineAudio.time = 0f;
    }

    // Plays and stops the boost sound when boosting starts or ends.
    void UpdateBoostAudio()
    {
        // Started boosting
        if (isBoosting && !wasBoosting)
        {
            boostAudio.time = 0f;
            boostAudio.Play();
        }

        // Stopped boosting
        if (!isBoosting && wasBoosting)
        {
            boostAudio.Stop();
        }

        wasBoosting = isBoosting;
    }

    // Plays and fades the tyre screech while the car is drifting.
    void UpdateDriftAudio()
    {
        bool shouldPlayDriftAudio = isDrifting && CanDriftOnGround();

        // Drift just started.
        if (shouldPlayDriftAudio && !wasDrifting)
        {
            driftAudio.volume = 0f;
            driftAudio.time = 0f;
            driftAudio.Play();
        }

        float targetVolume = shouldPlayDriftAudio
            ? maximumDriftVolume
            : 0f;

        driftAudio.volume = Mathf.MoveTowards(
            driftAudio.volume,
            targetVolume,
            driftVolumeSmooth * Time.deltaTime
        );

        // Stop the clip once it has faded out completely.
        if (!shouldPlayDriftAudio &&
            driftAudio.isPlaying &&
            driftAudio.volume <= 0f)
        {
            driftAudio.Stop();
            driftAudio.time = 0f;
        }

        wasDrifting = shouldPlayDriftAudio;
    }

    public void StartVictoryDonut(float delay)
    {
        StartCoroutine(StartVictoryDonutRoutine(delay));
    }

    private IEnumerator StartVictoryDonutRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        isDoingVictoryDonut = true;

        victoryDonutDirection = -1f;

        boostPressed = false;
        isBoosting = false;
    }

    public void SetVictoryRespawnPoint(Transform respawnPoint)
    {
        victoryRespawnPoint = respawnPoint;
        useVictoryRespawn = true;
    }

    public void StartLoserSequence()
    {
        if (isLosing)
            return;

        isLosing = true;

        LockControls();

        GameObject explosion = Instantiate(
            loserExplosionPrefab,
            loserExplosionPoint.position,
            loserExplosionPoint.rotation,
            loserExplosionPoint
        );

        explosion.transform.localScale = Vector3.one * 0.1f;

        SetEffect(loserSmoke, true);

        StartCoroutine(SlowLoserCar());
    }

    private IEnumerator SlowLoserCar()
    {
        float startingSpeed = currentSpeed;
        float elapsedTime = 0f;

        while (elapsedTime < loserSlowdownDuration)
        {
            elapsedTime += Time.deltaTime;

            float t = Mathf.Clamp01(
                elapsedTime / loserSlowdownDuration
            );

            currentSpeed = Mathf.Lerp(
                startingSpeed,
                0f,
                t
            );

            yield return null;
        }

        currentSpeed = 0f;

        rb.linearVelocity = new Vector3(
            0f,
            rb.linearVelocity.y,
            0f
        );
    }
}


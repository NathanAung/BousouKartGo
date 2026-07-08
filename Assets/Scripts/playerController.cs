using UnityEngine;
using UnityEngine.InputSystem;

public class playerController : MonoBehaviour
{
    float horizontalInput = 0f;
    Rigidbody rb;
    //BoxCollider boxCollider;
    [SerializeField] Transform orientation;
    [SerializeField] LayerMask layerMask;
    [SerializeField] float maxSpeed = 200f;
    [SerializeField] float accelerateForce = 750f;
    [SerializeField] Transform wheel1;
    [SerializeField] Transform wheel2;
    Vector3 safePosition;
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
        rb.AddForce(orientation.forward * accelerateForce * Time.fixedDeltaTime, ForceMode.Force);
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

      

        

        if (transform.position.y < 5.95f)
        {
            ResetCar();
        }

        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }



    }

    void ResetCar()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.position = safePosition + Vector3.up * 0.5f;

        rb.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);

        rb.Sleep();
        rb.WakeUp();
    }

    void CheckPosition()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 10f, layerMask))
        {
            if (hit.collider != null && hit.collider.CompareTag("OutOfTrackBounds"))
            {
                safePosition = hit.point + Vector3.up * 1.5f;
            }
        }
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
}

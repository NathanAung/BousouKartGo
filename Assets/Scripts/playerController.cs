using UnityEngine;
using UnityEngine.InputSystem;

public class playerController : MonoBehaviour
{
    float horizontalInput = 0f;
    Rigidbody rb;
    //BoxCollider boxCollider;
    [SerializeField] Transform orientation;
    [SerializeField] LayerMask layerMask;
    [SerializeField] float maxSpeed = 13f;
    [SerializeField] Transform wheel1;
    [SerializeField] Transform wheel2;
    Vector3 safePosition;
    RaycastHit hit;
    public InputAction move;
    float maxSteerAngle = 20f;

    private void OnEnable()
    {
        move.Enable();
    }

    private void OnDisable()
    {
        move.Disable();
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        //boxCollider = GetComponent<BoxCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        GetInput();
        CheckPosition();
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
    }

    void MoveCar()
    {
        rb.AddForce(orientation.forward * 750f * Time.fixedDeltaTime, ForceMode.Force);
        float yaw = rb.rotation.eulerAngles.y;
        yaw += horizontalInput * 50f * Time.fixedDeltaTime;

        Quaternion uprightRotation = Quaternion.Euler(0f, yaw, 0f);
        rb.MoveRotation(uprightRotation);

        //Debug.Log(rb.velocity.magnitude);
        if (rb.linearVelocity.magnitude > 13f)
            rb.linearVelocity = rb.linearVelocity.normalized * 13f;

        

        if (transform.position.y < 5f)
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
        float steer = horizontalInput * maxSteerAngle;

        wheel1.localRotation = Quaternion.Euler(0f, steer, 0f);
        wheel2.localRotation = Quaternion.Euler(0f, steer, 0f);
    }

}

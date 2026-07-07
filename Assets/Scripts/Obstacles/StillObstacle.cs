using UnityEngine;

public class StillObstacle : MonoBehaviour
{
    [Header("Impact Settings")]
    [SerializeField] private float blowAwayForce = 15f;
    [SerializeField] private float playerSpeedMultiplier = 0.7f;
    [SerializeField] private float upwardLift = 2f;

    private Rigidbody rb;
    private bool hasBeenHit = false; // Prevents the obstacle from triggering multiple times

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasBeenHit) return;

        // Check if the object hitting us is the Player
        if (collision.gameObject.CompareTag("Player"))
        {
            hasBeenHit = true;
            gameObject.layer = LayerMask.NameToLayer("Debris");

            // Slow down the player (Brake)
            Rigidbody playerRb = collision.gameObject.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                playerRb.linearVelocity *= playerSpeedMultiplier;
            }

            // Calculate the blow-away direction
            // Subtracting player position from our position gives us the exact vector pointing away from the impact
            Vector3 pushDirection = transform.position - collision.transform.position;
            pushDirection.y = upwardLift;

            // 3. Apply the force and a random spin for chaotic visuals
            rb.AddForce(pushDirection.normalized * blowAwayForce, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * blowAwayForce, ForceMode.Impulse);

            // destroy the obstacle after 5 seconds to clean up the scene
            Destroy(gameObject, 5f);
        }
    }
}

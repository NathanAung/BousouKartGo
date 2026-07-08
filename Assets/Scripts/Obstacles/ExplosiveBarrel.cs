using UnityEngine;

public class ExplosiveBarrel : MonoBehaviour
{
    [Header("Explosion Settings")]
    [SerializeField] private float explosionForce = 800f;
    [SerializeField] private float explosionRadius = 15f;
    [SerializeField] private float upwardModifier = 3f;

    [SerializeField] private float playerSpeedMultiplier = 0.4f;

    [SerializeField] private GameObject explosionEffectPrefab;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Slow down the player heavily
            Rigidbody playerRb = collision.gameObject.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                playerRb.linearVelocity *= playerSpeedMultiplier;
            }


            // Spawn the visual explosion
            if (explosionEffectPrefab != null)
            {
                GameObject effect = Instantiate(
                    explosionEffectPrefab,
                    transform.position,
                    Quaternion.identity
                );

                ParticleSystem ps = effect.GetComponentInChildren<ParticleSystem>();

                if (ps != null)
                {
                    ps.Play();
                    Destroy(effect, ps.main.duration + ps.main.startLifetime.constantMax);
                }
            }

            // AoE Physics Explosion
            // Find all colliders within the radius
            Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

            foreach (Collider nearbyObject in colliders)
            {
                Rigidbody nearbyRb = nearbyObject.GetComponent<Rigidbody>();

                // If the object has a Rigidbody, blast it away
                if (nearbyRb != null)
                {
                    nearbyRb.AddExplosionForce(explosionForce, transform.position, explosionRadius, upwardModifier, ForceMode.Impulse);

                }
            }

            Destroy(gameObject);
        }
    }
}

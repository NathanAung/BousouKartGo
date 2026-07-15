using UnityEngine;

public class ExplosiveBarrel : MonoBehaviour
{
    [Header("Explosion Settings")]
    [SerializeField] private float explosionForce = 800f;
    [SerializeField] private float explosionRadius = 15f;
    [SerializeField] private float upwardModifier = 3f;

    [SerializeField] private float playerSpeedMultiplier = 0.4f;

    [SerializeField] private GameObject explosionEffectPrefab;
    [SerializeField] private AudioClip explosionClip;
    [SerializeField] private AudioSource obstacleSFXSource;

    private bool hasExploded = false;


    // Explodes when hit by a player.
    private void OnCollisionEnter(Collision collision)
    {
        if (
            hasExploded ||
            !collision.gameObject.CompareTag("Player")
        )
        {
            return;
        }

        hasExploded = true;

        // Slow down the player.
        Rigidbody playerRb = collision.gameObject.GetComponent<Rigidbody>();

        if (playerRb != null)
        {
            playerRb.linearVelocity *= playerSpeedMultiplier;
        }

        // Spawn the explosion visual effect.
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

        // Apply explosion force to nearby rigidbodies.
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider nearbyObject in colliders)
        {
            Rigidbody nearbyRb = nearbyObject.GetComponent<Rigidbody>();

            if (nearbyRb != null)
            {
                nearbyRb.AddExplosionForce(
                    explosionForce,
                    transform.position,
                    explosionRadius,
                    upwardModifier,
                    ForceMode.Impulse
                );
            }
        }

        // Hide the barrel and disable further collisions.
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();

        foreach (MeshRenderer barrelRenderer in renderers)
        {
            barrelRenderer.enabled = false;
        }

        Collider[] barrelColliders = GetComponentsInChildren<Collider>();

        foreach (Collider barrelCollider in barrelColliders)
        {
            barrelCollider.enabled = false;
        }

        // Play the sound and destroy the hidden barrel after it finishes.
        if (
            explosionClip != null &&
            obstacleSFXSource != null
        )
        {
            obstacleSFXSource.PlayOneShot(explosionClip);
            Destroy(gameObject, explosionClip.length);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}


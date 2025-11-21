using UnityEngine;

public class AxeHit : MonoBehaviour
{
    [Header("Axe Settings")]
    public Transform axeBlade;               // Reference to axe blade (Metal)
    public float breakThreshold = 6.0f;     // Minimum speed required to break logs

    [Header("Sound Settings")]
    public AudioSource hitSound;             // Sound for successful hit
    public AudioSource stuckSound;           // Sound when axe gets stuck

    [Header("Log Settings")]
    public GameObject brokenLogPrefab;       // Prefab to instantiate when log breaks
    public float logHalfOffset = 0.2f;       // Distance to separate halves
    public float logSpawnHeight = 0.2f;      // Height above collision point

    private Vector3 lastPosition;            // Last frame position for velocity calculation
    private Vector3 currentVelocity;         // Current velocity based on movement
    private Rigidbody rb;

    // For tracking stuck state
    private FixedJoint stuckJoint = null;
    private GameObject stuckLog = null;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogWarning("Rigidbody not found on the axe! Velocity will be tracked manually.");
        }

        lastPosition = transform.position; // Initialize last position
    }

    void Update()
    {
        // Calculate velocity based on change in position
        currentVelocity = (transform.position - lastPosition) / Time.deltaTime;
        lastPosition = transform.position;
    }

    void OnCollisionEnter(Collision collision)
    {
        // Handle normal log
        Debug.Log($"Axe collided with: {collision.gameObject.name}");
        if (collision.gameObject.CompareTag("Log") && stuckLog == null && stuckJoint == null)
        {
            float impactForce = currentVelocity.magnitude;
            Debug.Log($"Impact speed: {impactForce}");

            if (impactForce >= breakThreshold)
            {
                // Strong hit: break the log
                if (hitSound != null) hitSound.Play();

                BreakLog(collision.gameObject, collision.transform);
                
            }
            else
            {
                // Weak hit: stick the axe
                if (stuckSound != null) stuckSound.Play();

                Rigidbody logRb = collision.rigidbody;
                if (logRb != null)
                {
                    stuckJoint = gameObject.AddComponent<FixedJoint>();
                    stuckJoint.connectedBody = logRb;
                    stuckJoint.breakForce = Mathf.Infinity;
                    stuckJoint.breakTorque = Mathf.Infinity;

                    stuckLog = collision.gameObject;

                    if (rb != null)
                        rb.isKinematic = true;
                }
            }
        }

        // Destroy half logs on collision
        if (collision.gameObject.CompareTag("HalfLog"))
        {
            if (hitSound != null) hitSound.Play();
            Destroy(collision.gameObject);
            DiscardSettings();
        }

        // If axe hits ground while stuck, break the log
        if (collision.gameObject.CompareTag("Ground") && stuckJoint != null && stuckLog != null)
        {
            Debug.Log("Axe collided with Ground while stuck in a log."); // log collision
            if (hitSound != null) hitSound.Play();

            Debug.Log($"Breaking stuck log: {stuckLog.name}"); // log breaking

            BreakLog(stuckLog, stuckLog.transform);

            Destroy(stuckJoint);            

            if (rb != null)
            {
                rb.isKinematic = false;
                Debug.Log("Axe Rigidbody set to non-kinematic, axe is now free."); // log Rigidbody change
            }
        }
    }

    // Method to spawn two halves of a log
    private void BreakLog(GameObject log, Transform logTransform)
    {
        if (brokenLogPrefab == null) return;

        Vector3 spawnPos = logTransform.position + Vector3.up * logSpawnHeight;
        Vector3 offset = logTransform.right * logHalfOffset;

        // Spawn left half
        GameObject half1 = Instantiate(brokenLogPrefab, spawnPos - offset, logTransform.rotation);
        Rigidbody half1Rb = half1.GetComponent<Rigidbody>();
        if (half1Rb != null) half1Rb.AddForce(-offset.normalized * 2f, ForceMode.Impulse);

        // Spawn right half
        GameObject half2 = Instantiate(brokenLogPrefab, spawnPos + offset, logTransform.rotation);
        Rigidbody half2Rb = half2.GetComponent<Rigidbody>();
        if (half2Rb != null) half2Rb.AddForce(offset.normalized * 2f, ForceMode.Impulse);

        Destroy(log);
        DiscardSettings();
    }

    private void DiscardSettings()
    {
        stuckJoint = null;
        stuckLog = null;
    }
}

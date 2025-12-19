using UnityEngine;

public class AxeHit : MonoBehaviour
{
    [Header("Axe Settings")]
    public Transform axeBlade;
    public float breakThreshold = 6.0f;

    [Header("Sound Settings")]
    public AudioSource hitSound;
    public AudioSource stuckSound;

    [Header("Log Settings")]
    public GameObject brokenLogPrefab;
    public float logHalfOffset = 0.2f;
    public float logSpawnHeight = 0.2f;

    private Rigidbody rb;

    private FixedJoint stuckJoint = null;
    private GameObject stuckLog = null;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
            Debug.LogWarning("Rigidbody not found on the axe! Velocity-based hits will be inaccurate.");

        Debug.Log("[AxeHit] Script initialized.");
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"[AxeHit] Collision with: {collision.gameObject.name}");

        if (collision.gameObject.CompareTag("Log") && stuckLog == null && stuckJoint == null)
        {
            // Используем скорость столкновения
            float impactForce = collision.relativeVelocity.magnitude;
            Debug.Log($"[AxeHit] Impact speed: {impactForce}, Threshold: {breakThreshold}");

            if (impactForce >= breakThreshold)
            {                
                BreakLog(collision.gameObject, collision.transform);
            }
            else
            {
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
                    {
                        rb.constraints = RigidbodyConstraints.FreezeRotationX |
                                         RigidbodyConstraints.FreezeRotationY |
                                         RigidbodyConstraints.FreezeRotationZ;
                    }

                    // Подключаем скрипт для удара об землю
                    LogGroundHit logScript = stuckLog.GetComponent<LogGroundHit>();
                    if (logScript != null)
                        logScript.AttachAxe(this);

                    Debug.Log($"[AxeHit] Axe stuck to log: {stuckLog.name}");
                }
            }
        }

        //if (collision.gameObject.CompareTag("HalfLog"))
        //{
        //    if (hitSound != null) hitSound.Play();
        //    Debug.Log($"[AxeHit] Collided with HalfLog: {collision.gameObject.name}, destroying it.");
        //    Destroy(collision.gameObject);
        //    DiscardSettings();
        //}
    }

    public void BreakLog(GameObject log, Transform logTransform)
    {
        if (brokenLogPrefab == null)
        {
            Debug.LogWarning("[AxeHit] brokenLogPrefab not assigned!");
            return;
        }

        Debug.Log($"[AxeHit] Breaking log: {log.name}");

        if (hitSound != null)
        {
            hitSound.Play();
        }

        Vector3 spawnPos = logTransform.position + Vector3.up * logSpawnHeight;
        Vector3 offset = logTransform.right * logHalfOffset;

        GameObject half1 = Instantiate(brokenLogPrefab, spawnPos - offset, logTransform.rotation);
        Rigidbody half1Rb = half1.GetComponent<Rigidbody>();
        if (half1Rb != null) half1Rb.AddForce(-offset.normalized * 2f, ForceMode.Impulse);

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
        if (rb != null)
            rb.constraints = RigidbodyConstraints.None;
    }
}

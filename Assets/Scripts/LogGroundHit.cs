using UnityEngine;

public class LogGroundHit : MonoBehaviour
{
    private float breakImpactThreshold = 5f;
    private AxeHit attachedAxe = null;

    public void AttachAxe(AxeHit axe)
    {
        attachedAxe = axe;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground") && attachedAxe != null)
        {
            float impactSpeed = collision.relativeVelocity.magnitude;
            Debug.Log($"[LogGroundHit] Log '{gameObject.name}' hit ground with speed {impactSpeed}");

            if (impactSpeed >= breakImpactThreshold)
            {
                attachedAxe.BreakLog(gameObject, transform);
                attachedAxe = null;
            }
        }
    }
}

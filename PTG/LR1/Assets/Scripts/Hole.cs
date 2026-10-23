using UnityEngine;

public class Hole : MonoBehaviour
{
    public LevelManager level;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Ball")) return;
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        if (level != null) level.Win();
    }
}

using UnityEngine;

public class FallZone : MonoBehaviour
{
    public LevelManager level;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball") && level != null) level.Lose();
    }
}

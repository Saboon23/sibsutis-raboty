using UnityEngine;

public class Shell : MonoBehaviour
{
    public float lifeTime = 4f;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }
}

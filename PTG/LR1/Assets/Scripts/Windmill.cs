using UnityEngine;

public class Windmill : MonoBehaviour
{
    public float speed = 110f;
    public Vector3 axis = Vector3.forward;

    void Update()
    {
        transform.Rotate(axis, speed * Time.deltaTime);
    }
}

using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 2.1f, -3.4f);
    public float lookHeight = 1.4f;
    public float smooth = 9f;

    void LateUpdate()
    {
        if (target == null) return;
        Vector3 desired = target.TransformPoint(offset);
        transform.position = Vector3.Lerp(transform.position, desired, smooth * Time.deltaTime);
        transform.LookAt(target.position + Vector3.up * lookHeight);
    }
}

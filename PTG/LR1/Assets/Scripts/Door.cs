using UnityEngine;

public class Door : MonoBehaviour
{
    public float liftHeight = 1.6f;
    public float period = 3.2f;
    public float openPart = 0.45f;
    public float moveSpeed = 5f;

    Vector3 closedPos;

    void Start()
    {
        closedPos = transform.position;
    }

    void Update()
    {
        bool open = Mathf.Repeat(Time.time, period) < period * openPart;
        Vector3 target = open ? closedPos + Vector3.up * liftHeight : closedPos;
        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
    }
}

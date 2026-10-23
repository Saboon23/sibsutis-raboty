using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BallController : MonoBehaviour
{
    public float maxPower = 16f;
    public float chargeSpeed = 11f;
    public float turnSpeed = 110f;
    public float stopThreshold = 0.15f;
    public Transform arrow;
    public LevelManager level;

    Rigidbody rb;
    float power;
    bool moving;
    float stoppedTime;
    float launchTime;
    Vector3 startPos;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        startPos = transform.position;
    }

    void Update()
    {
        if (level != null && level.Finished) { if (arrow != null) arrow.gameObject.SetActive(false); return; }
        if (moving)
        {
            if (arrow != null) arrow.gameObject.SetActive(false);
            bool onSurface = Physics.Raycast(transform.position, Vector3.down, GetComponent<SphereCollider>().radius * transform.lossyScale.y + 0.12f, ~0, QueryTriggerInteraction.Ignore);
            stoppedTime = onSurface && rb.velocity.magnitude < stopThreshold ? stoppedTime + Time.deltaTime : 0f;
            if (stoppedTime >= 0.25f && Time.time - launchTime > 0.3f)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                moving = false;
                power = 0f;
            }
            return;
        }

        if (arrow == null) return;
        arrow.gameObject.SetActive(true);
        arrow.position = transform.position;

        float h = Input.GetAxis("Horizontal");
        if (Mathf.Abs(h) > 0.01f)
            arrow.Rotate(Vector3.up, h * turnSpeed * Time.deltaTime, Space.World);

        if (Input.GetKey(KeyCode.Space))
        {
            power = Mathf.Min(power + chargeSpeed * Time.deltaTime, maxPower);
            arrow.localScale = new Vector3(1f, 1f, 0.4f + power / maxPower);
            if (level != null) level.SetPower(power / maxPower);
        }
        else if (power > 0f)
        {
            rb.AddForce(arrow.forward * power, ForceMode.VelocityChange);
            moving = true;
            stoppedTime = 0f;
            launchTime = Time.time;
            arrow.localScale = new Vector3(1f, 1f, 0.4f);
            if (level != null)
            {
                level.AddShot();
                level.SetPower(0f);
            }
        }
    }

    public void PutBack()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = startPos;
        moving = false;
        power = 0f;
    }
}

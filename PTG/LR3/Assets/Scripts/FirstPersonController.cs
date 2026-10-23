using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    public Transform cam;
    public float speed = 4.5f;
    public float mouseSensitivity = 2.2f;
    public float jumpForce = 5f;

    CharacterController cc;
    float pitch;
    float vy;

    void Start()
    {
        cc = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        transform.Rotate(Vector3.up, Input.GetAxis("Mouse X") * mouseSensitivity);
        pitch = Mathf.Clamp(pitch - Input.GetAxis("Mouse Y") * mouseSensitivity, -80f, 80f);
        if (cam != null) cam.localEulerAngles = new Vector3(pitch, 0f, 0f);

        Vector3 dir = transform.right * Input.GetAxis("Horizontal") +
                      transform.forward * Input.GetAxis("Vertical");
        if (dir.sqrMagnitude > 1f) dir.Normalize();

        if (cc.isGrounded)
        {
            vy = -1f;
            if (Input.GetKeyDown(KeyCode.Space)) vy = jumpForce;
        }
        else vy += Physics.gravity.y * Time.deltaTime;

        cc.Move((dir * speed + Vector3.up * vy) * Time.deltaTime);
    }
}

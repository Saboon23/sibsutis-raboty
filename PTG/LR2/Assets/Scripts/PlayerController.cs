using UnityEngine;
[RequireComponent(typeof(Rigidbody), typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    public float walkSpeed=2.4f, runSpeed=5.2f, backSpeed=1.5f, turnSpeed=130f, jumpForce=5.5f;
    public Transform groundCheck;
    public LayerMask groundMask;
    Rigidbody rb;
    Animator anim;
    bool grounded, attacking, attackEntered;
    float moveInput, turnInput, jumpTime=-10f;
    bool running, jumpRequested;
    void Start() {
        rb=GetComponent<Rigidbody>(); anim=GetComponent<Animator>();
        rb.constraints=RigidbodyConstraints.FreezeRotation;
    }
    void Update() {
        grounded=Time.time-jumpTime>0.18f && groundCheck && Physics.CheckSphere(groundCheck.position,0.16f,groundMask,QueryTriggerInteraction.Ignore);
        bool falling=!grounded && rb.velocity.y < -0.1f;
        anim.SetBool("Grounded",grounded); anim.SetBool("Falling",falling);
        var state=anim.GetCurrentAnimatorStateInfo(0);
        if (attacking) {
            if (state.IsTag("Attack")) attackEntered=true;
            if (falling || (attackEntered && !state.IsTag("Attack") && !anim.IsInTransition(0))) attacking=false;
        }
        if ((Input.GetKeyDown(KeyCode.LeftControl)||Input.GetKeyDown(KeyCode.RightControl)) && grounded && !attacking) {
            attacking=true; attackEntered=false; anim.ResetTrigger("Jump"); anim.SetTrigger("Attack");
        }
        running=Input.GetKey(KeyCode.LeftShift)||Input.GetKey(KeyCode.RightShift);
        moveInput=attacking?0f:Input.GetKey(KeyCode.W)?1f:Input.GetKey(KeyCode.D)?-1f:0f;
        turnInput=attacking?0f:(Input.GetKey(KeyCode.A)?1f:0f)-(Input.GetKey(KeyCode.S)?1f:0f);
        if(Input.GetKeyDown(KeyCode.Space)&&grounded&&!attacking) jumpRequested=true;
        anim.SetFloat("Speed",Mathf.Abs(moveInput)); anim.SetBool("Backward",moveInput<0f); anim.SetBool("Run",running&&moveInput!=0f);
        if(transform.position.y < -15f) { transform.position=new Vector3(0,1.1f,-6); rb.velocity=Vector3.zero; }
    }
    void FixedUpdate() {
        rb.MoveRotation(rb.rotation*Quaternion.Euler(0,turnInput*turnSpeed*Time.fixedDeltaTime,0));
        float speed=moveInput*(running?runSpeed:moveInput<0?backSpeed:walkSpeed);
        Vector3 v=transform.forward*speed; v.y=rb.velocity.y;
        if(jumpRequested) { jumpRequested=false; v.y=jumpForce; jumpTime=Time.time; anim.SetTrigger("Jump"); }
        rb.velocity=v;
    }
    void OnGUI() {
        GUI.Box(new Rect(12,12,570,65),"W — вперёд   D — назад   A — вправо   S — влево\nShift — бег   Space — прыжок   Ctrl — удар\nСостояние: "+anim.GetCurrentAnimatorClipInfo(0)[0].clip.name);
    }
}

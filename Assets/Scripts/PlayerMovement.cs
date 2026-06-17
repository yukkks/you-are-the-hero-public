using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float turnSpeed = 120f;
    public Animator characterAnimator;

    private Rigidbody rb;
    private float moveInput;
    private float turnInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (characterAnimator == null)
            characterAnimator = GetComponentInChildren<Animator>();

        if (characterAnimator != null)
            characterAnimator.SetBool("isMoving", false);
    }

    void Update()
    {
        moveInput = Input.GetAxisRaw("Vertical");     
        turnInput = Input.GetAxisRaw("Horizontal");   

        bool isMoving = Mathf.Abs(moveInput) > 0.1f || Mathf.Abs(turnInput) > 0.1f;

        if (characterAnimator != null)
            characterAnimator.SetBool("isMoving", isMoving);
    }

    void FixedUpdate()
    {
        // Left / Right rotates player
        Quaternion turnRotation = Quaternion.Euler(
            0f,
            turnInput * turnSpeed * Time.fixedDeltaTime,
            0f
        );

        rb.MoveRotation(rb.rotation * turnRotation);

        // FIXED: flipped direction
        Vector3 moveDirection =
            -transform.forward * moveInput * moveSpeed * Time.fixedDeltaTime;

        rb.MovePosition(rb.position + moveDirection);
    }
}
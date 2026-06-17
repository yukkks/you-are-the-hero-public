using UnityEngine;

public class RunnerMovement : MonoBehaviour
{
    public float forwardSpeed = 25f;
    public float laneMoveSpeed = 25f;
    public float laneOffset = 3f;

    private int currentLane = 1; // 0 = left, 1 = center, 2 = right
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            currentLane = Mathf.Max(0, currentLane - 1);
        }

        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            currentLane = Mathf.Min(2, currentLane + 1);
        }
    }

    void FixedUpdate()
    {
        Vector3 targetPosition = new Vector3((currentLane - 1) * laneOffset, rb.position.y, rb.position.z + forwardSpeed * Time.fixedDeltaTime);
        Vector3 newPosition = Vector3.Lerp(rb.position, targetPosition, laneMoveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(newPosition);
    }
}
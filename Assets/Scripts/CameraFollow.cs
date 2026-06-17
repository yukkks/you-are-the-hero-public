using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;

    [Header("Camera Position")]
    public Vector3 offset = new Vector3(0f, 5f, -8f);
    public float smoothSpeed = 8f;

    [Header("Look Target")]
    public float lookHeight = 1.4f;

    [Header("Collision")]
    public LayerMask collisionLayers;
    public float cameraRadius = 0.3f;
    public float minDistanceFromPlayer = 2f;

    void LateUpdate()
    {
        if (target == null) return;

        Quaternion yawOnly = Quaternion.Euler(0f, target.eulerAngles.y, 0f);

        Vector3 lookTarget = target.position + Vector3.up * lookHeight;
        Vector3 desiredPosition = target.position + yawOnly * offset;

        Vector3 direction = desiredPosition - lookTarget;
        float distance = direction.magnitude;
        direction.Normalize();

        Vector3 finalPosition = desiredPosition;

        if (Physics.SphereCast(
            lookTarget,
            cameraRadius,
            direction,
            out RaycastHit hit,
            distance,
            collisionLayers
        ))
        {
            float adjustedDistance = Mathf.Max(hit.distance - cameraRadius, minDistanceFromPlayer);
            finalPosition = lookTarget + direction * adjustedDistance;
        }

        transform.position = Vector3.Lerp(
            transform.position,
            finalPosition,
            smoothSpeed * Time.deltaTime
        );

        transform.LookAt(lookTarget);
    }
}
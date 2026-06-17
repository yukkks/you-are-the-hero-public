using UnityEngine;

public class OrbitObject : MonoBehaviour
{
    public Transform centerObject;
    public float orbitSpeed = 30f;

    void Update()
    {
        if (centerObject != null)
        {
            transform.RotateAround(centerObject.position, Vector3.up, orbitSpeed * Time.deltaTime);
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    float cameraDistance = 1;
    [SerializeField]
    float scrollSpeed;
    Vector3 cameraVector;
    public float minDistance;
    public float maxDistance;
    public GameObject camFollow;
    [SerializeField]
    float cameraRotationSpeed;
    Vector3 targetPosition;
    [SerializeField]
    float smoothTime = 0.3f;
    Vector3 velocity= Vector3.zero;
    void Start()
    {
        cameraVector = -transform.forward;
    }

    // Update is called once per frame
    void Update()
    {
        cameraDistance += -Input.mouseScrollDelta.y * scrollSpeed;
        cameraDistance = Mathf.Clamp(cameraDistance, minDistance, maxDistance);
        targetPosition = camFollow.transform.position + (cameraDistance * cameraVector); 
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }
}

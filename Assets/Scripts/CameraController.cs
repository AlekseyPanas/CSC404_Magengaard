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
    Vector3 velocity = Vector3.zero;
    [SerializeField]
    float rotateSpeed;
    void Start()
    {
        cameraVector = -transform.forward;
    }

    void Update()
    {        
        // if (Input.GetKey(KeyCode.Q)){
        //     camFollow.transform.Rotate(new Vector3(0, rotateSpeed, 0));
        // }
        // if (Input.GetKey(KeyCode.E)){
        //     camFollow.transform.Rotate(new Vector3(0, -rotateSpeed, 0));
        // }
        cameraDistance += -Input.mouseScrollDelta.y * scrollSpeed;
        cameraDistance = Mathf.Clamp(cameraDistance, minDistance, maxDistance);

        targetPosition = camFollow.transform.position + (cameraDistance * cameraVector); 
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }
}

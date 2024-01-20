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
    void Start()
    {
        cameraVector = -transform.forward;
    }

    // Update is called once per frame
    void Update()
    {
        cameraDistance += -Input.mouseScrollDelta.y * scrollSpeed;
        cameraDistance = Mathf.Clamp(cameraDistance, minDistance, maxDistance);
        transform.position = camFollow.transform.position + (cameraDistance * cameraVector); 
    }
}

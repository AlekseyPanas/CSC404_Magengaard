using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Scripting;

public class PlayerMovementController : NetworkBehaviour
{
    private Rigidbody rb;
    [SerializeField]
    private float moveSpeed;
    private Vector2 input;
    [SerializeField]
    float dashDuration;
    [SerializeField]
    float dashDistance;
    bool canMove = true;
    [SerializeField]
    float dashCD;
    float dashCDTimer = 0;

    [RequiredMember]
    public Camera camera;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        camera.enabled = IsOwner;
    }

    void Update()
    {
        if (IsLocalPlayer)
        {
            float yVel = rb.velocity.y;
            int x = 0;
            int z = 0;
            if (Input.GetKey(KeyCode.W))
            {
                z += 1;
            }

            if (Input.GetKey(KeyCode.S))
            {
                z -= 1;
            }

            if (Input.GetKey(KeyCode.A))
            {
                x -= 1;
            }

            if (Input.GetKey(KeyCode.D))
            {
                x += 1;
            }

            input = new Vector2(x, z);
            input.Normalize();

            var cameraTransform = camera.transform;
            Vector3 camRight = cameraTransform.right;
            Vector3 camForward = cameraTransform.forward;
            Vector2 right = new Vector2(camRight.x, camRight.z).normalized;
            Vector2 forward = new Vector2(camForward.x, camForward.z).normalized;
            Vector2 combined = (right * x + forward * z).normalized;
            if (canMove)
            {
                rb.velocity = new Vector3(combined.x, 0, combined.y) * moveSpeed + new Vector3(0, yVel, 0);
                if (rb.velocity.magnitude > 0)
                {
                    transform.forward = -rb.velocity.normalized;
                }
                
                SetVelocityServerRPC(combined, yVel);
            }

            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                if (Time.time > dashCDTimer)
                {
                    canMove = false;
                    Vector3 dashPos = combined * dashDistance;
                    Vector3 dashTargetPosition = new Vector3(dashPos.x, 0, dashPos.y);
                    dashCDTimer = Time.time + dashCD;
                    StartCoroutine(StartDash(dashDuration, transform.position + dashTargetPosition));
                }
            }
        }
    }

    IEnumerator StartDash(float dashDuration, Vector3 targetPosition){
        // Taylor: Not going to network sync this yet.
        float elapsedTime = 0;
        Vector3 currentPos = transform.position;
        while(elapsedTime < dashDuration){
            transform.position = Vector3.Lerp(currentPos, targetPosition, (elapsedTime/dashDuration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        canMove = true;
        transform.position = targetPosition;
        yield return null;
    }

    [ServerRpc]
    void SetVelocityServerRPC(Vector2 combined, float yVel)
    {
        rb.velocity = new Vector3(combined.x, 0, combined.y) * moveSpeed + new Vector3(0, yVel, 0);
    }
}

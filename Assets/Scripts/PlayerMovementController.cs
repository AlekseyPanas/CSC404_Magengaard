using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementController : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField]
    private float moveSpeed;
    private Vector2 input;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Debug.Log(Camera.main.transform.right);
        Debug.Log(Camera.main.transform.forward);
    }

    void Update()
    {
        float yVel = rb.velocity.y;
        int x = 0;
        int z = 0;
        if (Input.GetKey(KeyCode.W)){
            z += 1;
        }
        if (Input.GetKey(KeyCode.S)){
            z -= 1;
        }
        if (Input.GetKey(KeyCode.A)){
            x -= 1;
        }
        if (Input.GetKey(KeyCode.D)){
            x += 1;
        }
        input = new Vector2(x, z);
        input.Normalize();
        Vector3 camRight = Camera.main.transform.right;
        Vector3 camForward = Camera.main.transform.forward;
        Vector2 right = new Vector2(camRight.x, camRight.z).normalized;
        Vector2 forward = new Vector2(camForward.x, camForward.z).normalized;
        Vector2 combined = (right * x + forward * z).normalized;
        rb.velocity = new Vector3(combined.x, 0, combined.y) * moveSpeed + new Vector3(0,yVel,0);
    }
}

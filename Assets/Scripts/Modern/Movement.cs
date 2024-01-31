using Unity.Netcode;
using UnityEngine;

namespace Modern
{
    public class Movement : NetworkBehaviour
    {
        private CharacterController _controller;

        public float gravity = -0.00981f;
        public float speed = 2.0f;

        public Camera activeCamera;
        
        private DesktopControls _controls;
        
        private readonly NetworkVariable<Vector3> _velocity = new(
            new Vector3(), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        
        private void Start()
        {
            _controls = new DesktopControls();
            _controller = GetComponent<CharacterController>();
            
            _controls.Enable();
            _controls.Game.Enable();
        }

        public override void OnNetworkSpawn()
        {
            activeCamera.enabled = IsOwner;
        }

        private void UpdateVelocity()
        {
            if (!IsOwner)
            {
                return;
            }
        
            var velocity = _velocity.Value;
            var vertical = velocity.y + gravity * Time.deltaTime;
            var horizontal = _controls.Game.Movement.ReadValue<Vector2>() * speed;
            
            if (_controller.isGrounded)
            {
                vertical = -1.0f;
            }

            var cameraForward = activeCamera.transform.forward;
            var forward = new Vector3(cameraForward.x, 0, cameraForward.z);
            var right = new Vector3(cameraForward.z, 0, -cameraForward.x);

            velocity = forward * horizontal.y + right * horizontal.x + Vector3.up * vertical;
            
            _velocity.Value = velocity;
        }

        private void Update()
        {
            UpdateVelocity();

            var velocity = _velocity.Value;

            _controller.Move(velocity * Time.deltaTime);
            
            velocity.y = 0;

            if (velocity.sqrMagnitude > 0)
            {
                transform.forward = -velocity.normalized;
            }
        }
    }
}

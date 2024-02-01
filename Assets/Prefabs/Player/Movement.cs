using System.Linq.Expressions;
using Unity.Netcode;
using UnityEngine;

namespace Modern
{
    public class Movement : NetworkBehaviour
    {
        private CharacterController _controller;
        private Entity _entity;

        private GameObject _renderer;

        public float gravity = -0.00981f;
        public float speed = 2.0f;

        public Camera activeCamera;
        
        private DesktopControls _controls;
        
        private readonly NetworkVariable<Vector3> _velocity = new(
            new Vector3(), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        
        private void Awake()
        {
            _entity = GetComponent<Entity>();
            _renderer = transform.GetChild(0).gameObject;
            _controller = GetComponent<CharacterController>();

            _controls = new DesktopControls();
            
            _controls.Enable();
            _controls.Game.Enable();

            _entity.OnDeath += Death;
        }

        public void Respawn()
        {
            _renderer.SetActive(true);
            
            _controller.enabled = true;
            
            if (IsServer)
            {
                _entity.Reset();
            }
        }
        
        private void Death()
        {
            _renderer.SetActive(false);
            
            _controller.enabled = false;
            _controller.Move(-transform.position);
            transform.position = Vector3.zero;
            
            Invoke(nameof(Respawn), 5.0f);
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
            if (_entity.dead.Value)
            {
                return;
            }
            
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

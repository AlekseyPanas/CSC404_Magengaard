using System;
using Unity.Netcode;
using UnityEngine;

namespace Modern
{
    public class Shoot: NetworkBehaviour
    {
        public GameObject ball;

        private DesktopControls _controls;

        private void Start()
        {
            _controls = new DesktopControls();
            
            _controls.Enable();
            _controls.Game.Enable();

            _controls.Game.Tap.performed += _ => FireBall();
        }
        
        private void FireBall()
        {
            if (!IsOwner)
            {
                return;
            }
            
            // nonsense direction, just fun and simple
            var direction = (_controls.Game.CastDirection.ReadValue<Vector2>().normalized - Vector2.one / 2).normalized;
            
            FireBallServerRpc(transform.position, new Vector3(direction.x, 0, direction.y));
        }

        [ServerRpc]
        private void FireBallServerRpc(Vector3 position, Vector3 direction)
        {
            var projectile = Instantiate(ball, position, Quaternion.identity);
            
            projectile.GetComponent<Rigidbody>().velocity = direction.normalized * 10;
            projectile.GetComponent<NetworkObject>().Spawn();
        }
    }
}
using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Scripting;

namespace Modern
{
    public class FollowCamera : MonoBehaviour
    {
        private Transform followTransform = null;

        public float followSpeed = 0.00000001f;
        public Vector3 followDistance;

        private void Start() {
            followDistance = transform.position - followTransform.position;
            PlayerCameraLinkEvent.OwnPlayerSpawnedEvent += setPlayer;  // Subscribe to receive info when the player spawns in
        }

        private void setPlayer(Transform player) {
            followTransform = player;
        }

        private void Update() {
            if (followTransform != null) {
                transform.position = Vector3.Lerp(transform.position - followDistance, followTransform.position, followSpeed) + followDistance;
            }
        }
    }
}

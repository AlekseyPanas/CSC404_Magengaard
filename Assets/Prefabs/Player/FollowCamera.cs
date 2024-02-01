using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Scripting;

namespace Modern
{
    public class FollowCamera : MonoBehaviour
    {
        [RequiredMember] public Transform followTransform;

        public float followSpeed = 0.00000001f;
        public Vector3 followDistance;

        private void Start()
        {
            followDistance = transform.position - followTransform.position;
        }

        private void Update()
        {
            transform.position = Vector3.Lerp(transform.position - followDistance, followTransform.position, followSpeed) + followDistance;
        }
    }
}

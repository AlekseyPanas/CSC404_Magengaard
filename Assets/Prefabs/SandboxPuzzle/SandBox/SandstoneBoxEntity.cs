using System;
using Unity.Netcode;
using UnityEngine;
using FMODUnity;

public class SandstoneBoxEntity : NetworkBehaviour, IEffectListener<WindEffect>
{
    private Rigidbody rigidBody;
    private StudioEventEmitter audioSys;

    private Vector3 _velocity;
    
    void Awake() {
        audioSys = GetComponent<StudioEventEmitter>();
    }

    void IEffectListener<WindEffect>.OnEffect(WindEffect effect) {
        if (IsServer)
        {
            // The box is already moving!
            if (_velocity.magnitude > 0.05)
            {
                return;
            }
            
            var norm = effect.Velocity.normalized;
            var xDir = Math.Abs(norm.x) > Math.Abs(norm.z);
            
            Console.WriteLine(norm.ToString());
            if (xDir)
            {
                var sign = norm.x > 0 ? +1f : -1f;

                _velocity = new Vector3(sign, 0, 0) * 6;
            }
            else
            {
                var sign = norm.z > 0 ? +1f : -1f;

                _velocity = new Vector3(0, 0, sign) * 6;
            }
            
            audioSys.Play();
        }
    }

    void freezeY() {
        rigidBody.constraints |= RigidbodyConstraints.FreezePositionY;
    }

    // Start is called before the first frame update
    void Start() {
        rigidBody = GetComponent<Rigidbody>();
        rigidBody.drag = 0;
        Invoke("freezeY", 0.5f);
    }

    private void OnCollisionEnter(Collision other)
    {
        // needs normal check
        _velocity = Vector3.zero;
    }

    private void Update()
    {
        rigidBody.velocity = _velocity;
    }
}

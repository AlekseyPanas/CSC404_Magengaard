using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LocalAimSystem : AAimSystem
{
    private Transform ownPlayerTransform;
    private DesktopControls _controls;
    [SerializeField] private float radius;
    [SerializeField] private GameObject explosionIndicator;
    private RaycastHit hit;
    [SerializeField] private GameObject chargingParticle;

    public override void setPlayerTransform(Transform playerTransform) {ownPlayerTransform = playerTransform;}

    // Start is called before the first frame update
    void Start() {
        _controls = new DesktopControls();
        _controls.Enable();
        _controls.Game.Enable();
        _controls.Game.Fire.performed += onTap;
        explosionIndicator.transform.localScale = new Vector3(radius, 0.01f, radius);
    }

    new void OnDestroy() {
        base.OnDestroy();  // Parent function ensures unsubscribing from event
        //Debug.Log("\t\t\t\t\t\t\tChild OnDestroy, disabling controls...");
        _controls.Game.Fire.performed -= onTap;
        _controls.Disable();
        _controls.Game.Disable();
        Destroy(chargingParticle, 1f);
    }

    void onTap(UnityEngine.InputSystem.InputAction.CallbackContext ctx) {
        // Debug.Log("Tap performed");
        invokeAimingFinishedEvent(new SpellParamsContainer().setVector3(0, (hit.point - ownPlayerTransform.position).normalized));
    }

    void Update() {
        if(ownPlayerTransform != null){
            transform.position = ownPlayerTransform.position;
            Ray r = Camera.main.ScreenPointToRay(new Vector3(_controls.Game.MousePos.ReadValue<Vector2>().x, _controls.Game.MousePos.ReadValue<Vector2>().y));
            Vector3 direction;
            if (Physics.Raycast(r, out hit, Mathf.Infinity, layerMask)) {
                direction = (hit.point - ownPlayerTransform.position).normalized;
                direction = new Vector3(direction.x, 0, direction.z);
                transform.position = ownPlayerTransform.position;// + Const.SPELL_SPAWN_DISTANCE_FROM_PLAYER * direction;
            }
        }
        chargingParticle.transform.position = transform.position;
    }
}

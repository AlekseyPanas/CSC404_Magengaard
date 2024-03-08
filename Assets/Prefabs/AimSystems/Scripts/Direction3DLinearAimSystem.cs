using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Direction3DLinearAimSystem : AAimSystem
{
    private Transform ownPlayerTransform;
    private DesktopControls _controls;
    [SerializeField] private float range;
    [SerializeField] private GameObject trajectoryIndicator;
    [SerializeField] private GameObject explosionIndicator;
    private RaycastHit hit;
    private RaycastHit playerHit;

    public override void setPlayerTransform(Transform playerTransform) {ownPlayerTransform = playerTransform;}

    // Start is called before the first frame update
    void Start() {
        _controls = new DesktopControls();
        _controls.Enable();
        _controls.Game.Enable();
        _controls.Game.Fire.performed += onTap;
    }

    new void OnDestroy() {
        base.OnDestroy();  // Parent function ensures unsubscribing from event
        _controls.Game.Fire.performed -= onTap;
        _controls.Disable();
        _controls.Game.Disable();
    }

    void onTap(UnityEngine.InputSystem.InputAction.CallbackContext ctx) {
        invokeAimingFinishedEvent(new SpellParamsContainer().setVector3(0, (hit.point - ownPlayerTransform.position).normalized));
    }

    void DrawTrajectory(){
        if(ownPlayerTransform != null){
            transform.position = ownPlayerTransform.position;
            Ray r = Camera.main.ScreenPointToRay(new Vector3(_controls.Game.MousePos.ReadValue<Vector2>().x, _controls.Game.MousePos.ReadValue<Vector2>().y));
            Vector3 direction, difference;
            if (Physics.Raycast(r, out hit, Mathf.Infinity, layerMask)) {
                direction = (hit.point - ownPlayerTransform.position).normalized;
                Physics.Raycast(ownPlayerTransform.position, direction, out playerHit, Mathf.Infinity, layerMask); //ray from player to cursor
                Debug.DrawLine(ownPlayerTransform.position, playerHit.point, Color.red);
                difference = playerHit.point - ownPlayerTransform.position;
                float distance = difference.magnitude;
                if (Mathf.Min(distance,range)-2f <= 0) {
                    trajectoryIndicator.SetActive(false);
                }else{
                    trajectoryIndicator.SetActive(true);
                }
                trajectoryIndicator.transform.localScale = new Vector3(Mathf.Max(Mathf.Min(distance,range)-2f, 0), 0.2f, 0.2f);
                trajectoryIndicator.transform.localPosition = new Vector3(Mathf.Max(Mathf.Min(distance,range)/2 - 1f, 0) + 0.5f, 0 ,0);
                explosionIndicator.transform.localPosition = new Vector3(Mathf.Min(distance,range), 0, 0);
                transform.right = direction;
            }
        }
    }

    // Update is called once per frame
    void Update() {
        DrawTrajectory();
    }
}

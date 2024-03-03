using UnityEngine;

public class Direction3DAimSystem : AAimSystem
{
    private Transform ownPlayerTransform;
    private DesktopControls _controls;

    public override void setPlayerTransform(Transform playerTransform) {ownPlayerTransform = playerTransform;}

    // Start is called before the first frame update
    new void Start() {
        base.Start();
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
        invokeAimingFinishedEvent(new SpellParamsContainer().setVector3(0, (transform.position - ownPlayerTransform.position).normalized));
    }

    // Update is called once per frame
    void Update() {
        Ray r = Camera.main.ScreenPointToRay(new Vector3(_controls.Game.MousePos.ReadValue<Vector2>().x, _controls.Game.MousePos.ReadValue<Vector2>().y));
        RaycastHit hit;
        if (Physics.Raycast(r, out hit, layerMask)) {
            transform.position = hit.point;
        }
    }

}

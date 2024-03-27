using AHealthControllable = AControllable<PlayerHealthControllable, ControllerRegistrant>;
using Unity.Netcode;

/** 
* Default controller for the health system which manipulates health based on received effects
*/
public class PlayerHealthEffectController: NetworkBehaviour, IEffectListener<ImpactEffect> {

    private AHealthControllable _healthControllable;
    private ControllerRegistrant _registrant;
    private bool _isInControl = true;

    void Start() {
        _healthControllable = GetComponent<AHealthControllable>();
        _registrant = _healthControllable.RegisterDefault();
        _registrant.OnInterrupt += () => { _isInControl = false; };
        _registrant.OnResume += () => { _isInControl = true; };
    }

    public void OnEffect(ImpactEffect effect) {
        if (!_isInControl) return;

        _healthControllable.GetSystem(_registrant)?.Damage(effect.Amount, effect.Direction);
    }

}

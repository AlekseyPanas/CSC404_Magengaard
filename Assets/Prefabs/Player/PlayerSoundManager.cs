using FMODUnity;
using Unity.Netcode;

public class PlayerSoundManager: NetworkBehaviour {
    
    public AnimationEventReceiver _animEventReceiver;
    private StudioEventEmitter _emitter;
    
    void Awake() {
        _emitter = GetComponent<StudioEventEmitter>();

        _animEventReceiver.OnFootstepEvent += () => {
            _emitter.Play();
            _emitter.EventInstance.setVolume(0.2f);
        };
    }
}
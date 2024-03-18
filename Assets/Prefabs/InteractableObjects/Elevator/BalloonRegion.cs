using UnityEngine;
using UnityEngine.Events;

// Intentionally NOT making this script NetworkBehaviour
// It's either finish the ruins area on time or make this puzzle a working multiplayer puzzle
public class BalloonRegion : MonoBehaviour, IEffectListener<TemperatureEffect>
{
    private bool _lit;
    public GameObject fire;
    
    public UnityEvent onLight;

    void Light()
    {
        if (_lit)
        {
            return;
        }
        
        _lit = true;
        fire.SetActive(true);
        
        onLight.Invoke();
    }

    public void OnEffect(TemperatureEffect effect)
    {
        if (effect.TempDelta > 3)
        {
            Light();
        }
    }
}
using Unity.Netcode;
using UnityEngine;

public class ProtectorBarrier : NetworkBehaviour, IEffectListener<TemperatureEffect>, IEffectListener<WindEffect>, IEffectListener<ImpactEffect>
{
    [SerializeField] private float damageAngleThreshold;
    [SerializeField] private GameObject sparks;
    [SerializeField] private GameObject deflection;
    public void OnEffect(ImpactEffect effect)
    {
        Vector3 dir = new Vector3(effect.Direction.x, 0, effect.Direction.z).normalized;
        float angle = Vector3.Angle(dir, transform.forward);
        if (angle > damageAngleThreshold){
            //Deflect(dir);
        }
    }
    public void OnEffect(TemperatureEffect effect)
    {
        if(!effect.IsAttack) return;
        Vector3 dir = new Vector3(effect.Direction.x, 0, effect.Direction.z).normalized;
        float angle = Vector3.Angle(dir, transform.forward);
        if (angle > damageAngleThreshold){
            Deflect(dir);
        }
    }

    public void OnEffect(WindEffect effect)
    {
        Deflect(effect.Direction);
    }

    void Deflect(Vector3 dir){
        Vector3.Normalize(dir);
        GameObject s = Instantiate(sparks);
        GameObject d = Instantiate(deflection);
        s.transform.position = transform.position - dir/2;
        d.transform.position = transform.position - dir/1.5f;
        d.transform.forward = dir;
    }
}

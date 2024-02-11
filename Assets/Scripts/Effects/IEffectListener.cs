
using System.Collections.Generic;
using UnityEngine;

/**
 * Implement this to listen for effects of type T.
 */
public interface IEffectListener<T>
{
    /** Utiliy method to safely execute an effect on a list of objects */
    public static void sendEffect(List<GameObject> targets, T effectInstance) {
        foreach(GameObject g in targets) {
            IEffectListener<T> comp = g.GetComponent<IEffectListener<T>>();
            if (comp != null) {
                comp.OnEffect(effectInstance);
            }
        }
    }
    public static void sendEffect(GameObject target, T effectInstance) {
        IEffectListener<T> comp = target.GetComponent<IEffectListener<T>>();
        if (comp != null) {
            comp.OnEffect(effectInstance);
        }
    }
     
    void OnEffect(T effect);
}

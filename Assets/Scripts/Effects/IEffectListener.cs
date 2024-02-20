
using System.Collections.Generic;
using UnityEngine;

/**
 * Implement this to listen for effects of type T.
 */
public interface IEffectListener<T>
{
    /** Utility method to safely execute an effect on a list of objects */
    public static void SendEffect(List<GameObject> targets, T effectInstance) {
        foreach(GameObject g in targets) {
            g.GetComponent<IEffectListener<T>>()?.OnEffect(effectInstance);
        }
    }
    public static void SendEffect(GameObject target, T effectInstance) {
        target.GetComponent<IEffectListener<T>>()?.OnEffect(effectInstance);
    }
     
    public void OnEffect(T effect);
}

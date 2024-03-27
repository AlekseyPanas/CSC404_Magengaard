using System;
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
            List<IEffectListener<T>> l = new List<IEffectListener<T>>(g.GetComponents<IEffectListener<T>>());
            foreach(IEffectListener<T> e in l){
                e?.OnEffect(effectInstance);
            }
        }
    }
    public static void SendEffect(GameObject target, T effectInstance) {
        List<IEffectListener<T>> l = new List<IEffectListener<T>>(target.GetComponents<IEffectListener<T>>());
        foreach(IEffectListener<T> e in l){
            e?.OnEffect(effectInstance);
        }
    }
     
    public void OnEffect(T effect);
}

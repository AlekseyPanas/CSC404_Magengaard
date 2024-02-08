
using System.Collections.Generic;
using UnityEngine;

/**
 * Implement this to listen for effects of type T.
 */
public interface IEffectListener<T>
{
    public static void sendEffect(List<GameObject> targets, T effectInstance) {

    }
    
    void OnEffect(T effect);
}

using UnityEngine;


public static class Effect<T> {
    public delegate void TriggerEffect(T effectDetails);
    public static event TriggerEffect TriggerEffectEvent;

    public static void triggerEffect(T effectDetails) {
        TriggerEffectEvent(effectDetails);
    }
}

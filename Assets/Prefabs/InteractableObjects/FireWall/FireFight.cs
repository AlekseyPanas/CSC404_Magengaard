using UnityEngine;
using UnityEngine.Events;

// Not network behaviour. Don't have time to make fight management network based.
public class FireFight : MonoBehaviour
{
    public UnityEvent fightLock;
    public UnityEvent fightUnlock;
    
    public UnityEvent fightStart;

    private bool _started;
    
    public int conditionCount = 1;

    public void ConditionMet()
    {
        if (conditionCount > 0)
        {
            conditionCount--;

            if (conditionCount <= 0)
            {
                Invoke(nameof(EndFight), 3.0f);
            }
        }
    }
    
    private void StartFight()
    {
         fightStart.Invoke();
    }

    private void EndFight()
    {
        fightUnlock.Invoke();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Don't do anything if we were already solved or if this collision is not with a player.
        if (!other.CompareTag("Player") || _started)
        {
            return;
        }

        _started = true;
        
        fightLock.Invoke();
        
        Invoke(nameof(StartFight), 3.0f);
    }
}

using UnityEngine;

/** Any object that can be aggroed onto and wants to receive the info on being aggroed should implement this class
e.g player listening for aggro to determine when in combat */
public interface IAggroable {
    /** Called by an entity when they aggro an object implementing this interface */
    public void Aggro(GameObject who);
    /** Called by an enemy when they deaggro an object implementing this interface */
    public void DeAggro(GameObject who);
}

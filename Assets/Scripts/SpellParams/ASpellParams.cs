
using UnityEngine.EventSystems;

/** Every spell params class must have a method which builds the params from the container class received over the network 
   For example, a spell params class with only one direction vector might simply extract getVector(0) from the container
*/
public interface ISpellParams {
    void buildFromContainer(SpellParamsContainer container);
}

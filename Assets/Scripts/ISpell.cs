using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


public interface ISpell {
    /** Must be called before setParams */
    void setPlayerId(ulong playerId);

    /** One of these is called immediately after instantiating and before network spawning in order to set the appropriate initial parameters and perform pre-initialization.
    * The spellParams version is called when the spell succeeds while the parameterless one is called when the spell backfired.
    */
    void setParams(Direction2DSpellParams spellParams);
    void setParams();
}

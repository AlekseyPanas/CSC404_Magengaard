using UnityEngine;


public interface ISpell {
    void setPlayerId(ulong playerId);

    void preInitSpell();
}

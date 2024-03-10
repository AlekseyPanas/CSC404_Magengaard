/** Implemented by a spell projectile prefab */
public interface ISpell {
    /** Must be called before setParams */
    void setPlayerId(ulong playerId);

    /** One of these is called immediately after instantiating and before network spawning in order to set the appropriate initial parameters and perform pre-initialization.
    * The parameterless preInitBackfire is called is the spell backfired
    * !!!! IMPORTANT !!!! Do not set velocities or other physics stuff in pre-init. Do that in post-init once the objects authority has been established
    */
    void preInit(SpellParamsContainer spellParams);
    void preInitBackfire();
    void postInit();
}

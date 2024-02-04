/**
* Used for raw damage being dealt to an entity
*/
public class DamageEffect {
    public int Amount {get; private set;}

    public DamageEffect setDamageAmount(int amt) {
        Amount = amt;
        return this;
    }
}

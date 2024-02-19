/**
* Used for raw damage being dealt to an entity
*/
public class DamageEffect : AEffect {
    public int Amount {get; private set;}

    public DamageEffect SetDamageAmount(int amt) {
        Amount = amt;
        return this;
    }
}

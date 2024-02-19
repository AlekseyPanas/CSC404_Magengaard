/**
* Used for objects that react to electricity. Generated by lightning spells
*/
public class LightningEffect : AEffect {
    public float Voltage {get; private set;}

    public LightningEffect SetVoltageAmount(float voltage) {
        Voltage = voltage;
        return this;
    }
}

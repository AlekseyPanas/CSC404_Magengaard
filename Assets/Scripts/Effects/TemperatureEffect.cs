/**
* Used for changing the temperature of an object. Used by fire and ice spells
*/
public class TemperatureEffect : AEffect {
    public float TempDelta {get; private set;}

    public TemperatureEffect SetTemperature(float temperature) {
        TempDelta = temperature;
        return this;
    }
}

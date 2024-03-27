/**
* Used for changing the temperature of an object. Used by fire and ice spells
*/
public class TemperatureEffect : AMeshEffect {
    public float TempDelta { get; set; }
    public bool IsAttack { get; set; }
}

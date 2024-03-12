using Unity.Netcode;
using UnityEngine;

public class ChargeSystemSpellParams: ISpellParams {

    public GestureBinNumbers BinNumber {get; private set;}
    public Vector3 Direction3D {get; private set; }

    public void buildFromContainer(SpellParamsContainer container) {
        Direction3D = container.getVector3(0);
        BinNumber = (GestureBinNumbers)(int)container.getFloat(0);
    }
}

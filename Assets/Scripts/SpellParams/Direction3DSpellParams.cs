using Unity.Netcode;
using UnityEngine;

public class Direction3DSpellParams: ISpellParams {

    private Vector3 direction3d;
    public Vector3 Direction3D {get { return direction3d; } }

    public void buildFromContainer(SpellParamsContainer container) {
        direction3d = container.getVector3(0);
    }
}

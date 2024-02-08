using Unity.Netcode;
using UnityEngine;

public class Direction2DSpellParams: ISpellParams {

    private Vector2 direction2d;
    public Vector2 Direction2D {get { return direction2d; } }

    public void buildFromContainer(SpellParamsContainer container) {
        direction2d = new Vector2(container.getVector3(0).x, container.getVector3(0).y);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireIceDoor : ABarrierActivatable
{
    public GameObject door;
    protected override void BarrierDisable()
    {
        CloseDoor();
    }

    protected override void BarrierEnable()
    {
        OpenDoor();
    }

    void OpenDoor(){
        door.SetActive(false);
    }

    void CloseDoor(){
        door.SetActive(true);
    }
}

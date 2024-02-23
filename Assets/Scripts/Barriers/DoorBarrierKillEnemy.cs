using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorBarrierKillEnemy : ABarrierKillEnemies
{
    [SerializeField] private GameObject exitDoor;
    protected override void BarrierDisable()
    {
        exitDoor.GetComponent<Animator>().SetTrigger("Open");
    }

    protected override void BarrierEnable()
    {
        // nothing here causee door shouldnt be closed again
    }
}

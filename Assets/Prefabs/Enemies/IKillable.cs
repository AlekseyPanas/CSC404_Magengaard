using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IKillable{

    public event Action<GameObject> OnDeath;
}

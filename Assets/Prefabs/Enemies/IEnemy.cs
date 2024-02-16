using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEnemy{

    public event Action<GameObject> OnEnemyDeath;

    public void OnDeath();
}

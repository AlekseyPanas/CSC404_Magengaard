using UnityEngine;
using System;
public interface ITerminal {
    public event Action OnCrystalPlaced;
    public void UpdateState();
    public void ToggleDormant(bool isDormant);
    public bool IsDormant();
    public void PlaceCrystal();
    public void DisableTerminal();
    public void EnableTerminal();
}
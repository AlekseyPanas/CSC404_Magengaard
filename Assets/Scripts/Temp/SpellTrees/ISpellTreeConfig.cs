using System;
using Unity.VisualScripting;
using UnityEngine;

/**
* Attach a child of this class to a blank object for injection into the SpellSystem 
*/
public abstract class ISpellTreeConfig: MonoBehaviour {
    /**
    * Construct a specific spell tree configuration
    */
    public abstract SpellTreeDS buildTree();
}

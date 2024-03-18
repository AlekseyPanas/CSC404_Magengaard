using System.Collections.Generic;
using UnityEngine;

/**
* Attach a child of this class to a blank object for injection into the SpellSystem. This class configures a specific spell tree hierarchy.
* Note that when constructing the hierarchy, ******MAKE SURE created spells have an aim system prefab that generates correct params for the spell prefab******
*/
public abstract class ASpellTreeConfig: MonoBehaviour {
    /** Construct a specific spell tree configuration. Note that the root node is useless and should be a non-value 
    * (because the root acts as the connecting entrypoint to all element trees) */
    public abstract SpellTreeDS buildTree();

    /** Return a SpellTreeDS with null arguments. It is intended to be placed as the tree root; placing it anywhere else will break the code */
    public static SpellTreeDS getNullRoot() {
        return new SpellTreeDS(new SpellDS(null, new Gesture(), -1, new SpellElementColorPalette(new())));
    }
}

using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SpellTreeDS {
    
    private List<SpellTreeDS> children;
    private SpellDS value;

    public SpellTreeDS(SpellDS value) {
        children = new List<SpellTreeDS>();
        this.value = value;
    }

    /** Number of child subtrees from this node */
    public int NumChildren() {return children.Count;}

    /** Gets a child at a specific index */
    public SpellTreeDS getChild(int idx) {return children[idx];}

    /** Returns a read-only list of all child subtrees */
    public IList<SpellTreeDS> getChildren() {return children.AsReadOnly();}
    
    /** Add a child to this tree node */
    public void addChild(SpellTreeDS child) {children.Add(child);} 

    /** Get value at this tree node */
    public SpellDS getValue() {return value;}
}

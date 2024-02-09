using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


public class FullSpellTreeConfig : ASpellTreeConfig
{
    [SerializeField] private GameObject fireballPrefab;

    [SerializeField] private GameObject direction3dAimSystemPrefab;

    public override SpellTreeDS buildTree()
    {
        List<GestComp> fireballGestComponents = new List<GestComp>(){new GestComp(-45, 2), new GestComp(90, 2), new GestComp(90, 1), new GestComp(90, 1)};
        Gesture fireballGest = new Gesture(fireballGestComponents, 0.17f, -1);
        SpellDS fireballSpell = new SpellDS(fireballPrefab, direction3dAimSystemPrefab, fireballGest);
        
        SpellTreeDS fireballNode = new SpellTreeDS(fireballSpell);
        SpellTreeDS root = getNullRoot();
        root.addChild(fireballNode);

        return root;

        // TODO: Not finished, probably wont need the complete tree for a while
    }
}

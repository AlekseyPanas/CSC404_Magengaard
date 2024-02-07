using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


public class PvPSpellTreeConfig : ASpellTreeConfig
{
    [SerializeField] private GameObject sandstormPrefab;

    [SerializeField] private GameObject direction2dAimSystemPrefab;

    public override SpellTreeDS buildTree()
    {
        List<GestComp> sandstormGestComponents = new List<GestComp>(){new GestComp(-45, 2), new GestComp(90, 2), new GestComp(90, 1), new GestComp(90, 1)};
        Gesture sandstormGest = new Gesture(sandstormGestComponents, 0.17f, -1);
        SpellDS sandstormSpell = new SpellDS(sandstormPrefab, direction2dAimSystemPrefab, sandstormGest);
        
        SpellTreeDS sandstormNode = new SpellTreeDS(sandstormSpell);
        SpellTreeDS root = getNullRoot();
        root.addChild(sandstormNode);

        return root;
    }
}

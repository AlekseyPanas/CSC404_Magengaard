using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


public class FullSpellTreeConfig : ASpellTreeConfig
{
    [SerializeField] private GameObject fireballPrefab;

    [SerializeField] private GameObject fireballAimSystemPrefab;
    [SerializeField] private GameObject windImpulsePrefab;

    [SerializeField] private GameObject windImpulseAimSystemPrefab;


    public override SpellTreeDS buildTree()
    {
        List<GestComp> fireballGestComponents = new List<GestComp>(){new GestComp(-45, 2), new GestComp(90, 2), new GestComp(90, 1), new GestComp(90, 1)};
        List<GestComp> windImpulseGestComponents = new List<GestComp>(){new GestComp(45, 1), new GestComp(-90, 1), new GestComp(90, 1)};
        Gesture fireballGest = new Gesture(fireballGestComponents, 0.17f, -1);
        SpellDS fireballSpell = new SpellDS(fireballPrefab, fireballAimSystemPrefab, fireballGest);
        Gesture windImpulseGest = new Gesture(windImpulseGestComponents, 0.17f, -1);
        SpellDS windImpulseSpell = new SpellDS(windImpulsePrefab, windImpulseAimSystemPrefab, windImpulseGest);

        SpellTreeDS fireballNode = new SpellTreeDS(fireballSpell);
        SpellTreeDS windImpulseNode = new SpellTreeDS(windImpulseSpell);
        SpellTreeDS root = getNullRoot();
        root.addChild(fireballNode);
        root.addChild(windImpulseNode);

        return root;

        // TODO: Not finished, probably wont need the complete tree for a while
    }
}

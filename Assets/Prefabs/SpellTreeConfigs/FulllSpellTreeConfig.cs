using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


public class FullSpellTreeConfig : ASpellTreeConfig
{
    // fire
    [SerializeField] private GameObject fireImpulsePrefab;
    [SerializeField] private GameObject fireImpulseAimSystemPrefab;
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private GameObject fireballAimSystemPrefab;
    // ice
    [SerializeField] private GameObject iceImpulsePrefab;
    [SerializeField] private GameObject iceImpulseAimSystemPrefab;
    // wind
    [SerializeField] private GameObject windImpulsePrefab;
    [SerializeField] private GameObject windImpulseAimSystemPrefab;




    public override SpellTreeDS buildTree()
    {
        List<GestComp> fireImpulseGestComponents = new() { new GestComp(110, 2), new GestComp(140, 2), new GestComp(70, 1) };
        List<GestComp> fireballGestComponents = new() { new GestComp(0, 1), new GestComp(-135, 2) };

        List<GestComp> iceImpulseGestComponents = new() { new GestComp(-90, 1), new GestComp(90, 1), new GestComp(90, 1) };

        List<GestComp> windImpulseGestComponents = new() { new GestComp(45, 1), new GestComp(-90, 1), new GestComp(90, 1) };

        Gesture fireImpulseGest = new(fireImpulseGestComponents, 0.17f, -1);
        SpellDS fireImpulseSpell = new(fireImpulsePrefab, fireImpulseAimSystemPrefab, fireImpulseGest);
        Gesture fireballGest = new(fireballGestComponents, 0.17f, -1);
        SpellDS fireballSpell = new(fireballPrefab, fireballAimSystemPrefab, fireballGest);

        Gesture iceImpulseGest = new(iceImpulseGestComponents, 0.17f, -1);
        SpellDS iceImpulseSpell = new(iceImpulsePrefab, iceImpulseAimSystemPrefab, iceImpulseGest);

        Gesture windImpulseGest = new(windImpulseGestComponents, 0.17f, -1);
        SpellDS windImpulseSpell = new(windImpulsePrefab, windImpulseAimSystemPrefab, windImpulseGest);


        SpellTreeDS fireImpulseNode = new SpellTreeDS(fireImpulseSpell);
        SpellTreeDS fireballNode = new SpellTreeDS(fireballSpell);

        SpellTreeDS windImpulseNode = new SpellTreeDS(windImpulseSpell);

        SpellTreeDS iceImpulseNode = new SpellTreeDS(iceImpulseSpell);
        SpellTreeDS root = getNullRoot();


        fireImpulseNode.addChild(fireballNode);

        root.addChild(fireImpulseNode);
        root.addChild(windImpulseNode);
        root.addChild(iceImpulseNode);

        return root;

        // TODO: Not finished, probably wont need the complete tree for a while
    }
}

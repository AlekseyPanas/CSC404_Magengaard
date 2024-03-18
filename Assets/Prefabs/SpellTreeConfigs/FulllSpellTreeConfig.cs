using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


public class FullSpellTreeConfig : ASpellTreeConfig
{
    // fire
    [SerializeField] private GameObject fireImpulsePrefab;
    [SerializeField] private GameObject fireballPrefab;
    // ice
    [SerializeField] private GameObject iceImpulsePrefab;
    [SerializeField] private GameObject iceCubePrefab;
    // wind
    [SerializeField] private GameObject windImpulsePrefab;




    public override SpellTreeDS buildTree()
    {
        

        List<GestComp> fireImpulseGestComponents = new() { new GestComp(110, 2), new GestComp(140, 2), new GestComp(70, 1) };
        List<GestComp> accelerateGestComponents = new() { new GestComp(0, 1), new GestComp(-135, 2) };

        List<GestComp> iceImpulseGestComponents = new() { new GestComp(-90, 1), new GestComp(90, 1), new GestComp(90, 1) };

        List<GestComp> windImpulseGestComponents = new() { new GestComp(45, 1), new GestComp(-90, 1), new GestComp(90, 1) };

        Gesture fireImpulseGest = new(fireImpulseGestComponents, new float[]{ 0.8f, 0.5f, 0.3f, 0.2f });
        SpellDS fireImpulseSpell = new(fireImpulsePrefab, fireImpulseGest, 4, new FirePalette());
        Gesture fireballGest = new(accelerateGestComponents, new float[]{ 0.8f, 0.5f, 0.3f, 0.2f });
        SpellDS fireballSpell = new(fireballPrefab, fireballGest, 2, new FirePalette());

        Gesture iceImpulseGest = new(iceImpulseGestComponents, new float[]{ 0.8f, 0.5f, 0.3f, 0.2f });
        SpellDS iceImpulseSpell = new(iceImpulsePrefab, iceImpulseGest, 4, new IcePalette());
        Gesture iceCubeGest = new(accelerateGestComponents, new float[]{ 0.8f, 0.5f, 0.3f, 0.2f });
        SpellDS iceCubeSpell = new(iceCubePrefab, iceCubeGest, 2, new IcecPalette());

        Gesture windImpulseGest = new(windImpulseGestComponents, new float[]{ 0.8f, 0.5f, 0.3f, 0.2f });
        SpellDS windImpulseSpell = new(windImpulsePrefab, windImpulseGest, 4, new WindPalette());


        SpellTreeDS fireImpulseNode = new SpellTreeDS(fireImpulseSpell);
        SpellTreeDS fireballNode = new SpellTreeDS(fireballSpell);

        SpellTreeDS windImpulseNode = new SpellTreeDS(windImpulseSpell);

        SpellTreeDS iceImpulseNode = new SpellTreeDS(iceImpulseSpell);
        SpellTreeDS iceCubeNode = new SpellTreeDS(iceCubeSpell);
        SpellTreeDS root = getNullRoot();


        fireImpulseNode.addChild(fireballNode);
        iceImpulseNode.addChild(iceCubeNode);

        root.addChild(fireImpulseNode);
        root.addChild(windImpulseNode);
        root.addChild(iceImpulseNode);

        return root;

        // TODO: Not finished, probably wont need the complete tree for a while
    }
}

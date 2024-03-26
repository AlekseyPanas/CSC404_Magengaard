using System;
using UnityEngine;
using UnityEngine.UI;

public class TestContributor: MonoBehaviour, ISpellbookContributor {

    [SerializeField] private Texture2D _testTexture;
    [SerializeField] private Texture2D _testNormal;

    private Texture2D bakedTexture;
    private Texture2D bakedNotif;
    private Texture2D bakedNormal;

    void Start() {
        var tup = ISpellbookContributor.GetBaked(_testTexture, _testNormal);
        bakedTexture = tup.Item1;
        bakedNotif = tup.Item2;
        bakedNormal = tup.Item3;

        ISpellbookContributor.OnContributeContent(bakedTexture, bakedNotif, bakedNormal);
        ISpellbookContributor.OnContributeContent(bakedTexture, bakedNotif, bakedNormal);
        ISpellbookContributor.OnContributeContent(bakedTexture, bakedNotif, bakedNormal);
        ISpellbookContributor.OnContributeContent(bakedTexture, bakedNotif, bakedNormal);
    }

}
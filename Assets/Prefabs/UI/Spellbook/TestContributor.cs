using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TestContributor: ASpellbookContributor {

    [SerializeField] private Texture2D _testTexture;
    [SerializeField] private Texture2D _testNormal;

    void Start() {
        Bake(_testTexture, _testNormal);
        AddBakedContentToSpellbook();
    }

    private IEnumerator test() {
        for (int i = 0; i < 50; i++) {
            AddBakedContentToSpellbook();
            yield return new WaitForSeconds(1);
        }
        yield return null;
    }

}
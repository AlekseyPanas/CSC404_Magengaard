using UnityEngine;

public class SpellSystem: MonoBehaviour {
    private SpellTreeDS spellTree;
    [SerializeField] private ISpellTreeConfig config;
    
    private void Start() {
        spellTree = config.buildTree();
    }
}

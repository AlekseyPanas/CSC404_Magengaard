using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FrameDoings : MonoBehaviour
{
    private MeshRenderer _renderer;

    private List<Texture2D> _textures = new();

    private int _frame = 0;

    void NextFrame()
    {
        _frame++;

        if (_textures.Count == 0)
        {
            return;
        }
        
        var current = _frame % _textures.Count;
        
        var texture = _textures[current];
        
        _renderer.material.mainTexture = texture;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        _renderer = GetComponent<MeshRenderer>();

        // Hardcoded number of frames
        for (var x = 0; x <= 107; x++)
        {
            var tex = Resources.Load<Texture2D>(string.Format("Cave_animation_2_1-{0}", x));
            
            if (tex == null)
            {
                Debug.Log("Panic! Can't find cave animation frame.");
                
                continue;
            }
            
            _textures.Add(tex);
        }
        
        InvokeRepeating(nameof(NextFrame), 0.05f, 0.05f);
    }
}

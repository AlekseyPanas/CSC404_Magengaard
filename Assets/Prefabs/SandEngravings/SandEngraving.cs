using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class SandEngraving : MonoBehaviour
{
    private DecalProjector _rend;
    [SerializeField] private Texture2D _normalMap;

    // Start is called before the first frame update
    void Start()
    {
        _rend = GetComponent<DecalProjector>();
        StartCoroutine(fadeDecal());
    }

    // Update is called once per frame
    void Update()
    { 
    }

    IEnumerator fadeDecal() {
        for (int i = 0; i < 100; i++) {
            _rend.fadeFactor = _rend.fadeFactor - 0.01f;
            Debug.Log(_rend.fadeFactor);
            yield return new WaitForSeconds(0.02f);
        }
        
    }
}

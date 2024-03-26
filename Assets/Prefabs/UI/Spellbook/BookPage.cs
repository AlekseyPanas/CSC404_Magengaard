using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BookPage : MonoBehaviour {

    private Renderer _renderer;

    [SerializeField] private Transform _pageEffector;
    public Transform PageEffector { get { return _pageEffector; } }
    [SerializeField] private GameObject _pageMesh;
    [SerializeField] private Vector2 _fullTextureDims;  // Dims of the entire texture
    public Vector2 FullTextureDims { get { return _fullTextureDims; } }
    [SerializeField] private Vector2 _halfPageDims;  // Dims of the part of the texture corresponding to a page side (starts 0,0)
    public Vector2 HalfPageDims { get { return _halfPageDims; } }

    void Awake() {
        _renderer = _pageMesh.GetComponent<Renderer>();
    }

    private void _ThrowErrorIfDimsWrong(Texture2D tex) {
        if (tex.width != _fullTextureDims.x || tex.height != _fullTextureDims.y) {
            throw new System.Exception("Page texture must be of size " + _fullTextureDims.x + "by " + _fullTextureDims.y);
        }
    }

    public void SetLeftTexture(Texture2D tex) {
        _ThrowErrorIfDimsWrong(tex);
        _renderer.material.SetTexture("_BaseMap", tex);
    }

    public void SetRightTexture(Texture2D tex) {
        _ThrowErrorIfDimsWrong(tex);
        _renderer.material.SetTexture("_BaseMap", tex);
    }

    public void SetLeftNormal(Texture2D norm) {
        _ThrowErrorIfDimsWrong(norm);
        _renderer.materials[1].SetTexture("_BumpMap", norm);
    }

    public void SetRightNormal(Texture2D norm) {
        _ThrowErrorIfDimsWrong(norm);
        _renderer.materials[1].SetTexture("_BumpMap", norm);
    }
}

using System.Collections.Generic;
using UnityEngine;

public class TilePreviewer : MonoBehaviour
{
    //*********************************************************************
    // Data Members
    //*********************************************************************
    // Public
    //---------------------------------------------------------------------
    public Material previewMaterial;
    public Color validColor;
    public Color invalidColor;
    public string colorPropertyName = "_BaseColor";

    //---------------------------------------------------------------------
    // Private
    //---------------------------------------------------------------------
    private GameObject _currentPrefab = null;
    private GameObject _previewInstance = null;
    private Renderer[] _renderers;
    private MaterialPropertyBlock _mpb;
    private int _colorPropertyId;
    //*********************************************************************

    /// <summary>
    /// Unity awake method.
    /// </summary>
    void Awake() {
        _mpb = new MaterialPropertyBlock();
        _colorPropertyId = Shader.PropertyToID(colorPropertyName);
    }

    /// <summary>
    /// How to properly set a prefab to be previewed
    /// </summary>
    public void SetPrefab(GameObject prefab) {
        if (prefab == _currentPrefab) return;

        _currentPrefab = prefab;

        if (_previewInstance != null) {
            Destroy(_previewInstance);
            _previewInstance = null;
            _renderers = null;
        }

        SetValid(true);

        _previewInstance = Instantiate(prefab, transform);
        _previewInstance.name = $"{_currentPrefab.name}_PREVIEW";
        _renderers = _previewInstance.GetComponentsInChildren<Renderer>(true);

        for (int i = 0; i < _renderers.Length; i++) {
            var mats = _renderers[i].sharedMaterials;
            for (int j = 0; j < _renderers[i].sharedMaterials.Length; j++) {
                mats[j] = previewMaterial;
            }
            _renderers[i].sharedMaterials = mats;
        }
    }

    /// <summary>
    /// Enable the prefab parent.
    /// </summary>
    public void SetValid(bool status) {
        if (_previewInstance == null || _renderers == null) return;

        Color tint = status ? validColor : invalidColor;

        _mpb.Clear();
        _mpb.SetColor(_colorPropertyId, tint);

        for (int i = 0; i < _renderers.Length; i++)
            _renderers[i].SetPropertyBlock(_mpb);
    }

    /// <summary>
    /// Disable the preview.
    /// </summary>
    public void Hide() {
        if (_previewInstance != null) {
            _previewInstance.SetActive(false);
        }
    }
}

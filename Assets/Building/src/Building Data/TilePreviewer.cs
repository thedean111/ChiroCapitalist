using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem;

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
    public Color pendingMoveColor;
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
        if (prefab == _currentPrefab) { _previewInstance.SetActive(true); return;}

        _currentPrefab = prefab;

        if (_previewInstance != null) {
            Destroy(_previewInstance);
            _previewInstance = null;
            _renderers = null;
        }

        SetTint(TilePreviewState.Valid);

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
    public void SetTint(TilePreviewState state) {
        if (_previewInstance == null || _renderers == null) return;

        Color tint = state switch {
            TilePreviewState.Valid => validColor,
            TilePreviewState.Invalid => invalidColor,
            TilePreviewState.Pending_Move => pendingMoveColor,
            _ => invalidColor
        };

        _mpb.Clear();
        _mpb.SetColor(_colorPropertyId, tint);

        for (int i = 0; i < _renderers.Length; i++)
            _renderers[i].SetPropertyBlock(_mpb);
    }

    /// <summary>
    /// Enable the prefab parent.
    /// </summary>
    public void SetTint(Color color) {
        if (_previewInstance == null || _renderers == null) return;

        _mpb.Clear();
        _mpb.SetColor(_colorPropertyId, color);

        for (int i = 0; i < _renderers.Length; i++)
            _renderers[i].SetPropertyBlock(_mpb);
    }

    public void Toggle(bool status) {
        if (_previewInstance != null) {
            _previewInstance.SetActive(status);
        }
    }

    /// <summary>
    /// Sets the position and rotation relative to the parent.
    /// </summary>
    public void SetLocalPositionRotation(Vector3 localPos, Vector3 localRot, float time = 0.2f) {
        _previewInstance.transform.DORotate(localRot, time).SetEase(Ease.OutBack);
        _previewInstance.transform.DOLocalMove(localPos, time).SetEase(Ease.OutBack);
    }

    public void SetPositionRotation(Vector3 worldPos, Vector3 localRot) {
        _previewInstance.transform.position = worldPos;
        _previewInstance.transform.rotation = Quaternion.Euler(localRot);
    }
}

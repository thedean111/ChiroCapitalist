using UnityEngine;
using DG.Tweening;

public class GridPlacementMarker : MonoBehaviour
{
    public float activeEdgeWidth = 0.95f;

    private Vector3 defaultScale = new Vector3(1, 0.1f, 1);
    private MeshRenderer rend;

    void Awake()
    {
        rend = GetComponentInChildren<MeshRenderer>();
    }

    /// <summary>
    /// Resize the grid marker based on the provided input. Typically due to a change in player selection.
    /// <param name="size">The XZ size of the tile to represent. </param>
    /// </summary>
    public void Resize(Vector2Int size)
    {
        // TODO: Add a shader to the marker that can adapt to any tile size, and change colors on the bottom where the grid cells overlap.
        transform.DOScale(new Vector3(size.x, .1f, size.y), 0.25f).SetEase(Ease.OutCubic);
        rend.material.SetFloat("_Alpha", 1f);
    }

    /// <summary>
    /// Return the grid marker to its default state.
    /// </summary>
    public void Reset()
    {
        // TODO: Add a shader to the marker that can fill in.
        rend.material.SetFloat("_Alpha", 0.5f);
        transform.DOScale(defaultScale, 0.25f).SetEase(Ease.OutCubic);
        transform.localScale = defaultScale;
    }
}

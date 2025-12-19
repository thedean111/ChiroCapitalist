using UnityEngine;
using DG.Tweening;

public class GridPlacementMarker : MonoBehaviour
{
    //*********************************************************************
    // Data Members
    //*********************************************************************
    // Public
    //---------------------------------------------------------------------
    public Vector2Int Size {get; private set;}

    [Range(0,1f)] public float markerMoveTime = 0.2f;
    [Range(0,1f)] public float markerScaleTime = 0.2f;


    //---------------------------------------------------------------------
    // Private
    //---------------------------------------------------------------------
    private Vector3 _defaultMarkerPosition = Vector3.up * 50;
    private Vector3 _defaultScale = new Vector3(1, 0.1f, 1);
    private Transform _cellHighlighter;

    //*********************************************************************

    /// <summary>
    /// Unity awake method.
    /// </summary>
    void Awake()
    {
        _cellHighlighter = transform.GetChild(0);
        transform.position = _defaultMarkerPosition;
    }

    /// <summary>
    /// Tween the position of the marker to a new position.
    /// </summary>
    public void Move(Vector3 position) {
        if (transform.position == _defaultMarkerPosition) {
            transform.position = position;
        } else {
            transform.DOMove(position, markerMoveTime).SetEase(Ease.OutCubic);
        }
    }

    /// <summary>
    /// Resize the grid marker based on the provided input. Typically due to a change in player selection.
    /// <param name="size">The XZ size of the tile to represent. </param>
    /// </summary>
    public void Resize(Vector2Int size)
    {
        // TODO: Add a shader to the marker that can adapt to any tile size, and change colors on the bottom where the grid cells overlap.
        _cellHighlighter.DOScale(new Vector3(size.x, .1f, size.y), markerScaleTime).SetEase(Ease.OutCubic);
        Size = size;
    }

    /// <summary>
    /// Return the grid marker to its default state.
    /// </summary>
    public void Reset()
    {
        transform.position = _defaultMarkerPosition;
        Resize(Vector2Int.one);
    }
}

using UnityEngine;

public class GridPlacementMarker : MonoBehaviour
{
    private Vector3 defaultScale = new Vector3(1, 0.15f, 1);

    /// <summary>
    /// Resize the grid marker based on the provided input. Typically due to a change in player selection.
    /// <param name="size">The XZ size of the tile to represent. </param>
    /// </summary>
    public void Resize(Vector2Int size)
    {
        // TODO: Add a shader to the marker that can adapt to any tile size, and change colors on the bottom where the grid cells overlap.
        transform.localScale = new Vector3(size.x, 3, size.y);
    }

    /// <summary>
    /// Return the grid marker to its default state.
    /// </summary>
    public void Reset()
    {
        // TODO: Add a shader to the marker that can fill in.
        transform.localScale = defaultScale;
    }
}

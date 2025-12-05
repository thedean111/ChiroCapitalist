using UnityEngine;

[CreateAssetMenu(fileName = "PlaceableTile", menuName = "Scriptable Objects/PlaceableTile")]
public class PlaceableTile : ScriptableObject
{
    public Texture2D icon;
    public int cost;
    public Vector2Int size;
    public GameObject prefab;
}

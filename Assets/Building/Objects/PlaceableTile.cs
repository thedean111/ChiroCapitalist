using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlaceableTile", menuName = "Scriptable Objects/PlaceableTile")]
public class PlaceableTile : ScriptableObject
{
    public Texture2D icon;
    public int cost;
    public Vector2Int size;
    public CellType defaultType;        // Every cell covered by this tile will be this type by default
    public GameObject prefab;
    public GameObject hologram;
}

public enum CellType {
    NONE,
    ANY,
    HALLWAY,
    OFFICE,
    INTERFACE
}
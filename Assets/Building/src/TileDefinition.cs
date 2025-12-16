using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "TileDefinition", menuName = "Scriptable Objects/TileDefinition")]
public class TileDefinition : ScriptableObject
{
    [Header("UI Data")]
    public string tileName;
    public Texture2D icon;
    public string description;
    public int cost;

    [Header("Tile Footprint")]
    public Vector2Int size;
    public CellType type;

    [Header("Prefab")]
    public GameObject prefab;

    [Header("Special Cells")]
    public List<SpecialCellData> specialCells = new();

    public void GetFlags(Vector2Int localCell, out CellFlags flags) {
        flags = CellFlags.None;
        foreach (SpecialCellData sc in specialCells) {
            if (sc.localCoord == localCell) { 
                flags |= sc.overrideType;
            }
        }
    }
}

[System.Serializable]
public class SpecialCellData {
    public Vector2Int localCoord;
    public CellFlags overrideType;
}

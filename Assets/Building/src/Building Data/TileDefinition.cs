using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "TileDefinition", menuName = "Scriptable Objects/TileDefinition")]
public class TileDefinition : ScriptableObject
{    
    public TileDetailsFlags detailFlags { get; private set; }
    [Header("UI Data")]
    public string tileName;
    public Texture2D icon;
    public int cost;
    public string description;
    public bool usesLevel;
    public bool usesProgress;
    public bool usesDoctor;

    [Header("Tile Footprint")]
    public Vector2Int size;
    public CellType type;
    public CellFlags defaultFlag;

    [Header("Prefab")]
    public GameObject prefab;

    [Header("Special Cells")]
    public List<SpecialCellData> specialCells = new();

    void OnEnable() {
        detailFlags = TileDetailsFlags.None;
        if (usesLevel) detailFlags |= TileDetailsFlags.Level;
        if (usesProgress) detailFlags |= TileDetailsFlags.Progress;
        if (usesDoctor) detailFlags |= TileDetailsFlags.Doctor;
    }

    public void GetFlags(Vector2Int localCell, out CellFlags flags) {
        flags = defaultFlag;
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

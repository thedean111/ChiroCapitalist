public enum CellType {
    None,
    Hallway,
    Office
}

public enum CellFlags {
    None = 0,
    Interface = 1<<0,
    ConnectSameOrAnchor = 1<<1,
    Anchor = 1<<2,
    NoInterface = 1<<3,
    ExternalInterfaceOnly = 1<<4,
    BaseWallOnly = 1<<5
}

public enum WallType {
    None,
    Exterior,
    NoWindow,
    Interior,
    DoorExterior,
    DoorInterior
}

public enum TilePreviewState {
    Valid,
    Invalid,
    Pending_Move
}

// Every tile will show the description portion so its not defined here
public enum TileDetailsFlags
{
    None = 0,
    Level = 1 << 0,
    Progress = 1 << 1,
    Doctor = 1 << 2
}

public enum StatCategory {
    Strength,
    Technique,
    Magic
}

public enum PropLevelAction {
    Add,
    Remove,
    Replace
}
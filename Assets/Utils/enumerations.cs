public enum CellType {
    None,
    Hallway,
    Office
}

public enum CellFlags {
    None = 0,
    Interface = 1<<0,
    ConnectSameOrAnchor = 1<<1,
    Anchor = 1<<2
}

public enum WallType {
    None,
    Exterior,
    Interior,
    Door
}

public enum TilePreviewState {
    Valid,
    Invalid,
    Pending_Move
}
public enum CellType {
    None,
    Hallway,
    Office
}

public enum CellFlags {
    None = 0,
    Interface = 1<<0,
    Merge = 1<<1
}

public enum WallType {
    None,
    Exterior,
    Interior,
    Door
}
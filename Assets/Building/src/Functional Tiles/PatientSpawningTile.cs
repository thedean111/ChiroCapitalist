using System.Collections.Generic;
using UnityEngine;

public abstract class PatientSpawningTile : Tile
{
    //*********************************************************************
    // Data Members
    //*********************************************************************
    // Public
    //---------------------------------------------------------------------
    public List<PatientSpawnerLevelInfo> levelDetails = new();
    public bool IsPaused {get; private set;}

    //---------------------------------------------------------------------
    // Private
    //---------------------------------------------------------------------
    private float _lastSpawnTimestamp;
    private float _pauseTime;
    protected int _currentPatientCount;
    protected bool _spawnLock = false;
    //*********************************************************************

    /// <summary>
    /// Initialization logic for when this tile is spawned
    /// </summary>
    public override void PlaceTile()
    {
        base.PlaceTile();
    }

    /// <summary>
    /// Initialization logic for when this tile is spawned
    /// </summary>
    public override void Initialize() {
        base.Initialize();

        Level = 0;
        _lastSpawnTimestamp = Time.time;
        _currentPatientCount = 0;
    }

    /// <summary>
    /// Logic to execute when leveling up a tile that spawns patients.
    /// </summary>
    public void LevelUp() {
        Level++;
        LevelUpBehavior();
    }

    /// <summary>
    /// Custom level up logic for children to implement.
    /// </summary>
    protected abstract void LevelUpBehavior();

    /// <summary>
    /// Toggles the pause flag which will halt patient spawning and other functions of this tile.
    /// </summary>
    public void Pause() {
        IsPaused = true;
        _pauseTime = Time.time;
    }

    /// <summary>
    /// Toggles the pause flag which will continue patient spawning and other functions of this tile.
    /// </summary>
    public void Resume() {
        IsPaused = false;
        _lastSpawnTimestamp += _pauseTime;
    }

    /// <summary>
    /// Manages timestamps for spawning patients.
    /// </summary>
    protected virtual void Update() {
        if (IsPaused) { return; }

        // The spawnLock is here for finer control over when the spawn should actually happen.
        // For instance, spawning may be locked until an animation finishes.
        if ((Time.time - _lastSpawnTimestamp >= levelDetails[Level].spawnTime) &&
            (_currentPatientCount >= levelDetails[Level].patientCapacity) &&
            !_spawnLock) {
            _lastSpawnTimestamp = Time.time;
            _currentPatientCount++;
            HandleNewPatient(NPCFactory.Instance.GeneratePatientData());
        }
    }

    /// <summary>
    /// Default logic for letting a patient go.
    /// </summary>
    protected void ReleasePatient() {
        _currentPatientCount--;
        ReleasePatientBehavior();
    }

    /// <summary>
    /// Custom patient release logic for children to implement.
    /// </summary>
    protected abstract void ReleasePatientBehavior();

    /// <summary>
    /// What this tile should do with the passed in patient data, assuming it is a new patient to spawn.
    /// </summary>
    protected abstract void HandleNewPatient(PatientData patient);
}

// Simple container for level-specific tile data.
[System.Serializable]
public class PatientSpawnerLevelInfo {
    [Range(0, 30)] public int spawnTime;
    [Range(1f, 3f)] public float rewardModifier;
    [Range(1,5)] public int patientCapacity;
}
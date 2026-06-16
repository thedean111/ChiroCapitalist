using System.Collections.Generic;
using System.Collections;
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
    public int updateSteps = 20;
    public bool spawnOnInit = false;
    //---------------------------------------------------------------------
    // Private
    //---------------------------------------------------------------------
    private float _lastSpawnTimestamp;
    private float _pauseTime;
    private bool _spawningPatient;
    protected int _currentPatientCount;
    protected bool _spawnLock = false;
    protected int _step = 0;
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
        _lastSpawnTimestamp = Time.time;
        _currentPatientCount = 0;

        if (spawnOnInit)
            StartCoroutine(SpawnPatientCoroutine());
    }

    // /// <summary>
    // /// Logic to execute when leveling up a tile that spawns patients.
    // /// </summary>
    // public override void LevelUp() {
    //     base.LevelUp();
    //     LevelUpBehavior();
    // }

    // /// <summary>
    // /// Custom level up logic for children to implement.
    // /// </summary>
    // protected abstract void LevelUpBehavior();

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
    public IEnumerator SpawnPatientCoroutine() {
        _step = 0;
        float _stepSize = (float)levelDetails[Level].spawnTime / updateSteps;
        _spawningPatient = true;
        while (_step < updateSteps) {
            UpdateProgress((float)_step / updateSteps * 100);

            yield return new WaitForSeconds(_stepSize);    
            _step++;
        }
        
        _currentPatientCount++;
        _spawningPatient = false;
        HandleNewPatient(NPCFactory.Instance.GeneratePatientData());

        CompleteProgress();

        // Spawn until capacity is reached
        if (_currentPatientCount < levelDetails[Level].patientCapacity) {
            StartCoroutine(SpawnPatientCoroutine());
        }
    }

    /// <summary>
    /// Default logic for letting a patient go.
    /// </summary>
    protected void ReleasePatient() {
        _currentPatientCount--;

        // Spawn until capacity is reached
        if (!_spawningPatient &&
            _currentPatientCount < levelDetails[Level].patientCapacity) {
            StartCoroutine(SpawnPatientCoroutine());
        }
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
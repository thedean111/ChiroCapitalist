using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

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

    /// <summary>
    /// Logic to execute when leveling up a tile that spawns patients.
    /// </summary>
    public override bool LevelUp() {
        if (base.LevelUp()) {
            if (!_spawningPatient && !_spawnLock && _currentPatientCount < getLevelDetails().patientCapacity) {
                StartCoroutine(SpawnPatientCoroutine());
            }
            return true;
        }
        return false;
    }

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
    /// Gets the current level details with protected indexing
    /// </summary>
    public PatientSpawnerLevelInfo getLevelDetails() {
        return levelDetails[Mathf.Min(Level-1, levelDetails.Count-1)];
    }

    /// <summary>
    /// Manages timestamps for spawning patients.
    /// </summary>
    public IEnumerator SpawnPatientCoroutine() {
        _step = 0;
        float _stepSize = (float)getLevelDetails().spawnTime / updateSteps;
        _spawningPatient = true;
        while (_step < updateSteps) {
            UpdateProgress((float)_step / updateSteps * 100);

            yield return new WaitForSeconds(_stepSize);    
            _step++;
        }
        
        _currentPatientCount++;
        _spawningPatient = false;
        HandleNewPatient(NPCFactory.Instance.GeneratePatientData());

        // TODO: Check if the max amount of patients have spawned, if so adjust the UI to reflect we are at the
        // max. Add a css class to the progress bar components?
        CompleteProgress();

        // Spawn until capacity is reached
        if (_currentPatientCount < getLevelDetails().patientCapacity) {
            StartCoroutine(SpawnPatientCoroutine());
        }
    }

    /// <summary>
    /// Default logic for letting a patient go.
    /// </summary>
    protected void ReleasePatient() {
        _currentPatientCount--;

        // Spawn until capacity is reached
        if (!_spawningPatient && _currentPatientCount < getLevelDetails().patientCapacity) {
            // UIManager.Instance.UpdateProgressBarText("waiting...");
            UIManager.Instance.GetProgressBar().RemoveFromClassList("tile-details-max-patients");
            StartCoroutine(SpawnPatientCoroutine());
        }
        ReleasePatientBehavior();
    }

    /// <summary>
    /// Check if the max amount of patients have spawned. If so, update the progress bar to reflect
    /// this.
    /// </summary>
    public override void ProgressCompleted(ProgressBar bar) {
        if (_currentPatientCount >= getLevelDetails().patientCapacity) {
            // bar.title = "MAX";
            bar.AddToClassList("tile-details-max-patients");
        }
    }

    /// <summary>
    /// This logic should be ran when the tile is clicked.
    /// </summary>
    public override void UpdateProgressState(ProgressBar bar)
    {
        base.UpdateProgressState(bar);
        if (_currentPatientCount >= getLevelDetails().patientCapacity) {
            // bar.title = "MAX";
            bar.value = 100;
            bar.AddToClassList("tile-details-max-patients");
        } else if (_spawningPatient) {
            // bar.title = "waiting...";
            bar.RemoveFromClassList("tile-details-max-patients");
        } else {
            bar.value = 0;
            bar.RemoveFromClassList("tile-details-max-patients");
        }

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
    [Range(1,6)] public int patientCapacity;
}
using System.Collections.Generic;
using UnityEngine;

public class SpawnPointTile : PatientSpawningTile
{
    public List<PatientSpawner> spawnPoints = new();
    public string spawnAnimationName;
    public override void Initialize() {
        base.Initialize();

        foreach (PatientSpawner s in spawnPoints) {
            s.SetReleaseAction(ReleasePatient);
        }
    }

    // TODO: We need to implement a spawn point class that can be tied to patients in the HandleNewPatient method. Each spawn
    // point should have a floating UI element that is tied to it

    /// <summary>
    /// Attempt to spawn the patient on the next available spawn point.
    /// </summary>
    protected override void HandleNewPatient(PatientData patient)
    {  
        // NOTE: The patient should always be spawned on one of the visible chairs. So the patient cap should match the
        // prop rules with levels
        for (int i = 0; i < getLevelDetails().patientCapacity; i++) {
            if (spawnPoints[i].TrySpawnPatient(spawnAnimationName)) {
                return;
            }
        }
    }

    /// <summary>
    /// Remove the patient from its spawn point, resume the clock for patient spawning.
    /// </summary>
    protected override void ReleasePatientBehavior()
    {
    }


}

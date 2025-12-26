using UnityEngine;

public class AutoAdjustingOffice : PatientSpawningTile
{
    //*********************************************************************
    // Data Members
    //*********************************************************************
    // Public
    //---------------------------------------------------------------------
    public Patient patient;
    public float adjustmentTime = 3f;
    //---------------------------------------------------------------------
    // Private
    //---------------------------------------------------------------------
    private float _patientSpawnTime;
    //*********************************************************************

    /// <summary>
    /// Custom initialization for the AutoAdjustingOffice.
    /// </summary>
    public override void Initialize() {
        base.Initialize();
        if (patient == null) {
            Debug.LogWarning("AutoAdjustingOffice: Requires a patient object to operate properly!");
            return;
        }

        patient.gameObject.SetActive(false);
    }

    /// <summary>
    /// While there is an active patient
    /// </summary>
    protected override void Update() {
        base.Update();

        if (_currentPatientCount == 0) { return;}

        if (Time.time - _patientSpawnTime >= adjustmentTime) {
            ReleasePatient();
        }
    }

    /// <summary>
    /// Utilize the provided patient object to manifest the spawned patient data.
    /// TODO: Begin an adjustment sequence.
    /// </summary>
    protected override void HandleNewPatient(PatientData patientData)
    {
        if (patient == null) {
            return;
        }

        patient.SetData(patientData);
        patient.gameObject.SetActive(true);
        _patientSpawnTime = Time.time;
    }

    /// <summary>
    /// On top of the default behavior, update the decorations.
    /// </summary>
    protected override void LevelUpBehavior()
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Initialization logic for when this tile is spawned
    /// </summary>
    protected override void ReleasePatientBehavior()
    {
        _spawnLock = true;
        patient.gameObject.SetActive(false);
        _spawnLock = false;
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class AutoAdjustingOffice : PatientSpawningTile
{
    //*********************************************************************
    // Data Members
    //*********************************************************************
    // Public
    //---------------------------------------------------------------------
    public Patient patient;
    public Doctor doctor;
    public float adjustmentTime = 3f;
    public Transform poseableObjects;
    //---------------------------------------------------------------------
    // Private
    //---------------------------------------------------------------------
    private float _stepSize;
    private bool _adjustingPatient = false;
    private Transform[] objects;
    private StatCategory _currentAdjustmentStat;
    //*********************************************************************

    /// <summary>
    /// When this tile is focused update the UI accordingly
    /// </summary>
    public override void OnFocus() {
        UIManager.Instance.UpdateProgressBarProgress((float)_step / updateSteps * 100);
        UIManager.Instance.UpdateProgressBarText(_adjustingPatient ? "Adjusting patient..." : "waiting...");
        UIManager.Instance.UpdateDoctorDetails(doctor.data);

    }

    /// <summary>
    /// Update the doctor assigned to this tile. TODO: What is the appropriate way to handle progress and whatnot when doctor assignment happens.
    /// </summary>
    public override void UpdateDoctorAssignment(DoctorData newDoctor) {
        if (newDoctor == null) {
            doctor.data = null;
            doctor.gameObject.SetActive(false);

        } else {
            doctor.SetData(newDoctor);
            doctor.gameObject.SetActive(true);
            StartCoroutine(SpawnPatientCoroutine());
        }

        UIManager.Instance.UpdateDoctorDetails(doctor.data);
    }

    /// <summary>
    /// Custom initialization for the AutoAdjustingOffice.
    /// </summary>
    public override void Initialize() {
        base.Initialize();
        if (patient == null) {
            Debug.LogWarning("AutoAdjustingOffice: Requires a patient object to operate properly!");
            return;
        }
        objects = poseableObjects.GetComponentsInChildren<Transform>();
        doctor.data = null;
        doctor.gameObject.SetActive(false);
        patient.gameObject.SetActive(false);
    }

    /// <summary>
    /// Adjust the current patient over a period of time
    /// </summary>
    public IEnumerator AdjustPatientRoutine() {
        _step = 0;
        _spawnLock = true;

        while (_step < updateSteps) {
            UpdateProgress((float)_step / updateSteps * 100);

            yield return new WaitForSeconds(_stepSize);    
            _step++;
        }

        // Perform the action
        switch (_currentAdjustmentStat) {
            case StatCategory.Strength:
                patient.PlayAnimationClip($"Strength.{doctor.data.race.strength.action_patient.name}");
                doctor.PlayAnimationClip($"Strength.{doctor.data.race.strength.action_doctor.name}");
                break;

            case StatCategory.Technique:
                patient.PlayAnimationClip($"Technique.{doctor.data.race.technique.action_patient.name}");
                doctor.PlayAnimationClip($"Technique.{doctor.data.race.technique.action_doctor.name}");
                break;

            case StatCategory.Magic:
                patient.PlayAnimationClip($"Magic.{doctor.data.race.magic.action_patient.name}");
                doctor.PlayAnimationClip($"Magic.{doctor.data.race.magic.action_doctor.name}");
                break;

            default:
                break;
        }

        // Let the action animation play for a moment before doing anything
        yield return new WaitForSeconds(1f);

        // Then spawn a patient
        ReleasePatient();
        CompleteProgress();
        _spawnLock = false;

        // TODO: Fire off a unique idle routine
        // PlaySequence(PlayspaceService.Instance.idleSequences[Random.Range(0, PlayspaceService.Instance.idleSequences.Count)]);

    }

    /// <summary>
    /// Utilize the provided patient object to manifest the spawned patient data.
    /// </summary>
    protected override void HandleNewPatient(PatientData patientData)
    {
        if (patient == null) {
            return;
        }

        patient.SetData(patientData);
        patient.gameObject.SetActive(true);

        _currentAdjustmentStat = patientData.stats.GetDominantStat();
        switch (_currentAdjustmentStat) {
            case StatCategory.Strength:
                patient.PlayAnimationClip($"Strength.{doctor.data.race.strength.buildup_patient.name}", 0f);
                doctor.PlayAnimationClip($"Strength.{doctor.data.race.strength.buildup_doctor.name}", 0f);
                break;

            case StatCategory.Technique:
                patient.PlayAnimationClip($"Technique.{doctor.data.race.technique.buildup_patient.name}", 0f);
                doctor.PlayAnimationClip($"Technique.{doctor.data.race.technique.buildup_doctor.name}", 0f);
                break;

            case StatCategory.Magic:
                patient.PlayAnimationClip($"Magic.{doctor.data.race.magic.buildup_patient.name}", 0f);
                doctor.PlayAnimationClip($"Magic.{doctor.data.race.magic.buildup_doctor.name}", 0f);
                break;

            default:
                break;
        }
        _stepSize = adjustmentTime / updateSteps;
        _adjustingPatient = true;

        // TODO: Fire off an adjustment sequence based on the stats
        // PlaySequence(PlayspaceService.Instance.idleSequences[Random.Range(0, PlayspaceService.Instance.idleSequences.Count)]);

        StartCoroutine(AdjustPatientRoutine());
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
        patient.gameObject.SetActive(false);
        _adjustingPatient = false;
    }

    public override void ProgressCompleted(ProgressBar bar)
    {
        base.ProgressCompleted(bar);
        bar.title = _adjustingPatient ? "Adjusting patient..." : "waiting...";
    }

    /// <summary>
    /// How to translate the data in an office sequence to room behavior
    /// </summary>
    private void PlaySequence(OfficeSequence seq) {
        // Position all of the objects by the provided data
        doctor.transform.localPosition = seq.objectPose.doctorPose.localPosition;
        doctor.transform.localRotation = seq.objectPose.doctorPose.localRotation;
        patient.transform.localPosition = seq.objectPose.patientPose.localPosition;
        patient.transform.localRotation = seq.objectPose.patientPose.localRotation;
        
        for (int i = 0; i < objects.Length; i++) {
            if (i >= seq.objectPose.decorPose.Count)
                break;

            objects[i].localPosition = seq.objectPose.decorPose[i].localPosition;
            objects[i].localRotation = seq.objectPose.decorPose[i].localRotation;
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using DG.Tweening;

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
    
    public OfficeSequence adjustmentPose;
    public List<OfficeSequence> sittingIdles;
    public List<OfficeSequence> standingIdles;
    public ParticleSystem poseChangeEffect;
    //---------------------------------------------------------------------
    // Private
    //---------------------------------------------------------------------
    private float _stepSize;
    private bool _adjustingPatient = false;
    private List<Transform> objects = new();
    private StatCategory _currentAdjustmentStat;
    //*********************************************************************

    /// <summary>
    /// When this tile is focused update the UI accordingly
    /// </summary>
    public override void OnFocus() {
        UIManager.Instance.UpdateProgressBarProgress((float)_step / updateSteps * 100);
        // UIManager.Instance.UpdateProgressBarText(_adjustingPatient ? "Adjusting patient..." : "waiting...");
        UIManager.Instance.UpdateDoctorDetails(doctor.data);

    }

    /// <summary>
    /// Update the doctor assigned to this tile. TODO: What is the appropriate way to handle progress and whatnot when doctor assignment happens.
    /// </summary>
    public override void UpdateDoctorAssignment(DoctorData newDoctor) {
        if (newDoctor == null) {
            doctor.data = null;
            doctor.gameObject.SetActive(false);
            doctor.transform.position = Vector3.up * -15;
            patient.transform.position = Vector3.up * -15;

        } else {
            doctor.SetData(newDoctor);
            doctor.gameObject.SetActive(true);
            StartCoroutine(SpawnPatientCoroutine());
        }

        _spawnLock = false;
        UIManager.Instance.UpdateDoctorDetails(doctor.data);
        PlayIdle();
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

        objects.Clear();
        for (int i = 0; i < poseableObjects.childCount; i++) {
            objects.Add(poseableObjects.GetChild(i));
        }
        doctor.data = null;
        doctor.gameObject.SetActive(false);
        patient.gameObject.SetActive(false);
        doctor.transform.position = Vector3.up * -15;
        patient.transform.position = Vector3.up * -15;
        _spawnLock = true;
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
                patient.PlayAnimationClip($"Strength.{doctor.data.race.raceName.ToLower()}_strength_action_patient", 0.2f);
                doctor.PlayAnimationClip($"Strength.{doctor.data.race.raceName.ToLower()}_strength_action_doctor", 0.2f);
                break;

            case StatCategory.Technique:
                patient.PlayAnimationClip($"Technique.{doctor.data.race.raceName.ToLower()}_technique_action_patient", 0.2f);
                doctor.PlayAnimationClip($"Technique.{doctor.data.race.raceName.ToLower()}_technique_action_doctor", 0.2f);
                break;

            case StatCategory.Magic:
                patient.PlayAnimationClip($"Magic.{doctor.data.race.raceName.ToLower()}_magic_action_patient", 0.2f);
                doctor.PlayAnimationClip($"Magic.{doctor.data.race.raceName.ToLower()}_magic_action_doctor", 0.2f);
                break;

            default:
                break;
        }

        // Let the action animation play for a moment before doing anything
        yield return new WaitForSeconds(1f);

        // Reward the player
        // TODO: potential consider skill bonuses here?
        ProgressionManager.Instance.AwardNpcAdjustment(1);

        // Then spawn a patient
        ReleasePatient();
        CompleteProgress();
        _spawnLock = false;

        // Play Idle
        PlayIdle();
    }

    /// <summary>
    /// Utilize the provided patient object to manifest the spawned patient data.
    /// </summary>
    protected override void HandleNewPatient(PatientData patientData)
    {
        if (patient == null) {
            return;
        }

        _stepSize = adjustmentTime / updateSteps;
        _adjustingPatient = true;

        PlaySequence(adjustmentPose, false, ()=>{
            patient.SetData(patientData);
            patient.gameObject.SetActive(true);
            _currentAdjustmentStat = patientData.stats.GetDominantStat();
            switch (_currentAdjustmentStat) {
                case StatCategory.Strength:
                    patient.PlayAnimationClip($"Strength.{doctor.data.race.raceName.ToLower()}_strength_buildup_patient", 0f);
                    doctor.PlayAnimationClip($"Strength.{doctor.data.race.raceName.ToLower()}_strength_buildup_doctor", 0f);
                    break;

                case StatCategory.Technique:
                    patient.PlayAnimationClip($"Technique.{doctor.data.race.raceName.ToLower()}_technique_buildup_patient", 0f);
                    doctor.PlayAnimationClip($"Technique.{doctor.data.race.raceName.ToLower()}_technique_buildup_doctor", 0f);
                    break;

                case StatCategory.Magic:
                    patient.PlayAnimationClip($"Magic.{doctor.data.race.raceName.ToLower()}_magic_buildup_patient", 0f);
                    doctor.PlayAnimationClip($"Magic.{doctor.data.race.raceName.ToLower()}_magic_buildup_doctor", 0f);
                    break;

                default:
                    break;
            }
        });

        StartCoroutine(AdjustPatientRoutine());
    }

    // /// <summary>
    // /// On top of the default behavior, update the decorations.
    // /// </summary>
    // protected override void LevelUpBehavior()
    // {
    //     throw new System.NotImplementedException();
    // }

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
        // bar.title = _adjustingPatient ? "Adjusting patient..." : "waiting...";
    }

    /// <summary>
    /// Determine what idle sequence to play and fire it off
    /// </summary>
    private void PlayIdle() {
        int idleType = UnityEngine.Random.Range(0, 2);
        OfficeSequence seq = null;

        switch(idleType) {
            // Sitting sequence
            case 0:
                seq = sittingIdles[UnityEngine.Random.Range(0, sittingIdles.Count)];
                break;

            // Standing sequence
            case 1:
                seq = standingIdles[UnityEngine.Random.Range(0, standingIdles.Count)];
                break;
        }

        if (seq != null) {
            PlaySequence(seq, true);
        }
    }

    /// <summary>
    /// How to translate the data in an office sequence to room behavior.
    /// </summary>
    private void PlaySequence(OfficeSequence seq, bool overrideAnimation=false, Action onFade=null, bool fadeInOnly = false) {        
        
        // Fade everything off, move, then fade back in
        PlayspaceService.DitherFadeObject(poseableObjects, 0f, 0.3f, () => {
            for (int i = 0; i < objects.Count; i++) {
                if (i >= seq.objectPose.decorPose.Count)
                    break;

                objects[i].localPosition = seq.objectPose.decorPose[i].localPosition;
                objects[i].localRotation = seq.objectPose.decorPose[i].localRotation;
            }
            if (overrideAnimation) {
                patient.PlayAnimationClip(seq.patientAnimationName, 0f);
                doctor.PlayAnimationClip(seq.doctorAnimationName, 0f);
            }

            onFade?.Invoke();

            PlayspaceService.DitherFadeObject(poseableObjects, 1f, 0.3f, () =>{});
        });

        // poseChangeEffect.transform.position = doctor.transform.position;
        // poseChangeEffect.Play();
    }
}

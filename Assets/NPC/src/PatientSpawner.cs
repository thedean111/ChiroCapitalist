using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;

public class PatientSpawner : MonoBehaviour
{   
    public Vector3 buttonOffset;
    public GameObject patientPrefab;
    public Transform spawnPivot;
    [Range(0.05f, 1f)] public float fadeTime;

    public bool activePatient {get; private set;}

    private BoxCollider coll;
    private Patient patient;
    private UIDocument adjustmentButtonPivot;
    private Action onRelease;
    private VisualElement adjustmentButton;

    private void OnEnable()
    {
        spawnPivot = transform.Find("patient_pivot");
        adjustmentButtonPivot = transform.Find("button_pivot").GetComponent<UIDocument>();
        coll = GetComponent<BoxCollider>();

        if (adjustmentButton == null) {
            adjustmentButton = adjustmentButtonPivot.rootVisualElement.Q<VisualElement>("adjustment-button");
            adjustmentButton.SetEnabled(false);
            adjustmentButtonPivot.transform.DOLocalMoveY(2.3f, 1f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
        }

        if (patient == null) {
            activePatient = false;
            patient = Instantiate(patientPrefab, Vector3.down * 10, Quaternion.identity, spawnPivot).GetComponent<Patient>();
            patient.transform.localRotation = Quaternion.identity;
            patient.transform.localScale = Vector3.one;
            patient.SetData(NPCFactory.Instance.GeneratePatientData());
            PlayspaceService.DitherFadeObject(patient.transform, 0f, 0f, () => patient.gameObject.SetActive(false));
        }
    }

    public void SetReleaseAction(Action a) {
        onRelease = a;
    }

    /// <summary>
    /// Attempt to spawn a patient at this spawn point. Returns the success of the spawn.
    /// </summary>
    public bool TrySpawnPatient(string animationName) {
        if (activePatient) { return false; }

        coll.enabled = true;
        activePatient = true;
        patient.transform.localPosition = Vector3.zero;
        patient.SetData(NPCFactory.Instance.GeneratePatientData());
        patient.gameObject.SetActive(true);
        patient.PlayAnimationClip(animationName, 0f);
        PlayspaceService.DitherFadeObject(patient.transform, 1f, fadeTime, () => {
            adjustmentButton.SetEnabled(true);
        });

        
        return true;
    }

    /// <summary>
    /// Attempt to remove the patient at this point, if there is one.
    /// </summary>
    public bool TryRemovePatient(string animationName) {
        if (!activePatient) { return false; }

        coll.enabled = false;
        adjustmentButton.SetEnabled(false);
        patient.PlayAnimationClip(animationName, 0.1f);
        PlayspaceService.DitherFadeObject(patient.transform, 0f, fadeTime, () => {
            activePatient = false;
            onRelease?.Invoke();
            patient.gameObject.SetActive(false);
        });

        return true;
    }

    void LateUpdate()
    {
        if (activePatient){
            // 1. Get the target position to look at (the camera's position)
            Vector3 targetPosition = Camera.main.transform.position;

            // 2. Flatten the target position to the same height (Y) as the UI anchor.
            // This locks the rotation strictly to the vertical axis.
            targetPosition.y = adjustmentButtonPivot.transform.position.y;

            // 3. Make the object look at this flattened target position
            // Note: If your UI canvas is backwards, swap the order: (transform.position * 2) - targetPosition
            adjustmentButtonPivot.transform.LookAt(targetPosition);
                    }
    }

    public void ToggleOutline(bool status) {
        PlayspaceService.Instance.UpdateOutline(patient.transform, status, Color.white);
    }
}

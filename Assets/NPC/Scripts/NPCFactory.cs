using System;
using System.Collections.Generic;
using UnityEngine;

public class NPCFactory : MonoBehaviour
{
    private static NPCFactory instance = null;
    public static NPCFactory Instance { get { return instance; } }          

    [Header("Race Details")]
    public NPCRaceData[] races; // Each NPCRaceData contains colors, meshes, stat distributions, etc.

    //
    // PRIVATE DATA
    //

    // private Stack<Patient> patientObjects; // This collection of patients is designed such that created patients can be reused as needed
    private List<DoctorData> doctorData; // Collection of all the generated doctor data

    //
    //
    //

    void Awake()
    {
        if (instance == null) { instance = this; }
    }

    /// <summary>
    /// This function will return a randomized patient object
    /// </summary>
    public PatientData GeneratePatientData()
    {
        // Create a patient data
        PatientData pd = new PatientData();

        // Pick a random race, and populate the patient data object with its details
        races[UnityEngine.Random.Range(0, races.Length)].PopulateNPCData(pd);
        return pd;
    }

    /// <summary>
    /// This function will randomly generate information for a new doctor. It will be added to internal storage and be returned to the caller.
    /// </summary>
    public DoctorData GenerateDoctorData()
    {
        return new DoctorData();
    }

    /// <summary>
    /// Return the stored list of generated doctor data.
    /// </summary>
    public List<DoctorData> GetDoctorData()
    {
        return doctorData;
    }
}

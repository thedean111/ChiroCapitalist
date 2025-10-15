using System;
using System.Collections.Generic;
using UnityEngine;

public class NPCFactory : MonoBehaviour
{
    private static NPCFactory instance = null;
    public static NPCFactory Instance { get { return instance; } }

    [Header("NPC Prefab")]
    public NPC prefab; // Base prefab for NPCs              

    [Header("Race Details")]
    public NPCRaceData[] races; // Each NPCRaceData contains colors, meshes, stat distributions, etc.

    //
    // PRIVATE DATA
    //

    private Stack<Patient> patientObjects; // This collection of patients is designed such that created patients can be reused as needed
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
    public Patient GetPatient()
    {
        /*
            TODO:
                1. Check the stack to see if there is a patient not being used
                    a. If the stack has data, pop the top and use that as the GO to return
                    b. If the stack doesn't have data, create a new patient object to provide (GameObject.Instantiate())
                2. Randomize the patient by randomly selecting one of the NPCRaceData objects
                    a. NOTE: should the NPCRaceData contain a method that just returns the randomly selected data, provided a stat total?
                3. Return the randomized patient game object
        */

        return null;
    }

    /// <summary>
    /// When a patient object is done being used, it will add itself back to the factory using this method
    /// </summary>
    public void ReturnPatient(Patient patient)
    {
        patientObjects.Push(patient);
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

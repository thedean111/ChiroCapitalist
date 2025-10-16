using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public enum StatType
{
    STRENGTH,
    TECHNIQUE,
    MAGIC
}

public enum PatientDifficulty
{
    EASY=0,
    MEDIUM=1,
    HARD=2
}

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

        // The patient needs are separate from the race distribution, so generate the patient's needs here
        // Consult the progression manager for the current patient difficulty and a stat total for the patient
        float pool = ProgressionManager.Instance.GetStatTotal();
        PatientDifficulty difficulty = ProgressionManager.Instance.GetPatientDifficulty();

        // Compute patient stats based off of the pool and difficulty...
        // use this list to help
        List<StatType> types = new List<StatType> { StatType.MAGIC, StatType.STRENGTH, StatType.TECHNIQUE };

        // This can be done by looping based on the difficulty enumeration
        // Randomly compute weights for each required stat and normalize with the pool
        for (int i = 0; i <= (int)difficulty; i++)
        {
            int idx = Random.Range(0, types.Count);
            switch (types[idx])
            {
                case StatType.STRENGTH:
                    pd.stats.x = Random.Range(.2f, 1f);
                    break;
                case StatType.TECHNIQUE:
                    pd.stats.y = Random.Range(.2f, 1f);
                    break;
                case StatType.MAGIC:
                    pd.stats.z = Random.Range(.2f, 1f);
                    break;
                default:
                    break;
            }
            types.RemoveAt(idx);
        }

        // Currently, the stats in 'pd' just contain the randomly generated weights...
        // convert them to stat totals here by normalizing the weights
        pd.stats = (pd.stats / (pd.stats.x + pd.stats.y + pd.stats.z)) * pool;

        return pd;
    }

    /// <summary>
    /// This function will randomly generate information for a new doctor. It will be added to internal storage and be returned to the caller.
    /// </summary>
    public DoctorData GenerateDoctorData()
    {
        // Create a patient data
        DoctorData dd = new DoctorData();

        // Pick a random race, and populate the patient data object with its details
        races[UnityEngine.Random.Range(0, races.Length)].PopulateNPCData(pd);

        // TODO: NEED GACHA LOGIC HERE FOR GENERATING THE STATS
        // -> NPC Rarity influences stat pool, multipliers, aesthetics, etc.


        // Add the data to the stored list and return it to the caller
        doctorData.Add(dd);
        return new dd();
    }

    /// <summary>
    /// Return the stored list of generated doctor data.
    /// </summary>
    public List<DoctorData> GetDoctorData()
    {
        return doctorData;
    }
}

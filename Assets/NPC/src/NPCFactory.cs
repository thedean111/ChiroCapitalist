using System;
using System.Collections.Generic;
using Unity.VisualScripting;
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

    [Header("Baseline Info")]
    public GameObject basePrefab;
    public Material baseMaterial;

    [Header("Race Details")]
    public NPCRaceData[] races; // Each NPCRaceData contains colors, meshes, stat distributions, etc.

    [Header("Default Data")]
    public Texture2D defaultDoctorIcon;

    //
    // PRIVATE DATA
    //

    // private Stack<Patient> patientObjects; // This collection of patients is designed such that created patients can be reused as needed
    private List<DoctorData> doctorData; // Collection of all the generated doctor data
    private Dictionary<string, int> meshIdxMap = new Dictionary<string, int>();
    Transform meshRoot;
    //
    //
    //

    void Awake()
    {
        if (instance == null) { instance = this; }
        doctorData = new List<DoctorData>();
        
        MapCategoriesToIdx();
    }

    /// <summary>
    /// This will map all race and body part categories to indices within the base prefab under the "meshes" transform
    /// for easy access at run time. This is sort of hardcoded, but should be fine for this small game.
    /// </summary>
    private void MapCategoriesToIdx() {
        meshRoot = basePrefab.transform.GetChild(0).Find("meshes");
        for (int i = 0; i < meshRoot.childCount; i++) {
            Transform child = meshRoot.GetChild(i);
            if (child.name == "doctor") {
                meshIdxMap["doctor"] = i;
                MapCategoryPartToIdx("doctor", child);

            } else if (child.name == "generic") {
                meshIdxMap["generic"] = i;
                MapCategoryPartToIdx("generic", child);

            } else if (child.name == "elf") {
                meshIdxMap["elf"] = i;
                MapCategoryPartToIdx("elf", child);

            } else if (child.name == "human") {
                meshIdxMap["human"] = i;
                MapCategoryPartToIdx("human", child);

            } else if (child.name == "orc") {
                meshIdxMap["orc"] = i;
                MapCategoryPartToIdx("orc", child);

            }
        }
    }

    // Maybe there will be some weird hierarchy shuffling that will cause the order of body parts to be different in each race category.
    private void MapCategoryPartToIdx(string category, Transform catRoot) {
        for (int i = 0; i < catRoot.childCount; i++) {
            Transform child = catRoot.GetChild(i);
            string cName = child.name.Split('.')[0];
            if (cName == "head") {
                meshIdxMap[$"{category}_{cName}"] = i;
            } else if (cName == "feet") {
                meshIdxMap[$"{category}_{cName}"] = i;
            } else if (cName == "legs") {
                meshIdxMap[$"{category}_{cName}"] = i;
            } else if (cName == "torso") {
                meshIdxMap[$"{category}_{cName}"] = i;
            }
        }
    }

    /// <summary>
    /// This function will return a randomized patient object
    /// </summary>
    public PatientData GeneratePatientData()
    {
        // Create a patient data
        PatientData pd = new PatientData();

        // Pick a random race, and populate the patient data object with its details
        NPCRaceData randomRace = races[Random.Range(0, races.Length)];
        randomRace.PopulateNPCData(pd);

        // Select a random mesh for each body part based on an child-index path 
        pd.head = SelectPartMesh("head", randomRace.raceName, false);
        pd.legs = SelectPartMesh("legs", randomRace.raceName);
        pd.feet = SelectPartMesh("feet", randomRace.raceName);
        pd.torso = SelectPartMesh("torso", randomRace.raceName);


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
                    pd.stats.strength = Random.Range(.2f, 1f);
                    break;
                case StatType.TECHNIQUE:
                    pd.stats.technique = Random.Range(.2f, 1f);
                    break;
                case StatType.MAGIC:
                    pd.stats.magic = Random.Range(.2f, 1f);
                    break;
                default:
                    break;
            }
            types.RemoveAt(idx);
        }

        // Currently, the stats in 'pd' just contain the randomly generated weights...
        // convert them to stat totals here by normalizing the weights
        float total = pd.stats.strength + pd.stats.technique + pd.stats.magic;
        pd.stats.strength = Math.Clamp(pd.stats.strength / total * pool, 1, ProgressionManager.Instance.maxStatValue);
        pd.stats.technique = Math.Clamp(pd.stats.technique / total * pool, 1, ProgressionManager.Instance.maxStatValue);
        pd.stats.magic = Math.Clamp(pd.stats.magic / total * pool, 1, ProgressionManager.Instance.maxStatValue);
        pd.stats.ComputeNormalValues(ProgressionManager.Instance.maxStatValue);
        return pd;
    }

    private Vector3Int SelectPartMesh(string part, string raceName, bool randomGeneric = true) {
        string raceCategory = (randomGeneric && Random.Range(0, 2) == 1) ? "generic" : raceName.ToLower(); // 50% chance to just use a generic mesh
        int raceIdx = meshIdxMap[raceCategory];
        int partIdx = meshIdxMap[$"{raceCategory}_{part}"];
        if (randomGeneric && meshRoot.GetChild(raceIdx).GetChild(partIdx).childCount == 0) { // If the race has no meshes for the body part, use the generic set
            raceIdx = meshIdxMap["generic"];
            partIdx = meshIdxMap[$"generic_{part}"];
        }
        int meshIdx = Random.Range(0, meshRoot.GetChild(raceIdx).GetChild(partIdx).childCount);

        return new Vector3Int(raceIdx, partIdx, meshIdx);
    }

    /// <summary>
    /// This function will randomly generate information for a new doctor. It will be added to internal storage and be returned to the caller.
    /// </summary>
    public DoctorData GenerateDoctorData()
    {
        // Create a patient data
        DoctorData dd = new DoctorData();

        // Pick a random race, and populate the patient data object with its details
        NPCRaceData randomRace = races[Random.Range(0, races.Length)];
        randomRace.PopulateNPCData(dd);
        dd.icon = defaultDoctorIcon;

        // Make a random doctor
        // Select a random mesh for each body part based on an child-index path 
        dd.head = SelectPartMesh("head", randomRace.raceName, false);
        dd.legs = SelectPartMesh("legs", "doctor");
        dd.feet = SelectPartMesh("feet", "doctor");
        dd.torso = SelectPartMesh("torso", "doctor");

        // TODO: NEED GACHA LOGIC HERE FOR GENERATING THE STATS
        // -> NPC Rarity influences stat pool, multipliers, aesthetics, etc.


        // Add the data to the stored list and return it to the caller
        doctorData.Add(dd);
        return dd;
    }

    /// <summary>
    /// Return the stored list of generated doctor data.
    /// </summary>
    public List<DoctorData> GetDoctorData()
    {
        return doctorData;
    }
}

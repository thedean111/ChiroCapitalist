using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif
public class NPCGenerationTester : MonoBehaviour
{

    public GameObject patientPrefab;
    private List<string> anims = new List<string> { "action_strength", "action_technique", "action_magic" };

    [Range(0f, 1f)] public float crossFadeTime = 0.1f;
    /// <summary>
    /// This method should spawn a patient in the XZ plane, bounded by the game object's scale.
    /// </summary>
    public void SpawnPatient()
    {
        Patient p;
        if (Instantiate(patientPrefab, transform).TryGetComponent<Patient>(out p))
        {
            // Randomly generate patient data and provide it to the created object
            PatientData pd = NPCFactory.Instance.GeneratePatientData();
            p.SetData(pd);

            // LOG
            Debug.Log("Patient Stats: " + pd.stats);
            //

            // Randomly position the spawned object in the boundary
            Vector3 boundsScale = transform.Find("bounds").localScale;
            float halfXScale = boundsScale.x / 2;
            float halfZScale = boundsScale.z / 2;
            Vector3 spawnLocation = new(UnityEngine.Random.Range(-halfXScale, halfXScale), 0, UnityEngine.Random.Range(-halfZScale, halfZScale));
            p.transform.position = spawnLocation;

        }
        else
        {
            Debug.LogWarning("Cannot obtain the 'Patient' component from the spawned object!.");
        }
    }

    /// <summary>
    /// For each child with a Patient component, play a random animation
    /// </summary>
    public void PlayRandomAnimation()
    {
        foreach (Patient p in transform.GetComponentsInChildren<Patient>())
        {
            p.PlayAnimationClip(anims[UnityEngine.Random.Range(0, anims.Count)], crossFadeTime);
        }
    }

    /// <summary>
    /// Delete all children except the "bounds" object
    /// </summary>
    public void DeleteChildren()
    {
#if UNITY_EDITOR
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            if (string.Equals("bounds", transform.GetChild(i).name))
            {
                continue;
            }
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        EditorUtility.SetDirty(transform);
    #endif
    }
}

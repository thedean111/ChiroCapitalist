using UnityEngine;

[CreateAssetMenu(fileName = "newRaceData", menuName = "Game Data/NPC Race", order =1)]
public class NPCRaceData : ScriptableObject
{
    [Header("Mesh Data")]
    public Mesh head;
    public Mesh[] torso;
    public Mesh[] pants;
    public Mesh[] hair;
    public Mesh[] shoes;

    [Header("Colors")]
    public Gradient[] skinColors;
    public Gradient[] hairColors;
    public Gradient[] primaryColors;
    public Gradient[] secondaryColors;

    [Header("Stat Distribution")]
    [Range(0f, 1f)] public float wStrength;
    [Range(0f, 1f)] public float wTechnique;
    [Range(0f, 1f)] public float wMagic;


    /// <summary>
    /// Randomly select data from all the contained data in this race and populate the provided container.
    /// </summary>
    public void PopulateNPCData(NPCData data)
    {
        // Assign the meshes
        data.head = head;
        data.torso = torso[Random.Range(0, torso.Length)];
        data.pants = pants[Random.Range(0, pants.Length)];
        data.hair = hair[Random.Range(0, hair.Length)];
        data.shoes = shoes[Random.Range(0, shoes.Length)];

        // Colors
        data.skinColor = skinColors[Random.Range(0, skinColors.Length)].Evaluate(Random.Range(0f, 1f));

        data.hairColors.primary = hairColors[Random.Range(0, hairColors.Length)].Evaluate(Random.Range(0f, 1f));
        data.hairColors.secondary = hairColors[Random.Range(0, hairColors.Length)].Evaluate(Random.Range(0f, 1f));

        data.torsoColors.primary = primaryColors[Random.Range(0, primaryColors.Length)].Evaluate(Random.Range(0f, 1f));
        data.torsoColors.secondary = secondaryColors[Random.Range(0, secondaryColors.Length)].Evaluate(Random.Range(0f, 1f));

        data.pantsColors.primary = primaryColors[Random.Range(0, primaryColors.Length)].Evaluate(Random.Range(0f, 1f));
        data.pantsColors.secondary = secondaryColors[Random.Range(0, secondaryColors.Length)].Evaluate(Random.Range(0f, 1f));

        data.shoesColors.primary = primaryColors[Random.Range(0, primaryColors.Length)].Evaluate(Random.Range(0f, 1f));
        data.shoesColors.secondary = secondaryColors[Random.Range(0, secondaryColors.Length)].Evaluate(Random.Range(0f, 1f));
    }
}

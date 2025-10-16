using UnityEngine;
using ColorStudio;

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
    public CSPalette primaryColors;
    public CSPalette secondaryColors;
    public CSPalette tertiaryColors;

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

        // For the colors, first select the skin color that will be used for this NPC
        // ...this color should be the "accent" for all the meshes
        Color c_Skin = skinColors[Random.Range(0, skinColors.Length)].Evaluate(Random.Range(0f, 1f));

        primaryColors.BuildHueColors();
        secondaryColors.BuildHueColors();
        tertiaryColors.BuildHueColors();
        Color[] primary = primaryColors.BuildPaletteColors();
        Color[] secondary = primaryColors.BuildPaletteColors();
        Color[] tertiary = primaryColors.BuildPaletteColors();

        data.skinColors.primary = primary[Random.Range(0, primary.Length)];
        data.skinColors.secondary = secondary[Random.Range(0, secondary.Length)];
        data.skinColors.tertiary = tertiary[Random.Range(0, tertiary.Length)];
        data.skinColors.accent = c_Skin;

        data.hairColors.primary = hairColors[Random.Range(0, hairColors.Length)].Evaluate(Random.Range(0f,1f));
        data.hairColors.secondary = secondary[Random.Range(0, secondary.Length)];
        data.hairColors.tertiary = tertiary[Random.Range(0, tertiary.Length)];
        data.hairColors.accent = c_Skin;

        Debug.Log(primary);
        data.torsoColors.primary = primary[Random.Range(0, primary.Length)];
        data.torsoColors.secondary = secondary[Random.Range(0, secondary.Length)];
        data.torsoColors.tertiary = tertiary[Random.Range(0, tertiary.Length)];
        data.torsoColors.accent = c_Skin;

        data.pantsColors.primary = primary[Random.Range(0, primary.Length)];
        data.pantsColors.secondary = secondary[Random.Range(0, secondary.Length)];
        data.pantsColors.tertiary = tertiary[Random.Range(0, tertiary.Length)];
        data.pantsColors.accent = c_Skin;

        data.shoesColors.primary = primary[Random.Range(0, primary.Length)];
        data.shoesColors.secondary = secondary[Random.Range(0, secondary.Length)];
        data.shoesColors.tertiary = tertiary[Random.Range(0, tertiary.Length)];
        data.shoesColors.accent = c_Skin;
    }
}

using UnityEngine;
using ColorStudio;

public abstract class NPC : MonoBehaviour
{
    private Vector3Int headIdx, legsIdx, feetIdx, torsoIdx;
    private bool idxSet = false;
    private Transform meshRoot;
    private Animator anim;
    private bool isMale;

    void Awake()
    {
        meshRoot = transform.GetChild(0).Find("meshes");
        anim = GetComponentInChildren<Animator>();
    }

    public bool IsMale() { return isMale; }

    /// <summary>
    /// The generic functionality for giving an NPC its data will be to assign the meshes and their colors.
    /// </summary>
    public void SetData(NPCData data)
    {
        // Turn off the existing models so they don't overlap with the new ones
        if (idxSet) {
            meshRoot.GetChild(headIdx.x).GetChild(headIdx.y).GetChild(headIdx.z).gameObject.SetActive(false);
            meshRoot.GetChild(legsIdx.x).GetChild(legsIdx.y).GetChild(legsIdx.z).gameObject.SetActive(false);
            meshRoot.GetChild(feetIdx.x).GetChild(feetIdx.y).GetChild(feetIdx.z).gameObject.SetActive(false);
            meshRoot.GetChild(torsoIdx.x).GetChild(torsoIdx.y).GetChild(torsoIdx.z).gameObject.SetActive(false);
        }

        headIdx = data.head;
        legsIdx = data.legs;
        feetIdx = data.feet;
        torsoIdx = data.torso;
        isMale = data.isMale;

        // Set the colors on the material instances and activate the meshes
        SkinnedMeshRenderer head = meshRoot.GetChild(data.head.x).GetChild(data.head.y).GetChild(data.head.z).GetComponent<SkinnedMeshRenderer>();
        head.sharedMaterial = NPCFactory.Instance.baseMaterial;
        SetRendererColors(head, data.skinColors);
        head.gameObject.SetActive(true);

        SkinnedMeshRenderer legs = meshRoot.GetChild(data.legs.x).GetChild(data.legs.y).GetChild(data.legs.z).GetComponent<SkinnedMeshRenderer>();
        legs.sharedMaterial = NPCFactory.Instance.baseMaterial;
        SetRendererColors(legs, data.legsColors);
        legs.gameObject.SetActive(true);

        SkinnedMeshRenderer feet = meshRoot.GetChild(data.feet.x).GetChild(data.feet.y).GetChild(data.feet.z).GetComponent<SkinnedMeshRenderer>();
        feet.sharedMaterial = NPCFactory.Instance.baseMaterial;
        SetRendererColors(feet, data.feetColors);
        feet.gameObject.SetActive(true);

        SkinnedMeshRenderer torso = meshRoot.GetChild(data.torso.x).GetChild(data.torso.y).GetChild(data.torso.z).GetComponent<SkinnedMeshRenderer>();
        torso.sharedMaterial = NPCFactory.Instance.baseMaterial;
        SetRendererColors(torso, data.torsoColors);
        torso.gameObject.SetActive(true);

        idxSet = true;
    }

    /// <summary>
    /// Play an animation in this NPC's animator.
    /// </summary>
    public void PlayAnimationClip(string stateName)
    {
        PlayAnimationClip(stateName, 0.2f);
    }

    /// <summary>
    /// Play an animation in this NPC's animator.
    /// </summary>
    public void PlayAnimationClip(string stateName, float delta)
    {
        anim.CrossFade(stateName, delta);
    }

    /// <summary>
    /// Takes in a skinned mesh renderer and sets its primary, secondary, and skin colors.
    /// </summary>
    private void SetRendererColors(SkinnedMeshRenderer smr, NPCData.ColorSet colors)
    {
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        smr.GetPropertyBlock(mpb);
        mpb.SetColor("_Skin", colors.skin);
        mpb.SetColor("_Primary", colors.primary);
        mpb.SetColor("_Secondary", colors.secondary);
        mpb.SetColor("_Tertiary", colors.tertiary);
        mpb.SetColor("_Accent", colors.accent);
        smr.SetPropertyBlock(mpb);
    }

    /* 
    TODO:
        1. Generalized animation framework for different stat types
            a. "buildup", "action", "master" animations
            b. The "buildup" animation can loop and will be used to alter the total adjustment time
            c. The "action" animation is what plays at the end of the time (can be sped up slightly if needed)
            d. The "master" animation is what plays instead of the buildup/action should the necessary animation need to be super fast
        2. Easy way to dynamically set/play animations
            a. Patients only need one type of animations
            b. Doctors should contain all the types of animations
    */
}

public class NPCData
{
    // x - race, y - body part, z - mesh option
    public Vector3Int hair, head, torso, legs, feet;
    public ColorSet skinColors, hairColors, torsoColors, legsColors, feetColors;
    public string name;
    public NPCRaceData race;
    public NPCStats stats; // x - Strength, y - Technique, z - Magic
    public bool isMale;

    public NPCData() {
        stats = new NPCStats();
        isMale = Random.Range(0, 1f) < 0.5f; // 50% chance of being male/female
    }


    public struct ColorSet
    {
        public Color skin;
        public Color primary;
        public Color secondary;
        public Color tertiary;
        public Color accent;
        
    }

}

using System;
using UnityEngine;
using System.Collections.Generic;

public abstract class NPC : MonoBehaviour
{
    private SkinnedMeshRenderer hair, head, pants, shoes, torso;

    void Awake()
    {
        hair = transform.Find("hair").GetComponent<SkinnedMeshRenderer>();
        head = transform.Find("head").GetComponent<SkinnedMeshRenderer>();
        pants = transform.Find("pants").GetComponent<SkinnedMeshRenderer>();
        shoes = transform.Find("shoes").GetComponent<SkinnedMeshRenderer>();
        torso = transform.Find("torso").GetComponent<SkinnedMeshRenderer>();
    }

    /// <summary>
    /// Set the skinned meshes of the objects to that of the inputs.
    /// </summary>
    public virtual void SetSkinnedMeshes(Mesh mHair, Mesh mHead, Mesh mPants, Mesh mShoes, Mesh mTorso)
    {
        hair.sharedMesh = mHair;
        head.sharedMesh = mHead;
        pants.sharedMesh = mPants;
        shoes.sharedMesh = mShoes;
        torso.sharedMesh = mTorso;
    }

    /// <summary>
    /// The generic functionality for giving an NPC its data will be to assign the meshes and their colors.
    /// </summary>
    public void SetData(NPCData data)
    {
        SetSkinnedMeshes(data.hair, data.head, data.pants, data.shoes, data.torso);
        SetRendererColors(hair, data.skinColor, data.hairColors);
        SetRendererColors(head, data.skinColor, data.hairColors);
        SetRendererColors(pants, data.skinColor, data.pantsColors);
        SetRendererColors(shoes, data.skinColor, data.shoesColors);
        SetRendererColors(torso, data.skinColor, data.torsoColors);
    }

    private void SetRendererColors(SkinnedMeshRenderer smr, Color skin, NPCData.ColorSet colors)
    {
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        smr.GetPropertyBlock(mpb);
        mpb.SetColor("_Primary", colors.primary);
        mpb.SetColor("_Secondary", colors.secondary);
        mpb.SetColor("_Skin", skin);
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
    public Mesh hair, head, torso, pants, shoes;
    public Color skinColor;
    public ColorSet hairColors, torsoColors, pantsColors, shoesColors;
    public string name;
    // public Dictionary<StatType, float> stats;
    public Vector3 stats; // x - Strength, y - Technique, z - Magic

    // TODO: Stats


    public struct ColorSet
    {
        public Color primary;
        public Color secondary;
    }

}

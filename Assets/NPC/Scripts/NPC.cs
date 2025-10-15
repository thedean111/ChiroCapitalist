using UnityEngine;

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
    /// Functionality that needs to happen to remove this NPC from the screen.
    /// </summary>
    public virtual void Dismiss() { gameObject.SetActive(false); }
    
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

using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using System;

public class Tile : MonoBehaviour
{
    public List<SpecialCell> specialCells;
    public Transform decorRoot;
    public enum TileType {
        HALLWAY,
        OFFICE
    }
    public TileType tileType;

    public Transform propParent;
    public bool lockOutline = false;
    public bool isValid = true;
    public bool isDirty = false;
    public bool anchor;

    private Vector3 targetRotation = Vector3.zero;
    private Tween rotationTween = null;
    private bool outlineEnabled;
    private List<Outline> outlinedObjects = new List<Outline>();

    /// <summary>
    /// Logic for placing down a tile.
    /// </summary>
    /// <param name="animateTime"> Tween time for transform animations. </param>
    /// <param name="propRotation"> The euler angles that container for props should be set to. </param>
    public void Place(float animateTime, Vector3 propRotation)
    {
        // Any tile successfully placed starts off as valid
        isValid = true;

        if (decorRoot == null) { return; }
        propParent.Rotate(propRotation);
        
        bool isRoot = true;
        foreach (Transform child in decorRoot.GetComponentsInChildren<Transform>())
        {
            if (isRoot) {isRoot = false; continue; }

            Vector3 posT = child.localPosition;
            Vector3 scaleT = child.localScale;

            child.localScale = Vector3.zero;
            child.localPosition = posT + (Vector3.up * 1.2f);

            child.DOScale(scaleT, animateTime).SetEase(Ease.OutBack);
            child.DOLocalMove(posT, animateTime).SetEase(Ease.OutBack);
        }

        // Store a list of Outline components for this tile
        outlinedObjects.Clear();
        foreach (Outline o in transform.GetComponentsInChildren<Outline>()) {
            outlinedObjects.Add(o);
        }
    }

    /// <summary>
    /// Logic for rotating a tile.
    /// </summary>
    /// <param name="animateTime"> Tween time for transform animations. </param>
    public void Rotate(float animateTime, Action onComplete) {
        // Every time the player wants to rotate, just update the target rotation by 90 degrees
        targetRotation += Vector3.up * 90;
        if (rotationTween != null) { rotationTween.Kill(); }
        rotationTween = propParent.DOLocalRotate(targetRotation, animateTime).SetEase(Ease.OutBack).OnComplete(() => onComplete?.Invoke());
    }

    /// <summary>
    /// Turn the outline of all meshes on or off, if they have the Outline component.
    /// </summary>
    public void ToggleOutline(bool status) {
        if (lockOutline) {return;}
        outlineEnabled = status;

        foreach (Outline o in outlinedObjects)
        {
            o.enabled = outlineEnabled;
        }
    }

    /// <summary>
    /// Change the outline color of this tile
    /// </summary>
    public void ChangeOutlineColor(Color color) {
        foreach (Outline o in outlinedObjects)
        {
            o.OutlineColor = color;
        }
    }

    /// <summary>
    /// Force this tile to be highlighted based on its validity.
    /// </summary>
    public void ValidityOutline(Color valid, Color invalid)
    {
        lockOutline = false;
        // FIXME: DEBUG SO ITS OBVIOUS WHICH TILES ARE INVALID
        if (!isValid)
        {
            transform.DOMove(Vector3.one, 0.2f);
        }
        ChangeOutlineColor(isValid ? valid : invalid);
        ToggleOutline(true);
        lockOutline = true;
    }
}

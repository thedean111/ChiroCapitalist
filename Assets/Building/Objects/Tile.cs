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

    private Vector3 targetRotation = Vector3.zero;
    private Tween rotationTween = null;

    /// <summary>
    /// Logic for placing down a tile.
    /// </summary>
    /// <param name="animateTime"> Tween time for transform animations. </param>
    /// <param name="propRotation"> The euler angles that container for props should be set to. </param>
    public void Place(float animateTime, Vector3 propRotation)
    {
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
}

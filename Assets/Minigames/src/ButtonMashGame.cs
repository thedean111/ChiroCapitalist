using System;
using UnityEngine;
using UnityEngine.UIElements;

public class ButtonMashGame : BaseMinigame
{
    [Header("References")]
    public UIDocument buttonMashUI;

    [Header("Game Scaling")]
    public AnimationCurve statToSize;
    
    protected ButtonMashProgressBar mashProgress;

    void OnEnable()
    {
        mashProgress = buttonMashUI.rootVisualElement.Q<ButtonMashProgressBar>("button-mash-progress");
        buttonMashUI.rootVisualElement.pickingMode = PickingMode.Ignore;
        buttonMashUI.rootVisualElement.SetEnabled(false);
    }

    public override void OpenGame(NPCStats stats)
    {
        base.OpenGame(stats);
        
        // Enable the UI
        buttonMashUI.rootVisualElement.SetEnabled(true);
        buttonMashUI.rootVisualElement.pickingMode = PickingMode.Position;

        // Configure the initial state of the meter based on difficulty and skills
        mashProgress.progressMeter = 0;
        mashProgress.targetOffset = 0;
        mashProgress.targetHeight = statToSize.Evaluate(stats.n_strength) * 100f; // [0,1] -> [low strength, max strength]
    }

    public override void CompleteGame()
    {
        base.CompleteGame();

        buttonMashUI.rootVisualElement.pickingMode = PickingMode.Ignore;
        buttonMashUI.rootVisualElement.SetEnabled(false);
    }
}
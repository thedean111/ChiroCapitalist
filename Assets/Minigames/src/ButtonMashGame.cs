using System;
using System.Collections;
using DG.Tweening;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class ButtonMashGame : BaseMinigame
{
    [Header("References")]
    public UIDocument buttonMashUI;

    [Header("Game Scaling")]
    [Range(1, 50)] public int updateSteps = 10;
    [Range(0.1f, 10)] public float requiredTimeInRegion = 2;
    [Range(0, 1)] public float clickAmount;
    public AnimationCurve statToSize;
    public AnimationCurve deductionAmount;
    public AnimationCurve deductionSpeed;
    public AnimationCurve timeScoreScaling;
    [Range(1f, 15f)] public float worstCaseTime = 10;

    [Header("Feedback")]
    [Range(.5f, 1.5f)] public float punchStrength;
    [Range(0, 1f)] public float punchTime;

    protected ButtonMashProgressBar mashProgress;
    private bool _gameComplete = true;
    private float _timeInCompleteRegion;
    private bool _enteredCompleteRegion = false;
    private float _startTime;
    private bool _firstClick = false;
    private Tweener _clickTween;
    void OnEnable()
    {
        mashProgress = buttonMashUI.rootVisualElement.Q<ButtonMashProgressBar>("button-mash-progress");
        buttonMashUI.rootVisualElement.pickingMode = PickingMode.Ignore;
        buttonMashUI.rootVisualElement.SetEnabled(false);
        _clickTween = DOTween.To(
            () => mashProgress.style.scale.value.value, 
            v => mashProgress.style.scale = new StyleScale(v), 
            new Vector3(punchStrength, punchStrength, 1f), 
            punchTime
        )
        .SetLoops(2, LoopType.Yoyo)
        .SetEase(Ease.OutQuint) // Creates the snappy, bouncy "punch" behavior
        .SetAutoKill(false)
        .Pause();
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

        // When this flag is set to true the coroutine will finish
        _gameComplete = false;
        _enteredCompleteRegion = false;
        _firstClick = false;
        StartCoroutine(MeterDeductionCoroutine());
    }

    public override void CompleteGame()
    {
        score = timeScoreScaling.Evaluate(Mathf.Clamp(Time.time - _startTime, 0, worstCaseTime) / worstCaseTime);
        base.CompleteGame();

        _gameComplete = true;
        buttonMashUI.rootVisualElement.pickingMode = PickingMode.Ignore;
        buttonMashUI.rootVisualElement.SetEnabled(false);
    }

    public void Update() {
        if (_gameComplete) {
            return;
        }
        if (mashProgress.IsWithinThreshold()) {
            if (!_enteredCompleteRegion) {
                _enteredCompleteRegion = true;
                _timeInCompleteRegion = Time.time;
            } else if ((Time.time - _timeInCompleteRegion) > requiredTimeInRegion) {
                CompleteGame();
            }
        } else {
            _enteredCompleteRegion = false;
        }
    }

    public override void OnClick() {
        if (!_firstClick) { 
            _firstClick = true;
            _startTime = Time.time;
        }
        
        CameraController.Instance.PunchZoom(-mingameInteractZoomStrength, 0.1f);

        // TODO: Eventually make this scale with stats?
        mashProgress.progressMeter += clickAmount;
        mashProgress.style.scale = new StyleScale(new Vector3(1f, 1f, 1f));
        _clickTween.ChangeStartValue(new Vector3(1f, 1f, 1f));
        _clickTween.ChangeEndValue(new Vector3(punchStrength, punchStrength, 1f), punchTime);
        _clickTween.Restart();
    }

    private IEnumerator MeterDeductionCoroutine() {
        while (!_gameComplete) {
            mashProgress.progressMeter -= deductionAmount.Evaluate(stats.n_strength) / updateSteps;
            yield return new WaitForSeconds(deductionSpeed.Evaluate(stats.n_strength) / updateSteps);
        }

        yield return null;
    }
}
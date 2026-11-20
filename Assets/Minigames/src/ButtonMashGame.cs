using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;

public class ButtonMashGame : BaseMinigame
{   
    [Header("Game Scaling")]
    [Range(1, 50)] public int updateSteps = 10;
    [Range(0.1f, 10)] public float requiredTimeInRegion = 2;
    [Range(0, 1)] public float clickAmount;
    public AnimationCurve statToSize;
    public AnimationCurve deductionAmount;
    public AnimationCurve deductionSpeed;


    [Header("Feedback")]
    [Range(0f, 1f)] public float punchStrength;
    [Range(0f, 1f)] public float punchTime;
    public float fadeDuration = 0.25f;
    public AnimationCurve cameraShakeCurve;
    public Vector2 cameraZoomLimits;
    public AnimationCurve cameraZoomCurve;

    protected ButtonMashProgressBar mashProgress;
    private bool _gameComplete = true;
    private float _timeInCompleteRegion;
    private bool _enteredCompleteRegion = false;
    private bool _firstClick = false;
    private Tweener _weightTween;
    private float _weight;

    protected override void OnEnable()
    {
        mashProgress = minigameUI.rootVisualElement.Q<ButtonMashProgressBar>("button-mash-progress");
        base.OnEnable();
    }

    public override void OpenGame(Patient patient)
    {
        base.OpenGame(patient);

        _doctorAnim.SetFloat("strengthValue", 0);
        _patientAnim.SetFloat("strengthValue", 0);


        // Configure the initial state of the meter based on difficulty and skills
        mashProgress.progressMeter = 0;
        mashProgress.targetOffset = 0;
        mashProgress.targetHeight = statToSize.Evaluate(stats.n_strength) * 100f; // [0,1] -> [low strength, max strength]
        CameraController.Instance.SetMgZoomWeight(0);

        // When this flag is set to true the coroutine will finish
        _gameComplete = false;
        _enteredCompleteRegion = false;
        _firstClick = false;
        StartCoroutine(MeterDeductionCoroutine());
    }

    public override void CompleteGame()
    {
        CameraController.Instance.SetCameraNoiseAmplitude(0);

        if (score != 0) {
            _weight = 0;
            _weightTween = DOTween.To(() => _weight, x => _weight = x, 1f, fadeDuration).OnUpdate(() => {
                CameraController.Instance.SetMgZoomWeight(cameraZoomCurve.Evaluate(_weight));
            }).OnComplete(() => {
                base.CompleteGame();
            });
        } else {
            base.CompleteGame();
            _weight = 0;
        }

        _gameComplete = true;
        
    }

    protected override void Update() {
        if (_gameComplete) {
            return;
        }

        base.Update();

        if (mashProgress.IsWithinThreshold()) {
            if (!_enteredCompleteRegion) {
                _enteredCompleteRegion = true;
                _timeInCompleteRegion = Time.time;
            } else {
                float delta = Time.time - _timeInCompleteRegion;
                _weight = delta / requiredTimeInRegion;
                CameraController.Instance.SetCameraNoiseAmplitude(cameraShakeCurve.Evaluate(_weight));
                if (delta > requiredTimeInRegion) {
                    score = timeScoreScaling.Evaluate(Mathf.Clamp(Time.time - _startTime, 0, worstCaseTime) / worstCaseTime);
                    CompleteGame();
                }
            }

        } else if (_enteredCompleteRegion) {
            _weightTween?.Kill();
            _weightTween = DOTween.To(() => _weight, x => _weight = x, 0f, fadeDuration).OnUpdate(() => {
                CameraController.Instance.SetCameraNoiseAmplitude(cameraShakeCurve.Evaluate(_weight));
            });
            _enteredCompleteRegion = false;
        }
    }

    public override void OnClick() {
        if (!_firstClick) { 
            _firstClick = true;
            OnFirstClick();
        }
        
        float start = mashProgress.progressMeter;
        float targ = Mathf.Clamp01(start + clickAmount);
        DOVirtual.Float(start, targ, 0.5f, (x) => {
            mashProgress.progressMeter = x;
        });

    }

    private IEnumerator MeterDeductionCoroutine() {
        while (!_gameComplete) {
            mashProgress.progressMeter -= deductionAmount.Evaluate(stats.n_strength) / updateSteps;
            _doctorAnim.SetFloat("strengthValue", mashProgress.progressMeter);
            _patientAnim.SetFloat("strengthValue", mashProgress.progressMeter);
            yield return new WaitForSeconds(deductionSpeed.Evaluate(stats.n_strength) / updateSteps);
        }

        yield return null;
    }
}
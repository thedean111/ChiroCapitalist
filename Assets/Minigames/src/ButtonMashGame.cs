using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;

public class ButtonMashGame : BaseMinigame
{   
    [Header("Game-Specific References")]
    public Transform femaleSprite;
    public Transform maleSprite;
    public Transform leftHand;
    public Transform rightHand;

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

    [Header("Design")]
    public Gradient handSkinTone;

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
        // Show the 2D graphics
        // animator.SetFloat("NormalizedTime", 0f);
        // animator.SetBool("enabled", true);
        // NPCData pd = patient.GetData();
        // if (patient.IsMale()) {
        //     maleSprite.gameObject.SetActive(true);
        //     femaleSprite.gameObject.SetActive(false);

        //     maleSprite.GetComponent<SpriteRenderer>().sharedMaterial.SetColor("_Primary_Color", pd.torsoColors.primary);
        //     maleSprite.GetComponent<SpriteRenderer>().sharedMaterial.SetColor("_Secondary_Color", pd.skinColors.skin);
        // } else {
        //     femaleSprite.gameObject.SetActive(true);
        //     maleSprite.gameObject.SetActive(false);

        //     femaleSprite.GetComponent<SpriteRenderer>().sharedMaterial.SetColor("_Primary_Color", pd.torsoColors.primary);
        //     femaleSprite.GetComponent<SpriteRenderer>().sharedMaterial.SetColor("_Secondary_Color", pd.skinColors.skin);
        // }
        // Color handColor = handSkinTone.Evaluate(Random.Range(0f, 1f));
        // leftHand.GetComponent<SpriteRenderer>().sharedMaterial.SetColor("_Primary_Color", handColor);
        // rightHand.GetComponent<SpriteRenderer>().sharedMaterial.SetColor("_Primary_Color", handColor);

        // Configure the initial state of the meter based on difficulty and skills
        mashProgress.progressMeter = 0;
        mashProgress.targetOffset = 0;
        mashProgress.targetHeight = statToSize.Evaluate(stats.n_strength) * 100f; // [0,1] -> [low strength, max strength]
        CameraController.Instance.minigameZoomLevel = cameraZoomLimits.y;

        // When this flag is set to true the coroutine will finish
        _gameComplete = false;
        _enteredCompleteRegion = false;
        _firstClick = false;
        StartCoroutine(MeterDeductionCoroutine());
    }

    public override void CompleteGame()
    {
        base.CompleteGame();

        _weight = 0;
        _gameComplete = true;
        animator.SetBool("enabled", false);
        CameraController.Instance.SetCameraNoiseAmplitude(0);
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
        
        // TODO: Eventually make this scale with stats?
        CameraController.Instance.PunchZoom(-punchStrength, punchTime);
        float start = mashProgress.progressMeter;
        float targ = start + clickAmount;
        mashProgress.progressMeter += clickAmount;
        DOVirtual.Float(start, targ, 0.5f, (x) => {
            mashProgress.progressMeter = x;
        });

    }

    private IEnumerator MeterDeductionCoroutine() {
        while (!_gameComplete) {
            mashProgress.progressMeter -= deductionAmount.Evaluate(stats.n_strength) / updateSteps;

            // animator.SetFloat("NormalizedTime", mashProgress.progressMeter);
            // TODO: We need to break the normalized time into blocks and only update the float when
            // the value enters the next block!
            CameraController.Instance.SetZoom(
                cameraZoomLimits.y - cameraZoomCurve.Evaluate(mashProgress.progressMeter) * (cameraZoomLimits.y - cameraZoomLimits.x),
                0.1f
            );
            _doctorAnim.SetFloat("strengthValue", mashProgress.progressMeter);
            _patientAnim.SetFloat("strengthValue", mashProgress.progressMeter);
            yield return new WaitForSeconds(deductionSpeed.Evaluate(stats.n_strength) / updateSteps);
        }

        yield return null;
    }
}
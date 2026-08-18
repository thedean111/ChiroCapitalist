using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using DG.Tweening;
using UnityEditor.UIElements;

public class BaseMinigame : MonoBehaviour
{
    public Action onMinigameEnd;
    public StatCategory mainStat;
    public float mingameInteractZoomStrength = 0.5f;
    [Header("References")]
    public UIDocument minigameUI;
    public Animator animator;

    [Header("Base Settings")]
    public AnimationCurve patienceDepletionIntensity;
    public AnimationCurve patienceDepletionFrequency;
    public float maxPositionOffset = 3;
    public float maxRotationOffset = 3;
    public AnimationCurve timeScoreScaling;
    [Range(1f, 15f)] public float worstCaseTime = 10;
    public float patience = 5f; // Time in seconds until minigame ends
    public Gradient patienceColors;
    public List<Sprite> patienceEmojis = new List<Sprite>();
    public Vector2 angleRotationBounds;
    protected bool gameStarted = false; /* This flag identifies if the player has put in the first action for the game. When this happens the actual update loop
    for the game should start. */
    protected NPCStats stats; /* The stats of the current NPC being adjusted */
    protected bool _active = false;
    protected Animator _doctorAnim;
    protected Animator _patientAnim;

    private float _score;
    public float score {
        get { return _score; }
        set { _score = Mathf.Clamp01(value); }
    } /* Every minigame awards the player based on the score. It will always be between [0,1] where 0 is no reward and 1 is full reward */
    protected float _startTime;
    private Coroutine _patienceRoutine;
    private ProgressBar _patientPatience;
    private VisualElement _patientPatienceProgress;
    private VisualElement _patientPatienceBackground;
    private VisualElement _patientPatienceContainer;
    private VisualElement _patienceEmoji;
    private int _currentEmoji = 0;
    private int _emojiStepSize;
    private Tween _rotationTween;
    private float _currentAngle;

    protected virtual void OnEnable()
    {
        _patientPatience = minigameUI.rootVisualElement.Q<ProgressBar>("patient-patience-bar");
        _patientPatienceProgress = _patientPatience.Q(className: "unity-progress-bar__progress");
        _patientPatienceBackground = _patientPatience.Q(className: "unity-progress-bar__background");
        _patientPatienceContainer = _patientPatience.Q(className: "unity-progress-bar__container");
        _patienceEmoji = minigameUI.rootVisualElement.Q<VisualElement>("patience-emoji");

        minigameUI.rootVisualElement.pickingMode = PickingMode.Ignore;
        minigameUI.rootVisualElement.SetEnabled(false);

        _currentAngle = angleRotationBounds.x;
        _patienceEmoji.style.rotate = Quaternion.Euler(0, 0, _currentAngle);
        _rotationTween= DOTween.To(
            getter: () => _currentAngle,
            setter: angle =>
            {
                _currentAngle = angle;
                _patienceEmoji.style.rotate = Quaternion.Euler(0, 0, angle);
            },
            endValue: angleRotationBounds.y,
            duration: 1.5f
        )
        .SetEase(Ease.InOutSine)
        .SetLoops(-1, LoopType.Yoyo)
        .SetTarget(_patienceEmoji); 
        _rotationTween.Pause();
    }

    public void Init(Animator doctorAnim, Animator patientAnim) {
        _doctorAnim = doctorAnim;
        _patientAnim = patientAnim;
    }

    protected virtual void Update() {
// 1. Calculate depletion ratio (0.0 when full, 1.0 when empty)
        float progressPercent = Mathf.Clamp01(_patientPatience.value / _patientPatience.highValue);

        // Evaluate intensity based on curve
        float intensity = patienceDepletionIntensity.Evaluate(progressPercent);

        // If progress is near full, clear offsets and return
        if (intensity <= 0.001f)
        {
            _patientPatienceContainer.style.translate = Vector3.zero;
            _patientPatienceContainer.style.rotate = Quaternion.identity;
            return;
        }

        // 2. Generate smooth 2D noise offsets using Perlin Noise
        float seed = Time.time * patienceDepletionFrequency.Evaluate(progressPercent);

        // Remap Perlin Noise [0..1] range to [-1..1] with unique seed offsets for X, Y, and Rotation
        float noiseX = (Mathf.PerlinNoise(seed, 0f) * 2f - 1f);
        float noiseY = (Mathf.PerlinNoise(0f, seed) * 2f - 1f);
        float noiseRot = (Mathf.PerlinNoise(seed, seed) * 2f - 1f);

        // 3. Apply intensity scaling
        float offsetX = noiseX * maxPositionOffset * intensity;
        float offsetY = noiseY * maxPositionOffset * intensity;
        float offsetRot = noiseRot * maxRotationOffset * intensity;

        // 4. Update UI Toolkit transform properties
        _patientPatienceContainer.style.translate = new Vector3(offsetX, offsetY, 0f);
        _patientPatienceContainer.style.rotate = Quaternion.Euler(0f, 0f, offsetRot);
    }

    /// <summary>
    /// Handles how a minigame should be started. Children of this class will likely have more logic for setting up custom components.
    /// </summary>
    public virtual void OpenGame(Patient patient) {
            
        // Enable the UI
        minigameUI.rootVisualElement.SetEnabled(true);
        minigameUI.rootVisualElement.pickingMode = PickingMode.Position;
        _patienceEmoji.style.backgroundImage = new StyleBackground(patienceEmojis[0]);
        _currentEmoji = 0;
        _patientPatienceBackground.style.borderBottomColor = new StyleColor(Color.black);
        _patientPatienceBackground.style.borderTopColor = new StyleColor(Color.black);
        _patientPatienceBackground.style.borderLeftColor = new StyleColor(Color.black);
        _patientPatienceBackground.style.borderRightColor = new StyleColor(Color.black);

        _currentAngle = angleRotationBounds.x;
        _patienceEmoji.style.rotate = Quaternion.Euler(0, 0, _currentAngle);
        _rotationTween.Play();
        _active = true;
        gameStarted = false;
        score = 1;
        stats = patient.GetStats();
        _patientPatience.value = 100;
        _patientPatienceProgress.style.backgroundColor = patienceColors.Evaluate(1f);

        _emojiStepSize = (int)(100f / patienceEmojis.Count);
    }

    /// <summary>
    /// General behavior for completing a game. Currently just invokes the listeners of the end action.
    /// Children of this class will likely have custom logic for animations, effects, and other deconstruction.
    /// </summary>
    public virtual void CompleteGame() {
        Debug.Log("Ending " + this.GetType() + " with a score of: " + score);
        _active = false;
        _rotationTween.Pause();
        minigameUI.rootVisualElement.SetEnabled(false);
        minigameUI.rootVisualElement.pickingMode = PickingMode.Ignore;
        onMinigameEnd?.Invoke();
        StopCoroutine(_patienceRoutine);
    }

    public virtual void OnClick() {}

    protected void OnFirstClick() {
        _startTime = Time.time;
        _patienceRoutine = StartCoroutine(PatienceCountdown());
    }

    private IEnumerator PatienceCountdown() {
        float t = Time.time;
        bool hasPatience = true;
        while (hasPatience) {
            float delta = Time.time - t;
            float value = (patience - delta) / patience;
            _patientPatience.value = value * 100;
            _patientPatienceProgress.style.backgroundColor = patienceColors.Evaluate(value);
            if ((patienceEmojis.Count - ((int)_patientPatience.value / _emojiStepSize) != _currentEmoji) &&
                ((int)_patientPatience.value) % _emojiStepSize == 0) {
                _currentEmoji++;
                Sequence seq = DOTween.Sequence();

                // Step A: Tween scale down to 0
                seq.Append(DOTween.To(
                    getter: () => _patienceEmoji.style.scale.value.value,
                    setter: x => _patienceEmoji.style.scale = new StyleScale(x),
                    endValue: Vector3.zero,
                    duration: 0.7f
                ).SetEase(Ease.InQuart)); // Optional easing

                // Step B: Execute your custom code at scale 0
                seq.AppendCallback(() =>
                {
                    _patienceEmoji.style.backgroundImage = new StyleBackground(patienceEmojis[_currentEmoji]);
                });

                // Step C: Tween scale back up to 1
                seq.Append(DOTween.To(
                    getter: () => _patienceEmoji.style.scale.value.value,
                    setter: x => _patienceEmoji.style.scale = new StyleScale(x),
                    endValue: Vector3.one,
                    duration: 0.7f
                ).SetEase(Ease.OutCubic)); // Optional easing

                if (_currentEmoji == patienceEmojis.Count-1) {
                    _patientPatienceBackground.style.borderBottomColor = new StyleColor(Color.red);
                    _patientPatienceBackground.style.borderTopColor = new StyleColor(Color.red);
                    _patientPatienceBackground.style.borderLeftColor = new StyleColor(Color.red);
                    _patientPatienceBackground.style.borderRightColor = new StyleColor(Color.red);
                }
            }

            yield return new WaitForSeconds(0.01f);

            hasPatience = delta < patience;
        }

        // TODO: Provide feedback that the patient is unhappy because time (patience) ran out
        score = 0;
        CompleteGame();
    }
}

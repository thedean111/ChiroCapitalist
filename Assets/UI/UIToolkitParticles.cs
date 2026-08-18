using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using DG.Tweening;

public class UIToolkitParticles : MonoBehaviour
{
    [Header("UI & Camera References")]
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private Camera mainCamera; // The Overlay Camera
    [SerializeField] private ParticleSystem moneyParticles;
    [SerializeField] private ParticleSystem reputationParticles;

    [Header("Settings")]
    [SerializeField] private float particleDistanceFromCamera = 2.0f;
    [SerializeField] private float flightDuration = 0.6f;
    [SerializeField] private float curveStrength = 1f;
    [SerializeField] private float delayBeforeAttract = 0.3f;
    [SerializeField] private float maxParticleStagger = 1f;
    private int moneyIncrementAmount = 1;
    private int reputationIncrementAmount = 1;

    private VisualElement root;
    private VisualElement moneyElement;
    private VisualElement reputationElement;
    private ParticleSystem.Particle[] particleBuffer;

    private void OnEnable()
    {
        root = uiDocument.rootVisualElement;
        moneyElement = root.Q<VisualElement>("money-container");
        reputationElement = root.Q<VisualElement>("practice-license-button");
    }

    public void SetParticleNums(int moneyNum, int reputationNum) {
        ParticleSystem.Burst mB = moneyParticles.emission.GetBurst(0);
        mB.count = moneyNum;
        moneyParticles.emission.SetBurst(0, mB);
        
        ParticleSystem.Burst rB = reputationParticles.emission.GetBurst(0);
        rB.count = reputationNum;
        reputationParticles.emission.SetBurst(0, rB);
    }

    public void SetIncrementAmounts(int money, int reputation) {
        moneyIncrementAmount = money;
        reputationIncrementAmount = reputation;
    }

    /// <summary>
    /// Call this from your minigame when complete.
    /// </summary>
    public void TriggerRewardParticles(Vector3 startWorldPos)
    {
        // 1. Position the emitter over the minigame source element
        // Vector3 startPos = WorldToCameraPlane(startWorldPos);
        Vector3 startPos = startWorldPos + (Vector3.up * 3);
        moneyParticles.transform.position = startPos;
        reputationParticles.transform.position = startPos;

        // 2. Play particle burst
        moneyParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        moneyParticles.Play();
        reputationParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        reputationParticles.Play();

        // 3. Start fly-to-target routine
        DOVirtual.DelayedCall(delayBeforeAttract, () => AttractParticles(moneyElement, moneyParticles, OnMoneyTargetHit));
        DOVirtual.DelayedCall(delayBeforeAttract, () => AttractParticles(reputationElement, reputationParticles, OnReptuationTargetHit));
    }

    private void AttractParticles(VisualElement target, ParticleSystem pSys, Action onHitTarget)
    {
        
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[pSys.main.maxParticles];
        int count = pSys.GetParticles(particles);

        if (count == 0) { return; }

        // Calculate real-time world position of the target progress meter UI element
        Vector3 targetWorldPos =UIToolkitToCameraPlane(target);


        for (int i = 0; i < count; i++)
        {
            Vector3 particleStartPos = particles[i].position;

            // --- CALCULATE CURVED WAYPOINTS ---
            // Find midpoint between start and target
            Vector3 midPoint = Vector3.Lerp(particleStartPos, targetWorldPos, 0.5f);

            // Add a random outward offset to create a unique arc for each particle
            Vector2 randomOffset = UnityEngine.Random.insideUnitCircle.normalized * curveStrength;
            Vector3 controlPoint = midPoint + new Vector3(randomOffset.x, randomOffset.y, 0f);

            // Waypoints array for DOTween: [Arc Apex Point, Target Point]
            // Vector3[] pathWaypoints = new Vector3[] { controlPoint, targetWorldPos };

            // We use a dummy transform or value tween to animate the position
            // Since Unity Particles are struct values, we animate a Vector3 value via DOTween:
            int particleIndex = i; // Cache index for closure safety
            uint particleSeed = particles[i].randomSeed;
            DOVirtual.Float(0f, 1f, flightDuration, t =>
            {
                // Recalculate target position in case it moves
                Vector3 targetWorldPos =UIToolkitToCameraPlane(target);

                // Evaluate Quadratic Bezier Formula along path (0 to 1)
                float oneMinusT = 1f - t;
                Vector3 currentPos = (oneMinusT * oneMinusT * particleStartPos) +
                                    (2f * oneMinusT * t * controlPoint) +
                                    (t * t * targetWorldPos);

                // Fetch active particles into buffer
                int maxParticles = pSys.main.maxParticles;
                if (particleBuffer == null || particleBuffer.Length < maxParticles)
                    particleBuffer = new ParticleSystem.Particle[maxParticles];

                int activeCount = pSys.GetParticles(particleBuffer);
                // Find the specific particle using its UNIQUE SEED ID instead of index
                for (int i = 0; i < activeCount; i++)
                {
                    if (particleBuffer[i].randomSeed == particleSeed)
                    {
                        particleBuffer[i].position = currentPos;
                        pSys.SetParticles(particleBuffer, activeCount);
                        break;
                    }
                }
            })
            .SetDelay(UnityEngine.Random.Range(0f, maxParticleStagger))
            .SetEase(Ease.InQuad) 
            .OnComplete(() =>
            {
                int maxParticles = pSys.main.maxParticles;
                if (particleBuffer == null || particleBuffer.Length < maxParticles)
                    particleBuffer = new ParticleSystem.Particle[maxParticles];

                int activeCount = pSys.GetParticles(particleBuffer);

                // Find particle by SEED ID and force instant death
                for (int i = 0; i < activeCount; i++)
                {
                    if (particleBuffer[i].randomSeed == particleSeed)
                    {
                        // Set lifetime negative to guarantee instant destruction
                        particleBuffer[i].remainingLifetime = -1f; 
                        pSys.SetParticles(particleBuffer, activeCount);
                        break;
                    }
                }

                onHitTarget?.Invoke();
            });
        }
    }

    /// <summary>
    /// Maps a 3D World position onto the Main Camera's fixed particle plane.
    /// </summary>
    private Vector3 WorldToCameraPlane(Vector3 worldPos)
    {
        Vector3 screenPoint = mainCamera.WorldToScreenPoint(worldPos);
        screenPoint.z = particleDistanceFromCamera; // Fixed depth relative to camera lens
        return mainCamera.ScreenToWorldPoint(screenPoint);
    }

    /// <summary>
    /// Maps a UI Toolkit VisualElement onto the Main Camera's fixed particle plane.
    /// </summary>
    private Vector3 UIToolkitToCameraPlane(VisualElement element)
    {
        // 1. Get element center in normalized panel space (0.0 to 1.0)
        VisualElement root = element.panel.visualTree;
        
        // Calculate position relative to the root UI container width/height
        float normalizedX = element.worldBound.center.x / root.layout.width;
        float normalizedY = element.worldBound.center.y / root.layout.height;

        // 2. Map normalized UI coordinates directly to actual Monitor Screen Pixels
        float screenX = normalizedX * Screen.width;
        float screenY = (1f - normalizedY) * Screen.height; // Invert Y axis

        // 3. Project to World Space via Camera Lens
        Vector3 screenPoint = new Vector3(screenX, screenY, particleDistanceFromCamera);
        return mainCamera.ScreenToWorldPoint(screenPoint);
    }

    private void OnMoneyTargetHit()
    {
        Debug.Log("Particle hit money!");
        ProgressionManager.Instance.AdjustMoney(moneyIncrementAmount);
        // Trigger meter increment & visual punch scale in UI Toolkit
        moneyElement.style.scale = new Scale(new Vector2(.85f, .85f));
        moneyElement.schedule.Execute(() =>
        {
            moneyElement.style.scale = new Scale(new Vector2(.8f, .8f));
        }).StartingIn(25);
    }

    private void OnReptuationTargetHit()
    {
        Debug.Log("Particle hit reputation!");
        ProgressionManager.Instance.AdjustReputation(reputationIncrementAmount);
        // Trigger meter increment & visual punch scale in UI Toolkit
        reputationElement.style.scale = new Scale(new Vector2(1.05f, 1.05f));
        reputationElement.schedule.Execute(() =>
        {
            reputationElement.style.scale = new Scale(new Vector2(1.0f, 1.0f));
        }).StartingIn(25);
    }
}

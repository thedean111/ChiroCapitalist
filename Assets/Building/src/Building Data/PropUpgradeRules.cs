using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/*
This scriptable object contains data that drives the behavior of Tile props on level up. For a given prop and tile level, these rules should be referenced so 
the prop may be updated accordingly.
*/
[CreateAssetMenu(fileName = "PropUpgradeRules", menuName = "Scriptable Objects/PropUpgradeRules")]
public class PropUpgradeRules : ScriptableObject
{
    public Dictionary<int, PropUpgradeRule> ruleDict = new Dictionary<int, PropUpgradeRule>();
    [SerializeField] private List<PropUpgradeRule> ruleList = new List<PropUpgradeRule>();
    private bool initialized = false;

    private void OnEnable() {
        initialized = false;
        ruleDict.Clear();
    }

    /// <summary>
    /// Reformat the rules list into a dictionary so it is easier to lookup rules by level instead of iterating all the lists every level up.
    /// </summary>
    public void InitializeRules() {
        if (initialized) {
             return;
        }
        initialized = true;

        foreach (PropUpgradeRule rule in ruleList) {
            if (!ruleDict.ContainsKey(rule.level)) {
                ruleDict.Add(rule.level, rule);
            }
        }
    }
}

[Serializable]
public class PropUpgradeRule {
    [Range(1, 10)]
    public int level;
    public PropLevelAction action;
    public GameObject prefab;

    // TODO: Play an effect here
    public void Execute(GameObject target) {
        switch (action) {
            case PropLevelAction.Add:
                Transform t = UnityEngine.Object.Instantiate(prefab, target.transform).transform;
                t.localScale = Vector3.zero;
                t.DOScale(1, 0.2f).SetEase(Ease.OutBack);
                break;

            case PropLevelAction.Remove:
                target.transform.DOScale(0f, 0.2f).OnComplete(() => {
                    target.transform.DOKill();
                    UnityEngine.Object.DestroyImmediate(target);
                });
                break;

            
            case PropLevelAction.Replace:
                UnityEngine.Object.DestroyImmediate(target.transform.GetChild(0).gameObject);
                target.transform.DOShakeScale(0.2f, 0.3f);
                UnityEngine.Object.Instantiate(prefab, target.transform);
                break;
        }
    }
}

[Serializable]
public class PropRuleAssociation {
    public GameObject prop;
    public List<PropUpgradeRules> ruleSet;
    private int setIdx;

    public void Initialize(System.Random rng) {
        setIdx = rng.Next(0, ruleSet.Count);
        ruleSet[setIdx].InitializeRules();
    }

    public PropUpgradeRules GetRules() {
        return ruleSet[setIdx];
    }
}
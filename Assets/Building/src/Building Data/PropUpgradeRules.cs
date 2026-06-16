using System;
using System.Collections.Generic;
using UnityEngine;

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
                UnityEngine.Object.Instantiate(prefab, target.transform);
                break;

            case PropLevelAction.Remove:
                UnityEngine.Object.DestroyImmediate(target);
                break;

            
            case PropLevelAction.Replace:
                UnityEngine.Object.DestroyImmediate(target.transform.GetChild(0).gameObject);
                UnityEngine.Object.Instantiate(prefab, target.transform);
                break;
        }
    }
}

[Serializable]
public class PropRuleAssociation {
    public GameObject prop;
    public PropUpgradeRules ruleSet;
}
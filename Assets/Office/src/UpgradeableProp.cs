using System.Linq;
using UnityEngine;

public class UpgradeableProp : MonoBehaviour
{
    public PropUpgradeRules rules; // The rules apply directly to this game object
    public int Level {get; private set;}
    private int maxLevel;

    void Start()
    {
        rules.InitializeRules();
        maxLevel = rules.ruleDict.Keys.Max();
        Level = 1;
    }

    public void LevelUp() {
        if (Level >= maxLevel) { 
            Debug.LogWarning("Prop at max level");
            return;
        }

        Level++;
        if (rules.ruleDict.TryGetValue(Level, out PropUpgradeRule r)) {
            r.Execute(gameObject);
        }
    }
}

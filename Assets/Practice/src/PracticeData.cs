using UnityEngine;

[CreateAssetMenu(fileName = "PracticeData", menuName = "Scriptable Objects/PracticeData")]
public class PracticeData : ScriptableObject
{
    public int reputation;
    public int money;
    public int tweenMoney; // Ui should copy this field, and it always chases 'money'
}

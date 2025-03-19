using UnityEngine;

[CreateAssetMenu(fileName = "QuestInfoSO", menuName = "ScriptableObjects/QuestInfoSO", order = 1)]

public class QuestInfoSO : ScriptableObject
{
    [SerializeField] public string id { get; private set; }

    [Header("General")]
    public string displayName;

    [Header("Requirements")]
    public QuestInfoSO[] questPrerequisites;

    [Header("Steps")]
    public GameObject[] questStepPrefabs;

    [Header("Rewards")]
    //public int experienceReward;
    public int upgradeCurrencyReward;

    private void Awake() // ensure the id is always the name of the Scriptable Object
    {

        id = this.name;
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif 
    }
}

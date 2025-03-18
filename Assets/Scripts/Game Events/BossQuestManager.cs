using UnityEngine;
using System.Collections.Generic;

public class BossQuestManager : MonoBehaviour
{
    public static BossQuestManager instance;

    [Header("Config")]
    [SerializeField] private GameObject parentComponent;
    private enemyAI enemyScript;
    public bool isComplete;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        instance = this;
        enemyScript = gameObject.GetComponentInParent<enemyAI>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isComplete)
        {
            if (enemyScript.isDead)
            {
                isComplete = true;

            }
        }

    }
}

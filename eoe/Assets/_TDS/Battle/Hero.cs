using System.Collections;
using System.Collections.Generic;
using _GameToolkit.Statistics;
using _TDS.Battle;
using _TDS.GameConfig;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class Hero : MonoBehaviour
{
    public bool IsAttacking {get; private set;}

    private Dictionary<StatId, Stat> stats;
    
    public async UniTaskVoid Initialize(HeroConfigData heroConfigData)
    {
        stats = new Dictionary<StatId, Stat>();
        
    }

    protected virtual IEnumerator AutoAttackEnumerator()
    {
        while (!IsAttacking)
        {
            
        }
    }
}

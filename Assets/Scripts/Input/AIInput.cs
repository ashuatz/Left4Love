using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using Zombi;
using System.Collections.Generic;

public class AIInput : BaseInput
{
    #region Inspector
    [Header("Component")]
    [SerializeField] private NavMeshAgent m_Agent;
    [SerializeField] private Transform[] m_MoveEnablePos;

    [Header("Balance")]
    [SerializeField] private float m_EnemySearchDist;
    [SerializeField] private float m_RunawayDist;
    #endregion

    #region Event
    private void Update()
    {
        
    }
    #endregion
    #region Function
    private Transform GetBaseTargetPos(int forceRandIndex = -1)
    {
        int random = (forceRandIndex == -1) ? Random.Range(0, 3) : forceRandIndex;

        if(random == 0)
        {
            //플레이어를 공격하러 간다!
            ZombiManager zombiManager = ZombiManager.Instance;
            List<GameObject> ownerList = zombiManager.GetPlayerOwnerList();
            while(true)
            {
                int index = Random.Range(0, ownerList.Count);
                if (ownerList[index] != gameObject)
                    return ownerList[index].transform;
            }
        }
        else if(random == 1)
        {
            //적 생성지점으로 간다.
            SpawnManager spawnManager = SpawnManager.Instance;
        }
        else if(random == 2)
        {
            //그냥 랜덤지점으로 간다.
            PosMake posMake = PosMake.Instance;
        }

        return null;
    }
    /// <summary>
    /// 적을 검색합니다.
    /// </summary>
    /// <returns></returns>
    private Transform GetEnemyInSearchDist()
    {
        ZombiManager zombiManager = ZombiManager.Instance;
        List<GameObject> ownerList = zombiManager.GetAllOwnerList();
        for (int i = 0; i < ownerList.Count; ++i)
        {
            if(ownerList[i] != gameObject)
            {
            }
        }

        return null;
    }
    /// <summary>
    /// 도주할 방향을 가져옵니다. 도주할필요 없으면 null
    /// </summary>
    /// <returns></returns>
    private Vector3? GetRunaway()
    {
        return null;
    }
    #endregion
}

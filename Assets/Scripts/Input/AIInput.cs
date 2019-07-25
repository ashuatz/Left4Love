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
    private Transform GetBaseTargetPos()
    {
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

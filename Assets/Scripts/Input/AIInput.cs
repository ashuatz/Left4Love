using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using Zombi;
using System.Collections.Generic;
using System;

public class AIInput : BaseInput
{
    #region Value
    public override event Action<Vector2> OnMoveDirection;
    public override event Action<Vector2> OnViewDirection;

    public override event Action<bool> OnClick;
    public override event Action OnSpectialClick;
    #endregion

    #region Inspector
    [Header("Component")]
    [SerializeField] private NavMeshAgent m_Agent;

    [Header("Balance")]
    [SerializeField] private float m_EnemySearchDist;
    [SerializeField] private float m_RunawayDist;
    #endregion
    #region Value
    private Transform m_BasePos;
    #endregion

    #region Event
    private IEnumerator Start()
    {
        while(true)
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(5.0f, 10.0f));
            OnSpectialClick?.Invoke();
        }
    }
    private void Update()
    {
        //기본 목표 위치 설정
        if (m_BasePos == null)
            m_BasePos = GetBaseTargetPos();

        //기본 목표 위치 도착시 제거
        if (m_BasePos && Vector3.Distance(transform.position, m_BasePos.position) < 1.0f)
            m_BasePos = null;

        //거리 내의 공격 가능한 적 방향으로 공격
        Transform enemy = GetEnemyInSearchDist();
        if(enemy)
        {
            Vector3 dir = (enemy.transform.position - transform.position);
            dir.y = 0; dir.Normalize();
            OnViewDirection?.Invoke(new Vector2(dir.x, dir.z));
            OnClick?.Invoke(true);
        }

        //도주해야 할 때 도주합니다.
        Vector3? runaway = GetRunaway();
        if(runaway != null)
        {
            Vector3 pos = transform.position + runaway.Value * 5;
            Transform target = PosMake.Instance.GetNearestPos(pos);
            m_Agent.SetDestination(target.position);
            return;
        }
        //기본 목표 위치로 쭉죽 이동
        if (m_BasePos)
        {
            m_Agent.SetDestination(m_BasePos.position);

            //일정 거리 이상 가까워지면 ㅂㅂ
            if (Vector3.Distance(m_BasePos.position, transform.position) < 5.0f)
                m_BasePos = null;

            return;
        }
    }
    #endregion
    #region Function
    /// <summary>
    /// 기본적으로 향할 타겟 위치를 가져옵니다.
    /// </summary>
    /// <param name="forceRandIndex"></param>
    /// <returns></returns>
    private Transform GetBaseTargetPos(int forceRandIndex = -1)
    {
        int random = (forceRandIndex == -1) ? UnityEngine.Random.Range(0, 3) : forceRandIndex;

        if(random == 0)
        {
            //플레이어를 공격하러 간다!
            ZombiManager zombiManager = ZombiManager.Instance;
            List<GameObject> ownerList = zombiManager.GetPlayerOwnerList();
            while(true)
            {
                int index = UnityEngine.Random.Range(0, ownerList.Count);
                if (ownerList[index] != gameObject)
                    return ownerList[index].transform;
            }
        }
        else if(random == 1)
        {
            //적 생성지점으로 간다.
            SpawnManager spawnManager = SpawnManager.Instance;
            return spawnManager.GetRandomZombiSpawnPos();
        }
        else if(random == 2)
        {
            //그냥 랜덤지점으로 간다.
            PosMake posMake = PosMake.Instance;
            return posMake.GetRandomPos();
        }

        return null;
    }
    /// <summary>
    /// 거리 내의 공격 가능한 적을 검색합니다.
    /// </summary>
    /// <returns></returns>
    private Transform GetEnemyInSearchDist()
    {
        ZombiManager zombiManager = ZombiManager.Instance;
        List<GameObject> ownerList = zombiManager.GetAllOwnerList();
        for (int i = 0; i < ownerList.Count; ++i)
        {
            if (ownerList[i] != gameObject)
            {
                //플레이어 검색
                if (Vector3.Distance(transform.position, ownerList[i].transform.position) <= m_EnemySearchDist)
                {
                    Ray ray = new Ray(transform.position, (ownerList[i].transform.position - transform.position).normalized);
                    RaycastHit hit;
                    Physics.Raycast(ray, out hit);

                    if(hit.collider.attachedRigidbody)
                        return hit.collider.attachedRigidbody.transform;
                }

                //좀비 검색
                List<ZombiCharacter> zombiList = zombiManager.GetSpawnedZombiList(ownerList[i]);
                for (int j = 0; j < zombiList.Count; ++j)
                {
                    Ray ray = new Ray(transform.position, (zombiList[j].transform.position - transform.position).normalized);
                    RaycastHit hit;
                    Physics.Raycast(ray, out hit);

                    if (hit.collider.attachedRigidbody)
                        return hit.collider.attachedRigidbody.transform;
                }
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
        Vector3 baseDir = Vector3.zero;

        ZombiManager zombiManager = ZombiManager.Instance;
        List<GameObject> ownerList = zombiManager.GetAllOwnerList();
        for (int i = 0; i < ownerList.Count; ++i)
        {
            if (ownerList[i] != gameObject)
            {
                //좀비 검색
                List<ZombiCharacter> zombiList = zombiManager.GetSpawnedZombiList(ownerList[i]);
                for (int j = 0; j < zombiList.Count; ++j)
                {
                    if(Vector3.Distance(transform.position, zombiList[j].transform.position) < m_RunawayDist)
                    {
                        baseDir += transform.position - zombiList[j].transform.position;
                    }
                }
            }
        }

        if (baseDir != Vector3.zero)
            return baseDir.normalized;
        else
            return null;
    }
    #endregion
}

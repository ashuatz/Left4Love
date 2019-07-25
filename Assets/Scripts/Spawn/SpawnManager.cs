using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core;
using Zombi;

public class SpawnManager : MonoSingleton<SpawnManager>
{
    #region Type
    [System.Serializable] private struct ZombiSpawnData
    {
        public ZombiTypeEnum[] zombiType;       //좀비 타입
        public float[] zombiPersent;            //각 좀비 타입의 생성 확률 (0 ~ 100)
        public float startSpawnTime;            //첫 생성 시간
        public float spawnTime;                 //생성 시간
        public int spawnCount;                  //생성 갯수
    }
    [System.Serializable] private struct ItemSpawnData
    {
        public float startSpawnTime;            //첫 생성 시간
        public float spawnTime;                 //생성 시간
    }
    [System.Serializable] private struct ZombiAttackData
    {
        public ZombiTypeEnum[] zombiType;       //좀비 타입
        public float[] zombiPersent;            //각 좀비 타입의 생성 확률 (0 ~ 100)
        public float attackTime;                //공격 시간
        public int firstAttackCount;            //첫번째 공격의 좀비 수
        public int addAttackCount;              //두번째 공격부터의 좀비 추가 수
    }
    #endregion

    #region Inspector
    [Header("Component")]
    [SerializeField] private Transform m_ItemSpawnPos;
    [SerializeField] private Transform[] m_ZombiSpawnPos;

    [Header("Balanace")]
    [SerializeField] private ZombiSpawnData m_ZombiSpawnData;
    [SerializeField] private ItemSpawnData m_ItemSpawnData;
    [SerializeField] private ZombiAttackData m_ZombiAttackData;
    #endregion
    #region Value
    private bool m_IsInited;

    private float m_ZombiSpawnTimer;

    private float m_ItemSpawnTimer;

    private float m_ZombiAttackTimer;
    private int m_ZombiAttackCount;
    #endregion

    #region Event
    protected override void Awake()
    {
        base.Awake();
        Init();
    }
    private void Init()
    {
        m_IsInited = true;

        m_ZombiSpawnTimer = m_ZombiSpawnData.startSpawnTime;
        m_ItemSpawnTimer = m_ItemSpawnData.startSpawnTime;
        m_ZombiAttackTimer = m_ZombiAttackData.attackTime;
        m_ZombiAttackCount = m_ZombiAttackData.firstAttackCount;
    }
    private void Update()
    {
        if (!m_IsInited)
            return;

        //사용할것들
        ZombiManager zombiManager = ZombiManager.Instance;

        //ZombiSpawn - 좀비 생성
        m_ZombiSpawnTimer -= Time.deltaTime;
        if (m_ZombiSpawnTimer <= 0)
        {
            for (int i = 0; i < m_ZombiSpawnPos.Length; ++i)
            {
                for (int j = 0; j < m_ZombiSpawnData.spawnCount; ++j)
                {
                    int zombiIndex = GetRandomIndex(m_ZombiSpawnData.zombiPersent);
                    Vector3 targetPos = m_ZombiSpawnPos[i].position + new Vector3(Random.Range(-0.1f, 0.1f), 0, Random.Range(-0.1f, 0.1f));
                    zombiManager.SpawnZombi(gameObject, targetPos, m_ZombiSpawnData.zombiType[zombiIndex]);
                }
            }

            m_ZombiSpawnTimer = m_ZombiSpawnData.spawnTime;
        }

        //ItemSpawn - 아이템 생성
        m_ItemSpawnTimer -= Time.deltaTime;
        if(m_ItemSpawnTimer <= 0)
        {
            //TODO : 아이템 생성해야함

            m_ItemSpawnTimer = m_ItemSpawnData.spawnTime;
        }

        //ZombiAttack - 바깥에서부터 좀비 공격
        m_ZombiAttackTimer -= Time.deltaTime;
        if(m_ZombiAttackTimer <= 0)
        {
            m_ZombiAttackTimer = m_ZombiAttackData.attackTime;
            m_ZombiAttackCount += m_ZombiAttackData.addAttackCount;
        }
    }
    #endregion
    #region Function
    private int GetRandomIndex(float[] persent)
    {
        float random = Random.Range(0.0f, 100.0f);
        for (int i = 0; i < persent.Length; ++i)
        {
            if (persent[i] <= random)
                return i;
        }

        return 0;
    }
    #endregion
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Core;
using UnityEngine.AI;

namespace Zombi
{
    public class ZombiCharacter : MonoBehaviour, IDamage
    {
        #region Type
        [System.Serializable] public struct BodyPartObjStruct
        {
            public SpriteRenderer body;
            public SpriteRenderer hand;
            public SpriteRenderer leg2;
            public SpriteRenderer leg1;
            public SpriteRenderer head;
            public SpriteRenderer hair;
        }
        [System.Serializable] public struct BodyPartSprStruct
        {
            public Sprite body;
            public Sprite hand;
            public Sprite leg2;
            public Sprite leg1;
            public Sprite head;
            public Sprite hair;
        }

        public enum ZombiState
        {
            Spawn,
            Idle,
            Move,
            Attack,
            Die
        }

        private delegate ZombiState StateNextEvent();
        #endregion

        #region Inspector
        [Header("Component")]
        [SerializeField] private NavMeshAgent m_Agent;
        [SerializeField] private Rigidbody m_Rigidbody;
        [SerializeField] private Transform m_ZombiRoot;
        [SerializeField] private Animator m_FrontAnimator;
        [SerializeField] private Collider m_Collider;
        [SerializeField] private BodyPartObjStruct m_FrontObj;
        [SerializeField] private ParticleSystem[] m_OwnerChangePar;

        [Header("Sprite")]
        [SerializeField] private BodyPartSprStruct[] m_FrontSpr;
        [SerializeField] private BodyPartSprStruct[] m_BackSpr;

        [Header("Balance")]
        [SerializeField] private int m_MaxHP;                   //체력
        [SerializeField] private int m_Damage;                  //공격력
        [SerializeField] private float m_ZombiSearchDist;       //다른 적 좀비를 검색하는 거리
        [SerializeField] private float m_OwnerMinDist;          //주인님이랑 최저거리
        [SerializeField] private float m_AutoDieTime;           //자동 사망 시간
        [SerializeField] private float m_SlowTime;              //주인님 변경시 느려지는 시간
        #endregion
        #region Get,Set
        public GameObject ownerPlayer
        {
            get;
            private set;
        }
        public ZombiTypeEnum zombiType
        {
            get;
            private set;
        }
        public ZombiState zombiState
        {
            get;
            private set;
        }

        public bool IsDamageEnable
        {
            get
            {
                bool isNotDied = zombiState != ZombiState.Die;
                bool isNotSpawning = zombiState != ZombiState.Spawn;

                return isNotDied & isNotSpawning;
            }
        }

        public int Priority
        {
            get;
            set;
        }

        public bool IsHealEnable => throw new NotImplementedException();
        #endregion
        #region Value
        private GameObject m_TargetPlayer;                          //공격 타겟 플레이어
        private ZombiCharacter m_TargetZombi;                       //공격 타겟 좀비

        private Action[] m_StateUpdate;                             //각 State의 Update
        private StateNextEvent[] m_StateNext;                       //각 State의 다음으로 넘어가는 조건

        private bool m_IsInited;
        private SubjectValue<int> m_HP = new SubjectValue<int>();   //몬스터 HP
        private int m_SpriteIndex;

        private float m_SlowTimer;
        #endregion

        #region Event
        internal void Init(GameObject owner)
        {
            SetOwner(owner);
            m_HP.value = m_MaxHP;

            if (m_IsInited)
                return;
            else
                m_IsInited = true;

            m_FrontAnimator.SetTrigger("Spawn");
            m_StateUpdate = new Action[] { StateSpawning, StateIdle, StateMove, StateAttack, StateDie };
            m_StateNext = new StateNextEvent[] { StateSpawningNext, StateIdleNext, StateMoveNext, StateAttackNext, StateDieNext };
        }

        //Unity Evnet
        private void Update()
        {
            SetState(m_StateNext[(int)zombiState]());
            m_StateUpdate[(int)zombiState]();

            m_Rigidbody.velocity = Vector3.zero;

            bool isRight = (0 <= m_Agent.velocity.x);
            m_FrontAnimator.transform.localScale = new Vector3(isRight ? 1 : -1, 1, 1);
            SetSprite(m_SpriteIndex);

            m_AutoDieTime -= Time.deltaTime;
            if (m_AutoDieTime <= 0)
                SetState(ZombiState.Die);

            if (0 < m_SlowTimer)
            {
                m_SlowTimer -= Time.deltaTime;
                if (m_SlowTimer <= 0)
                    m_Agent.speed *= 2.0f;
            }
        }
        private void OnCollisionStay(Collision collision)
        {
            if (!collision.collider.attachedRigidbody)
                return;

            if (zombiState != ZombiState.Attack)
            {
                Player player = collision.collider.attachedRigidbody.GetComponent<Player>();
                ZombiCharacter zombi = collision.collider.attachedRigidbody.GetComponent<ZombiCharacter>();
                IDamage iDamage = null;
                if (player && player.gameObject != ownerPlayer)
                    iDamage = player;
                else if (zombi && zombi.ownerPlayer != ownerPlayer)
                    iDamage = zombi;

                if(iDamage != null)
                {
                    iDamage.Damage(m_Damage, gameObject);
                    SetState(ZombiState.Attack);
                }
            }
        }

        //IDamage
        public void Damage(int damage, GameObject attacker)
        {
            m_HP.value = Mathf.Max(m_HP.value - damage, 0);

            if (m_HP.value <= 0)
            {
                if (attacker.GetComponent<ZombiCharacter>())
                    SetState(ZombiState.Die);
                else
                {
                    ZombiManager.Instance.ChangeZombiOwner(this, attacker);
                    SetState(ZombiState.Idle);
                    m_SlowTimer = m_SlowTime;
                    m_Agent.speed *= 0.5f;
                    m_OwnerChangePar[GetOwnerIndex(attacker)].Play();
                }
            }
        }
        public void Heal(int amount)
        {
            m_HP.value = Mathf.Min(m_HP.value + amount, m_MaxHP);
        }

        //Animation Event
        public void OnSpawnEnd()
        {
            SetState(ZombiState.Idle);
        }
        public void OnAttackEnd()
        {
            SetState(ZombiState.Idle);
        }
        public void OnDieEnd()
        {
            ZombiManager zombiManager = ZombiManager.Instance;
            zombiManager.DestroyZombi(this);
        }
        #endregion
        #region State
        //Spawning
        private void StateSpawning()
        {
        }
        private ZombiState StateSpawningNext()
        {
            return ZombiState.Spawn;
        }

        //Idle
        private void StateIdle()
        {
        }
        private ZombiState StateIdleNext()
        {
            Transform target = UpdateMoveTarget();
            if (target)
                return ZombiState.Move;
            else
                return ZombiState.Idle;
        }

        //Move
        private void StateMove()
        {
            Transform target = UpdateMoveTarget();
            if (target)
            {
                float dist = Vector3.Distance(transform.position, target.position);
                if (target.gameObject == ownerPlayer && dist < m_OwnerMinDist)
                {
                    m_Agent.velocity = Vector3.zero;
                    return;
                }

                m_Agent.SetDestination(target.position);
            }
        }
        private ZombiState StateMoveNext()
        {
            Transform target = UpdateMoveTarget();
            if (target)
                return ZombiState.Move;
            else
                return ZombiState.Idle;
        }
        private Transform UpdateMoveTarget()
        {
            //공격 불가능한 좀비인 경우 제거
            if (m_TargetZombi && !m_TargetZombi.IsDamageEnable)
                m_TargetZombi = null;

            //공격할 좀비가 없을 경우 공격할 좀비를 설정한다 (물론 이 코드 이후에도 없을수도 있다)
            if (m_TargetZombi == null)
                SetTargetZombi(GetNextAttackZombi());
            if (m_TargetPlayer == null)
                m_TargetPlayer = GetNextAttackPlayer();

            //이동 타겟 구하기
            Transform target = null;
            if (m_TargetZombi && !m_TargetPlayer)       //타겟이 좀비만 있을때
                target = m_TargetZombi.transform;
            else if (!m_TargetZombi && m_TargetPlayer)  //타겟이 플레이어만 있을때
                target = m_TargetPlayer.transform;
            else if(m_TargetPlayer && m_TargetZombi)    //타겟이 둘 다 있을 때
            {
                float zombiPriority = GetAttackZombiPriority(m_TargetZombi, false);
                float playerPriority = GetAttackPlayerPriority(m_TargetPlayer);

                target = (zombiPriority < playerPriority) ? m_TargetZombi.transform : m_TargetPlayer.transform;
            }
            else                                        //타겟이 없을때
            {
                if (ownerPlayer && ownerPlayer.GetComponent<Player>())
                    target = ownerPlayer.transform;
            }

            return target;
        }

        //Attack
        private void StateAttack()
        {
        }
        private ZombiState StateAttackNext()
        {
            return ZombiState.Attack;
        }

        //Die
        private void StateDie()
        {
        }
        private ZombiState StateDieNext()
        {
            return ZombiState.Die;
        }
        #endregion
        #region Function
        //Private
        /// <summary>
        /// 상태를 설정한다.
        /// </summary>
        /// <param name="state">상태</param>
        private void SetState(ZombiState state)
        {
            //State 변경
            ZombiState oldState = zombiState;
            zombiState = state;

            //사망상태에서 다른걸로는 못넘어간다.
            if (oldState == ZombiState.Die)
                zombiState = ZombiState.Die;

            //State 변경시의 추가 처리
            if (oldState != zombiState)
            {
                //Attack이나 이동이 아닌 경우는 다른 좀비를 타겟으로 하지 않는다.
                if(zombiState != ZombiState.Attack && zombiState != ZombiState.Move)
                    SetTargetZombi(null);

                //사망시 콜라이더 소멸
                if (zombiState == ZombiState.Die)
                {
                    m_Collider.enabled = false;
                    m_Rigidbody.isKinematic = true;
                    m_Rigidbody.velocity = Vector3.zero;
                    m_Agent.enabled = false;
                }

                //변경된 State에 따른 애니메이션 재생
                m_FrontAnimator.SetTrigger(zombiState.ToString());
            }
        }
        /// <summary>
        /// 해당 좀비의 주인을 설정한다.
        /// </summary>
        /// <param name="owner">주인</param>
        private void SetOwner(GameObject owner)
        {
            ownerPlayer = owner;
            SetSprite(GetOwnerIndex(owner));
        }

        //Private Util
        /// <summary>
        /// (좀비) 타겟을 설정한다.
        /// </summary>
        /// <param name="target">타겟</param>
        private void SetTargetZombi(ZombiCharacter target)
        {
            if (m_TargetZombi)
                m_TargetZombi.Priority -= 1;

            m_TargetZombi = target;

            if (m_TargetZombi)
                m_TargetZombi.Priority += 1;
        }
        /// <summary>
        /// 다음번으로 공격할 좀비를 구한다.
        /// </summary>
        /// <returns></returns>
        private ZombiCharacter GetNextAttackZombi()
        {
            //사용할것들
            ZombiManager zombiManager = ZombiManager.Instance;

            //주인이 같지 않은 모든 좀비를 순회해서 공격 가능한지 구한다.
            List<GameObject> ownerList = zombiManager.GetAllOwnerList();
            ZombiCharacter nextZombi = null;
            float nextZombiPriority = float.MaxValue;
            for (int i = 0; i < ownerList.Count; ++i)
            {
                if (ownerList[i] != ownerPlayer)
                {
                    List<ZombiCharacter> zombiList = zombiManager.GetSpawnedZombiList(ownerList[i]);
                    for (int j = 0; j < zombiList.Count; ++j)
                    {
                        //좀비의 우선순위 / 공격가능을 구한다.
                        if(GetAttackZombiEnable(zombiList[j]))
                        {
                            float priority = GetAttackZombiPriority(zombiList[j], true);
                            if(priority < nextZombiPriority)
                            {
                                nextZombi = zombiList[j];
                                nextZombiPriority = priority;
                            }
                        }
                    }
                }
            }

            return nextZombi;
        }
        /// <summary>
        /// 다음번으로 공격할 플레이어를 구한다.
        /// </summary>
        /// <returns></returns>
        private GameObject GetNextAttackPlayer()
        {
            //사용할것들
            ZombiManager zombiManager = ZombiManager.Instance;

            //플레이어를 순회해서 공격 가능한지 구한다
            List<GameObject> ownerList = zombiManager.GetPlayerOwnerList();
            GameObject nextOwner = null;
            float nextOwnerPriority = float.MaxValue;
            for (int i = 0; i < ownerList.Count; ++i)
            {
                if (GetAttackPlayerEnable(ownerList[i]))
                {
                    float priority = GetAttackPlayerPriority(ownerList[i]);
                    if (priority < nextOwnerPriority)
                    {
                        nextOwner = ownerList[i];
                        nextOwnerPriority = priority;
                    }
                }
            }

            return nextOwner;
        }
        /// <summary>
        /// 공격 가능한 좀비인지 구한다.
        /// </summary>
        /// <param name="target">타겟</param>
        /// <returns></returns>
        private bool GetAttackZombiEnable(ZombiCharacter target)
        {
            bool isDistEnable = Vector3.Distance(transform.position, target.transform.position) <= m_ZombiSearchDist;

            return isDistEnable & target.IsDamageEnable;
        }
        /// <summary>
        /// 공격 우선순위 점수를 구한다. (낮을수록 공격 우선순위가 높다.)
        /// </summary>
        /// <param name="target">공격 타겟(후보)</param>
        /// <returns></returns>
        private float GetAttackZombiPriority(ZombiCharacter target, bool isUseAddScore)
        {
            float dist = Vector3.Distance(transform.position, target.transform.position);
            float addScore = target.Priority;

            return dist + (isUseAddScore ? addScore : 0);
        }
        /// <summary>
        /// 공격 가능한 플레이어인지 구한다.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        private bool GetAttackPlayerEnable(GameObject target)
        {
            Player player = target.GetComponent<Player>();
            if (player)
            {
                bool isDistEnable = Vector3.Distance(transform.position, target.transform.position) <= m_ZombiSearchDist;

                return isDistEnable & player.IsDamageEnable;
            }
            else
                return false;
        }
        /// <summary>
        /// 공격 우선순위 점수를 구한다. (낮을수록 공격 우선순윙가 높다.)
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        private float GetAttackPlayerPriority(GameObject target)
        {
            float dist = Vector3.Distance(transform.position, target.transform.position);

            return dist;
        }
        /// <summary>
        /// 스프라이트를 설정합니다.
        /// </summary>
        /// <param name="index"></param>
        private void SetSprite(int index)
        {
            m_SpriteIndex = index;
            bool isBack = (m_Agent.velocity.z <= 0);

            if (isBack)
            {
                m_FrontObj.body.sprite = m_FrontSpr[index].body;
                m_FrontObj.hand.sprite = m_FrontSpr[index].hand;
                m_FrontObj.hair.sprite = m_FrontSpr[index].hair;
                m_FrontObj.leg2.sprite = m_FrontSpr[index].leg2;
                m_FrontObj.leg1.sprite = m_FrontSpr[index].leg1;
                m_FrontObj.head.sprite = m_FrontSpr[index].head;
                m_FrontObj.hair.sprite = m_FrontSpr[index].hair;

                Vector3 pos = m_FrontObj.hair.transform.localPosition;
                pos.z = 0.05f;
                m_FrontObj.hair.transform.localPosition = pos;
            }
            else
            {
                m_FrontObj.body.sprite = m_BackSpr[index].body;
                m_FrontObj.hand.sprite = m_BackSpr[index].hand;
                m_FrontObj.hair.sprite = m_BackSpr[index].hair;
                m_FrontObj.leg2.sprite = m_BackSpr[index].leg2;
                m_FrontObj.leg1.sprite = m_BackSpr[index].leg1;
                m_FrontObj.head.sprite = m_BackSpr[index].head;
                m_FrontObj.hair.sprite = m_BackSpr[index].hair;

                Vector3 pos = m_FrontObj.hair.transform.localPosition;
                pos.z = -0.05f;
                m_FrontObj.hair.transform.localPosition = pos;
            }
        }
        /// <summary>
        /// 주인의 Index를 구합니다.
        /// </summary>
        /// <param name="owner"></param>
        /// <returns></returns>
        private int GetOwnerIndex(GameObject owner)
        {
            List<GameObject> ownerList = ZombiManager.Instance.GetPlayerOwnerList();
            if (owner.GetComponent<Player>())
            {
                for (int i = 0; i < ownerList.Count; ++i)
                {
                    if (owner == ownerList[i])
                    {
                        return i;
                    }
                }
            }

            return ownerList.Count + 1;
        }
        #endregion
    }
}
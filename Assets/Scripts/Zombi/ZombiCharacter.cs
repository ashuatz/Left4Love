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
        public enum ZombiState
        {
            Spawning,
            Idle,
            Move,
            Attack,
            Love,
            Die
        }

        private delegate ZombiState StateNextEvent();
        #endregion

        #region Inspector
        [Header("Component")]
        [SerializeField] private NavMeshAgent m_Agent;

        [Header("Balance")]
        [SerializeField] private int m_MaxHP;                   //체력
        [SerializeField] private int m_Damage;                  //공격력
        [SerializeField] private float m_Speed;                 //이동속도
        [SerializeField] private float m_ZombiSearchDist;       //다른 적 좀비를 검색하는 거리
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
                bool isNotDied = zombiState == ZombiState.Die;
                bool isNotLove = zombiState == ZombiState.Love;
                bool isNotSpawning = zombiState == ZombiState.Spawning;

                return isNotDied & isNotLove & isNotSpawning;
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
        private GameObject m_TargetPlayer;                      //공격 타겟 플레이어
        private ZombiCharacter m_TargetZombi;                   //공격 타겟 좀비

        private Action[] m_StateUpdate;                         //각 State의 Update
        private StateNextEvent[] m_StateNext;                   //각 State의 다음으로 넘어가는 조건

        private SubjectValue<int> m_HP;                         //몬스터 HP
        #endregion

        #region Event
        internal void Init(GameObject owner)
        {
            ownerPlayer = owner;
            m_StateUpdate = new Action[] { StateSpawning, StateIdle, StateMove, StateAttack, StateLove, StateDie };
            m_StateNext = new StateNextEvent[] { StateSpawningNext, StateIdleNext, StateMoveNext, StateAttackNext, StateLoveNext, StateDieNext };

            m_HP.value = m_MaxHP;
        }

        //Unity Evnet
        private void Update()
        {
            //SetState(m_StateNext[(int)zombiState]());
            //m_StateUpdate[(int)zombiState]();
        }
        private void OnCollisionStay(Collision collision)
        {
            
        }

        //IDamage
        public void Damage(int damage, GameObject attacker)
        {
            m_HP.value = Mathf.Max(m_HP.value - damage, 0);
        }
        public void Heal(int amount)
        {
            m_HP.value = Mathf.Min(m_HP.value + amount, m_MaxHP);
        }
        #endregion
        #region State
        //Spawning
        private void StateSpawning()
        {
        }
        private ZombiState StateSpawningNext()
        {
            return ZombiState.Spawning;
        }

        //Idle
        private void StateIdle()
        {
        }
        private ZombiState StateIdleNext()
        {
            return ZombiState.Idle;
        }

        //Move
        private void StateMove()
        {
            Transform target = UpdateMoveTarget();
            if (target)
            {
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
            //공격할 좀비가 없을 경우 공격할 좀비를 설정한다 (물론 이 코드 이후에도 없을수도 있다)
            if (m_TargetZombi == null)
                SetTargetZombi(GetNextAttackZombi());

            //이동 타겟 구하기
            Transform target = null;
            if (m_TargetZombi)
                target = m_TargetZombi.transform;
            else
                target = (m_TargetPlayer ? m_TargetPlayer : ownerPlayer).transform;

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

        //Love
        private void StateLove()
        {
        }
        private ZombiState StateLoveNext()
        {
            return ZombiState.Love;
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

            //State 변경시의 추가 처리
            if(oldState != state)
            {
                //Attack이나 이동이 아닌 경우는 다른 좀비를 타겟으로 하지 않는다.
                if(state != ZombiState.Attack && state != ZombiState.Move)
                    SetTargetZombi(null);
            }
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
            List<GameObject> ownerList = zombiManager.GetOwnerList();
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
                        if(GetAttackEnable(zombiList[j]))
                        {
                            float priority = GetAttackPriority(zombiList[j]);
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
        /// 공격 가능한 좀비인지 구한다.
        /// </summary>
        /// <param name="target">타겟</param>
        /// <returns></returns>
        private bool GetAttackEnable(ZombiCharacter target)
        {
            bool isDistEnable = Vector3.Distance(transform.position, target.transform.position) <= m_ZombiSearchDist;

            return isDistEnable & target.IsDamageEnable;
        }
        /// <summary>
        /// 공격 우선순위 점수를 구한다. (낮을수록 공격 우선순위가 높다.)
        /// </summary>
        /// <param name="target">공격 타겟(후보)</param>
        /// <returns></returns>
        private float GetAttackPriority(ZombiCharacter target)
        {
            float dist = Vector3.Distance(transform.position, target.transform.position);
            float addScore = target.Priority;

            return dist + addScore;
        }
        #endregion
    }
}
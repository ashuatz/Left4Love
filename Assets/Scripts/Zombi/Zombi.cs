using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zombi
{
    public class Zombi : MonoBehaviour, IDamage
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
        #endregion

        #region Inspector
        [Header("Balance")]
        [SerializeField] private int m_MaxHP;                   //체력
        [SerializeField] private int m_Damage;                  //공격력
        [SerializeField] private float m_ZombiSearchDist;       //다른 적 좀비를 검색하는 거리
        #endregion
        #region Get,Set
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
                return (zombiState == ZombiState.Die);
            }
        }

        public int Priority
        {
            get;
            set;
        }
        #endregion
        #region Value
        private GameObject m_OwnerPlayer;                       //주인 플레이어
        private GameObject m_TargetPlayer;                      //공격 타겟 플레이어
        private Zombi m_TargetZombi;                            //공격 타겟 좀비
        #endregion

        #region Event
        internal void Init(GameObject owner)
        {

        }

        //Unity Evnet
        private void Update()
        {
            if (zombiState == ZombiState.Move)
            {
                //현재 State에서의 행동 - 이동 타겟 구하기
                Transform target = null;
                if (m_TargetZombi == null)
                {
                    

                    target = (m_TargetPlayer ? m_TargetPlayer : m_OwnerPlayer).transform;
                }
                if (m_TargetZombi)
                    target = m_TargetZombi.transform;

                //실제 이동 처리
                if(target)
                {

                }

                //다른 State로
                if (target)
                {

                }
                else
                    SetState(ZombiState.Idle);
            }

        }

        //IDamage
        public void Damage(int damage)
        {

        }
        #endregion
        #region Function
        /// <summary>
        /// 상태를 설정한다.
        /// </summary>
        /// <param name="state">상태</param>
        private void SetState(ZombiState state)
        {
            ZombiState oldState = zombiState;
            zombiState = state;

            //State 변경시의 추가 처리
            if(oldState != state)
            {
                if(state != ZombiState.Attack && state != ZombiState.Move)
                {
                    SetTargetZombi(null);
                }
            }
        }
        /// <summary>
        /// (좀비) 타겟을 설정한다.
        /// </summary>
        /// <param name="target">타겟</param>
        private void SetTargetZombi(Zombi target)
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
        private Zombi GetNextAttackZombi()
        {
            //사용할것들
            ZombiManager zombiManager = ZombiManager.Instance;

            //주인이 같지 않은 모든 좀비를 순회해서 공격 가능한지 구한다.
            List<GameObject> ownerList = zombiManager.GetOwnerList();
            Zombi nextZombi = null;
            float nextZombiPriority = float.MaxValue;
            for (int i = 0; i < ownerList.Count; ++i)
            {
                if (ownerList[i] != m_OwnerPlayer)
                {
                    List<Zombi> zombiList = zombiManager.GetSpawnedZombiList(ownerList[i]);
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
        private bool GetAttackEnable(Zombi target)
        {
            bool isDistEnable = Vector3.Distance(transform.position, target.transform.position) <= m_ZombiSearchDist;
            bool isNotDied = target.zombiState == ZombiState.Die;
            bool isNotLove = target.zombiState == ZombiState.Love;
            bool isNotSpawning = target.zombiState == ZombiState.Spawning;

            return isDistEnable & isNotDied & isNotLove & isNotSpawning;
        }
        /// <summary>
        /// 공격 우선순위 점수를 구한다. (낮을수록 공격 우선순위가 높다.)
        /// </summary>
        /// <param name="target">공격 타겟(후보)</param>
        /// <returns></returns>
        private float GetAttackPriority(Zombi target)
        {
            float dist = Vector3.Distance(transform.position, target.transform.position);
            float addScore = target.Priority;

            return dist + addScore;
        }
        #endregion
    }
}
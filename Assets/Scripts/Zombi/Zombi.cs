using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zombi
{
    public class Zombi : MonoBehaviour
    {
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

        private void Update()
        {
            if (m_TargetZombi == null)
            {
                GameObject targetPlayer = (m_TargetPlayer ? m_TargetPlayer : m_OwnerPlayer);
            }

        }
        #endregion
    }
}
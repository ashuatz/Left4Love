using Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zombi
{
    public class ZombiManager : MonoSingleton<ZombiManager>
    {
        #region Inspector
        [Header("Prefab")]
        [SerializeField] private GameObject[] m_ZombiPrefab;
        #endregion
        #region Value
        private List<GameObject> m_Player = new List<GameObject>();
        private List<GameObject> m_All = new List<GameObject>();
        private Dictionary<GameObject, List<ZombiCharacter>> m_SpawnedZombi = new Dictionary<GameObject, List<ZombiCharacter>>();
        #endregion

        #region Function
        //Public
        /// <summary>
        /// 주인 리스트에 추가합니다.
        /// </summary>
        /// <param name="owner"></param>
        public void AddOwner(GameObject owner)
        {
            if (!m_SpawnedZombi.ContainsKey(owner))
            {
                m_SpawnedZombi.Add(owner, new List<ZombiCharacter>());
                if (owner.GetComponent<Player>())
                    m_Player.Add(owner);
                m_All.Add(owner);
            }
        }
        /// <summary>
        /// 좀비의 플레이어 주인 리스트를 가져옵니다.
        /// </summary>
        /// <returns></returns>
        public List<GameObject> GetPlayerOwnerList()
        {
            return m_Player;
        }
        /// <summary>
        /// 좀비의 모든 주인 리스트를 가져옵니다.
        /// </summary>
        /// <returns></returns>
        public List<GameObject> GetAllOwnerList()
        {
            return m_All;
        }
        /// <summary>
        /// 현재 생성되어 있는 좀비 리스트를 가져옵니다.
        /// </summary>
        /// <param name="owner">어디 소속의 좀비를 가져올지</param>
        /// <returns></returns>
        public List<ZombiCharacter> GetSpawnedZombiList(GameObject owner)
        {
            return GetZombiPool(owner);
        }
        /// <summary>
        /// 좀비를 생성합니다.
        /// </summary>
        /// <param name="owner">해당 좀비의 주인</param>
        public ZombiCharacter SpawnZombi(GameObject owner, Vector3 targetPos, ZombiTypeEnum type)
        {
            GameObject go = Instantiate(m_ZombiPrefab[(int)type], targetPos, Quaternion.identity);
            ZombiCharacter zombi = go.GetComponent<ZombiCharacter>();
            zombi.Init(owner);

            List<ZombiCharacter> pool = GetZombiPool(owner);
            pool.Add(zombi);

            return zombi;
        }
        /// <summary>
        /// 좀비를 제거합니다.
        /// </summary>
        /// <param name="zombi">제거할 좀비</param>
        public void DestroyZombi(ZombiCharacter zombi)
        {
            GetZombiPool(zombi.ownerPlayer).Remove(zombi);
            Destroy(zombi.gameObject);
        }
        /// <summary>
        /// 좀비 주인을 변경합니다.
        /// </summary>
        /// <param name="zombi"></param>
        /// <param name="nextOwner"></param>
        public void ChangeZombiOwner(ZombiCharacter zombi, GameObject nextOwner)
        {
            GetZombiPool(zombi.ownerPlayer).Remove(zombi);
            zombi.Init(nextOwner);
            GetZombiPool(zombi.ownerPlayer).Add(zombi);
        }
        #endregion
        #region Function
        private List<ZombiCharacter> GetZombiPool(GameObject owner)
        {
            List<ZombiCharacter> zombiPool;
            if (!m_SpawnedZombi.TryGetValue(owner, out zombiPool))
            {
                zombiPool = new List<ZombiCharacter>();
                m_SpawnedZombi.Add(owner, zombiPool);

                if (owner.GetComponent<Player>())
                {
                    m_Player.Add(owner);
                    m_All.Add(owner);
                }
            }

            return zombiPool;
        }
        #endregion
    }
}
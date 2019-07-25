using Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zombi
{
    public class ZombiManager : MonoSingleton<ZombiManager>
    {
        #region Type
        private struct ZombiPoolStruct
        {
            public List<Zombi> spawned;
            public List<Zombi> died;
        }
        #endregion

        #region Value
        private Dictionary<GameObject, ZombiPoolStruct> m_SpawnedZombi = new Dictionary<GameObject, ZombiPoolStruct>();
        #endregion

        #region Event
        protected override void Awake()
        {
            base.Awake();

            if (Instance == this)
            {

            }
        }
        #endregion
        #region Function
        //Public
        /// <summary>
        /// 현재 생성되어 있는 좀비 리스트를 가져옵니다.
        /// </summary>
        /// <param name="player">어떤 플레이어의 좀비를 가져올지</param>
        /// <returns></returns>
        public List<Zombi> GetSpawnedZombi(GameObject player)
        {
            ZombiPoolStruct zombiPool;
            if (m_SpawnedZombi.TryGetValue(player, out zombiPool))
                return zombiPool.spawned;
            else
                return null;
        }
        /// <summary>
        /// 좀비를 생성합니다.
        /// </summary>
        /// <param name="player"></param>
        public void SpawnZombi(GameObject player)
        {

        }
        #endregion
    }
}
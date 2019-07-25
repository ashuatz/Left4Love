using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zombi
{
    public class ZombiCharacterAniEvent : MonoBehaviour
    {
        #region Inspector
        [Header("Component")]
        [SerializeField] private ZombiCharacter m_ZombiCharacter;
        #endregion

        #region Event
        //Animation Event
        public void OnSpawnEnd()
        {
            m_ZombiCharacter.OnSpawnEnd();
        }
        public void OnAttackEnd()
        {
            m_ZombiCharacter.OnAttackEnd();
        }
        public void OnDieEnd()
        {
            m_ZombiCharacter.OnDieEnd();
        }
        #endregion
    }
}
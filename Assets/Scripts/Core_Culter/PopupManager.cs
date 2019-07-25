using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PopupSystem
{
    /// <summary>
    /// 팝업 관리기능이 포함된 클래스입니다.
    /// </summary>
    public class PopupManager : MonoBehaviour
    {
        #region Inspector
        [Header("Popup")]
        [SerializeField] protected Popup[] m_InitPopup;   //해당 매니저가 포함 할 팝업

        [Header("Popup Animation")]
        [SerializeField] private AnimationCurve m_PopupOpenScaleCurve;  //팝업 열릴때 애니메이션 커브
        [SerializeField] private AnimationCurve m_PopupCloseScaleCurve; //팝업 닫힐때 애니메이션 커브

        [Header("Preference")]
        [SerializeField] private Camera m_UICamera;
        [SerializeField] private float m_PopupDistanceStart = 10;  //팝업과 UI 거리 시작지점
        [SerializeField] private float m_PopupDistance = -1.0f;    //팝업간에 거리
        [SerializeField] private float m_PopupChangeSpeed = 5.0f;  //팝업이 열리거나 닫히는 속도(실제 걸리는 시간 = 1 / valPopupChangeSpeed)
        [SerializeField] private bool m_IsPopupCloseByEscape = true;  //Escape버튼으로 팝업을 닫을 수 있는지
        #endregion
        #region Get,Set
        /// <summary>
        /// Init으로 초기화가 되었는지를 의미합니다.
        /// </summary>
        public bool isInited
        {
            get
            {
                return m_OpenPopup != null;
            }
        }
        /// <summary>
        /// 팝업이 변경되는 속도입니다.
        /// </summary>
        public float popupChangeSpeed
        {
            get
            {
                return m_PopupChangeSpeed;
            }
        }
        /// <summary>
        /// 현재 열려있는 팝업의 갯수입니다.
        /// </summary>
        public int openPopupCount
        {
            get
            {
                return m_OpenPopupStackPoint;
            }
        }
        /// <summary>
        /// 팝업이 Escape버튼으로 닫히는 기능을 사용할지를 의미합니다.
        /// </summary>
        public bool isPopupCloseByEscape
        {
            get
            {
                return m_IsPopupCloseByEscape;
            }
            set
            {
                m_IsPopupCloseByEscape = value;
            }
        }
        #endregion
        #region Value
        private int m_OpenPopupStackPoint;  //현재 열려있는 팝업의 스택 포인트입니다.
        private Popup[] m_OpenPopup;        //현재 열려있는 팝업입니다.
        #endregion

        #region Event
        //UnityEvent
        private void Awake()
        {
            Init();
        }
        private void Update()
        {
            if (0 < m_OpenPopupStackPoint && Input.GetKeyDown(KeyCode.Escape))
            {
                Popup nowPopup = m_OpenPopup[m_OpenPopupStackPoint - 1];

                if (nowPopup.isCloseByEscape)
                    nowPopup.Close();
            }
        }

        //PopupManagerEvent
        /// <summary>
        /// 팝업을 초기화합니다.
        /// </summary>
        public virtual void Init()
        {
            for (int i = 0; i < m_InitPopup.Length; ++i)
                m_InitPopup[i].Init(this, m_UICamera);

            m_OpenPopup = new Popup[m_InitPopup.Length];
        }
        /// <summary>
        /// 팝업을 초기화합니다. 동시에 m_InitPopup을 변경합니다.
        /// </summary>
        /// <param name="initPopup">어떤 팝업을 사용하여 초기화할지</param>
        public void Init(Popup[] initPopup)
        {
            m_InitPopup = initPopup;
            Init();
        }
        #endregion
        #region Function
        //Public
        /// <summary>
        /// 등록된 팝업의 언어를 전부 업데이트 합니다.
        /// </summary>
        public void UpdateLanguage()
        {
            for (int i = 0; i < m_InitPopup.Length; ++i) m_InitPopup[i].UpdateLanguage();
        }

        //Internal
        /// <summary>
        /// 해당 팝업을 스택에 넣습니다.
        /// </summary>
        /// <param name="popup">넣을 팝업</param>
        internal void PushPopup(Popup popup)
        {
            if (m_OpenPopupStackPoint < m_OpenPopup.Length)
            {
                m_OpenPopup[m_OpenPopupStackPoint++] = popup;
                popup.SetSort(m_PopupDistanceStart + m_OpenPopupStackPoint * m_PopupDistance, m_OpenPopupStackPoint);
            }
        }
        /// <summary>
        /// 해당 팝업을 스택에서 뺍니다.
        /// </summary>
        /// <param name="popup">뺄 팝업</param>
        internal void PopPopup(Popup popup)
        {
            for (int i = 0; i < m_OpenPopupStackPoint; ++i)
            {
                if (m_OpenPopup[i] == popup)
                {
                    m_OpenPopup[i] = null;
                    m_OpenPopupStackPoint -= 1;

                    for (int j = i; j < m_OpenPopupStackPoint; ++j) m_OpenPopup[j] = m_OpenPopup[j + 1];
                }
            }
        }

        /// <summary>팝업이 열릴 때 시간에 따른 Scale값을 가져옵니다.</summary>
        /// <param name="time">normalize 된 시간</param>
        internal float GetPopupOpenScale(float time)
        {
            return m_PopupOpenScaleCurve.Evaluate(time);
        }
        /// <summary>팝업이 닫힐 때 시간에 따른 Scale값을 가져옵니다.</summary>
        /// <param name="time">normalize 된 시간</param>
        internal float GetPopupCloseScale(float time)
        {
            return m_PopupCloseScaleCurve.Evaluate(time);
        }
        #endregion
    }
}
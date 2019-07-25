using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PopupSystem
{
    /// <summary>
    /// 팝업 기본 클래스입니다.
    /// </summary>
    public abstract class Popup : MonoBehaviour
    {
        #region Type
        protected delegate IEnumerator ChangeAniCoroutine();
        #endregion

        #region Inspector
        [Header("Component")]
        [SerializeField] protected Canvas m_Canvas; //Canvas
        [SerializeField] protected RectTransform m_FrameTransform;  //RectTransform

        [Header("Option")]
        [SerializeField] private bool m_IsCloseByEscape;    //Escape버튼을 사용해 해당 팝업을 종료 할 수 있는지
        #endregion
        #region Value
        private PopupManager m_PopupManager;    //해당 팝업이 속한 팝업매니저
        private Coroutine m_OpenCoroutine;      //팝업 열기 애니메이션 코루틴
        private Coroutine m_CloseCoroutine;     //팝업 닫기 애니메이션 코루틴
        private Vector3 m_FrameOriginalScale;   //프레임의 원래 크기
        #endregion

        #region Get, Set
        /// <summary>
        /// 해당 팝업이 어떤 팝업매니저에 속해있는지 가져옵니다.
        /// </summary>
        public PopupManager popupManager
        {
            get
            {
                return m_PopupManager;
            }
        }
        /// <summary>
        /// 해당 팝업이 초기화되었는지 가져옵니다.
        /// </summary>
        public bool isInited
        {
            get
            {
                return m_PopupManager != null;
            }
        }
        /// <summary>
        /// Escape버튼을 사용해 해당 팝업을 종료 할 수 있는지 가져옵니다.
        /// </summary>
        public bool isCloseByEscape
        {
            get
            {
                return m_IsCloseByEscape;
            }
            protected set
            {
                m_IsCloseByEscape = value;
            }
        }
        #endregion

        #region Event
        //PopupEvent
        /// <summary>
        /// 팝업을 초기화합니다.
        /// </summary>
        /// <param name="manager">해당 팝업을 포함하는 매니저입니다.</param>
        /// <param name="uiCamera">해당 팝업을 렌더링할 카메라입니다.</param>
        internal virtual void Init(PopupManager manager, Camera uiCamera)
        {
            m_PopupManager = manager;

            m_Canvas.worldCamera = uiCamera;
            if (m_FrameTransform) m_FrameOriginalScale = m_FrameTransform.localScale;

            gameObject.SetActive(false);
        }

        /// <summary>
        /// 언어를 업데이트 해야 할 때 호출되는 이벤트입니다.
        /// </summary>
        protected abstract void OnUpdateLanguage();
        /// <summary>
        /// 언어 외 컨텐츠를 업데이트 해야 할 때 호출되는 이벤트입니다.
        /// </summary>
        protected abstract void OnUpdateContent();
        /// <summary>
        /// 팝업이 열리기 시작 할 때 호출되는 이벤트입니다.
        /// </summary>
        protected virtual void OnStartOpen()
        {

        }
        /// <summary>
        /// 팝업이 닫히기 시작 할 때 호출되는 이벤트입니다.
        /// </summary>
        protected virtual void OnStartClose()
        {
        }
        /// <summary>
        /// 팝업이 열린 직후 호출되는 이벤트입니다.
        /// </summary>
        protected virtual void OnEndOpen()
        {
        }
        /// <summary>
        /// 팝업이 종료 된 직후 호출되는 이벤트입니다.
        /// </summary>
        protected virtual void OnEndClose()
        {
        }
        #endregion
        #region Function
        //Public
        /// <summary>
        /// 팝업 열기
        /// </summary>
        public virtual void Open()
        {
            OpenByCoroutine(OpenCoroutine_Default);
            UpdateContent();
            UpdateLanguage();
        }
        /// <summary>
        /// 팝업 닫기
        /// </summary>
        public virtual void Close()
        {
            CloseByCoroutine(CloseCoroutine_Default);
        }
        /// <summary>
        /// 언어를 업데이트한다.
        /// </summary>
        public void UpdateLanguage()
        {
            if (gameObject.activeSelf)
                OnUpdateLanguage();
        }
        /// <summary>
        /// 언어 외 컨텐츠를 업데이트한다.
        /// </summary>
        public void UpdateContent()
        {
            if (gameObject.activeSelf)
                OnUpdateContent();
        }

        //Internal
        internal void SetSort(float distance, int order)
        {
            m_Canvas.planeDistance = distance;
            m_Canvas.sortingOrder = order;
        }   //캔버스의 sortingOrder, Distance 값 셋팅

        //Private Function
        /// <summary>
        /// 원하는 애니메이션 코루틴을 사용하여 팝업을 엽니다.
        /// </summary>
        /// <param name="coroutine">애니메이션 코루틴</param>
        protected void OpenByCoroutine(ChangeAniCoroutine coroutine)
        {
            if (isInited)
            {
                if (!gameObject.activeInHierarchy)
                {
                    m_PopupManager.PushPopup(this);
                    gameObject.SetActive(true);
                    OnStartOpen();

                    if (m_OpenCoroutine == null)
                    {
                        if (m_FrameTransform)
                            m_OpenCoroutine = StartCoroutine(coroutine());
                        else
                            EndOpen();
                    }
                }
            }
            else
                Debug.LogError("Open need Init");
        }
        /// <summary>
        /// 원하는 애니메이션 코루틴을 사용하여 팝업을 닫습니다.
        /// </summary>
        /// <param name="coroutine">애니메이션 코루틴</param>
        protected void CloseByCoroutine(ChangeAniCoroutine coroutine)
        {
            if (isInited)
            {
                if (gameObject.activeInHierarchy && m_OpenCoroutine == null)
                {
                    OnStartClose();

                    if (m_CloseCoroutine == null)
                    {
                        if (m_FrameTransform)
                            m_CloseCoroutine = StartCoroutine(coroutine());
                        else
                            EndClose();
                    }
                }
            }
            else
                Debug.LogError("Close need Init");
        }

        private IEnumerator OpenCoroutine_Default()
        {
            float timer = 0;

            while (true)
            {
                timer += Time.unscaledDeltaTime * m_PopupManager.popupChangeSpeed;
                m_FrameTransform.localScale = m_FrameOriginalScale * Mathf.Lerp(0, 1.0f, m_PopupManager.GetPopupOpenScale(timer));

                if (1.0f <= timer)
                {
                    m_FrameTransform.localScale = m_FrameOriginalScale;
                    EndOpen();
                    break;
                }
                else
                    yield return null;
            }
            yield break;
        }   //열기 애니메이션 코루틴(디폴트 : 스케일 애니메이션)
        private IEnumerator CloseCoroutine_Default()
        {
            float timer = 0;

            while (true)
            {
                timer += Time.unscaledDeltaTime * m_PopupManager.popupChangeSpeed;
                m_FrameTransform.localScale = m_FrameOriginalScale * Mathf.Lerp(1.0f, 0, m_PopupManager.GetPopupCloseScale(timer));

                if (1.0f <= timer)
                {
                    m_FrameTransform.localScale = Vector3.zero;

                    EndClose();
                    break;
                }
                else
                    yield return null;
            }
            yield break;
        }   //닫기 애니메이션 코루틴(디폴트 : 스케일 애니메이션)

        protected void EndOpen()
        {
            m_OpenCoroutine = null;
            OnEndOpen();
        }   //열기 애니메이션 완료
        protected void EndClose()
        {
            m_CloseCoroutine = null;
            gameObject.SetActive(false);
            m_PopupManager.PopPopup(this);
            OnEndClose();
        }   //닫기 애니메이션 완료
        #endregion
    }
}
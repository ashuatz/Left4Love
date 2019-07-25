using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PopupSystem;
using UnityEngine.SceneManagement;

public class WinPopup : Popup
{
    public static WinPopup instance
    {
        get;
        private set;
    }

    #region Inspector
    [SerializeField] private GameObject m_WinText;
    [SerializeField] private GameObject m_LoseText;
    #endregion

    #region Event
    internal override void Init(PopupManager manager, Camera uiCamera)
    {
        base.Init(manager, uiCamera);

        instance = this;
    }
    public void Open(bool isWin)
    {
        base.Open();

        Time.timeScale = 0;
        m_WinText.SetActive(isWin);
        m_LoseText.SetActive(!isWin);

        StartCoroutine(SceneChangeCoroutine());
    }
    protected override void OnUpdateContent()
    {
    }
    protected override void OnUpdateLanguage()
    {
    }
    #endregion
    #region function
    private IEnumerator SceneChangeCoroutine()
    {
        yield return new WaitForSecondsRealtime(5.0f);

        Time.timeScale = 1;
        Application.Quit();
    }
    #endregion
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PopupSystem;

public class HelpPopup : Popup
{
    public static HelpPopup instance
    {
        get;
        private set;
    }


    #region Event
    internal override void Init(PopupManager manager, Camera uiCamera)
    {
        base.Init(manager, uiCamera);

        instance = this;
    }
    protected override void OnUpdateContent()
    {
    }
    protected override void OnUpdateLanguage()
    {
    }
    protected override void OnStartOpen()
    {
        base.OnStartOpen();

        Time.timeScale = 0;
    }
    protected override void OnStartClose()
    {
        base.OnStartClose();

        Time.timeScale = 1.0f;
    }
    #endregion
}

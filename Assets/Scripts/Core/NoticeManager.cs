using UnityEngine;
using System.Collections;
using Core;

public class NoticeManager : MonoSingleton<NoticeManager>
{
    [SerializeField]
    private GameObject NoticeItemOrigin;

    [SerializeField]
    private Transform Root;
    
    protected override void Awake()
    {
        base.Awake();
    }
    
    public void ShowNotice(NoticeItem.NoticeItemInfo info)
    {
        info.AddOnComplete(ReleaseNoticeItem);

        var itemObj = PoolManager.SpawnObject(NoticeItemOrigin);
        var item = CacheManager.Get<NoticeItem>(itemObj);

        item.transform.SetParent(Root, false);
        item.ShowNotice(info);
    }

    private void ReleaseNoticeItem(NoticeItem item)
    {
        PoolManager.ReleaseObject(item.gameObject);
    }
}
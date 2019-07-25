using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core;

public class PosMake : MonoSingleton<PosMake>
{
    #region Value
    private Transform[] m_Pos;
    #endregion

    #region Event
    protected override void Awake()
    {
        base.Awake();

        List<Transform> posList = new List<Transform>(transform.childCount);
        for (int i = 0; i < posList.Capacity; ++i)
            posList.Add(transform.GetChild(i));
        m_Pos = posList.ToArray();
    }
    #endregion
    #region Function
    //Public
    /// <summary>
    /// 단순히 랜덤한 위치를 가져옵니다.
    /// </summary>
    /// <returns></returns>
    public Transform GetRandomPos()
    {
        return m_Pos[Random.Range(0, m_Pos.Length)];
    }
    /// <summary>
    /// 해당 위치에서 가장 가까운 위치를 가져옵니다.
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public Transform GetNearestPos(Vector3 pos)
    {
        Transform nearest = null;
        float nearestDist = float.MaxValue;
        for(int i=0;i<m_Pos.Length;++i)
        {
            float dist = Vector3.Distance(pos, m_Pos[i].position);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = m_Pos[i];
            }
        }

        return nearest;
    }

    //Private Editor
    [ContextMenu("ASDF")]
    private void ASDF()
    {
        for (int x = 0; x < 15; ++x)
        {
            for (int y = 0; y < 15; ++y)
            {
                GameObject go = new GameObject($"{x},{y}");
                go.transform.SetParent(transform);
                go.transform.localPosition = new Vector3(-22 + x * 3, 0, -22 + y * 3);
            }
        }
    }
    #endregion
}

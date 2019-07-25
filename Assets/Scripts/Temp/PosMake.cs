using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PosMake : MonoBehaviour
{
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
}

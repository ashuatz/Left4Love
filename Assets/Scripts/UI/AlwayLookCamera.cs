﻿using UnityEngine;
using System.Collections;

public class AlwayLookCamera : MonoBehaviour
{
    void Update()
    {
        transform.LookAt(Camera.main.transform);
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


public class BaseInput : MonoBehaviour
{
    public virtual event Action<Vector2> OnMoveDirection;
    public virtual event Action<Vector2> OnViewDirection;

    public virtual event Action<bool> OnClick;
    public virtual event Action OnSpectialClick;
}

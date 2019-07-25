using UnityEngine;
using System.Collections;
using System;

public class CollisionRiser : MonoBehaviour
{
    public event Action<Collider> OnTriggerEnterRiser;
    public event Action<Collider> OnTriggerStayRiser;
    public event Action<Collider> OnTriggerExitRiser;

    private void OnTriggerEnter(Collider other)
    {
        OnTriggerEnterRiser?.Invoke(other);
    }
    private void OnTriggerStay(Collider other)
    {
        OnTriggerStayRiser?.Invoke(other);
    }
    private void OnTriggerExit(Collider other)
    {
        OnTriggerExitRiser?.Invoke(other);
    }
}

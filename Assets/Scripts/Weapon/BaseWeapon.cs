using UnityEngine;
using System.Collections;
using System;

public abstract class BaseWeapon : MonoBehaviour
{
    [SerializeField]
    protected GameObject bulletOrigin;
    [SerializeField]
    protected float bulletSpeed;
    [SerializeField]
    protected float DefaultCoolDown;
    [SerializeField]
    protected float DefaultReChargeCoolDown;
    [SerializeField]
    protected int DefaultCapacity;
    [SerializeField]
    protected float Range;
    [SerializeField]
    protected float DefaultDamage;
    
    public virtual float CoolDown { get; protected set; }
    public virtual float ReChargeCoolDown { get; protected set; }
    public virtual float Damage { get => DefaultDamage; }

    public abstract IEnumerator Fire(Vector3 pos, Vector3 dir, Action onComplete);
}

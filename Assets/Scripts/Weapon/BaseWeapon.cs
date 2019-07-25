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
    protected float BulletAliveTime;
    [SerializeField]
    protected int DefaultDamage;
    [SerializeField]
    protected Transform StartPosition;
    
    public virtual float CoolDown { get; protected set; }
    public virtual float ReChargeCoolDown { get; protected set; }
    public virtual float Capacity { get; protected set; }
    public virtual int Damage { get => DefaultDamage; }

    public virtual GameObject Owner { get; protected set; }

    public abstract void Initialize(GameObject Owner);
    public abstract IEnumerator Fire(Vector3 dir, Action onComplete);
}

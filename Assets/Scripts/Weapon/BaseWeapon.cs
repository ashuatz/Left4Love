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

    [SerializeField]
    protected ParticleSystem Effect;

    [SerializeField]
    protected Transform GunTransform;
    
    public virtual float CoolDown { get; protected set; }
    public virtual float ReChargeCoolDown { get; protected set; }
    public virtual float Capacity { get; protected set; }
    public virtual int Damage { get => DefaultDamage; }

    public virtual GameObject Owner { get; protected set; }

    public virtual void ReverseSprite(bool isInverse)
    {
        GunTransform.localScale = new Vector3(1,isInverse ? -1 : 1, 1);
    }
    
    public virtual void Rotate(Vector2 dir)
    {
        var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        transform.localRotation = Quaternion.AngleAxis(-angle, Vector3.forward);
    }
    public abstract void Initialize(GameObject Owner);
    public abstract IEnumerator Fire(Vector3 dir, Action onComplete);
}

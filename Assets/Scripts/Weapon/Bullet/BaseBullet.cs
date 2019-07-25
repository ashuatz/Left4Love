using UnityEngine;
using System.Collections;

public abstract class BaseBullet : MonoBehaviour
{
    [SerializeField]
    protected Rigidbody rigid;
    [SerializeField]
    protected float speed;

    public virtual int Damage { get; protected set; }
    public virtual int HealAmount { get; protected set; }

    protected GameObject Owner;

    public abstract void Initialize(Vector3 pos, Vector3 dir, int dmg, GameObject owner);
    public abstract void OnTriggerEnter(Collider other);
}

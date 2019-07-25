using UnityEngine;
using System.Collections;
using Zombi;

public class Bullet : BaseBullet
{
    public override void Initialize(Vector3 pos, Vector3 dir, int dmg, GameObject owner)
    {
        transform.position = pos;
        rigid.velocity = dir.normalized * speed;
        HealAmount = Damage = dmg;
        Owner = owner;
    }

    public override void OnTriggerEnter(Collider other)
    {
        if (other.attachedRigidbody)
        {
            ZombiCharacter zombi = other.attachedRigidbody.GetComponent<ZombiCharacter>();
            if (zombi != null && zombi.ownerPlayer != Owner)
            {
                zombi.Damage(Damage, Owner);
                PoolManager.ReleaseObject(gameObject);
            }
            else if (zombi != null)
            {
                zombi.Heal(HealAmount);
            }
        }
        if (other.name.Equals("map"))
        {
            PoolManager.ReleaseObject(gameObject);
        }
    }
}

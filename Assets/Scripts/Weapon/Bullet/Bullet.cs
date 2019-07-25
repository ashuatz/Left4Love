using UnityEngine;
using System.Collections;
using Zombi;

public class Bullet : BaseBullet
{
    [SerializeField]
    private ParticleSystem Hit;
    private bool isUse = false;
    public override void Initialize(Vector3 pos, Vector3 dir, int dmg, GameObject owner)
    {
        isUse = false;
        transform.position = pos;
        rigid.velocity = dir.normalized * speed;
        HealAmount = Damage = dmg;
        Owner = owner;

        StartCoroutine(AutoRelease());
    }

    private IEnumerator AutoRelease()
    {
        yield return new WaitForSeconds(4f);
        PoolManager.ReleaseObject(gameObject);
    }

    public override void OnTriggerEnter(Collider other)
    {
        if (other.attachedRigidbody && !isUse )
        {
            Hit.Play();
            isUse = true;

            Player player = other.GetComponent<Player>();
            if(player != null && Owner != player)
            {
                player.Damage(Damage, Owner);

                //PoolManager.ReleaseObject(gameObject);
            }

            ZombiCharacter zombi = other.attachedRigidbody.GetComponent<ZombiCharacter>();
            if (zombi != null && zombi.ownerPlayer != Owner)
            {
                zombi.Damage(Damage, Owner);
                //PoolManager.ReleaseObject(gameObject);
            }
            else if (zombi != null)
            {
                zombi.Heal(HealAmount);
            }
        }
        if (other.name.Contains("map"))
        {
            PoolManager.ReleaseObject(gameObject);
        }
    }
}

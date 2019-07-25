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
        var main = Hit.main;
        main.stopAction = ParticleSystemStopAction.Callback;

        transform.position = pos;
        rigid.velocity = dir.normalized * speed;
        HealAmount = Damage = dmg;
        Owner = owner;
        
        StartCoroutine(AutoRelease());
    }

    private void OnParticleSystemStopped()
    {
        PoolManager.ReleaseObject(gameObject);
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
            isUse = true;

            Player player = other.GetComponent<Player>();
            if(player != null && Owner != player)
            {
                Hit.Play();
                player.Damage(Damage, Owner);
                rigid.velocity = Vector3.zero;
            }

            ZombiCharacter zombi = other.attachedRigidbody.GetComponent<ZombiCharacter>();
            if (zombi != null && zombi.ownerPlayer != Owner)
            {
                Hit.Play();
                zombi.Damage(Damage, Owner);
                rigid.velocity = Vector3.zero;
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

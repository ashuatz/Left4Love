using UnityEngine;
using System.Collections;
using System;

public class DefaultWeapon : BaseWeapon
{
    public override float ReChargeCoolDown { get => DefaultReChargeCoolDown; }
    public override float CoolDown { get => DefaultCoolDown; }

    public override void Initialize(GameObject Owner)
    {
        this.Owner = Owner;
        Capacity = new Core.SubjectValue<int>(DefaultCapacity);
    }

    public override IEnumerator Fire(Vector3 dir, Action onComplete)
    {
        var obj = PoolManager.SpawnObject(bulletOrigin);
        var bullet = CacheManager.Get<Bullet>(obj);
        dir = dir.normalized + new Vector3(UnityEngine.Random.Range(-0.1f, 0.1f), 0, UnityEngine.Random.Range(-0.1f, 0.1f));
        bullet.Initialize(StartPosition.position, new Vector3(dir.x, 0, dir.y), Damage, Owner);
        Effect.Play();

        Capacity.value -= 1;

        if (Capacity.value <= 0)
        {
            yield return new WaitForSeconds(ReChargeCoolDown);
            Capacity.value = DefaultCapacity;
        }
        else
        {
            yield return new WaitForSeconds(CoolDown);
        }
        onComplete?.Invoke();
    }

}

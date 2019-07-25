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
        Capacity = DefaultCapacity;
    }

    public override IEnumerator Fire(Vector3 dir, Action onComplete)
    {
        var obj = PoolManager.SpawnObject(bulletOrigin);
        var bullet = CacheManager.Get<Bullet>(obj);
        bullet.Initialize(StartPosition.position, new Vector3(dir.x, 0, dir.y), Damage, Owner);

        Capacity -= 1;
        if (Capacity <= 0)
        {
            yield return new WaitForSeconds(ReChargeCoolDown);
            Capacity = DefaultCapacity;
        }
        else
        {
            yield return new WaitForSeconds(CoolDown);
        }
        onComplete?.Invoke();
    }

}

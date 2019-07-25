using UnityEngine;
using System.Collections;
using System;

public class DefaultWeapon : BaseWeapon
{
    public override IEnumerator Fire(Vector3 pos, Vector3 dir,Action onComplete)
    {
        var obj = PoolManager.SpawnObject(bulletOrigin);
        var bullet = CacheManager.Get<Bullet>(obj);
        bullet.Initialize(pos, new Vector3(dir.x, 0, dir.y));

        yield return new WaitForSeconds(DefaultCoolDown);
        onComplete?.Invoke();
    }
}

using UnityEngine;

interface IDamage
{
    /// <summary>
    /// 해당 캐릭터가 공격 가능한지 여부
    /// </summary>
    bool IsDamageEnable
    {
        get;
    }

    bool IsHealEnable { get; }

    /// <summary>
    /// 데미지를 입힌다.
    /// </summary>
    /// <param name="damage">입힐 데미지 양</param>
    /// <param name="attacker">공격한 플레이어 또는 좀비</param>
    void Damage(int damage, GameObject attacker);
    void Heal(int amount);
}
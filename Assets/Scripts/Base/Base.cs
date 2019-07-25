interface IDamage
{
    /// <summary>
    /// 해당 캐릭터가 공격 가능한지 여부
    /// </summary>
    bool IsDamageEnable
    {
        get;
    }

    /// <summary>
    /// 데미지를 입힌다.
    /// </summary>
    /// <param name="damage">입힐 데미지 양</param>
    void Damage(int damage);
}
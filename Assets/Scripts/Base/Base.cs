interface IDamage
{
    /// <summary>
    /// 해당 캐릭터가 죽었는지 여부
    /// (죽었으면 더이상 공격하지 않기 위함)
    /// </summary>
    bool IsDied
    {
        get;
    }

    /// <summary>
    /// 데미지를 입힌다.
    /// </summary>
    /// <param name="damage">입힐 데미지 양</param>
    void Damage(int damage);
}
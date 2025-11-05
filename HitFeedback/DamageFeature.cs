using System;

namespace HitFeedback
{
    [Flags] 
    public enum DamageFeature
    {
        /// <summary>
        /// 未指定或未分类的伤害特性。
        /// </summary>
        Undefined = 0,
        /// <summary>
        /// 普通物理伤害或其他未特别定义的伤害（对应 DamageTypes.normal）。
        /// </summary>
        NormalDamage = 1,
        /// <summary>
        /// 真实伤害，不受护甲或其他减伤效果影响（对应 DamageTypes.realDamage）。
        /// </summary>
        RealDamage = 2,
        /// <summary>
        /// 伤害来自增益（Buff）或持续效果。
        /// (基于 DamageInfo.isFromBuffOrEffect)
        /// </summary>
        BuffOrEffectDamage = 4,
        /// <summary>
        /// 伤害无视目标的护甲。
        /// (基于 DamageInfo.ignoreArmor)
        /// </summary>
        ArmorIgnoringDamage = 8,
        /// <summary>
        /// 伤害是暴击伤害。
        /// (基于 DamageInfo.crit > 0)
        /// </summary>
        CriticalDamage = 16,
        /// <summary>
        /// 伤害包含护甲穿透效果。
        /// (基于 DamageInfo.armorPiercing > 0)
        /// </summary>
        ArmorPiercingDamage = 32,
        /// <summary>
        /// 伤害是爆炸类型。
        /// (基于 DamageInfo.isExplosion)
        /// </summary>
        ExplosionDamage = 64,
        /// <summary>
        /// 伤害具有护甲破坏效果。
        /// (基于 DamageInfo.armorBreak > 0)
        /// </summary>
        ArmorBreakingDamage = 128,
        /// <summary>
        /// 伤害可能带有元素效果（如果有 elementFactors 存在且非空）。
        /// </summary>
        ElementalDamage = 256,
        /// <summary>
        /// 伤害可能附带Buff效果。
        /// (基于 DamageInfo.buffChance > 0 或 buff != null)
        /// </summary>
        /// <remarks>
        /// 注意：这个可以与 BuffOrEffectDamage 区分，BuffOrEffectDamage 是伤害本身来自Buff，
        /// 而นี้是伤害造成时附带施加Buff的效果。
        /// </remarks>
        OnHitBuffApply = 512,
        /// <summary>
        /// 伤害可能附带流血效果。
        /// (基于 DamageInfo.bleedChance > 0)
        /// </summary>
        OnHitBleed = 1024,
    }
}
#1 VARIABLES MAP
 - Attack 			| float | Primary ATK multiplier |
 - Defense			| float (0-1) | Damage reduction percentage |
 - MaxHealth		| float | Used in HP-based damage/heal/shield |
 - Shield			| float | Used in HP-based damage/heal/shield |
 - CritRate			| float (0-1) | Critical hit chance |
 - CritDamage		| float | Bonus critical damage (added to 1.0 base) |
 - DamageReduction 	| float | Flat damage subtracted after all multipliers |
 
#2 PRIMARY DAMAGE FORMULAS
	1A. Player attacking Monster

float attack = _casterData.Attack * _gearData.Attack;

float critDamageMultiplier = 1 + (_isCrit ? _casterData.CritDamageMultiplierBonus + _gearData.CritDamageMultiplierBonus : 0);

float finalDamage = _params.BaseDamage
                  * _gearData.Power
                  * attack
                  * critDamageMultiplier
                  * intensity;
				  
	1B. Monster attacking Player

float playerDefFactor = 1 - player.UnitStats.Get(StatName.Defense).Value;
float playerAttackTypeResistanceFactor = 1 - GetAttackTypeResistance(player.UnitStats, attackType);

// Dodge check: 1 = hit, 0 = dodged
float dodgeFactor = 1 - Dodge(player);

float finalDamage = _params.BaseDamage
                  * _casterData.Attack
                  * playerDefFactor
                  * playerAttackTypeResistanceFactor
                  * dodgeFactor
                  * intensity;
				  
#3 DAMAGE APPLICATION
After damage is calculated, it goes through `TakeDamage`:

public float TakeDamage(DamageData damageData)
{
    // 1. Min damage clamp
    if (0 < damageData.Damage && damageData.Damage < 1) damageData.Damage = 1;

    // 2. Damage Reduction (flat subtraction)
    damageData.Damage = Mathf.Clamp(
        damageData.Damage - _statHolder.Get(StatName.DamageReduction).Value,
        0, float.MaxValue);

    // 3. Shield absorption
    damageData.Damage = ShieldDamage(damageData.Damage, ...);

    // 4. Apply to HP
    if (damageData.Damage > 0)
    {
        _currentHp = Mathf.Clamp(_currentHp - damageData.Damage,
            CanNotBellow1 ? 1 : 0, MaxHP);
    }
}  

float ShieldDamage(float damage, ...)
{
    if (_currentShield <= 0) return damage;
    float damageShield = Mathf.Min(_currentShield, damage);
    _currentShield -= damageShield;
    damage -= damageShield;
    return Mathf.Max(damage, 0);
}

#4 SECONDARY (ALTERNATIVE) FORMULA

	 (Player -> Monster)
1. Base Skill Damage         = SkillData.Params[0] (BaseDamage from effect params)

2. Gear Power Multiplier     = Gear.StatHolder.Get(StatName.None).Value

3. Attack Multiplier         = Caster.StatHolder.Get(StatName.Attack).Value
                               * Gear.StatHolder.Get(StatName.Attack).Value

4. Crit Check                = Roll(Caster.CritRate + Gear.CritRate)
   Crit Multiplier           = 1 + (isCrit ? Caster.CritDamage + Gear.CritDamage : 0)

5. Intensity                 = Effect scale (1.0 for primary, BlastDamagePercent for AoE splash)

6. Final Raw Damage          = BaseSkillDamage
                               * GearPower
                               * Attack
                               * CritMultiplier
                               * Intensity

7. DamageReduction           = RawDamage - Target.StatHolder.Get(StatName.DamageReduction).Value
                               (clamped to >= 0)

8. Shield Absorption         = Subtract from current shield first
                               (shield -= damage, remaining damage = max(damage - shield, 0))

9. HP Application           = CurrentHP - finalDamage
                               (clamped to [CanNotBellow1 ? 1 : 0, MaxHP])
							   
	(Monster -> Player)
1. Base Skill Damage         = Monster skill effect BaseDamage

2. Attack Multiplier         = Monster.StatHolder.Get(StatName.Attack).Value
                               (no gear attack multiplier for monsters)

3. Defense Factor            = 1 - TargetPlayer.UnitStats.Get(StatName.Defense).Value
                               (Defense is treated as a direct percentage reduction)

4. Attack Type Resistance    = 1 - TargetPlayer.GetAttackTypeResistance(Melee/Ranged)

5. Dodge Check               = Roll(Target.Get(StatName.DodgeRate).Value)
                               If dodged, factor = 0

6. Final Damage              = BaseDamage * Attack * DefenseFactor
                               * AttackTypeResistanceFactor * DodgeFactor * Intensity

7-8. Same DamageReduction + Shield + HP as above	
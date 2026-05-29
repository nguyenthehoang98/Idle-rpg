# Stat System

## Design Goals

- Simple and scalable stat system.
- Support Equipment, Buff, Pet, Talent.
- Easy for AI agents and developers to understand.
- Use additive growth as the primary progression method.

---

## Core Stats

| Stat | Description |
|------|-------------|
| HP | Maximum health |
| Attack | Base damage |
| Defense | Damage mitigation |
| AttackSpeed | Attack frequency |
| CritChance | Critical hit chance |
| CritDamage | Critical damage multiplier |

---

## Formula

### Generic Stat

```text
FinalStat =
(Base + Flat)
* (1 + Increased)
```

### Attack

```text
Attack =
(BaseAttack + FlatAttack)
* (1 + AttackPercent)
```

### HP

```text
HP =
(BaseHP + FlatHP)
* (1 + HPPercent)
```

### Defense

```text
Defense =
(BaseDefense + FlatDefense)
* (1 + DefensePercent)
```

---

## Damage Formula

```text
FinalDamage =
Attack
* SkillMultiplier
* (1 + DamageBonusPercent)
* CritMultiplier
```

---

## Modifier Types

### Flat

```text
Attack +50
HP +500
```

### Increased

```text
Attack +20%
HP +15%
```

---

## Design Rules

1. Progression should mainly come from Flat and Increased modifiers.
2. Avoid More Multipliers in early development.
3. Every stat source should be converted into a Modifier.
4. Keep formulas centralized and data-driven.
5. New stats should not require rewriting existing systems.

---

## Future Sources

- Equipment
- Buff
- Pet
- Talent
- Research
- Artifact

All of them should contribute through the same Modifier system.

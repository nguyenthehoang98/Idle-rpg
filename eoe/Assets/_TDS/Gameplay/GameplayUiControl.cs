using System.Collections.Generic;
using _TDS.Battle;
using UnityEngine;

namespace _TDS.Gameplay
{
    public sealed class GameplayUiControl : MonoBehaviour
    {
        [SerializeField] private GameplayUiCommon uiCommon;
        [SerializeField] private CircuitTickRunner circuitRunner;

        private readonly List<Hero> heroes = new List<Hero>();

        private void OnEnable()
        {
            Hero.OnHeroEnable += RegisterHero;
            Hero.OnHeroDisable += RegisterHero;

            foreach (Hero hero in Hero.AliveHeroes) RegisterHero(hero);
        }

        private void OnDisable()
        {
            Hero.OnHeroEnable -= RegisterHero;
            Hero.OnHeroDisable -= RegisterHero;
        }

        private void Update()
        {
            if (uiCommon == null) return;

            int currentHealth = 0;
            int maxHealth = 0;
            for (int i = 0; i < heroes.Count; i++)
            {
                Hero hero = heroes[i];
                if (hero == null) continue;
                currentHealth += hero.CurrentHealth;
                maxHealth += hero.MaxHealth;
            }

            uiCommon.SetHealth(currentHealth, maxHealth);
            uiCommon.SetEnergy(circuitRunner?.Circuit);
        }

        private void RegisterHero(Hero hero)
        {
            if (hero != null && !heroes.Contains(hero)) heroes.Add(hero);
        }
    }
}

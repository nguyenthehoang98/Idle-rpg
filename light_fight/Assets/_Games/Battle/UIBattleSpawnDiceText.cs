using System.Collections.Generic;
using DamageNumbersPro;
using UnityEngine;

namespace _Games.Battle
{
    public class UIBattleSpawnDiceText : MonoBehaviour
    {
        [SerializeField] private Transform[] points;
        [SerializeField] private Color[] colors;
        [SerializeField] private DamageNumber damageNumber;

        private readonly int[] temps = new int[BattleConst.MAX_DICE_NUMBER];

        public void Spawn(List<int> numbers)
        {
            for (int i = 0; i < temps.Length; i++)
            {
                temps[i] = 0;
            }

            for (int i = 0; i < numbers.Count; i++)
            {
                temps[numbers[i] - 1]++;
            }

            int index = 0;
            for (int i = 0; i < temps.Length; i++)
            {
                int stack = temps[i];
                if (stack <= 1) continue;
                Transform parent = points[index];
                Color color = colors[stack - 2];
                var dn = damageNumber.Spawn();
                dn.leftTextSettings.color = color;
                dn.leftText = (i + 1).ToString();
                dn.rightTextSettings.color = color;
                dn.rightText = "x" + stack;
                dn.transform.SetParent(parent);
                dn.transform.localPosition = Vector3.zero;
                dn.transform.localRotation = Quaternion.Euler(0, 0, 0);
                dn.transform.localScale = Vector3.one;
                index++;
            }
        }
    }
}
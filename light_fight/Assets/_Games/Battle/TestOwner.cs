using UnityEngine;

namespace _Games.Battle
{
    public class TestOwner : MonoBehaviour
    {
        [SerializeField, Range(1, 25)] private int loop = 1;
        [SerializeField] private int targetFPS = 30;
        [SerializeField] private TestSetting setting;
        
        private TestLogic logic;
        private float tickInterval;
        private float accumulator;

        private void Awake()
        {
            Application.runInBackground = true;
            tickInterval = 1f / targetFPS;
            logic = new TestLogic(setting);
        }

        private void OnDrawGizmos()
        {
            if (logic == null) return;
            
            logic.Draw();
        }

        private void OnGUI()
        {
            if (logic == null) return;
            
            int[] numbers = logic.DiceNumbers();
            for (int i = 0; i < numbers.Length; i++)
            {
                DrawCell(numbers[i].ToString(), new Vector2(0.1f + 0.1f * i, 0.2f), 0.08f, Color.gray, Color.white);
            }
        }

        private void Update()
        {
            if (logic == null) return;
            
            accumulator += Time.deltaTime * loop;
            float f = tickInterval;
            while (accumulator >= f)
            {
                logic.Tick(f);
                accumulator -= f;
            }
        }

        private void DrawCell(string text, Vector2 normalizedPos, float normalizedSize, Color background,
            Color textColor)
        {
            // scale theo màn hình
            float size = Mathf.Min(Screen.width, Screen.height) * normalizedSize;

            Rect rect = new Rect(
                normalizedPos.x * Screen.width - size * 0.5f,
                normalizedPos.y * Screen.height - size * 0.5f,
                size,
                size);

            // background
            Color oldColor = GUI.color;
            GUI.color = background;
            GUI.Box(rect, GUIContent.none);

            // text
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = Mathf.RoundToInt(size * 0.35f);
            style.normal.textColor = textColor;

            GUI.Label(rect, text, style);

            GUI.color = oldColor;
        }
    }
}
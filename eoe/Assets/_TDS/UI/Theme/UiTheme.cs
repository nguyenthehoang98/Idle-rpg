using UnityEngine;

namespace _TDS.UI
{
    [CreateAssetMenu(fileName = "UiTheme", menuName = "TDS/UI/Theme")]
    public sealed class UiTheme : ScriptableObject
    {
        [SerializeField] private Vector2 referenceResolution = new Vector2(1080f, 2400f);
        [SerializeField] private float spacingUnit = 4f;
        [SerializeField] private int displaySize = 40;
        [SerializeField] private int headingSize = 24;
        [SerializeField] private int bodySize = 18;
        [SerializeField] private int labelSize = 14;
        [SerializeField] private int captionSize = 12;
        [SerializeField] private Color background = Hex("0B1220");
        [SerializeField] private Color surface = Hex("172238");
        [SerializeField] private Color surfaceElevated = Hex("243552");
        [SerializeField] private Color primary = Hex("5EEAD4");
        [SerializeField] private Color accent = Hex("FDE047");
        [SerializeField] private Color warning = Hex("F97316");
        [SerializeField] private Color danger = Hex("FB7185");
        [SerializeField] private Color textPrimary = Hex("F8FAFC");
        [SerializeField] private Color textSecondary = Hex("A8B5C7");

        public Vector2 ReferenceResolution => referenceResolution;
        public float SpacingUnit => spacingUnit;
        public int DisplaySize => displaySize;
        public int HeadingSize => headingSize;
        public int BodySize => bodySize;
        public int LabelSize => labelSize;
        public int CaptionSize => captionSize;
        public Color Background => background;
        public Color Surface => surface;
        public Color SurfaceElevated => surfaceElevated;
        public Color Primary => primary;
        public Color Accent => accent;
        public Color Warning => warning;
        public Color Danger => danger;
        public Color TextPrimary => textPrimary;
        public Color TextSecondary => textSecondary;

        private static Color Hex(string value)
        {
            return ColorUtility.TryParseHtmlString($"#{value}", out Color color)
                ? color
                : Color.magenta;
        }
    }
}

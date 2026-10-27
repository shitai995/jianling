#if UNITY_EDITOR
using UnityEngine;

namespace GuJian.Editor
{
    // Editor-only reference renderer for Tool.drawString's offsets, independent of
    // OriginalTextMarkup and OriginalGui. Created temporarily by validation only.
    public sealed class TextStyleReferenceProbe : MonoBehaviour
    {
        public GUIStyle style;
        public int fontType;
        public float textHeight;

        private void OnGUI()
        {
            var matrix = GUI.matrix;
            var color = GUI.color;
            try
            {
                GUI.matrix = Matrix4x4.identity;
                GUI.color = Color.black;
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
                GUI.color = Color.white;
                float scale = Mathf.Min(Screen.width / 533f, Screen.height / 320f);
                GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 533 * scale) / 2,
                    (Screen.height - 320 * scale) / 2, 0), Quaternion.identity, Vector3.one * scale);
                GUI.BeginGroup(new Rect(10, 50, 513, 270));
                try
                {
                    string edge = fontType == 2 ? "<color=#00CCFF>描边样例AB</color>" : "<color=#FF00FF>描边样例AB</color>";
                    if (fontType == 1)
                    {
                        GUI.Label(new Rect(0, -1, 513, textHeight), edge, style);
                        GUI.Label(new Rect(0, 1, 513, textHeight), edge, style);
                        GUI.Label(new Rect(-1, 0, 513, textHeight), edge, style);
                        GUI.Label(new Rect(1, 0, 513, textHeight), edge, style);
                    }
                    else if (fontType == 2)
                        GUI.Label(new Rect(-1, -1, 513, textHeight), edge, style);
                    GUI.Label(new Rect(0, 0, 513, textHeight), "<color=#FFFFFF>描边样例AB</color>", style);
                }
                finally { GUI.EndGroup(); }
            }
            finally { GUI.matrix = matrix; GUI.color = color; }
        }
    }
}
#endif

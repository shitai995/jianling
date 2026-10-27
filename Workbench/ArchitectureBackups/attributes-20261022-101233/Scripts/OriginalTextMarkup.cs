using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace GuJian
{
    public static class OriginalTextMarkup
    {
        public sealed class DrawRun
        {
            public string foreground, edge;
            public int fontType;
        }

        private sealed class TextRun
        {
            public string text;
            public int foreground = 0xffffff, edge, fontType = 1;
        }

        public static DrawRun[] ToDrawRuns(string text)
        {
            text = text ?? "";
            var pieces = new System.Collections.Generic.List<TextRun>();
            var stack = new System.Collections.Generic.Stack<TextRun>();
            stack.Push(new TextRun());
            int start = 0;
            foreach (Match match in Regex.Matches(text, @"<c=([^>]*)>|</c>"))
            {
                int front = 0xffffff, edge = 0;
                sbyte type = 1;
                bool closing = match.Value == "</c>";
                if (!closing)
                {
                    string[] fields = match.Groups[1].Value.Split(',');
                    if (fields.Length > 3 ||
                        (fields[0].Trim().Length > 0 && !int.TryParse(fields[0].Trim(), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out front)) ||
                        (fields.Length > 1 && fields[1].Trim().Length > 0 && !int.TryParse(fields[1].Trim(), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out edge)) ||
                        (fields.Length > 2 && fields[2].Trim().Length > 0 && !sbyte.TryParse(fields[2].Trim(), out type))) continue;
                }
                AppendRun(pieces, stack.Peek(), text.Substring(start, match.Index - start));
                start = match.Index + match.Length;
                if (closing) { if (stack.Count > 1) stack.Pop(); }
                else stack.Push(new TextRun { foreground = front, edge = edge, fontType = type });
            }
            AppendRun(pieces, stack.Peek(), text.Substring(start));
            var result = new DrawRun[pieces.Count];
            for (int target = 0; target < pieces.Count; target++)
            {
                var front = new StringBuilder();
                var edge = new StringBuilder();
                for (int i = 0; i < pieces.Count; i++)
                {
                    TextRun piece = pieces[i];
                    front.Append("<color=#").Append((piece.foreground & 0xffffff).ToString("X6")).Append(i == target ? "FF>" : "00>").Append(piece.text).Append("</color>");
                    edge.Append("<color=#").Append((piece.edge & 0xffffff).ToString("X6")).Append(i == target ? "FF>" : "00>").Append(piece.text).Append("</color>");
                }
                result[target] = new DrawRun { foreground = front.ToString(), edge = edge.ToString(), fontType = pieces[target].fontType };
            }
            return result;
        }

        private static void AppendRun(System.Collections.Generic.List<TextRun> runs, TextRun style, string text)
        {
            if (text.Length == 0) return;
            runs.Add(new TextRun { text = text, foreground = style.foreground, edge = style.edge, fontType = style.fontType });
        }

        public static string ToUnity(string text)
        {
            int depth = 0;
            string result = Regex.Replace(text ?? "", @"<c=([^>]*)>|</c>", match =>
            {
                if (match.Value == "</c>")
                {
                    if (depth == 0) return "";
                    depth--; return "</color>";
                }
                string[] fields = match.Groups[1].Value.Split(',');
                if (fields.Length > 3) return match.Value;
                int color = 0xffffff, ignored;
                if (fields[0].Trim().Length > 0 && !int.TryParse(fields[0].Trim(), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out color)) return match.Value;
                if (fields.Length > 1 && fields[1].Trim().Length > 0 && !int.TryParse(fields[1].Trim(), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ignored)) return match.Value;
                sbyte fontType;
                if (fields.Length > 2 && fields[2].Trim().Length > 0 && !sbyte.TryParse(fields[2].Trim(), out fontType)) return match.Value;
                depth++;
                return "<color=#" + (color & 0xffffff).ToString("X6") + ">";
            });
            var output = new StringBuilder(result);
            while (depth-- > 0) output.Append("</color>");
            return output.ToString();
        }
    }
}

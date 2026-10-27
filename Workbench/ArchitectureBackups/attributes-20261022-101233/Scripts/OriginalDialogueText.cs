using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace GuJian
{
    // StyledText.formatText: preserve source span boundaries, integer widths and pending characters.
    public static class OriginalDialogueText
    {
        public sealed class Span
        {
            public string text;
            public int width, foreground = 0xffffff, edge, fontType = 1;
            public OriginalTextMarkup.DrawRun[] drawRuns;
        }
        public sealed class Line
        {
            public readonly List<Span> spans = new List<Span>();
            public string Text { get { var s = new StringBuilder(); foreach (var p in spans) s.Append(p.text); return s.ToString(); } }
        }
        private sealed class Style
        {
            public int foreground = 0xffffff, edge, fontType = 1;
        }

        public static Line[] Format(string text, int contentWidth, int chinaWidth, Func<string, int> measure)
        {
            text = text ?? "";
            var lines = new List<Line>();
            var stack = new Stack<Style>(); stack.Push(new Style());
            Line line = null;
            var buffer = new StringBuilder();
            int lineWidth = 0, i = 0;
            bool switchLine = true;
            char? pending = null;
            Action flush = () =>
            {
                if (buffer.Length == 0) return;
                string value = buffer.ToString(); var style = stack.Peek();
                var span = new Span { text = value, width = measure(value), foreground = style.foreground, edge = style.edge, fontType = style.fontType };
                span.drawRuns = new[] { new OriginalTextMarkup.DrawRun {
                    foreground = "<color=#" + (span.foreground & 0xffffff).ToString("X6") + ">" + value + "</color>",
                    edge = "<color=#" + (span.edge & 0xffffff).ToString("X6") + ">" + value + "</color>", fontType = span.fontType } };
                line.spans.Add(span); lineWidth += span.width; buffer.Length = 0;
            };
            while (i < text.Length)
            {
                if (switchLine)
                {
                    line = new Line(); lines.Add(line); lineWidth = 0; buffer.Length = 0;
                    if (pending.HasValue) { buffer.Append(pending.Value); pending = null; }
                    switchLine = false;
                }
                char ch = text[i];
                if (ch == '\n') { flush(); i++; switchLine = true; continue; }
                Style opened; bool closing, icon; int length;
                if (ch == '<' && TryStyle(text, i, out opened, out closing, out icon, out length))
                {
                    // Source talkAS is null: icon tags are consumed without width or a span boundary.
                    if (!icon)
                    {
                        flush();
                        if (closing) { if (stack.Count > 1) stack.Pop(); }
                        else stack.Push(opened);
                    }
                    i += length; continue;
                }
                if (measure(buffer.ToString()) + lineWidth + chinaWidth > contentWidth)
                {
                    flush();
                    if (i != text.Length - 1) { pending = ch; switchLine = true; }
                    else { line = new Line(); lines.Add(line); lineWidth = 0; buffer.Append(ch); flush(); }
                }
                else
                {
                    buffer.Append(ch);
                    if (i == text.Length - 1) flush();
                }
                i++;
            }
            return lines.ToArray();
        }

        private static bool TryStyle(string text, int start, out Style style, out bool closing, out bool icon, out int length)
        {
            style = null; closing = icon = false; length = 0;
            int end = text.IndexOf('>', start + 1); if (end < 0) return false;
            string tag = text.Substring(start + 1, end - start - 1).ToLowerInvariant();
            if (tag == "/c") closing = true;
            else if (Regex.IsMatch(tag, @"^i=\s*[+-]?\d*\s*$")) icon = true;
            else
            {
                if (!tag.StartsWith("c=")) return false;
                string[] fields = tag.Substring(2).Split(',');
                var value = new Style(); sbyte type;
                if (fields.Length > 3 ||
                    (fields[0].Trim().Length > 0 && !int.TryParse(fields[0].Trim(), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value.foreground)) ||
                    (fields.Length > 1 && fields[1].Trim().Length > 0 && !int.TryParse(fields[1].Trim(), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value.edge))) return false;
                if (fields.Length > 2 && fields[2].Trim().Length > 0)
                { if (!sbyte.TryParse(fields[2].Trim(), out type)) return false; value.fontType = type; }
                style = value;
            }
            length = end - start + 1; return true;
        }
    }
}

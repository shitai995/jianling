using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace GuJian
{
    // Independent of Unity so the original script language can be checked in isolation.
    public sealed class OriginalScriptMachine
    {
        public Func<string, string[]> LoadScript;
        public Func<int, object[], object> Command;
        public Func<bool> IsWaiting;
        public Action<string> Trace;
        public Action<string, bool> ScriptStarted;
        public readonly Dictionary<string, int> Values = new Dictionary<string, int>();
        public bool Running => current != null || pending.Count > 0;
        public string CurrentScript => current?.name ?? "";
        public int Instruction => current?.position ?? 0;
        public string Error { get; private set; }
        private readonly Queue<PendingScript> pending = new Queue<PendingScript>();
        private readonly Random random = new Random();
        private Frame current;

        private sealed class PendingScript
        {
            public string name;
            public bool loading;
        }

        private sealed class Frame
        {
            public string name;
            public string[] code;
            public int position;
            public readonly Dictionary<string, object> locals = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        public void Enqueue(string name) => Enqueue(name, false);
        public void Enqueue(string name, bool loading) => pending.Enqueue(new PendingScript
        {
            name = name.Replace(".s", "").TrimStart('/'), loading = loading
        });
        public void Clear() { current = null; pending.Clear(); Error = null; }
        public int Get(string name) => Values.TryGetValue(name, out var value) ? value : 0;

        public void Tick(int budget = 256)
        {
            if (Error != null) return;
            try
            {
                while (budget-- > 0 && !(IsWaiting?.Invoke() ?? false))
                {
                    if (current == null || current.position >= current.code.Length)
                    {
                        if (pending.Count == 0) { current = null; return; }
                        var next = pending.Dequeue();
                        ScriptStarted?.Invoke(next.name, next.loading);
                        current = new Frame { name = next.name, code = LoadScript(next.name), position = 0 };
                        if (current.code == null) throw new InvalidOperationException("Missing script " + next.name);
                    }
                    var line = current.code[current.position++].Trim();
                    if (line.Length == 0) continue;
                    Trace?.Invoke(current.name + ":" + current.position + " " + line);
                    if (line.StartsWith("if("))
                    {
                        if (!Bool(Evaluate(line.Substring(2)))) SeekForward("if(", "endif", true);
                    }
                    else if (line == "else") SeekForward("if(", "endif", false);
                    else if (line == "endif") { }
                    else if (line.StartsWith("while("))
                    {
                        if (!Bool(Evaluate(line.Substring(5)))) SeekForward("while(", "loop", false);
                    }
                    else if (line == "loop") SeekLoop();
                    else if (line == "ret" || line.StartsWith("return") || line == "end") current = null;
                    else if (line.EndsWith(":") || Regex.IsMatch(line, @"^\w+(?:\[[^\]]+\])+$")) DeclareArray(line);
                    else
                    {
                        var assignment = Regex.Match(line, @"^(\w+)((?:\[[^\]]+\])*)\s*=(.*)$", RegexOptions.Singleline);
                        if (assignment.Success)
                        {
                            string name = assignment.Groups[1].Value;
                            object result = Evaluate(assignment.Groups[3].Value);
                            var indices = Regex.Matches(assignment.Groups[2].Value, @"\[([^\]]+)\]");
                            if (indices.Count == 0) current.locals[name] = result;
                            else
                            {
                                var array = (object[])current.locals[name];
                                for (int i = 0; i + 1 < indices.Count; i++) array = (object[])array[Int(Evaluate(indices[i].Groups[1].Value))];
                                array[Int(Evaluate(indices[indices.Count - 1].Groups[1].Value))] = result;
                            }
                        }
                        else Evaluate(line);
                    }
                }
            }
            catch (Exception ex)
            {
                Error = CurrentScript + ":" + Instruction + " " + ex.Message;
                throw new InvalidOperationException("Original script: " + Error, ex);
            }
        }

        private void SeekForward(string open, string close, bool stopAtElse)
        {
            int depth = 0;
            for (int i = current.position; i < current.code.Length; i++)
            {
                string line = current.code[i].Trim();
                if (line.StartsWith(open)) depth++;
                else if (line == close)
                {
                    if (depth-- == 0) { current.position = i + 1; return; }
                }
                else if (stopAtElse && depth == 0 && line == "else") { current.position = i + 1; return; }
            }
            throw new InvalidOperationException("Unmatched " + close);
        }

        private void SeekLoop()
        {
            int depth = 0;
            for (int i = current.position - 2; i >= 0; i--)
            {
                string line = current.code[i].Trim();
                if (line == "loop") depth++;
                else if (line.StartsWith("while("))
                    if (depth-- == 0) { current.position = i; return; }
            }
            throw new InvalidOperationException("Unmatched loop");
        }

        private void DeclareArray(string line)
        {
            string name = line.Substring(0, line.IndexOf('['));
            var dimensions = Regex.Matches(line, @"\[([^\]]+)\]");
            current.locals[name] = MakeArray(dimensions, 0);
        }

        private object[] MakeArray(MatchCollection dimensions, int index)
        {
            var array = new object[Int(Evaluate(dimensions[index].Groups[1].Value))];
            for (int i = 0; i < array.Length; i++)
                array[i] = index + 1 < dimensions.Count ? (object)MakeArray(dimensions, index + 1) : 0;
            return array;
        }

        public object Evaluate(string expression) => new Expression(this, expression).Read();
        public static int Int(object value) => value == null ? 0 : value is bool b ? (b ? 1 : 0) : Convert.ToInt32(value, CultureInfo.InvariantCulture);
        public static bool Bool(object value) => value is bool b ? b : Int(value) != 0;

        private object Call(int id, object[] args)
        {
            object A(int i) => i < args.Length ? args[i] : 0;
            switch (id)
            {
                case 0: case 1: Trace?.Invoke(Convert.ToString(A(0))); return 0;
                case 2: return Math.Min(Int(A(0)), Int(A(1)));
                case 3: return Math.Max(Int(A(0)), Int(A(1)));
                case 4: return ((object[])A(0)).Length;
                case 5: return Math.Abs(Int(A(0)));
                case 6: return random.Next(Int(A(0)), Int(A(1)));
                case 7: return Int(A(0)) % Int(A(1));
                case 8: return Convert.ToString(A(0)).Length;
                case 9: Values[Convert.ToString(A(0))] = Int(A(1)); return 0;
                case 10: return Get(Convert.ToString(A(0)));
                default: return Command(id, args);
            }
        }

        private sealed class Expression
        {
            private readonly OriginalScriptMachine vm;
            private readonly string text;
            private int pos;
            private string token;
            private bool literal;
            public Expression(OriginalScriptMachine machine, string source) { vm = machine; text = source; Next(); }
            private void Next()
            {
                while (pos < text.Length && char.IsWhiteSpace(text[pos])) pos++;
                literal = false;
                if (pos == text.Length) { token = ""; return; }
                char c = text[pos++];
                if (c == '"')
                {
                    literal = true;
                    var value = new StringBuilder();
                    bool closed = false;
                    while (pos < text.Length)
                    {
                        c = text[pos++];
                        if (c == '"') { closed = true; break; }
                        if (c == '\\' && pos < text.Length && text[pos] == '"') c = text[pos++];
                        value.Append(c);
                    }
                    if (!closed) throw new FormatException("Unterminated string");
                    token = value.ToString();
                }
                else if (char.IsLetterOrDigit(c) || c == '_')
                {
                    int start = pos - 1;
                    while (pos < text.Length && (char.IsLetterOrDigit(text[pos]) || text[pos] == '_')) pos++;
                    token = text.Substring(start, pos - start);
                }
                else
                {
                    token = c.ToString();
                    if (pos < text.Length && ((c == '<' && (text[pos] == '=' || text[pos] == '>')) || (c == '>' && text[pos] == '='))) token += text[pos++];
                }
            }
            private void Expect(string expected)
            {
                if (literal || token != expected) throw new FormatException("Expected " + expected + ", got " + token);
                Next();
            }
            public object Read()
            {
                object value = Binary(0);
                if (token.Length != 0) throw new FormatException("Unexpected token " + token);
                return value;
            }
            private static int Priority(string op)
            {
                switch (op)
                {
                    case "|": return 1;
                    case "&": return 2;
                    case "=": case "<>": case ">": case "<": case ">=": case "<=": return 3;
                    case "+": case "-": return 4;
                    case "*": case "/": return 5;
                    default: return 0;
                }
            }
            private object Binary(int minimum)
            {
                object value = Primary();
                while (!literal && Priority(token) > minimum)
                {
                    string op = token;
                    int priority = Priority(op);
                    Next();
                    object right = Binary(priority);
                    switch (op)
                    {
                        case "+": value = value is string || right is string ? (object)(Convert.ToString(value) + right) : Int(value) + Int(right); break;
                        case "-": value = Int(value) - Int(right); break;
                        case "*": value = Int(value) * Int(right); break;
                        case "/": value = Int(value) / Int(right); break;
                        case "=": value = Equals(value, right); break;
                        case "<>": value = !Equals(value, right); break;
                        case ">": value = Int(value) > Int(right); break;
                        case "<": value = Int(value) < Int(right); break;
                        case ">=": value = Int(value) >= Int(right); break;
                        case "<=": value = Int(value) <= Int(right); break;
                        case "&": value = Bool(value) & Bool(right); break;
                        case "|": value = Bool(value) | Bool(right); break;
                    }
                }
                return value;
            }
            private object Primary()
            {
                if (literal) { string value = token; Next(); return value; }
                if (token == "-" || token == "!" || token == "+")
                {
                    string op = token; Next(); object value = Primary();
                    return op == "!" ? (object)!Bool(value) : op == "-" ? -Int(value) : Int(value);
                }
                if (token == "(") { Next(); object value = Binary(0); Expect(")"); return value; }
                string name = token;
                if (name.Length == 0) throw new FormatException("Missing expression");
                Next();
                if (name == "true") return true;
                if (name == "false") return false;
                if (int.TryParse(name, out int integer)) return integer;
                if (token == "(")
                {
                    Next();
                    var args = new List<object>();
                    if (token != ")")
                    {
                        do
                        {
                            args.Add(Binary(0));
                            if (token != ",") break;
                            Next();
                        } while (true);
                    }
                    Expect(")");
                    if (!name.StartsWith("x", StringComparison.OrdinalIgnoreCase)) throw new FormatException("Unknown function " + name);
                    return vm.Call(int.Parse(name.Substring(1), NumberStyles.HexNumber), args.ToArray());
                }
                if (vm.current == null || !vm.current.locals.TryGetValue(name, out var result)) throw new FormatException("Unknown variable " + name);
                while (token == "[") { Next(); int index = Int(Binary(0)); Expect("]"); result = ((object[])result)[index]; }
                return result;
            }
        }
    }
}

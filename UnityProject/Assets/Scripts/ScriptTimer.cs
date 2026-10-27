using System;
using UnityEngine;

namespace GuJian
{
    [Serializable]
    public sealed class ScriptTimerSnapshot
    {
        public string script;
        public float seconds;
        public bool paused;
    }

    // Owns one original-script countdown and exposes expiry as a one-shot callback key.
    public sealed class ScriptTimer
    {
        public string Script { get; private set; }
        public float Seconds { get; private set; }
        public bool Paused { get; private set; }
        public bool Active => Script != null;
        public string RemainingText
        {
            get
            {
                if (!Active) return null;
                int seconds = Mathf.CeilToInt(Mathf.Max(0, Seconds));
                return "剩余时间 " + seconds / 60 + ":" + (seconds % 60).ToString("00");
            }
        }
        public void Start(float seconds, string script) { Seconds = seconds; Script = script; Paused = false; }
        public void SetSeconds(float seconds) { Seconds = seconds; }
        public void SetScript(string script) { Script = script; }
        public void Pause() { if (Active) Paused = true; }
        public void Resume() { if (Active) Paused = false; }
        public void Cancel() { Script = null; }
        public string Tick(float delta, bool blocked)
        {
            if (!Active || Paused || blocked) return null;
            Seconds -= delta;
            if (Seconds > 0) return null;
            string callback = Script; Script = null; return callback;
        }
        public ScriptTimerSnapshot Capture() => new ScriptTimerSnapshot { script = Script, seconds = Seconds, paused = Paused };
        public void Restore(ScriptTimerSnapshot state)
        {
            Script = state == null || string.IsNullOrEmpty(state.script) ? null : state.script;
            Seconds = state == null || float.IsNaN(state.seconds) || float.IsInfinity(state.seconds) ? 0 : Mathf.Max(0, state.seconds);
            Paused = state != null && state.paused;
        }
    }
}

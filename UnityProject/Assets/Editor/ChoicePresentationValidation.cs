using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GuJian.Editor
{
    public static class ChoicePresentationValidation
    {
        [MenuItem("GuJian/Native UI/Validate choice presentation")]
        public static void Run()
        {
            var checks = new List<string>(); string error = null;
            try
            {
                if (!EditorApplication.isPlaying) throw new Exception("Enter Play first");
                var game = Object.FindObjectOfType<GuJianGame>();
                if (!game) throw new Exception("Game missing");
                game.Begin(); game.Machine.Clear();
                game.Machine.Command(0x2D, new object[] { "<c=ffff00>Prompt</c>", new object[] { "one", "two", "three", "four" }, false });
                var snapshot = game.CurrentChoice;
                Require(snapshot != null && snapshot.Information.Contains("color=") && snapshot.Options.Length == 4 && !snapshot.CanCancel, "snapshot missing formatted information or choices");
                snapshot.Options[0] = "modified";
                Require(game.CurrentChoice.Options[0] == "one", "snapshot changed VM choices");
                checks.Add("Formatted choice snapshot preserves content and cannot mutate the game options");
                game.SubmitAction(GameAction.Down); game.SubmitAction(GameAction.Down); game.SubmitAction(GameAction.Down);
                Require(game.CurrentChoice.Focus == 3 && game.CurrentChoice.FirstVisible == 1, "focus or three-row scrolling changed");
                checks.Add("Choice snapshot exposes the original focus and three-row scrolling");
                Require(!game.SelectChoice(-1) && !game.SelectChoice(4) && game.DialogueVisible, "invalid row closed a choice");
                var flag = typeof(GuJianGame).GetField("loadingVisible", BindingFlags.Instance | BindingFlags.NonPublic);
                bool previous = (bool)flag.GetValue(game);
                try { flag.SetValue(game, true); Require(!game.SelectChoice(0) && game.DialogueVisible, "overlay allowed a row click"); }
                finally { flag.SetValue(game, previous); }
                checks.Add("Invalid rows and clicks underneath an overlay do not resolve the choice");
                Require(game.SelectChoice(1) && game.LastChoice == 1 && !game.DialogueVisible && game.CurrentChoice == null, "selected row did not reach the production script result");
                Require(!game.SubmitAction(GameAction.Skill), "selection leaked a same-frame gameplay action");
                checks.Add("Selecting a row uses the production result and preserves same-frame modal isolation");
                game.Machine.Command(0x2D, new object[] { "", new object[] { "only" }, true });
                Require(game.CurrentChoice.CanCancel, "closable flag missing");
                game.SubmitAction(GameAction.Back);
                Require(game.LastChoice == -1 && game.CurrentChoice == null, "back did not cancel closable choice");
                checks.Add("Closable choices retain the production Back behavior");
            }
            catch (Exception e) { error = e.ToString(); }
            string dir = Path.GetFullPath("../Reference/Validation/2026-10-31-m2"); Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir, "choice-presentation.json"), JsonUtility.ToJson(new ReconstructionValidation.Report {
                passed = error == null, utc = DateTime.UtcNow.ToString("O"), checks = checks.ToArray(), error = error }, true));
            if (error == null) Debug.Log("GUJIAN_CHOICE_PRESENTATION_PASSED " + checks.Count);
            else Debug.LogError("GUJIAN_CHOICE_PRESENTATION_FAILED " + error);
        }
        private static void Require(bool ok, string message) { if (!ok) throw new Exception(message); }
    }
}

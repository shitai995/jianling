using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace GuJian.Editor
{
    public static class RemoveSpiritValidation
    {
        private const BindingFlags F = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        [MenuItem("古剑/验证原版x72移除剑灵")]
        public static void Run()
        {
            if (!EditorApplication.isPlaying) throw new InvalidOperationException("Enter Play mode first");
            var game = UnityEngine.Object.FindObjectOfType<GuJianGame>();
            if (!game) throw new InvalidOperationException("GuJianGame is missing");
            float previousScale = Time.timeScale;
            string previousSave = game.saveFileName;
            var checks = new List<string>();
            string error = null;
            try
            {
                Time.timeScale = 0;
                game.saveFileName = "validation-remove-spirit.json";
                game.Begin();
                game.Machine.Clear();
                game.Machine.Values["event"] = -999;
                game.Machine.Enqueue("200");
                game.Machine.Tick();
                if (game.Map == null || game.Machine.Running) throw new Exception("Stage setup failed");

                game.Machine.Command(0x64, new object[] { 12 });
                var spirit = game.Actors[-2];
                var movementField = typeof(GuJianGame).GetField("movements", F);
                var movementList = (System.Collections.IList)movementField.GetValue(game);
                var moveType = movementList.GetType().GetGenericArguments()[0];
                var move = Activator.CreateInstance(moveType);
                moveType.GetField("actor", F).SetValue(move, spirit);
                movementList.Add(move);

                game.Machine.Command(0x72, Array.Empty<object>());
                if (game.Actors.ContainsKey(-2)) throw new Exception("x72 kept the spirit in the actor registry");
                if (((System.Collections.IDictionary)typeof(GuJianGame).GetField("fighters", F).GetValue(game)).Contains(-2))
                    throw new Exception("x72 kept the spirit combat registration");
                if (movementList.Cast<object>().Any(item => moveType.GetField("actor", F).GetValue(item) as OriginalActor == spirit))
                    throw new Exception("x72 kept a movement job for the removed spirit");
                if (!spirit.gameObject.activeSelf) throw new Exception("x72 unexpectedly disabled the spirit before deferred destruction");
                checks.Add("x72 removes the spirit actor, unregisters combat callbacks, cancels its scripted movement, and schedules the GameObject for Unity deferred destruction");
            }
            catch (Exception e) { error = e.ToString(); }
            finally
            {
                game.Begin();
                game.Machine.Clear();
                typeof(GuJianGame).GetProperty("AtTitle").SetValue(game, true);
                game.saveFileName = previousSave;
                Time.timeScale = previousScale;
                File.WriteAllText("../Reference/Validation/remove-spirit.json", JsonUtility.ToJson(
                    new ReconstructionValidation.Report
                    {
                        passed = error == null,
                        utc = DateTime.UtcNow.ToString("O"),
                        checks = checks.ToArray(),
                        error = error
                    }, true));
                Debug.Log("GUJIAN_REMOVE_SPIRIT_VALIDATION " + (error == null) + " " + error);
            }
            if (error != null) throw new Exception("x72 validation failed: " + error);
        }
    }
}

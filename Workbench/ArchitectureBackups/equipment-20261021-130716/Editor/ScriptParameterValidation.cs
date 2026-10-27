using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GuJian.Editor
{
    public static class ScriptParameterValidation
    {
        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }

        [MenuItem("古剑/验证脚本参数边界")]
        public static void Run()
        {
            Require(EditorApplication.isPlaying, "Play first");
            var game = UnityEngine.Object.FindObjectOfType<GuJianGame>();
            Require(game, "GuJian scene required");
            var checks = new List<string>();
            string error = null;
            float oldScale = Time.timeScale;
            try
            {
                game.Begin();
                game.Machine.Clear();

                const int definition = 201;
                int[] ids = { 910, 911, 912, 913, 914 };
                int[] requested = { 0, 9, 10, 150, 151 };
                foreach (int id in ids)
                {
                    if (game.Actors.ContainsKey(id)) game.Machine.Command(0x31, new object[] { id });
                    game.Machine.Command(0x30, new object[] { id, definition, "l", 300, 250, 2 });
                }

                foreach (int value in requested)
                {
                    game.Machine.Command(0x3E, new object[] { definition, value });
                    int expected = Mathf.Min(value, 150);
                    foreach (int id in ids)
                        Require(game.Actors[id].level == expected,
                            "x3E level mismatch at " + value + " for actor " + id + ": " + game.Actors[id].level);
                }
                checks.Add("x3E preserves requested enemy levels 0, 9, 10 and 150, and caps only 151 to 150 per the original script host");

                game.Machine.Command(0x3E, new object[] { definition, 7 });
                foreach (int id in ids)
                    Require(game.Actors[id].level == 7, "x3E did not update every matching definition");
                checks.Add("x3E updates every live actor with the matching definition ID");
            }
            catch (Exception ex)
            {
                error = ex.ToString();
            }
            finally
            {
                Time.timeScale = oldScale;
                var report = new ReconstructionValidation.Report
                {
                    passed = error == null,
                    utc = DateTime.UtcNow.ToString("O"),
                    checks = checks.ToArray(),
                    error = error
                };
                Directory.CreateDirectory("../Reference/Validation");
                File.WriteAllText("../Reference/Validation/script-parameter.json", JsonUtility.ToJson(report, true));
                Debug.Log("GUJIAN_SCRIPT_PARAMETER_VALIDATION " + report.passed + " " + error);
            }
        }
    }
}

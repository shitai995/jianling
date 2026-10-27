using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace GuJian.Editor
{
    public static class MaximumPowerValidation
    {
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        private static GuJianGame game;
        private static string previousSave;
        private static float previousScale;

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new Exception(message);
        }

        private static void SetEnergy(int value) => ValidationState.Set(game, "energy", value);

        [MenuItem("古剑/验证原版必杀怒气状态")]
        public static void Run()
        {
            if (!EditorApplication.isPlaying) throw new InvalidOperationException("Enter Play mode first");
            game = UnityEngine.Object.FindObjectOfType<GuJianGame>();
            if (!game) throw new InvalidOperationException("GuJianGame is missing");
            previousSave = game.saveFileName;
            previousScale = Time.timeScale;
            bool passed = false;
            string error = null;
            try
            {
                Time.timeScale = 0;
                game.saveFileName = "validation-maximum-power.json";
                game.Begin();
                game.Machine.Clear();
                game.Machine.Values["event"] = -999;
                game.Machine.Enqueue("200");
                game.Machine.Tick();

                game.Machine.Command(0x7D, new object[] { true });
                Require(game.Energy == 1000 && (bool)typeof(GuJianGame).GetField("maximumPower", Private).GetValue(game),
                    "x7D(true) must enable maximum power and set energy to 1000");
                SetEnergy(2000);
                Require(game.TryCriticalKill() && game.Energy == 2000, "Maximum-power critical kill consumed energy");

                game.Begin();
                game.Machine.Clear();
                game.Machine.Values["event"] = -999;
                game.Machine.Enqueue("200");
                game.Machine.Tick();
                game.Machine.Command(0x7D, new object[] { false });
                Require(game.Energy == 1000 && !(bool)typeof(GuJianGame).GetField("maximumPower", Private).GetValue(game),
                    "x7D(false) must disable maximum power and still set energy to 1000");
                SetEnergy(2000);
                Require(game.TryCriticalKill() && game.Energy == 0, "Normal critical kill must consume energy");

                game.Begin();
                Require(!(bool)typeof(GuJianGame).GetField("maximumPower", Private).GetValue(game), "Stage reset retained maximum power");
                passed = true;
            }
            catch (Exception e) { error = e.ToString(); }
            finally
            {
                game.Begin();
                game.Machine.Clear();
                typeof(GuJianGame).GetProperty("AtTitle").SetValue(game, true);
                game.saveFileName = previousSave;
                Time.timeScale = previousScale;
                File.WriteAllText("../Reference/Validation/maximum-power.json", JsonUtility.ToJson(
                    new ReconstructionValidation.Report
                    {
                        passed = passed,
                        utc = DateTime.UtcNow.ToString("O"),
                        checks = new[]
                        {
                            "x7D true/false both set energy to 1000 while selecting the original maximum-power flag",
                            "Critical kill preserves energy with maximum-power enabled and consumes it when disabled",
                            "Stage transition resets maximum-power state"
                        },
                        error = error
                    }, true));
                Debug.Log("GUJIAN_MAXIMUM_POWER_VALIDATION " + passed + " " + error);
                game = null;
            }
            if (!passed) throw new Exception("Maximum-power validation failed: " + error);
        }
    }
}

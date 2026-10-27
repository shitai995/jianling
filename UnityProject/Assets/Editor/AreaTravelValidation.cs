using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace GuJian.Editor
{
    public static class AreaTravelValidation
    {
        private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
        private static object Get(GuJianGame g, string name) => typeof(GuJianGame).GetField(name, Flags).GetValue(g);
        private static void Require(bool ok, string why) { if (!ok) throw new Exception(why); }
        private static void Settle(GuJianGame g)
        {
            for (int i = 0; i < 100 && g.Machine.Running; i++)
            {
                if (g.DialogueVisible) g.ContinueDialogue();
                g.Machine.Tick(4096);
            }
            Require(!g.Machine.Running && g.LastError == null, "Scene fixture did not settle");
        }
        [MenuItem("古剑/验证区域开放与地图旅行")]
        public static void Run()
        {
            Require(EditorApplication.isPlaying, "Enter Play mode first");
            var g = UnityEngine.Object.FindObjectOfType<GuJianGame>();
            var checks = new List<string>(); string error = null, previousSave = g.saveFileName;
            var previousWait = g.Machine.IsWaiting; float scale = Time.timeScale;
            Time.timeScale = 0; g.saveFileName = "validation-area-travel.json";
            try
            {
                g.Begin(); g.Machine.Clear(); g.Machine.Values["event"] = 9999;
                g.Machine.IsWaiting = () => false;
                g.Machine.Enqueue("200"); Settle(g);
                var open = (bool[])Get(g, "openAreas");
                Require(open[10] && !open[0] && !open[9], "Scene x96 failed to record visited area");
                int stage = g.Machine.Get("stageID");
                Require(!g.TravelToArea(-1) && !g.TravelToArea(13) && !g.TravelToArea(9) && !g.Machine.Running && g.Machine.Get("stageID") == stage, "Closed/invalid destination changed scene or VM");
                checks.Add("Actual stage 200 x96 opens Jianzhong; invalid and unvisited destinations reject without changing stage or queue");

                g.Machine.Command(0x48, new object[] { 1 });
                Require(g.Machine.Get("stageID") == 1 && open[1] && (bool)Get(g, "menu") && (bool)Get(g, "worldMapOpen") && previousWait(), "x48 missing world-map entry marker or menu wait");
                Require(g.TravelToArea(1), "Opened city rejected"); Settle(g);
                Require(g.Machine.Get("stageID") == 334 && g.Actors[-1].mapPosition == new Vector2(61, 250) && !(bool)Get(g, "menu") && !(bool)Get(g, "worldMapOpen"), "City world-map entry used stale previous-stage position");
                Require(open[11], "Late city x96 did not record variant");
                checks.Add("x48 enters mandatory world-map choice; shared UI travel leaves map state and loads original city entry coordinates from stageID 1, retaining late-city area index");

                Require(g.TravelToArea(10), "Visited Jianzhong no longer selectable"); Settle(g);
                Require(g.Machine.Get("stageID") == 200 && open[1] && open[10] && !open[9], "Return travel lost visited regions");
                g.Save(); g.Load(); Settle(g);
                open = (bool[])Get(g, "openAreas");
                Require(open[1] && open[10] && open[11] && !open[9] && g.Machine.Get("stageID") == 200, "Area persistence mismatch");
                checks.Add("City -> visited Jianzhong return and real Save/Load retain visited regions and closed Liejian");

                string path = Path.Combine(Application.persistentDataPath, g.saveFileName);
                var saveType = typeof(GuJianGame).GetNestedType("SaveData", BindingFlags.NonPublic);
                var data = JsonUtility.FromJson(File.ReadAllText(path), saveType);
                saveType.GetField("openAreas").SetValue(data, new bool[13]);
                File.WriteAllText(path, JsonUtility.ToJson(data)); g.Load(); Settle(g);
                open = (bool[])Get(g, "openAreas");
                Require(open[10] && !open[1] && !open[9] && g.TravelToArea(10), "Old save lost its currently visited region"); Settle(g);
                checks.Add("Historical v3 save with current area flag absent repairs only current area; unrelated regions remain closed");

                g.Begin(); g.Machine.Clear(); open = (bool[])Get(g, "openAreas");
                Require(Array.TrueForAll(open, x => !x) && !g.TravelToArea(10), "New game retained previous exploration");
                checks.Add("New game clears exploration and rejects travel to previously visited areas until opened again");
            }
            catch (Exception ex) { error = ex.ToString(); }
            finally
            {
                g.Machine.IsWaiting = previousWait; g.saveFileName = previousSave; Time.timeScale = scale;
                File.WriteAllText("../Reference/Validation/area-travel.json", JsonUtility.ToJson(new ReconstructionValidation.Report { passed = error == null, utc = DateTime.UtcNow.ToString("O"), checks = checks.ToArray(), error = error }, true));
                Debug.Log("GUJIAN_AREA_TRAVEL " + (error == null) + " " + error);
            }
        }
    }
}

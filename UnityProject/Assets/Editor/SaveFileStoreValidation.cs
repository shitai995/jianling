using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace GuJian.Editor
{
    public static class SaveFileStoreValidation
    {
        private static void Require(bool value, string reason)
        {
            if (!value) throw new Exception(reason);
        }

        [MenuItem("古剑/验证原子存档写入")]
        public static void Run()
        {
            var checks = new List<string>();
            string error = null;
            // All fixture files are known, under a fresh directory; no player saves are used.
            string root = Path.Combine(Path.GetTempPath(), "gujian-save-" + Guid.NewGuid().ToString("N"));
            string folder = Path.Combine(root, "records");
            string path = Path.Combine(folder, "record.json");
            string first = "{\"version\":3,\"recordName\":\"剑冢\",\"money\":12345}";
            string second = "{\"version\":3,\"recordName\":\"无双城\",\"money\":6789}";
            string third = "{\"version\":3,\"recordName\":\"清溪村\",\"money\":200}";
            try
            {
                SaveFileStore.Write(path, first);
                Require(File.ReadAllBytes(path).SequenceEqual(Encoding.UTF8.GetBytes(first)), "Initial UTF-8 bytes changed");
                Require(!File.Exists(path + ".tmp") && !File.Exists(path + ".bak"), "Initial commit left transaction files");
                checks.Add("First write creates missing save directories and preserves exact Unicode JSON bytes without a BOM");

                SaveFileStore.Write(path, second);
                Require(File.ReadAllText(path) == second && File.ReadAllText(path + ".bak") == first &&
                    !File.Exists(path + ".tmp"), "Replacement did not preserve previous snapshot");
                checks.Add("Replacing a save commits the new snapshot and retains the previous complete snapshot in .bak");

                SaveFileStore.Write(path, third);
                Require(File.ReadAllText(path) == third && File.ReadAllText(path + ".bak") == second,
                    "Backup did not roll forward");
                checks.Add("Repeated commits retain the immediately preceding snapshot, including when .bak already exists");

                File.WriteAllText(path + ".tmp", "interrupted write");
                SaveFileStore.Write(path, first);
                Require(File.ReadAllText(path) == first && File.ReadAllText(path + ".bak") == third &&
                    !File.Exists(path + ".tmp"), "Interrupted temporary file was used as a save");
                checks.Add("A stale temporary file is replaced by a fresh transaction and is never loaded as a save");

                Directory.CreateDirectory(path + ".tmp");
                bool rejected = false;
                try { SaveFileStore.Write(path, second); }
                catch (IOException) { rejected = true; }
                catch (UnauthorizedAccessException) { rejected = true; }
                finally { Directory.Delete(path + ".tmp"); }
                Require(rejected && File.ReadAllText(path) == first && File.ReadAllText(path + ".bak") == third,
                    "Failed staging changed the committed save or backup");
                checks.Add("A staging-path failure preserves both committed files and propagates the storage error");

                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    rejected = false;
                    using (var locked = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None))
                    {
                        try { SaveFileStore.Write(path, second); }
                        catch (IOException) { rejected = true; }
                    }
                    Require(rejected && File.ReadAllText(path) == first && File.ReadAllText(path + ".bak") == third &&
                        !File.Exists(path + ".tmp"), "Failed commit changed the save, backup, or left a temporary file");
                    checks.Add("Windows replacement against a locked save fails safely, retains both snapshots, and cleans the staged file");
                }

                rejected = false;
                try { SaveFileStore.Write(path, null); }
                catch (ArgumentNullException) { rejected = true; }
                Require(rejected && File.ReadAllText(path) == first, "Null payload damaged existing save");
                checks.Add("A null snapshot is rejected before touching an existing save");

                rejected = false;
                try { SaveFileStore.Write(" ", first); }
                catch (ArgumentException) { rejected = true; }
                Require(rejected && File.ReadAllText(path + ".bak") == third, "Empty path was accepted");
                checks.Add("An empty save path is rejected before file operations");
            }
            catch (Exception exception) { error = exception.ToString(); }
            finally
            {
                try
                {
                    foreach (string file in new[] { path, path + ".bak", path + ".tmp" })
                        if (File.Exists(file)) File.Delete(file);
                    if (Directory.Exists(path + ".tmp")) Directory.Delete(path + ".tmp");
                    if (Directory.Exists(folder)) Directory.Delete(folder);
                    if (Directory.Exists(root)) Directory.Delete(root);
                }
                catch (Exception exception) { error = (error ?? "") + "\nFixture cleanup: " + exception; }
            }
            var report = new ReconstructionValidation.Report
            {
                passed = error == null, utc = DateTime.UtcNow.ToString("O"), checks = checks.ToArray(), error = error
            };
            Directory.CreateDirectory("../Reference/Validation");
            File.WriteAllText("../Reference/Validation/save-file-store.json", JsonUtility.ToJson(report, true));
            Debug.Log("GUJIAN_SAVE_FILE_STORE " + report.passed + " " + error);
        }
    }
}

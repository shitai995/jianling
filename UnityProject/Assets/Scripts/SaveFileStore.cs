using System;
using System.IO;
using System.Text;

namespace GuJian
{
    /// <summary>
    /// Synchronous, single-writer save commits. JSON format and restore order belong to callers.
    /// A replacement keeps the previous file in .bak; a failed write leaves the live file intact.
    /// Replacement errors propagate: there is deliberately no delete-then-move fallback.
    /// </summary>
    public static class SaveFileStore
    {
        public static void Write(string path, string contents)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Save path is required", nameof(path));
            if (contents == null) throw new ArgumentNullException(nameof(contents));

            string destination = Path.GetFullPath(path);
            Directory.CreateDirectory(Path.GetDirectoryName(destination));
            string temporary = destination + ".tmp";
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(contents);
                using (var stream = new FileStream(temporary, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush(true);
                }
                if (File.Exists(destination)) File.Replace(temporary, destination, destination + ".bak");
                else File.Move(temporary, destination);
            }
            catch
            {
                // Cleanup must never hide the original storage error or touch the live save.
                try { File.Delete(temporary); }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }
                throw;
            }
        }
    }
}

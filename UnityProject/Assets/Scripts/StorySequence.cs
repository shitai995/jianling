using System;
using UnityEngine;

namespace GuJian
{
    [CreateAssetMenu(menuName = "古剑/剧情序列", fileName = "NewStory")]
    public sealed class StorySequence : ScriptableObject
    {
        [SerializeField] private string scriptId;
        [SerializeField] private string displayName;
        [SerializeField, TextArea(2, 5)] private string notes;
        [SerializeField] private string[] instructions = new string[0];
        public string ScriptId => scriptId;
        public string DisplayName => string.IsNullOrEmpty(displayName) ? scriptId : displayName;
        public int InstructionCount => instructions == null ? 0 : instructions.Length;
        // The VM owns its execution frame. Callers cannot accidentally edit the saved source array.
        public string[] ReadInstructions() => instructions == null ? new string[0] : (string[])instructions.Clone();
        public static string NormalizeId(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("剧情标识不能为空", nameof(name));
            string id = name.Trim().TrimStart('/');
            if (id.EndsWith(".json", StringComparison.OrdinalIgnoreCase)) id = id.Substring(0, id.Length - 5);
            else if (id.EndsWith(".s", StringComparison.OrdinalIgnoreCase)) id = id.Substring(0, id.Length - 2);
            if (id.Length == 0 || id.Contains("/") || id.Contains("\\") || id == "." || id == "..")
                throw new ArgumentException("剧情标识应为不含路径的文件名", nameof(name));
            return id;
        }
#if UNITY_EDITOR
        public void ReplaceInstruction(int index, string expected, string replacement)
        {
            if (Application.isPlaying) throw new InvalidOperationException("退出播放模式后编辑剧情资产");
            if (instructions == null || index < 0 || index >= instructions.Length || instructions[index] != expected)
                throw new InvalidOperationException("剧情步骤已改变，请重新扫描地图事件后编辑");
            instructions[index] = replacement;
        }
        public void Import(string id, string title, string[] source)
        {
            if (Application.isPlaying) throw new InvalidOperationException("退出播放模式后编辑剧情资产");
            scriptId = NormalizeId(id); displayName = title;
            instructions = source == null ? new string[0] : (string[])source.Clone();
        }
#endif
    }
}

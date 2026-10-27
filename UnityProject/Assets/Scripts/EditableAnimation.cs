using System;
using UnityEngine;

namespace GuJian
{
    // The draft is editable; gameplay consumes only a validated, published snapshot.
    public sealed class EditableAnimation : ScriptableObject
    {
        public OriginalAnimation draft;
        [SerializeField, HideInInspector] private OriginalAnimation published;
        public OriginalAnimation ReadPublished()
        {
            if (published == null) throw new InvalidOperationException("动作尚未发布：" + name);
            return Copy(published);
        }
        public static OriginalAnimation Copy(OriginalAnimation source) =>
            source == null ? null : JsonUtility.FromJson<OriginalAnimation>(JsonUtility.ToJson(source));
#if UNITY_EDITOR
        public void Publish() { published = Copy(draft); }
#endif
    }
}

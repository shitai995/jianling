using UnityEngine;

namespace GuJian.Editor
{
    // Derived authoring index; gameplay continues to execute StorySequence instructions.
    public sealed class MapEventCatalog : ScriptableObject
    {
        public int formatVersion = 1;
        public MapEventStage[] stages;
    }
}

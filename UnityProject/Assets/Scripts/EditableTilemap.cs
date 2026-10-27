using UnityEngine;
using UnityEngine.Tilemaps;

namespace GuJian
{
    // The editor writes these Tilemaps and bakes collision into the existing map catalog.
    // Loading a map only instantiates the saved prefab; it never creates tiles or renderers.
    public sealed class EditableTilemap : MonoBehaviour
    {
        public string mapId;
        public int columns = 32, rows = 24, cellWidth = 32, cellHeight = 16;
        public Tilemap background, decoration, foreground, collision;
    }
}

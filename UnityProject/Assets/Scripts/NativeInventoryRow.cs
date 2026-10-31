using UnityEngine;
using UnityEngine.EventSystems;
namespace GuJian
{
    public sealed class NativeInventoryRow : MonoBehaviour, IPointerClickHandler
    {
        internal NativeInventoryView owner;
        internal int index;
        public void OnPointerClick(PointerEventData e) { if (e.button == PointerEventData.InputButton.Left && owner) owner.Select(index); }
    }
}

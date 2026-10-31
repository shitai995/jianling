using UnityEngine;
using UnityEngine.EventSystems;
namespace GuJian
{
    public sealed class NativeEquipmentRow : MonoBehaviour, IPointerClickHandler
    {
        internal NativeEquipmentView owner; internal int index;
        public void OnPointerClick(PointerEventData e) { if (e.button == PointerEventData.InputButton.Left && owner) owner.Select(index); }
    }
}

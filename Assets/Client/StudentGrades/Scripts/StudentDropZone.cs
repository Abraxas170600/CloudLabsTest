using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Client.StudentGrades.Scripts
{
    public sealed class StudentDropZone : MonoBehaviour,
        IDropHandler,
        IPointerEnterHandler,
        IPointerExitHandler
    {
        [SerializeField] private Classification classification;
        [SerializeField] private RectTransform content;

        [Tooltip(
            "Opcional. Último hijo del Content, " +
            "con LayoutElement y sin raycasts.")]
        [SerializeField] private GameObject dropHint;

        public Classification Classification => classification;

        public RectTransform Content => content;

        private void OnEnable()
        {
            SetHint(false);
        }

        private void OnDisable()
        {
            SetHint(false);
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (content == null ||
                eventData.pointerDrag == null)
            {
                return;
            }

            StudentDragCard card =
                eventData.pointerDrag.GetComponent<StudentDragCard>();

            if (card != null && card.TryDrop(this, eventData))
            {
                SetHint(false);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (eventData.pointerDrag == null)
            {
                return;
            }

            StudentDragCard card =
                eventData.pointerDrag.GetComponent<StudentDragCard>();

            if (card != null && card.IsDragging)
            {
                SetHint(true);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            SetHint(false);
        }

        public void SetHint(bool visible)
        {
            if (dropHint == null)
            {
                return;
            }

            if (visible)
            {
                dropHint.transform.SetAsLastSibling();
            }

            dropHint.SetActive(visible);
        }
    }
}
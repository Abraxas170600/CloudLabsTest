using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Client.StudentGrades.Scripts
{
    [RequireComponent(
        typeof(RectTransform),
        typeof(CanvasGroup))]
    public sealed class StudentDragCard : MonoBehaviour,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler
    {
        [SerializeField] private TMP_Text studentText;
        [SerializeField] private TMP_Text gradeText;

        [SerializeField] private StudentAvatarView avatar;

        [SerializeField] private GameObject dragDecoration;

        [SerializeField]
        private Vector2 dragSize =
            new Vector2(244f, 80f);

        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        private RectTransform dragLayer;

        private Transform originalParent;
        private Vector2 originalSize;
        private int originalSiblingIndex;

        private int activePointerId;
        private Vector2 pointerOffset;

        private Func<bool> canBeginDrag;
        private bool acceptedDrop;

        public Student Data { get; private set; }

        public Classification Selection { get; private set; }

        public bool IsDragging { get; private set; }

        public event Action<StudentDragCard> Dropped;
        public event Action<bool> DragStateChanged;

        private void Awake()
        {
            CacheComponents();
        }

        public void Bind(
            Student student,
            RectTransform layer,
            int visualIndex = 0,
            Func<bool> dragPermission = null,
            Classification initialClassification = Classification.Unassigned)
        {
            CacheComponents();

            Data = student;
            dragLayer = layer;
            canBeginDrag = dragPermission;

            Selection = initialClassification;

            studentText.richText = false;
            gradeText.richText = false;

            studentText.text = student.FullName;

            gradeText.text =
                $"Nota: {GradeRules.FormatGrade(student.FinalGrade)}";

            avatar.Bind(student, visualIndex);

            dragDecoration.SetActive(false);

            RefreshFeedback();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (IsDragging ||
                Data == null ||
                dragLayer == null ||
                eventData.button != PointerEventData.InputButton.Left ||
                (canBeginDrag != null && !canBeginDrag()))
            {
                return;
            }

            IsDragging = true;
            acceptedDrop = false;
            activePointerId = eventData.pointerId;

            originalParent = transform.parent;
            originalSiblingIndex = transform.GetSiblingIndex();
            originalSize = rectTransform.rect.size;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 grabPoint);

            Vector2 normalizedGrab = new Vector2(
                grabPoint.x / Mathf.Max(1f, originalSize.x),
                grabPoint.y / Mathf.Max(1f, originalSize.y));

            rectTransform.SetParent(dragLayer, false);

            rectTransform.anchorMin =
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);

            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = dragSize;

            rectTransform.localScale = Vector3.one;
            rectTransform.localRotation = Quaternion.identity;

            rectTransform.SetAsLastSibling();

            pointerOffset =
                -Vector2.Scale(normalizedGrab, dragSize);

            Move(eventData);

            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 1f;

            dragDecoration.SetActive(true);

            DragStateChanged?.Invoke(true);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (IsDragging &&
                !acceptedDrop &&
                eventData.pointerId == activePointerId)
            {
                Move(eventData);
            }
        }

        public bool TryDrop(
            StudentDropZone zone,
            PointerEventData eventData)
        {
            if (!IsDragging ||
                acceptedDrop ||
                eventData.pointerId != activePointerId ||
                zone == null ||
                zone.Content == null)
            {
                return false;
            }

            acceptedDrop = true;
            Selection = zone.Classification;

            AttachTo(zone.Content);
            rectTransform.SetAsLastSibling();

            RefreshFeedback();

            Dropped?.Invoke(this);

            return true;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (IsDragging &&
                eventData.pointerId == activePointerId)
            {
                CancelOrFinishDrag();
            }
        }

        private void OnDisable()
        {
            CancelOrFinishDrag();
        }

        private void OnApplicationFocus(bool focused)
        {
            if (!focused)
            {
                CancelOrFinishDrag();
            }
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused)
            {
                CancelOrFinishDrag();
            }
        }

        public void CancelOrFinishDrag()
        {
            if (!IsDragging)
            {
                return;
            }

            IsDragging = false;

            if (!acceptedDrop && originalParent != null)
            {
                AttachTo(originalParent);

                rectTransform.SetSiblingIndex(
                    Mathf.Clamp(
                        originalSiblingIndex,
                        0,
                        originalParent.childCount - 1));
            }

            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1f;

            dragDecoration.SetActive(false);

            RefreshFeedback();

            originalParent = null;

            DragStateChanged?.Invoke(false);
        }

        private void Move(PointerEventData eventData)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                dragLayer,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 pointer))
            {
                rectTransform.anchoredPosition =
                    pointer + pointerOffset;
            }
        }

        private void AttachTo(Transform parent)
        {
            rectTransform.SetParent(parent, false);

            rectTransform.localScale = Vector3.one;
            rectTransform.localRotation = Quaternion.identity;
            rectTransform.sizeDelta = originalSize;
        }

        private void RefreshFeedback()
        {
            bool assigned =
                Selection != Classification.Unassigned;

            bool correct =
                assigned && GradeRules.IsCorrect(Data, Selection);
        }

        private void CacheComponents()
        {
            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
            }

            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }
        }
    }
}
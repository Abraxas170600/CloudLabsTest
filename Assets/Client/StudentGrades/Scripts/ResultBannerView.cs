using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Client.StudentGrades.Scripts
{
    public enum BannerKind
    {
        Information,
        Success,
        Error
    }

    [RequireComponent(
        typeof(CanvasGroup),
        typeof(LayoutElement))]
    public sealed class ResultBannerView : MonoBehaviour
    {
        [SerializeField] private Image background;
        [SerializeField] private Image statusIcon;

        [SerializeField] private TMP_Text messageText;
        [SerializeField] private ScrollRect messageScroll;

        [SerializeField] private Sprite successIcon;
        [SerializeField] private Sprite errorIcon;
        [SerializeField] private Sprite informationIcon;

        [SerializeField] private Sprite successBackground;
        [SerializeField] private Sprite errorBackground;
        [SerializeField] private Sprite informationBackground;

        private CanvasGroup group;
        private LayoutElement layout;

        public void Show(string message, BannerKind kind)
        {
            CacheComponents();

            messageText.richText = false;
            messageText.text = message;

            group.alpha = 1f;
            group.blocksRaycasts = true;
            group.interactable = true;

            layout.minHeight = 56f;

            layout.preferredHeight = message.Contains("\n")
                ? 120f
                : 56f;

            layout.flexibleHeight = 0f;

            switch (kind)
            {
                case BannerKind.Success:
                    Apply(
                        successBackground,
                        successIcon,
                        new Color32(224, 245, 232, 255),
                        new Color32(16, 99, 57, 255));
                    break;

                case BannerKind.Error:
                    Apply(
                        errorBackground,
                        errorIcon,
                        new Color32(252, 230, 233, 255),
                        new Color32(155, 35, 46, 255));
                    break;

                default:
                    Apply(
                        informationBackground,
                        informationIcon,
                        new Color32(224, 239, 252, 255),
                        new Color32(27, 60, 110, 255));
                    break;
            }

           messageScroll.StopMovement();

            Vector2 position =
                messageScroll.content.anchoredPosition;

            messageScroll.content.anchoredPosition =
                new Vector2(position.x, 0f);
        }

        public void Hide(bool collapse = false)
        {
            CacheComponents();

            group.alpha = 0f;
            group.blocksRaycasts = false;
            group.interactable = false;

            messageText.text = "";

            layout.minHeight = collapse ? 0f : 56f;
            layout.preferredHeight = layout.minHeight;
            layout.flexibleHeight = 0f;
        }

        private void Apply(
            Sprite panel,
            Sprite icon,
            Color tint,
            Color textColor)
        {
            background.sprite = panel;

            background.color = panel != null
                ? Color.white
                : tint;

            statusIcon.sprite = icon;
            statusIcon.enabled = icon != null;
            statusIcon.preserveAspect = true;

            messageText.color = textColor;
        }

        private void CacheComponents()
        {
            if (group == null)
            {
                group = GetComponent<CanvasGroup>();
            }

            if (layout == null)
            {
                layout = GetComponent<LayoutElement>();
            }
        }
    }
}
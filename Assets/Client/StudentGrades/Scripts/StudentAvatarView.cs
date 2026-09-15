using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Client.StudentGrades.Scripts
{
    public sealed class StudentAvatarView : MonoBehaviour
    {
        [SerializeField] private Image background;
        [SerializeField] private TMP_Text initialsText;

        [SerializeField]
        private Color[] palette =
        {
            new Color32(84, 150, 208, 255),
            new Color32(39, 152, 94, 255),
            new Color32(206, 68, 74, 255),
            new Color32(168, 130, 210, 255),
            new Color32(222, 160, 60, 255),
            new Color32(70, 170, 190, 255)
        };

        public void Bind(Student student, int visualIndex)
        {
            background.color =
                palette != null && palette.Length > 0
                    ? palette[Mathf.Abs(visualIndex % palette.Length)]
                    : new Color32(84, 150, 208, 255);

            initialsText.richText = false;

            initialsText.text =
                Initial(student.FirstName) +
                Initial(student.LastName);
        }

        private static string Initial(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "?";
            }

            return StringInfo
                .GetNextTextElement(value.Trim())
                .ToUpperInvariant();
        }
    }
}
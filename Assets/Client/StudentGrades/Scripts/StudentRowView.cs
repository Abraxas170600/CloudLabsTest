using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Assets.Client.StudentGrades.Scripts
{
    public sealed class StudentRowView : MonoBehaviour
    {
        [FormerlySerializedAs("firstNameText")]
        [SerializeField] private TMP_Text fullNameText;

        [SerializeField] private TMP_Text roleText;
        [SerializeField] private TMP_Text codeText;
        [SerializeField] private TMP_Text emailText;
        [SerializeField] private TMP_Text gradeText;

        [SerializeField] private Image gradeBackground;
        [SerializeField] private StudentAvatarView avatar;

        [SerializeField] private Toggle approvedToggle;
        [SerializeField] private TMP_Text selectionText;

        public Student Data { get; private set; }

        public Classification Selection => approvedToggle.isOn
            ? Classification.Approved
            : Classification.Failed;

        public event Action SelectionChanged;

        private void OnEnable()
        {
            approvedToggle.onValueChanged.AddListener(
                OnToggleChanged);
        }

        private void OnDisable()
        {
            approvedToggle.onValueChanged.RemoveListener(
                OnToggleChanged);
        }

        public void Bind(Student student, int visualIndex = 0)
        {
            Data = student;

            Write(fullNameText, student.FullName);
            Write(roleText, "Estudiante");
            Write(codeText, student.Code);
            Write(emailText, student.Email);

            Write(
                gradeText,
                GradeRules.FormatGrade(student.FinalGrade));

            avatar.Bind(student, visualIndex);

            gradeBackground.color =
                GradeRules.Expected(student) == Classification.Approved
                    ? new Color32(39, 152, 94, 255)
                    : new Color32(206, 68, 74, 255);

            approvedToggle.SetIsOnWithoutNotify(false);

            RefreshSelection();
        }

        private void OnToggleChanged(bool value)
        {
            RefreshSelection();
            SelectionChanged?.Invoke();
        }

        private void RefreshSelection()
        {
            Write(
                selectionText,
                GradeRules.Label(Selection));

            bool approved =
                Selection == Classification.Approved;

            selectionText.color = approved
                ? new Color32(15, 143, 72, 255)
                : new Color32(203, 43, 58, 255);
        }

        private static void Write(TMP_Text field, string value)
        {
            field.richText = false;
            field.text = value;
        }
    }
}
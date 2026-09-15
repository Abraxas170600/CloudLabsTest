using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Client.StudentGrades.Scripts
{
    public sealed class StudentAppController : MonoBehaviour
    {
        [Header("Shared")]
        [SerializeField] private ScreenHeaderView header;
        [SerializeField] private RectTransform dragLayer;
        [SerializeField] private Button reloadButton;

        [Header("Table")]
        [SerializeField] private GameObject tablePanel;
        [SerializeField] private StudentRowView rowPrefab;
        [SerializeField] private RectTransform rowsContent;
        [SerializeField] private ResultBannerView tableBanner;
        [SerializeField] private Button validateTableButton;
        [SerializeField] private Button continueButton;

        [Header("Drag and drop")]
        [SerializeField] private GameObject dragPanel;
        [SerializeField] private StudentDragCard cardPrefab;

        [SerializeField] private StudentDropZone approvedZone;
        [SerializeField] private StudentDropZone failedZone;

        [SerializeField] private ResultBannerView dragBanner;

        [SerializeField] private Button validateDragButton;

        private readonly List<Student> students =
            new List<Student>();

        private readonly List<StudentRowView> rows =
            new List<StudentRowView>();

        private readonly List<StudentDragCard> cards =
            new List<StudentDragCard>();

        private bool dragging;

        private void OnEnable()
        {
            reloadButton.onClick.AddListener(Reload);

            validateTableButton.onClick.AddListener(
                ValidateTable);

            continueButton.onClick.AddListener(
                ShowDragPanel);

            validateDragButton.onClick.AddListener(
                ValidateDrag);

            RefreshButtons();
        }

        private void OnDisable()
        {
            foreach (StudentDragCard card in cards)
            {
                if (card != null)
                {
                    card.CancelOrFinishDrag();
                }
            }

            reloadButton.onClick.RemoveListener(Reload);

            validateTableButton.onClick.RemoveListener(
                ValidateTable);

            continueButton.onClick.RemoveListener(
                ShowDragPanel);

            validateDragButton.onClick.RemoveListener(
                ValidateDrag);
        }

        private void Start()
        {
            Reload();
        }

        private void Reload()
        {
            if (dragging)
            {
                return;
            }

            ClearViews();
            students.Clear();

            ShowTablePanel();

            tableBanner.Hide();
            dragBanner.Hide(true);

            StudentLoadResult result = StudentRepository.Load(
                Path.Combine(
                    Application.streamingAssetsPath,
                    "estudiantes.json"));

            if (result.Error == null)
            {
                students.AddRange(result.Students);
            }

            header.SetStudentCount(students.Count);

            for (int index = 0; index < students.Count; index++)
            {
                Student student = students[index];

                StudentRowView row = Instantiate(
                    rowPrefab,
                    rowsContent,
                    false);

                row.name = $"Row_{student.Code}";
                row.Bind(student, index);

                row.SelectionChanged += OnTableSelectionChanged;

                rows.Add(row);

                StudentDragCard card = Instantiate(
                    cardPrefab,
                    approvedZone.Content,
                    false);

                card.name = $"Card_{student.Code}";

                card.Bind(
                    student,
                    dragLayer,
                    index,
                    CanBeginDrag,
                    initialClassification: approvedZone.Classification);

                card.Dropped += OnCardDropped;
                card.DragStateChanged += OnDragStateChanged;

                cards.Add(card);
            }

            UpdateProgress();
            RefreshButtons();
        }

        private bool CanBeginDrag()
        {
            return !dragging && isActiveAndEnabled;
        }

        private void ValidateTable()
        {
            if (dragging)
            {
                return;
            }

            var selections =
                new Dictionary<string, Classification>(
                    StringComparer.OrdinalIgnoreCase);

            foreach (StudentRowView row in rows)
            {
                selections[row.Data.Code] = row.Selection;
            }

            ShowValidation(
                tableBanner,
                GradeRules.Validate(students, selections));
        }

        private void ValidateDrag()
        {
            if (dragging)
            {
                return;
            }

            var selections =
                new Dictionary<string, Classification>(
                    StringComparer.OrdinalIgnoreCase);

            foreach (StudentDragCard card in cards)
            {
                selections[card.Data.Code] = card.Selection;
            }

            ShowValidation(
                dragBanner,
                GradeRules.Validate(students, selections));
        }

        private void ShowDragPanel()
        {
            if (dragging || students.Count == 0)
            {
                return;
            }

            tablePanel.SetActive(false);
            dragPanel.SetActive(true);

            header.SetScreen(true);
        }

        private void ShowTablePanel()
        {
            if (dragging)
            {
                return;
            }

            dragPanel.SetActive(false);
            tablePanel.SetActive(true);

            header.SetScreen(false);
        }

        private void OnTableSelectionChanged()
        {
            tableBanner.Show(
                "Selección modificada. Pulsa Validar.",
                BannerKind.Information);
        }

        private void OnCardDropped(StudentDragCard card)
        {
            UpdateProgress();

            string label =
                $"{card.Data.FullName} [{card.Data.Code}]";

            if (card.Selection == Classification.Unassigned)
            {
                dragBanner.Show(
                    $"{label} volvió a Sin clasificar.",
                    BannerKind.Information);

                return;
            }

            bool correct = GradeRules.IsCorrect(
                card.Data,
                card.Selection);

            string message =
                $"{label}: {GradeRules.Label(card.Selection)}. ";

            message += correct
                ? "Correcto."
                : "Incorrecto: corresponde " +
                  $"{GradeRules.Label(GradeRules.Expected(card.Data))} " +
                  $"con nota {GradeRules.FormatGrade(card.Data.FinalGrade)}.";

            dragBanner.Show(
                message,
                correct ? BannerKind.Success : BannerKind.Error);
        }

        private void OnDragStateChanged(bool active)
        {
            dragging = active;

            if (active)
            {
                dragBanner.Hide(true);
            }

            if (!active)
            {
                approvedZone.SetHint(false);
                failedZone.SetHint(false);
            }

            RefreshButtons();
        }

        private void UpdateProgress()
        {
            int approved = 0;
            int failed = 0;

            foreach (StudentDragCard card in cards)
            {
                if (card.Selection == Classification.Approved)
                {
                    approved++;
                }
                else if (card.Selection == Classification.Failed)
                {
                    failed++;
                }
            }

            int pending =
                students.Count - approved - failed;
        }

        private void RefreshButtons()
        {
            bool ready =
                students.Count > 0 && !dragging;

            reloadButton.interactable = !dragging;

            validateTableButton.interactable = ready;
            continueButton.interactable = ready;
            validateDragButton.interactable = ready;
        }

        private void ShowValidation(
            ResultBannerView banner,
            ValidationReport report)
        {
            string message = report.Success
                ? $"¡Todo correcto! {students.Count} de {students.Count} " +
                  "estudiantes cargados clasificados correctamente."
                : report.Message;

            banner.Show(
                message,
                report.Success
                    ? BannerKind.Success
                    : BannerKind.Error);
        }

        private void ClearViews()
        {
            foreach (StudentRowView row in rows)
            {
                if (row == null)
                {
                    continue;
                }

                row.SelectionChanged -= OnTableSelectionChanged;

                row.gameObject.SetActive(false);

                Destroy(row.gameObject);
            }

            foreach (StudentDragCard card in cards)
            {
                if (card == null)
                {
                    continue;
                }

                card.Dropped -= OnCardDropped;
                card.DragStateChanged -= OnDragStateChanged;

                card.gameObject.SetActive(false);

                Destroy(card.gameObject);
            }

            rows.Clear();
            cards.Clear();
        }
    }
}
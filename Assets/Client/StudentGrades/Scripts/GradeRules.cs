using System.Collections.Generic;
using System.Globalization;

namespace Assets.Client.StudentGrades.Scripts
{
    public sealed class ValidationReport
    {
        public bool Success { get; }
        public string Message { get; }

        public ValidationReport(bool success, string message)
        {
            Success = success;
            Message = message;
        }
    }

    public static class GradeRules
    {
        public const decimal PassingGrade = 3m;

        public static Classification Expected(Student student)
        {
            return student.FinalGrade >= PassingGrade
                ? Classification.Approved
                : Classification.Failed;
        }

        public static bool IsCorrect(
            Student student,
            Classification selected)
        {
            return selected == Expected(student);
        }

        public static string Label(Classification value)
        {
            switch (value)
            {
                case Classification.Approved:
                    return "Aprobado";

                case Classification.Failed:
                    return "Reprobado";

                default:
                    return "Sin clasificar";
            }
        }

        public static string FormatGrade(decimal grade)
        {
            return grade.ToString(
                "0.0###########################",
                CultureInfo.InvariantCulture);
        }

        public static ValidationReport Validate(
            IReadOnlyList<Student> students,
            IReadOnlyDictionary<string, Classification> selections)
        {
            if (students == null || students.Count == 0)
            {
                return new ValidationReport(
                    false,
                    "No hay estudiantes válidos para evaluar.");
            }

            var errors = new List<string>();

            int correctCount = 0;

            foreach (Student student in students)
            {
                Classification selected =
                    Classification.Unassigned;

                if (selections != null)
                {
                    selections.TryGetValue(
                        student.Code,
                        out selected);
                }

                Classification expected = Expected(student);

                if (selected == expected)
                {
                    correctCount++;
                    continue;
                }

                errors.Add(
                    $"{student.FullName} [{student.Code}]: " +
                    $"marcado como {Label(selected)}; " +
                    $"corresponde {Label(expected)} con nota " +
                    $"{FormatGrade(student.FinalGrade)}.");
            }

            if (errors.Count == 0)
            {
                return new ValidationReport(
                    true,
                    $"Clasificación correcta: " +
                    $"{correctCount}/{students.Count} " +
                    "estudiantes cargados.");
            }

            return new ValidationReport(
                false,
                $"Correctos: {correctCount}/{students.Count}.\n" +
                string.Join("\n", errors));
        }
    }
}
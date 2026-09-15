using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Client.StudentGrades.Scripts
{
    public sealed class ScreenHeaderView : MonoBehaviour
    {
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text subtitleText;
        [SerializeField] private TMP_Text studentCountText;

        [SerializeField] private Image screenIcon;

        [SerializeField] private Sprite tableIcon;
        [SerializeField] private Sprite dragIcon;

        public void SetScreen(bool dragScreen)
        {
            titleText.text = dragScreen
                ? "Clasificación por arrastre"
                : "Panel de Notas";

            subtitleText.text = dragScreen
                ? "Prueba Técnica Unity · Paso 2 — " +
                  "arrastra cada estudiante a su zona"
                : "Prueba Técnica Unity · Colegio — " +
                  "Revisión de notas";

            screenIcon.sprite = dragScreen
                ? dragIcon
                : tableIcon;

            screenIcon.preserveAspect = true;
        }

        public void SetStudentCount(int count)
        {
            studentCountText.text = count == 1
                ? "1 estudiante"
                : $"{count} estudiantes";
        }
    }
}
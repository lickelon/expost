using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Expost.RuleReconstruction
{
    [DisallowMultipleComponent]
    public sealed class RulePreview : MonoBehaviour
    {
        [SerializeField] private List<Image> cells = new();

        public IReadOnlyList<Image> Cells => cells;

        public RulePreviewView ToView()
        {
            return new RulePreviewView((RectTransform)transform, cells);
        }

        public void Bind(IEnumerable<Image> previewCells)
        {
            cells.Clear();
            cells.AddRange(previewCells);
        }
    }
}

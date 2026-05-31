using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Expost.RuleReconstruction
{
    public readonly struct BoardCellView
    {
        public readonly GridPosition Position;
        public readonly Image Background;
        public readonly Text Label;

        public BoardCellView(GridPosition position, Image background, Text label)
        {
            Position = position;
            Background = background;
            Label = label;
        }
    }

    public readonly struct MismatchSummary
    {
        public readonly int NeedMore;
        public readonly int Excess;

        public MismatchSummary(int needMore, int excess)
        {
            NeedMore = needMore;
            Excess = excess;
        }

        public int Total => NeedMore + Excess;
    }

    public readonly struct RulePreviewView
    {
        public readonly RectTransform Root;
        public readonly List<Image> Cells;

        public RulePreviewView(RectTransform root, List<Image> cells)
        {
            Root = root;
            Cells = cells;
        }
    }

    public readonly struct RangeIconView
    {
        public readonly RectTransform Root;
        public readonly RectTransform Center;
        public readonly List<RectTransform> Dots;

        public RangeIconView(RectTransform root, RectTransform center, List<RectTransform> dots)
        {
            Root = root;
            Center = center;
            Dots = dots;
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Expost.RuleReconstruction
{
    public readonly struct RuleReconstructionBoardView
    {
        public readonly RectTransform Root;
        public readonly List<BoardCellView> Cells;

        public RuleReconstructionBoardView(RectTransform root, List<BoardCellView> cells)
        {
            Root = root;
            Cells = cells;
        }
    }

    public static class RuleReconstructionBoardBuilder
    {
        public static RuleReconstructionBoardView Build(
            RuleReconstructionUiFactory ui,
            RectTransform boardPanel,
            RectTransform existingRoot,
            Text resultBannerText,
            StageData stage,
            Color cellColor)
        {
            if (existingRoot != null)
            {
                Object.Destroy(existingRoot.gameObject);
            }

            var boardRoot = ui.CreatePanel("Board", boardPanel, Color.clear);
            RuleReconstructionUiFactory.Anchor(boardRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-182f, -182f), new Vector2(182f, 182f));
            resultBannerText.rectTransform.SetAsLastSibling();

            var grid = boardRoot.gameObject.AddComponent<GridLayoutGroup>();
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 5;
            grid.cellSize = new Vector2(68f, 68f);
            grid.spacing = new Vector2(4f, 4f);

            var cells = new List<BoardCellView>();
            for (var y = stage.Height - 1; y >= 0; y--)
            {
                for (var x = 0; x < stage.Width; x++)
                {
                    var cell = ui.CreatePanel($"Cell{x}_{y}", boardRoot, cellColor);
                    var label = ui.CreateText("Value", cell, string.Empty, 25, TextAnchor.MiddleCenter);
                    RuleReconstructionUiFactory.Stretch(label.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                    cells.Add(new BoardCellView(new GridPosition(x, y), cell.GetComponent<Image>(), label));
                }
            }

            return new RuleReconstructionBoardView(boardRoot, cells);
        }
    }
}

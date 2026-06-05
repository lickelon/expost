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
            RectTransform boardRoot,
            Text resultBannerText,
            StageData stage,
            RuleReconstructionBoardCell cellPrefab)
        {
            resultBannerText.rectTransform.SetAsLastSibling();

            var cells = new List<BoardCellView>();
            var existingCells = new List<RuleReconstructionBoardCell>();
            foreach (Transform child in boardRoot)
            {
                if (child.TryGetComponent<RuleReconstructionBoardCell>(out var existingCell))
                {
                    existingCells.Add(existingCell);
                }
            }

            var requiredCount = stage.Width * stage.Height;
            while (existingCells.Count < requiredCount)
            {
                existingCells.Add(Object.Instantiate(cellPrefab, boardRoot));
            }

            for (var index = requiredCount; index < existingCells.Count; index++)
            {
                existingCells[index].gameObject.SetActive(false);
            }

            var cellIndex = 0;
            for (var y = stage.Height - 1; y >= 0; y--)
            {
                for (var x = 0; x < stage.Width; x++)
                {
                    var cell = existingCells[cellIndex];
                    cell.name = $"Cell{x}_{y}";
                    cell.gameObject.SetActive(true);
                    cell.Label.text = string.Empty;
                    cells.Add(new BoardCellView(new GridPosition(x, y), cell.Background, cell.Label));
                    cellIndex++;
                }
            }

            return new RuleReconstructionBoardView(boardRoot, cells);
        }
    }
}

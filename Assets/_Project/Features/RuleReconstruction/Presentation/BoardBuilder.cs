using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Expost.RuleReconstruction
{
    public readonly struct BoardView
    {
        public readonly RectTransform Root;
        public readonly List<BoardCellView> Cells;

        public BoardView(RectTransform root, List<BoardCellView> cells)
        {
            Root = root;
            Cells = cells;
        }
    }

    public static class BoardBuilder
    {
        public static BoardView Build(
            RectTransform boardCellRoot,
            Text resultBannerText,
            StageData stage,
            BoardCell cellPrefab)
        {
            resultBannerText.rectTransform.SetAsLastSibling();

            var cells = new List<BoardCellView>();
            var existingCells = new List<BoardCell>();
            foreach (Transform child in boardCellRoot)
            {
                if (child.TryGetComponent<BoardCell>(out var existingCell))
                {
                    existingCells.Add(existingCell);
                }
            }

            var requiredCount = stage.Width * stage.Height;
            while (existingCells.Count < requiredCount)
            {
                existingCells.Add(Object.Instantiate(cellPrefab, boardCellRoot));
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

            return new BoardView(boardCellRoot, cells);
        }
    }
}

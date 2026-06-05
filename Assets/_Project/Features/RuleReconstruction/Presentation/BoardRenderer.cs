using System;
using System.Collections.Generic;
using UnityEngine;

namespace Expost.RuleReconstruction
{
    public static class BoardRenderer
    {
        public static void Render(
            IReadOnlyList<BoardCellView> boardCells,
            BoardState displayBoard,
            StageData currentStage,
            bool showMismatch,
            HashSet<GridPosition> activeAffectedCells,
            Color wrongTextColor,
            Color needMoreTextColor,
            Color affectedTextColor,
            Func<BoxColor, Color> getSourceColor)
        {
            if (displayBoard == null || boardCells.Count != displayBoard.Width * displayBoard.Height)
            {
                return;
            }

            foreach (var view in boardCells)
            {
                var cell = displayBoard.GetCell(view.Position.X, view.Position.Y);
                var targetCell = currentStage.TargetBoard.GetCell(view.Position.X, view.Position.Y);
                var isWrong = showMismatch && !Validator.IsCellCorrect(cell, targetCell);
                var isAffected = !cell.HasSource && activeAffectedCells.Contains(view.Position);

                view.Background.color = cell.HasSource ? getSourceColor(cell.SourceColor) : view.DefaultBackgroundColor;
                view.Label.text = GetBoardCellLabel(cell, targetCell, isWrong);
                view.Label.color = isWrong ? GetMismatchColor(cell, targetCell, wrongTextColor, needMoreTextColor) : isAffected ? affectedTextColor : view.DefaultLabelColor;
            }
        }

        private static string GetBoardCellLabel(CellState cell, CellState targetCell, bool isWrong)
        {
            if (cell.HasSource)
            {
                return string.Empty;
            }

            if (!isWrong)
            {
                return cell.Number.ToString();
            }

            var difference = targetCell.Number - cell.Number;
            return difference > 0 ? $"+{difference}" : $"-{-difference}";
        }

        private static Color GetMismatchColor(CellState cell, CellState targetCell, Color excessColor, Color needMoreColor)
        {
            return targetCell.Number > cell.Number ? needMoreColor : excessColor;
        }
    }
}

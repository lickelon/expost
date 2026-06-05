using System;
using System.Collections.Generic;
using UnityEngine;

namespace Expost.RuleReconstruction
{
    public static class RuleReconstructionBoardRenderer
    {
        public static void Render(
            IReadOnlyList<BoardCellView> boardCells,
            BoardState displayBoard,
            StageData currentStage,
            bool showMismatch,
            HashSet<GridPosition> activeAffectedCells,
            Color cellColor,
            Color numberTextColor,
            Color wrongTextColor,
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

                view.Background.color = cell.HasSource ? getSourceColor(cell.SourceColor) : cellColor;
                view.Label.text = GetBoardCellLabel(cell, targetCell, isWrong);
                view.Label.fontSize = 21;
                view.Label.color = isWrong ? GetMismatchColor(cell, targetCell, wrongTextColor) : isAffected ? affectedTextColor : numberTextColor;
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

        private static Color GetMismatchColor(CellState cell, CellState targetCell, Color excessColor)
        {
            return targetCell.Number > cell.Number ? new Color(1f, 0.86f, 0.20f) : excessColor;
        }
    }
}

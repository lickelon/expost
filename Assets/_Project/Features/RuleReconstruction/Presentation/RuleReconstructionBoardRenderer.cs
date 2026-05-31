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
                view.Label.fontSize = isWrong ? 14 : 25;
                view.Label.color = isWrong ? wrongTextColor : isAffected ? affectedTextColor : Color.white;
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
            return difference > 0 ? $"+{difference}\nNEED" : $"-{-difference}\nOVER";
        }
    }
}

using System.Collections.Generic;

namespace Expost.RuleReconstruction
{
    public sealed class RuleReconstructionBoardPresenter
    {
        private readonly RuleReconstructionView view;
        private readonly List<BoardCellView> boardCells = new();

        public RuleReconstructionBoardPresenter(RuleReconstructionView view)
        {
            this.view = view;
        }

        public void Rebuild(StageData stage)
        {
            boardCells.Clear();
            var boardView = RuleReconstructionBoardBuilder.Build(view.BoardRoot, view.ResultBannerText, stage, view.BoardCellPrefab);
            boardCells.AddRange(boardView.Cells);
        }

        public void Render(BoardState displayBoard, StageData currentStage, bool showMismatch, HashSet<GridPosition> activeAffectedCells)
        {
            RuleReconstructionBoardRenderer.Render(
                boardCells,
                displayBoard,
                currentStage,
                showMismatch,
                activeAffectedCells,
                view.WrongTextColor,
                view.NeedMoreTextColor,
                view.AffectedTextColor,
                view.GetSourceColor);
        }
    }
}

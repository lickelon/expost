using System.Collections.Generic;

namespace Expost.RuleReconstruction
{
    public sealed class BoardPresenter
    {
        private readonly GameView view;
        private readonly List<BoardCellView> boardCells = new();

        public BoardPresenter(GameView view)
        {
            this.view = view;
        }

        public void Rebuild(StageData stage)
        {
            boardCells.Clear();
            var boardView = BoardBuilder.Build(view.BoardRoot, view.ResultBannerText, stage, view.BoardCellPrefab);
            boardCells.AddRange(boardView.Cells);
        }

        public void Render(BoardState displayBoard, StageData currentStage, bool showMismatch, HashSet<GridPosition> activeAffectedCells)
        {
            BoardRenderer.Render(
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

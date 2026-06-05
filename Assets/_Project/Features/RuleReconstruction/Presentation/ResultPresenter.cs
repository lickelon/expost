namespace Expost.RuleReconstruction
{
    public sealed class ResultPresenter
    {
        private readonly GameView view;

        public ResultPresenter(GameView view)
        {
            this.view = view;
        }

        public void Render(bool showResultBanner, bool showMismatch, bool showResult, bool isComplete, bool isClear)
        {
            var showClear = showResultBanner && !showMismatch && showResult && isComplete && isClear;
            view.ResultBannerIcon.enabled = showClear;
            view.ResultBannerIcon.color = view.ClearTextColor;
        }
    }
}

using System;
using UnityEngine;
using UnityEngine.UI;

namespace Expost.RuleReconstruction
{
    public sealed class RuleReconstructionStagePresenter
    {
        private readonly RuleReconstructionView view;
        private readonly RuleReconstructionSession session;
        private readonly RuleReconstructionRunController runController;

        public RuleReconstructionStagePresenter(
            RuleReconstructionView view,
            RuleReconstructionSession session,
            RuleReconstructionRunController runController,
            Action<int> moveStage,
            Action startRun,
            Action showTarget)
        {
            this.view = view;
            this.session = session;
            this.runController = runController;

            DisableRaycastTarget(view.TestButton.transform.parent);
            view.PrevButton.onClick.RemoveAllListeners();
            view.PrevButton.onClick.AddListener(() => moveStage(-1));
            view.NextButton.onClick.RemoveAllListeners();
            view.NextButton.onClick.AddListener(() => moveStage(1));
            view.TestButton.onClick.RemoveAllListeners();
            view.TestButton.onClick.AddListener(() => startRun());
            view.TargetButton.onClick.RemoveAllListeners();
            view.TargetButton.onClick.AddListener(() => showTarget());
            view.RefreshStaticButtonVisuals();
        }

        public void Render()
        {
            view.TitleText.text = $"{session.StageIndex + 1:00}/{session.StageCount:00}";
            view.BoardTitleText.text = string.Empty;
            view.StatusText.text = string.Empty;
            view.AnalysisText.text = string.Empty;
            view.ResultBannerText.text = string.Empty;
            view.ResultBannerText.enabled = false;

            UpdateNavigationButtons();
            UpdateActionButtons();
        }

        private void UpdateNavigationButtons()
        {
            RuleReconstructionIconFactory.ApplyToButton(view.PrevButton, ButtonIconKind.Previous);
            view.PrevButton.interactable = !runController.IsRunning && !session.IsFirstStage;

            RuleReconstructionIconFactory.ApplyToButton(view.NextButton, ButtonIconKind.Next);
            view.NextButton.interactable = !runController.IsRunning && session.IsCurrentStageCleared && !session.IsLastStage;
        }

        private void UpdateActionButtons()
        {
            view.TestButton.interactable = (!runController.ShowResult || runController.ShowMismatch) && !runController.IsRunning;
            view.TargetButton.interactable = runController.ShowResult && !runController.ShowMismatch && !runController.IsRunning;
        }

        private static void DisableRaycastTarget(Transform target)
        {
            if (target == null)
            {
                return;
            }

            var graphic = target.GetComponent<Graphic>();
            if (graphic != null)
            {
                graphic.raycastTarget = false;
            }
        }
    }
}

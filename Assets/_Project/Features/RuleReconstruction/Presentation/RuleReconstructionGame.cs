using UnityEngine;

namespace Expost.RuleReconstruction
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Rule Reconstruction/Rule Reconstruction Game")]
    public sealed class RuleReconstructionGame : MonoBehaviour
    {
        private static readonly BoxColor[] AllColors =
        {
            BoxColor.Red,
            BoxColor.Blue,
            BoxColor.Green,
            BoxColor.Yellow
        };

        [SerializeField] private RuleReconstructionView view;

        private RuleReconstructionSession session;
        private RuleReconstructionRunController runController;
        private RuleReconstructionStagePresenter stagePresenter;
        private RuleReconstructionSidebarPresenter sidebarPresenter;
        private RuleReconstructionBoardPresenter boardPresenter;
        private RuleReconstructionResultPresenter resultPresenter;

        private void Awake()
        {
            EnsureInitialized();
        }

        private void Update()
        {
            EnsureInitialized();
            if (session == null)
            {
                return;
            }

            stagePresenter.Render();
            sidebarPresenter.Render();
            boardPresenter.Render(runController.DisplayBoard, session.CurrentStage, runController.ShowMismatch, runController.ActiveAffectedCells);
            resultPresenter.Render(runController.ShowResultBanner, runController.ShowMismatch, runController.ShowResult, session.IsComplete, session.ValidationResult.IsClear);
        }

        private void EnsureInitialized()
        {
            if (session != null)
            {
                return;
            }

            if (!TryBindSceneView())
            {
                Debug.LogError("RuleReconstructionGame requires scene-defined RuleReconstructionView references.", this);
                return;
            }

            session = new RuleReconstructionSession(StageRepository.LoadStages(), AllColors);
            runController = new RuleReconstructionRunController(this, session);
            stagePresenter = new RuleReconstructionStagePresenter(view, session, runController, MoveStage, runController.StartRun, runController.ShowTarget);
            sidebarPresenter = new RuleReconstructionSidebarPresenter(view, session, runController.ResetAttemptState);
            boardPresenter = new RuleReconstructionBoardPresenter(view);
            resultPresenter = new RuleReconstructionResultPresenter(view);

            sidebarPresenter.Rebuild();
            boardPresenter.Rebuild(session.CurrentStage);
            runController.ShowTarget();
        }

        private bool TryBindSceneView()
        {
            if (view == null)
            {
                view = GetComponentInChildren<RuleReconstructionView>(true);
            }

            if (view == null)
            {
                view = FindFirstObjectByType<RuleReconstructionView>(FindObjectsInactive.Include);
            }

            return view != null && view.HasRequiredReferences();
        }

        private void MoveStage(int delta)
        {
            if (runController.IsRunning
                || delta > 0 && (!session.IsCurrentStageCleared || session.IsLastStage)
                || delta < 0 && session.IsFirstStage)
            {
                return;
            }

            session.MoveStage(delta);
            runController.ShowTarget();
            sidebarPresenter.Rebuild();
            boardPresenter.Rebuild(session.CurrentStage);
        }
    }
}

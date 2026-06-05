using UnityEngine;

namespace Expost.RuleReconstruction
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Rule Reconstruction/Game Controller")]
    public sealed class GameController : MonoBehaviour
    {
        private static readonly BoxColor[] AllColors =
        {
            BoxColor.Red,
            BoxColor.Blue,
            BoxColor.Green,
            BoxColor.Yellow
        };

        [SerializeField] private GameView view;

        private PuzzleSession session;
        private RunController runController;
        private StagePresenter stagePresenter;
        private SidebarPresenter sidebarPresenter;
        private BoardPresenter boardPresenter;
        private ResultPresenter resultPresenter;

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
                Debug.LogError("GameController requires scene-defined GameView references.", this);
                return;
            }

            session = new PuzzleSession(StageRepository.LoadStages(), AllColors);
            runController = new RunController(this, session);
            stagePresenter = new StagePresenter(view, session, runController, MoveStage, runController.StartRun, runController.ShowTarget);
            sidebarPresenter = new SidebarPresenter(view, session, runController.ResetAttemptState);
            boardPresenter = new BoardPresenter(view);
            resultPresenter = new ResultPresenter(view);

            sidebarPresenter.Rebuild();
            boardPresenter.Rebuild(session.CurrentStage);
            runController.ShowTarget();
        }

        private bool TryBindSceneView()
        {
            if (view == null)
            {
                view = GetComponentInChildren<GameView>(true);
            }

            if (view == null)
            {
                view = FindFirstObjectByType<GameView>(FindObjectsInactive.Include);
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

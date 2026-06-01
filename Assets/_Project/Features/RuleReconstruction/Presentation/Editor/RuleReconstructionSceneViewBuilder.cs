using UnityEditor;
using UnityEngine;

namespace Expost.RuleReconstruction.Editor
{
    public static class RuleReconstructionSceneViewBuilder
    {
        private static readonly Color PageColor = new(0.18f, 0.29f, 0.47f);
        private static readonly Color PanelColor = new(0.13f, 0.23f, 0.39f);

        [MenuItem("GameObject/Rule Reconstruction/Game View", false, 10)]
        public static void CreateGameView()
        {
            RuleReconstructionCanvasFactory.EnsureEventSystem(false);

            var gameObject = new GameObject("Rule Reconstruction Game");
            Undo.RegisterCreatedObjectUndo(gameObject, "Create Rule Reconstruction Game");
            var game = gameObject.AddComponent<RuleReconstructionGame>();

            var canvas = RuleReconstructionCanvasFactory.Create(gameObject.transform, PageColor);

            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var ui = new RuleReconstructionUiFactory(font, new Color(0.28f, 0.37f, 0.50f));
            var staticView = RuleReconstructionStaticViewFactory.Create(canvas, ui, PageColor, PanelColor);

            var view = canvas.gameObject.AddComponent<RuleReconstructionView>();
            view.Bind(
                canvas,
                staticView.Sidebar,
                staticView.BoardPanel,
                staticView.TitleText,
                staticView.BoardTitleText,
                staticView.StatusText,
                staticView.AnalysisText,
                staticView.ResultBannerText,
                staticView.PrevButton,
                staticView.NextButton,
                staticView.TestButton,
                staticView.TargetButton);

            var serializedGame = new SerializedObject(game);
            serializedGame.FindProperty("view").objectReferenceValue = view;
            serializedGame.FindProperty("buildRuntimeLayoutWhenMissing").boolValue = false;
            serializedGame.ApplyModifiedPropertiesWithoutUndo();

            Selection.activeGameObject = gameObject;
            EditorUtility.SetDirty(game);
        }
    }
}

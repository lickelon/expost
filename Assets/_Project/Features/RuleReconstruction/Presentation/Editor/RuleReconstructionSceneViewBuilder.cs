using UnityEditor;

namespace Expost.RuleReconstruction.Editor
{
    public static class RuleReconstructionSceneViewBuilder
    {
        [MenuItem("GameObject/Rule Reconstruction/Game View", false, 10)]
        public static void CreateGameView()
        {
            EditorUtility.DisplayDialog(
                "Rule Reconstruction",
                "Rule Reconstruction UI is scene-authored. Duplicate or edit the existing scene UI instead of generating it from code.",
                "OK");
        }
    }
}

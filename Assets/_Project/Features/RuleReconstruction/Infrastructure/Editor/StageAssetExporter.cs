using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Expost.RuleReconstruction.Editor
{
    public static class StageAssetExporter
    {
        private const string OutputFolder = "Assets/_Project/Resources/Stages";

        [MenuItem("Tools/Rule Reconstruction/Export Prototype Stages")]
        public static void Export()
        {
            Directory.CreateDirectory(OutputFolder);

            var stages = PrototypeStageFactory.CreateStages();
            for (var index = 0; index < stages.Count; index++)
            {
                var stage = stages[index];
                var path = $"{OutputFolder}/{ToAssetName(stage.Name)}.asset";
                var asset = AssetDatabase.LoadAssetAtPath<StageAsset>(path);
                if (asset == null)
                {
                    asset = ScriptableObject.CreateInstance<StageAsset>();
                    AssetDatabase.CreateAsset(asset, path);
                }

                asset.Apply(stage, index + 1);
                EditorUtility.SetDirty(asset);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Exported {stages.Count} rule reconstruction stage assets to {OutputFolder}.");
        }

        private static string ToAssetName(string value)
        {
            var normalized = Regex.Replace(value, "[^a-zA-Z0-9]+", "_").Trim('_');
            return string.IsNullOrEmpty(normalized) ? "Stage" : normalized;
        }
    }
}

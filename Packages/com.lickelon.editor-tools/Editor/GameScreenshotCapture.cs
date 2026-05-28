using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Lickelon.EditorTools
{
    public static class GameScreenshotCapture
    {
        private const string OutputFolder = "Assets/Screenshots";

        [MenuItem("Tools/Capture/Game Screenshot")]
        public static void Capture()
        {
            FocusGameView();
            Directory.CreateDirectory(OutputFolder);

            var fileName = $"game_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            var path = Path.Combine(OutputFolder, fileName);
            var absolutePath = Path.GetFullPath(path);

            EditorApplication.QueuePlayerLoopUpdate();
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            ScreenCapture.CaptureScreenshot(absolutePath);
            Debug.Log($"Game screenshot saved: {path}");
        }

        private static void FocusGameView()
        {
            var gameViewType = Type.GetType("UnityEditor.GameView,UnityEditor");
            if (gameViewType == null)
            {
                return;
            }

            EditorWindow.GetWindow(gameViewType).Focus();
        }
    }
}

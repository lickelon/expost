using UnityEngine;

namespace Expost.RuleReconstruction
{
    public static class RuleReconstructionBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            if (Object.FindAnyObjectByType<RuleReconstructionGame>() != null)
            {
                return;
            }

            var gameObject = new GameObject("Rule Reconstruction Game");
            gameObject.AddComponent<RuleReconstructionGame>();
            Object.DontDestroyOnLoad(gameObject);
        }
    }
}

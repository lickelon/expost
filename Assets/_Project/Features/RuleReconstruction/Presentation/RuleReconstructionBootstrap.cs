using UnityEngine;

namespace Expost.RuleReconstruction
{
    public static class RuleReconstructionBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            if (Object.FindAnyObjectByType<RuleReconstructionPrototype>() != null)
            {
                return;
            }

            var gameObject = new GameObject("Rule Reconstruction Game");
            gameObject.AddComponent<RuleReconstructionPrototype>();
            Object.DontDestroyOnLoad(gameObject);
        }
    }
}

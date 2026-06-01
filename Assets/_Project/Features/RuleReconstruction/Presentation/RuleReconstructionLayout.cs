using UnityEngine;
using UnityEngine.UI;

namespace Expost.RuleReconstruction
{
    public static class RuleReconstructionLayout
    {
        public static void AnchorActionButtons(Button runButton, Button targetButton)
        {
            RuleReconstructionUiFactory.Anchor(runButton.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -160f), new Vector2(-4f, -108f));
            RuleReconstructionUiFactory.Anchor(targetButton.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(1f, 1f), new Vector2(4f, -160f), new Vector2(0f, -108f));
        }
    }
}

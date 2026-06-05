using System.Collections.Generic;
using UnityEngine;

namespace Expost.RuleReconstruction
{
    [DisallowMultipleComponent]
    public sealed class RangeIcon : MonoBehaviour
    {
        [SerializeField] private RectTransform center;
        [SerializeField] private List<RectTransform> dots = new();

        public RangeIconView ToView()
        {
            return new RangeIconView((RectTransform)transform, center, dots);
        }

        public void Bind(RectTransform center, IEnumerable<RectTransform> dots)
        {
            this.center = center;
            this.dots.Clear();
            this.dots.AddRange(dots);
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Expost.RuleReconstruction
{
    public static class StageRepository
    {
        public static List<StageData> LoadStages()
        {
            var assets = Resources.LoadAll<StageAsset>("Stages");

            if (assets.Length == 0)
            {
                return PrototypeStageFactory.CreateStages();
            }

            var stages = new List<StageData>();
            Array.Sort(assets, (left, right) =>
            {
                var orderComparison = left.order.CompareTo(right.order);
                return orderComparison != 0
                    ? orderComparison
                    : string.CompareOrdinal(left.stageName, right.stageName);
            });

            foreach (var asset in assets)
            {
                stages.Add(asset.ToStageData());
            }

            return stages;
        }
    }
}

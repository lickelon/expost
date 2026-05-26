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

            foreach (var asset in assets)
            {
                stages.Add(asset.ToStageData());
            }

            stages.Sort((left, right) => string.CompareOrdinal(left.Name, right.Name));
            return stages;
        }
    }
}

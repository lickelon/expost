using System.Collections.Generic;
using UnityEngine;

namespace Expost.RuleReconstruction
{
    [CreateAssetMenu(menuName = "Rule Reconstruction/Stage")]
    public sealed class StageAsset : ScriptableObject
    {
        public string stageName;
        public int width = 5;
        public int height = 5;
        public List<SourceBoxData> sources = new();
        public List<ColorRuleData> answerRules = new();

        public StageData ToStageData()
        {
            var stage = new StageData
            {
                Name = string.IsNullOrEmpty(stageName) ? name : stageName,
                Width = width,
                Height = height,
                AnswerRules = new RuleSet()
            };

            stage.Sources.AddRange(sources);

            foreach (var answer in answerRules)
            {
                stage.AnswerRules.Set(answer.Color, new Rule(answer.Direction, answer.Range, EffectType.AddNumber));
            }

            stage.TargetBoard = RuleSimulator.Simulate(stage, stage.AnswerRules);

            return stage;
        }
    }
}

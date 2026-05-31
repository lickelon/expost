using System.Collections.Generic;
using UnityEngine;

namespace Expost.RuleReconstruction
{
    [CreateAssetMenu(menuName = "Rule Reconstruction/Stage")]
    public sealed class StageAsset : ScriptableObject
    {
        public string stageName;
        public string packId = "main";
        public int order;
        public int difficulty = 1;
        [TextArea] public string designerNote;
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

        public void Apply(StageData stage, int order)
        {
            stageName = stage.Name;
            this.order = order;
            width = stage.Width;
            height = stage.Height;
            sources = new List<SourceBoxData>();
            answerRules = new List<ColorRuleData>();

            foreach (var source in stage.Sources)
            {
                sources.Add(new SourceBoxData(source.X, source.Y, source.Color));
            }

            foreach (var color in StageRuleAnalyzer.GetStageColors(stage))
            {
                if (stage.AnswerRules.TryGet(color, out var rule))
                {
                    answerRules.Add(new ColorRuleData(color, rule.Direction, rule.Range));
                }
            }
        }
    }
}

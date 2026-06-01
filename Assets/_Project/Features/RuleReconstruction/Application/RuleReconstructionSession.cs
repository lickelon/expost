using System.Collections.Generic;

namespace Expost.RuleReconstruction
{
    public sealed class RuleReconstructionSession
    {
        private static readonly DirectionType[] DirectionOptions =
        {
            DirectionType.Cross,
            DirectionType.Diagonal,
            DirectionType.Horizontal,
            DirectionType.Vertical,
            DirectionType.AllAround
        };

        private static readonly RangeType[] RangeOptions =
        {
            RangeType.One,
            RangeType.Two
        };

        private static readonly EffectType[] EffectOptions =
        {
            EffectType.AddNumber,
            EffectType.SubtractNumber
        };

        private readonly List<StageData> stages;
        private readonly Dictionary<BoxColor, DirectionType> selectedDirections = new();
        private readonly Dictionary<BoxColor, RangeType> selectedRanges = new();
        private readonly Dictionary<BoxColor, EffectType> selectedEffects = new();
        private readonly HashSet<int> clearedStageIndices = new();
        private int stageIndex;

        public RuleReconstructionSession(List<StageData> stages, IReadOnlyList<BoxColor> colors)
        {
            this.stages = stages;

            foreach (var color in colors)
            {
                selectedDirections[color] = DirectionType.Cross;
                selectedRanges[color] = RangeType.One;
                selectedEffects[color] = EffectType.AddNumber;
            }

            ResetSimulation();
        }

        public int AppliedSourceCount { get; private set; }
        public int ActiveSourceIndex { get; private set; } = -1;
        public BoardState ResultBoard { get; private set; }
        public ValidationResult ValidationResult { get; private set; }
        public int StageIndex => stageIndex;
        public int StageCount => stages.Count;
        public StageData CurrentStage => stages[stageIndex];
        public bool IsComplete => AppliedSourceCount >= CurrentStage.Sources.Count;
        public bool IsFirstStage => stageIndex == 0;
        public bool IsLastStage => stageIndex >= stages.Count - 1;
        public bool IsCurrentStageCleared => clearedStageIndices.Contains(stageIndex);

        public DirectionType GetDirection(BoxColor color)
        {
            return selectedDirections[color];
        }

        public RangeType GetRange(BoxColor color)
        {
            return selectedRanges[color];
        }

        public EffectType GetEffect(BoxColor color)
        {
            return selectedEffects[color];
        }

        public void MoveStage(int delta)
        {
            stageIndex = Clamp(stageIndex + delta, 0, stages.Count - 1);
            ResetSimulation();
        }

        public void MarkCurrentStageCleared()
        {
            clearedStageIndices.Add(stageIndex);
        }

        public void CycleDirection(BoxColor color)
        {
            var nextIndex = (IndexOf(DirectionOptions, selectedDirections[color]) + 1) % DirectionOptions.Length;
            selectedDirections[color] = DirectionOptions[nextIndex];
            ResetSimulation();
        }

        public void SetDirection(BoxColor color, DirectionType direction)
        {
            selectedDirections[color] = direction;
            ResetSimulation();
        }

        public void CycleRange(BoxColor color)
        {
            var nextIndex = (IndexOf(RangeOptions, selectedRanges[color]) + 1) % RangeOptions.Length;
            selectedRanges[color] = RangeOptions[nextIndex];
            ResetSimulation();
        }

        public void SetRange(BoxColor color, RangeType range)
        {
            selectedRanges[color] = range;
            ResetSimulation();
        }

        public void CycleEffect(BoxColor color)
        {
            var nextIndex = (IndexOf(EffectOptions, selectedEffects[color]) + 1) % EffectOptions.Length;
            selectedEffects[color] = EffectOptions[nextIndex];
            ResetSimulation();
        }

        public void SetEffect(BoxColor color, EffectType effect)
        {
            selectedEffects[color] = effect;
            ResetSimulation();
        }

        public void ResetSimulation()
        {
            AppliedSourceCount = 0;
            ActiveSourceIndex = -1;
            RefreshResult();
        }

        public void SetActiveSource(int sourceIndex)
        {
            ActiveSourceIndex = sourceIndex;
        }

        public void ClearActiveSource()
        {
            ActiveSourceIndex = -1;
        }

        public void ApplyNextSource()
        {
            if (IsComplete)
            {
                return;
            }

            AppliedSourceCount++;
            RefreshResult();
        }

        public HashSet<GridPosition> GetAffectedCells(int sourceIndex)
        {
            var cells = new HashSet<GridPosition>();

            if (sourceIndex < 0 || sourceIndex >= CurrentStage.Sources.Count)
            {
                return cells;
            }

            var source = CurrentStage.Sources[sourceIndex];
            var rules = BuildSelectedRules();

            if (!rules.TryGet(source.Color, out var rule))
            {
                return cells;
            }

            foreach (var position in RuleSimulator.GetAffectedPositions(CurrentStage, source, rule))
            {
                cells.Add(position);
            }

            return cells;
        }

        private void RefreshResult()
        {
            ResultBoard = RuleSimulator.Simulate(CurrentStage, BuildSelectedRules(), AppliedSourceCount);
            ValidationResult = Validator.Validate(ResultBoard, CurrentStage.TargetBoard);
        }

        private RuleSet BuildSelectedRules()
        {
            var ruleSet = new RuleSet();

            foreach (var color in StageRuleAnalyzer.GetStageColors(CurrentStage))
            {
                ruleSet.Set(color, new Rule(selectedDirections[color], selectedRanges[color], selectedEffects[color]));
            }

            return ruleSet;
        }

        private static int IndexOf<T>(IReadOnlyList<T> values, T target)
        {
            for (var index = 0; index < values.Count; index++)
            {
                if (EqualityComparer<T>.Default.Equals(values[index], target))
                {
                    return index;
                }
            }

            return 0;
        }

        private static int Clamp(int value, int min, int max)
        {
            if (value < min)
            {
                return min;
            }

            if (value > max)
            {
                return max;
            }

            return value;
        }
    }
}

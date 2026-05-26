using System.Collections.Generic;

namespace Expost.RuleReconstruction
{
    public static class RuleSimulator
    {
        public static BoardState Simulate(StageData stage, RuleSet rules)
        {
            return Simulate(stage, rules, stage.Sources.Count);
        }

        public static BoardState Simulate(StageData stage, RuleSet rules, int appliedSourceCount)
        {
            var board = new BoardState(stage.Width, stage.Height);

            foreach (var source in stage.Sources)
            {
                board.PlaceSource(source);
            }

            var maxCount = appliedSourceCount < stage.Sources.Count ? appliedSourceCount : stage.Sources.Count;

            for (var index = 0; index < maxCount; index++)
            {
                var source = stage.Sources[index];
                if (!rules.TryGet(source.Color, out var rule))
                {
                    continue;
                }

                foreach (var position in GetAffectedPositions(stage, source, rule))
                {
                    ApplyEffect(board, position, rule);
                }
            }

            return board;
        }

        public static List<GridPosition> GetAffectedPositions(StageData stage, SourceBoxData source, Rule rule)
        {
            var positions = new List<GridPosition>();
            var directions = GetDirections(rule.Direction);
            var distance = rule.Range == RangeType.One ? 1 : 2;

            foreach (var direction in directions)
            {
                for (var step = 1; step <= distance; step++)
                {
                    var x = source.X + direction.X * step;
                    var y = source.Y + direction.Y * step;

                    if (x < 0 || x >= stage.Width || y < 0 || y >= stage.Height)
                    {
                        continue;
                    }

                    if (IsSourcePosition(stage, x, y))
                    {
                        continue;
                    }

                    positions.Add(new GridPosition(x, y));
                }
            }

            return positions;
        }

        private static bool IsSourcePosition(StageData stage, int x, int y)
        {
            foreach (var source in stage.Sources)
            {
                if (source.X == x && source.Y == y)
                {
                    return true;
                }
            }

            return false;
        }

        private static void ApplyEffect(BoardState board, GridPosition position, Rule rule)
        {
            if (rule.Effect == EffectType.AddNumber)
            {
                board.AddNumber(position.X, position.Y, 1);
            }
        }

        private static IReadOnlyList<GridPosition> GetDirections(DirectionType direction)
        {
            return direction switch
            {
                DirectionType.Cross => CrossDirections,
                DirectionType.Diagonal => DiagonalDirections,
                DirectionType.Horizontal => HorizontalDirections,
                DirectionType.Vertical => VerticalDirections,
                DirectionType.AllAround => AllAroundDirections,
                _ => CrossDirections
            };
        }

        private static readonly GridPosition[] CrossDirections =
        {
            new(0, 1),
            new(1, 0),
            new(0, -1),
            new(-1, 0)
        };

        private static readonly GridPosition[] DiagonalDirections =
        {
            new(1, 1),
            new(1, -1),
            new(-1, -1),
            new(-1, 1)
        };

        private static readonly GridPosition[] HorizontalDirections =
        {
            new(1, 0),
            new(-1, 0)
        };

        private static readonly GridPosition[] VerticalDirections =
        {
            new(0, 1),
            new(0, -1)
        };

        private static readonly GridPosition[] AllAroundDirections =
        {
            new(0, 1),
            new(1, 1),
            new(1, 0),
            new(1, -1),
            new(0, -1),
            new(-1, -1),
            new(-1, 0),
            new(-1, 1)
        };
    }
}

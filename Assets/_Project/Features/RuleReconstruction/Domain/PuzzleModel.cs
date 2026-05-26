using System;
using System.Collections.Generic;

namespace Expost.RuleReconstruction
{
    public enum BoxColor
    {
        Red,
        Blue,
        Green,
        Yellow
    }

    public enum DirectionType
    {
        Cross,
        Diagonal,
        Horizontal,
        Vertical,
        AllAround
    }

    public enum RangeType
    {
        One,
        Two
    }

    public enum EffectType
    {
        AddNumber
    }

    [Serializable]
    public readonly struct GridPosition : IEquatable<GridPosition>
    {
        public readonly int X;
        public readonly int Y;

        public GridPosition(int x, int y)
        {
            X = x;
            Y = y;
        }

        public bool Equals(GridPosition other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            return obj is GridPosition other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }
    }

    [Serializable]
    public sealed class SourceBoxData
    {
        public int X;
        public int Y;
        public BoxColor Color;

        public SourceBoxData()
        {
        }

        public SourceBoxData(int x, int y, BoxColor color)
        {
            X = x;
            Y = y;
            Color = color;
        }
    }

    [Serializable]
    public sealed class ColorRuleData
    {
        public BoxColor Color;
        public DirectionType Direction;
        public RangeType Range;

        public ColorRuleData()
        {
        }

        public ColorRuleData(BoxColor color, DirectionType direction, RangeType range)
        {
            Color = color;
            Direction = direction;
            Range = range;
        }
    }

    [Serializable]
    public sealed class Rule
    {
        public DirectionType Direction;
        public RangeType Range;
        public EffectType Effect;

        public Rule(DirectionType direction, RangeType range, EffectType effect)
        {
            Direction = direction;
            Range = range;
            Effect = effect;
        }
    }

    public sealed class RuleSet
    {
        private readonly Dictionary<BoxColor, Rule> rules = new();

        public void Set(BoxColor color, Rule rule)
        {
            rules[color] = rule;
        }

        public bool TryGet(BoxColor color, out Rule rule)
        {
            return rules.TryGetValue(color, out rule);
        }
    }

    public sealed class CellState
    {
        public bool HasSource;
        public BoxColor SourceColor;
        public int Number;

        public bool Matches(CellState other)
        {
            return HasSource == other.HasSource
                && SourceColor == other.SourceColor
                && Number == other.Number;
        }
    }

    public sealed class BoardState
    {
        private readonly CellState[,] cells;

        public int Width { get; }
        public int Height { get; }

        public BoardState(int width, int height)
        {
            Width = width;
            Height = height;
            cells = new CellState[width, height];

            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    cells[x, y] = new CellState();
                }
            }
        }

        public CellState GetCell(int x, int y)
        {
            return cells[x, y];
        }

        public bool IsInside(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }

        public void PlaceSource(SourceBoxData source)
        {
            var cell = cells[source.X, source.Y];
            cell.HasSource = true;
            cell.SourceColor = source.Color;
        }

        public void AddNumber(int x, int y, int amount)
        {
            cells[x, y].Number += amount;
        }
    }

    public sealed class StageData
    {
        public string Name;
        public int Width;
        public int Height;
        public List<SourceBoxData> Sources = new();
        public BoardState TargetBoard;
        public RuleSet AnswerRules;
    }

    public readonly struct ValidationResult
    {
        public readonly bool IsClear;
        public readonly int WrongCellCount;

        public ValidationResult(bool isClear, int wrongCellCount)
        {
            IsClear = isClear;
            WrongCellCount = wrongCellCount;
        }
    }
}

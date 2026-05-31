using System.Collections.Generic;

namespace Expost.RuleReconstruction
{
    public static class RuleReconstructionPreviewPattern
    {
        public static HashSet<int> GetAffectedCells(DirectionType direction)
        {
            var cells = new HashSet<int>();

            switch (direction)
            {
                case DirectionType.Cross:
                    cells.Add(1);
                    cells.Add(3);
                    cells.Add(5);
                    cells.Add(7);
                    break;
                case DirectionType.Diagonal:
                    cells.Add(0);
                    cells.Add(2);
                    cells.Add(6);
                    cells.Add(8);
                    break;
                case DirectionType.Horizontal:
                    cells.Add(3);
                    cells.Add(5);
                    break;
                case DirectionType.Vertical:
                    cells.Add(1);
                    cells.Add(7);
                    break;
                case DirectionType.AllAround:
                    for (var index = 0; index < 9; index++)
                    {
                        if (index != 4)
                        {
                            cells.Add(index);
                        }
                    }
                    break;
            }

            return cells;
        }
    }
}

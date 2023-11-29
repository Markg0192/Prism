using Aspose.Words.Lists;
using Prism.CustomDialogs;
using System.Collections.Generic;
using Tekla.Structures.Analysis.Operations;
using Tekla.Structures.Model;

namespace Prism
{
    public static class UniClassCodes
    {
        public static TableRow GetUniClassDetailForPart(string jobName, Part part)
        {
            TableData td = UniClass_Codes.ReadTableData(Constants.ModelProjectInforLocation(jobName));

            foreach (TableRow row in td.Rows)
            {
                if (row.Filter != " ")
                {
                    if (Tekla.Structures.Model.Operations.Operation.ObjectMatchesToFilter(part, row.Filter))
                    {
                        return row;
                    }
                }
            }
            return null;
        }

        private static List<Part> GetObjects(string filterName)
        {
            List<Part> partsThatMatchFilter = new List<Part>();
            ModelObjectEnumerator Moe = new Tekla.Structures.Model.UI.ModelObjectSelector().GetSelectedObjects();
            foreach (ModelObject mO in Moe)
            {
                if (mO is Part)
                {
                    if (Tekla.Structures.Model.Operations.Operation.ObjectMatchesToFilter(mO, filterName))
                    {
                        partsThatMatchFilter.Add(mO as Part);
                    }
                }
            }

            return partsThatMatchFilter;
        }
    }
}
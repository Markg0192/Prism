using System;
using System.Collections;
using System.Linq;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;

namespace Prism
{
    public class PrelimMarker
    {
        public void AddPrelimMarks(SevModelEnumerator modelEnum, Model myModel)
        {
            var allParts = modelEnum.SelectedModelParts.Cast<Part>().ToList();
            var groupedParts = allParts.GroupBy(p => new { profile = p.Profile.ProfileString, length = GetPartLength(p), material = p.Material.MaterialString });

            foreach (var gp in groupedParts)
            {
                int currentLastNumber = 0;
                ProjectInfo pInfo = myModel.GetProjectInfo();
                string prismLastNumberAttributeName = "";

                foreach (Part p in gp)
                {
                    prismLastNumberAttributeName = "PRISM" + p.AssemblyNumber.StartNumber;
                    pInfo.GetUserProperty(prismLastNumberAttributeName, ref currentLastNumber);
                    if (currentLastNumber == 0)
                    {
                        Console.WriteLine("Failed to read last number");
                        currentLastNumber = 1;
                        pInfo.SetUserProperty(prismLastNumberAttributeName, currentLastNumber);
                    }
                    else
                    {
                        Console.WriteLine("Last number read" + currentLastNumber);
                    }
                    p.SetUserProperty("PRELIM_MARK", (currentLastNumber + p.AssemblyNumber.StartNumber - 1).ToString());
                }
                currentLastNumber++;
                pInfo.SetUserProperty(prismLastNumberAttributeName, currentLastNumber);
            }
        }

        public double GetPartLength(Part myPart)
        {
            ArrayList points = myPart.GetCenterLine(true);
            Point start = points[0] as Point;
            Point end = points[1] as Point;
            double Length = Distance.PointToPoint(end, start);
            return Length;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Media;
using Tekla.Structures;
using Tekla.Structures.Drawing;
using Tekla.Structures.DrawingInternal;
using Tekla.Structures.Model;
using Drawing = Tekla.Structures.Drawing.Drawing;

namespace Prism
{
    public class PrismDrawing
    {
        public PrismDrawing(string[] information)
        {
            DrawingNumber = information[0].TrimEnd(' ').TrimStart(' ');
            DrawingRevision = information[1].TrimEnd(' ').TrimStart(' ');
            DrawingPartName = information[2].TrimEnd(' ').TrimStart(' ');
            DrawingSize = information[3].TrimEnd(' ').TrimStart(' ');
            IsFrozen = information[5].TrimEnd(' ').TrimStart(' ') == "1";
            IsLocked = information[6].TrimEnd(' ').TrimStart(' ') == "1";
            ChangeMessage = information[7].TrimEnd(' ').TrimStart(' ');
            DrawingType = information[8].TrimEnd(' ').TrimStart(' ');
            DrawingFolder = GetDrawingType(information[4].TrimEnd(' ').TrimStart(' '), DrawingType);
		}

        private Enums.DrawingFolder GetDrawingType(string folder, string type)
        {
            switch (folder)
            {
                case string t when t.Contains("ASS"):
                    return Enums.DrawingFolder.ASS;
                case string t when t.Contains("FIT"):
                    return Enums.DrawingFolder.FIT;
                case string t when t.Contains("SHA"):
                    return Enums.DrawingFolder.SHA;
                case string t when t.Contains("PRT"):
                    return Enums.DrawingFolder.PRT;
                case string t when t.Contains("PGC"):
                    return Enums.DrawingFolder.PGC;
                case string t when t.Contains("WLD"):
                    return Enums.DrawingFolder.WLD;
                case string t when t.Contains("Not Required") && type == "W":
                    return Enums.DrawingFolder.NotRequired;
                case string t when t.Contains("Not Required") && type == "A":
                    return Enums.DrawingFolder.AssNotRequired;
                default:
                    return Enums.DrawingFolder.Default;
            }
        }

        public string DrawingType { get; set; }
        public string DrawingNumber { get; set; }
        public string DrawingRevision { get; set; }
        public string DrawingPartName { get; set; }
        public string DrawingSize { get; set; }
        public bool IsFrozen { get; set; }
        public bool IsLocked { get; set; }
        public string ChangeMessage { get; set; }
        public Enums.DrawingFolder DrawingFolder { get; set; }

    }
}
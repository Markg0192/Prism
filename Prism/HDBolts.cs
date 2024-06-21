using System.Collections.Generic;
using Tekla.Structures.Model;

namespace Prism
{
    public static class HDBolts
    {
        public static void StampConnectionCodeOnMainMember(SelectedObjects selectedObjects)
        {
            foreach (PrismPart prismPart in selectedObjects.PrismParts)
            {
                Beam b = prismPart.Part as Beam;
                ModelObjectEnumerator partConnections = b.GetComponents();
                foreach (var connection in partConnections)
                {
                    if (connection is Connection C)
                    {
                        if (C.Number == 1042)
                        {
                            b.SetUserProperty("CONN_CODE_END1", C.Code);
                        }
                    }
                }
            }
        }

        public static List<PrismPart> GetHdBoltItems(SelectedObjects selectedObjects, bool getBolt)
        {
            List<PrismPart> HdBolts = new List<PrismPart>();
            foreach (PrismPart prismPart in selectedObjects.PrismParts)
            {
                Beam b = prismPart.Part as Beam;
                ModelObjectEnumerator partConnections = b.GetComponents();
                foreach (var connection in partConnections)
                {
                    if (connection is Detail C)
                    {
                        string name = C.Name;
                        if (C.Number == 1042)
                        {
                            ModelObjectEnumerator connectionItems = C.GetChildren();
                            foreach (var item in connectionItems)
                            {
                                string myClass = "4";
                                if (getBolt)
                                {
                                    myClass = "2";
                                }
                              
                                if (item is Part part && part.Class == myClass)
                                {
                                    HdBolts.Add(new PrismPart(part));
                                }
                            }
                        }
                    }
                }
            }
            return HdBolts;
        }
    }
}
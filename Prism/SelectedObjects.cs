using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Tekla.Structures.Drawing;
using Tekla.Structures.Model;
using static Prism.Enums;
using ModelObject = Tekla.Structures.Model.ModelObject;
using Operation = Tekla.Structures.Model.Operations.Operation;
using Part = Tekla.Structures.Model.Part;

namespace Prism
{
    /// <summary>
    /// The SelecedObjects class gets and stores model objects for our use elsewhere.
    /// </summary>
    public class SelectedObjects
    {
        private ModelObjectEnumerator Moe;
        public DrawingHandler MyDrawingHandler;

        public SelectedObjects(StageTypes stageType, string phaseNum, string issueNum, ToolStrip toolStrip = null, ToolStripStatusLabel statusLabel = null)
        {
            NumbersUpToDate = true;
            AssembliesList = new List<Assembly>();
            SelectedModelParts = new List<Part>();
            LockedParts = new List<ModelObject>();
            SeversafeParts = new List<Part>();
            NonSeversafeParts = new List<Part>();
            MyDrawingHandler = new DrawingHandler();

            MyMarks = new List<string>();

            Moe = new Tekla.Structures.Model.UI.ModelObjectSelector().GetSelectedObjects();
            ProcessModelObjects(stageType, phaseNum, issueNum, toolStrip, statusLabel);

            PartWeight = Math.Round(PartWeight / 1000, 3);
        }

        public double SmallestX = 100000000;
        public double SmallestY = 100000000;
        public double SmallestZ = 100000000;
        public double BiggestX = -100000000;
        public double BiggestY = -100000000;
        public double BiggestZ = -100000000;

        public List<PrismBoltGroup> PrismBoltGroups = new List<PrismBoltGroup>();

        public double PartWeight { get; set; }
        public bool NumbersUpToDate { get; set; }
        public bool SeversafePresent = false;
        public List<Assembly> AssembliesList { get; set; }
        public List<Part> SelectedModelParts { get; set; }
        public List<string> MyMarks { get; set; }
        public List<ModelObject> LockedParts { get; set; }
        public List<ModelObject> FabsecParts = new List<ModelObject>();
        public List<ModelObject> NonFabsecParts = new List<ModelObject>();
        public List<Part> SeversafeParts { get; set; }
        public List<Part> NonSeversafeParts { get; set; }
        public List<Part> OmittedParts = new List<Part>();

        private void CheckXYZSize(Part myPart)
        {
            if (myPart is Beam beam)
            {
                SmallestX = Math.Min(beam.EndPoint.X, Math.Min(beam.StartPoint.X, SmallestX));
                SmallestY = Math.Min(beam.EndPoint.Y, Math.Min(beam.StartPoint.Y, SmallestY));
                SmallestZ = Math.Min(beam.EndPoint.Z, Math.Min(beam.StartPoint.Z, SmallestZ));

                BiggestX = Math.Max(beam.EndPoint.X, Math.Max(beam.StartPoint.X, BiggestX));
                BiggestY = Math.Max(beam.EndPoint.Y, Math.Max(beam.StartPoint.Y, BiggestY));
                BiggestZ = Math.Max(beam.EndPoint.Z, Math.Max(beam.StartPoint.Z, BiggestZ));
            }
        }

        private void ProcessModelObjects(StageTypes stageType, string phaseNum, string issueNum, ToolStrip toolStrip, ToolStripStatusLabel statusLabel)
        {
            int currentCount = 0;
            int totalCount = toolStrip == null ? -1 : Moe.GetSize();

            foreach (var myObject in Moe)
            {
                if(totalCount > 0) UpdateStatusLabelWithProcessCount(ref currentCount, toolStrip, statusLabel, totalCount);
                if (!NumbersUpToDate) return;

                // Process directly if RocketPacket or not a BaseComponent
                if (stageType == StageTypes.RocketPacket || !(myObject is BaseComponent myComponent))
                {
                    ProcessObject(myObject, stageType, phaseNum, issueNum);
                }
                else
                {
                    ProcessChildren(myComponent, stageType, phaseNum, issueNum);
                }
            }
        }

        private static void UpdateStatusLabelWithProcessCount(ref int processedCount, ToolStrip toolStrip, ToolStripStatusLabel statusLabel, int totalCount)
        {
            int currentCount = Interlocked.Increment(ref processedCount);
            double progressPercentage = (double)currentCount / totalCount * 100;

            // Throttle UI updates to maintain responsiveness
            if (currentCount % 5 == 0 || currentCount == totalCount)
            {
                toolStrip.Invoke(new System.Action(() =>
                {
                    if (currentCount < totalCount)
                    {
                        statusLabel.Text = $"Processing part: {currentCount} of {totalCount} ({progressPercentage:N1}%)";
                    }
                }));
            }
        }

        private void ProcessChildren(BaseComponent component, StageTypes stageType, string phaseNum, string issueNum)
        {
            var childrenEnumerator = component.GetChildren();
            var allDescendants = new List<object>();

            while (childrenEnumerator.MoveNext())
            {
                var child = childrenEnumerator.Current;
                if (child == null) continue;

                allDescendants.Add(child);

                if (child is BaseComponent componentChild)
                {
                    var grandchildrenEnumerator = componentChild.GetChildren();
                    while (grandchildrenEnumerator.MoveNext())
                    {
                        var grandChild = grandchildrenEnumerator.Current;
                        if (grandChild != null)
                        {
                            allDescendants.Add(grandChild);
                        }
                    }
                }
            }

            foreach (var descendant in allDescendants)
            {
                ProcessObject(descendant, stageType, phaseNum, issueNum);
            }
        }

        private void ProcessObject(object myObject, StageTypes stageType, string phaseNum, string issueNum)
        {
            if (!(myObject is Part myPart) || !IsValidPart(myPart)) return;

            bool isSeversafe = IsSeversafePart(myPart);
            if (stageType == StageTypes.FAB || stageType == StageTypes.RocketPacket)
            {
                CheckXYZSize(myPart);
                if (!isSeversafe && !Operation.IsNumberingUpToDate(myPart))
                {
                    PrismWarnings.NumberingIsNotUpToDate();
                    NumbersUpToDate = false;
                    return;
                }
            }

            CategorizeAndProcessPart(myPart, isSeversafe);
            UpdatePartWeight(myPart);
            AddPartMark(myPart);
            ProcessAssembly(myPart, phaseNum, issueNum);
        }

        private void CategorizeAndProcessPart(Part myPart, bool isSeversafe)
        {
            SelectedModelParts.Add(myPart);
            if (isSeversafe)
            {
                SeversafeParts.Add(myPart);
                SeversafePresent = true;
            }
            else
            {
                if (IsLocked(myPart)) LockedParts.Add(myPart);
                NonSeversafeParts.Add(myPart);
                ProcessFabsecPart(myPart);
            }
        }

        private void UpdatePartWeight(Part myPart)
        {
            double weight = 0;
            if (myPart.GetReportProperty(ModelUDA.Weight(), ref weight))
            {
                PartWeight += weight;
            }
        }

        private void AddPartMark(Part myPart)
        {
            MyMarks.Add(myPart.GetPartMark());
        }

        private void ProcessAssembly(Part myPart, string phaseNum, string issueNum)
        {
            if (myPart.GetAssembly() is Assembly assembly)
            {
                string assemblyId = assembly.Identifier.ToString();
                if (!AssembliesList.Any(x => x.Identifier.ToString() == assemblyId))
                {
                    AssembliesList.Add(assembly);

                    var boltsFromAssembly = GetBoltsFromAssembly(assembly);
                    if (boltsFromAssembly != null)
                    {
                        var prismBoltGroupsForAssembly = boltsFromAssembly
                            .Where(boltGroup => boltGroup.Bolt) // Filter out BoltGroup objects where Bolt is false (Holes)
                            .Select(boltGroup => new PrismBoltGroup(boltGroup, phaseNum, issueNum)) //Cast those bolGroups as PrismBoltGroups
                            .ToList();

                        PrismBoltGroups.AddRange(prismBoltGroupsForAssembly);
                    }
                }
            }
        }

        private void ProcessFabsecPart(Part myPart)
        {
            if (myPart.Profile.ProfileString.StartsWith("PG"))
            {
                FabsecParts.Add(myPart);
            }
            else
            {
                NonFabsecParts.Add(myPart);
            }
        }

        private bool IsSeversafePart(Part myPart)
        {
            if (myPart.Name.Contains("SS-"))
            {
                return true;
            }
            return false;
        }

        private bool IsLocked(Part myPart)
        {
            string isLocked = "";
            myPart.GetReportProperty("OBJECT_LOCKED", ref isLocked);
            if (isLocked == "Yes")
            {
                return true;
            }
            return false;
        }

        private bool IsValidPart(Part p)
        {
            if (p.Name == "GROUT")
            {
                return false;
            }
            if (p.Profile.ProfileString.StartsWith("HEX"))
            {
                return false;
            }
            if (p.Profile.ProfileString.StartsWith("ROD"))
            {
                return false;
            }
            if (p.Name.StartsWith("HD"))
            {
                return false;
            }
            return true;
        }

        public void GetCorrectModelSelection()
        {
            ArrayList selectList = new ArrayList();
            foreach (Part part in SelectedModelParts)
            {
                selectList.Add(part);
            }
            Tekla.Structures.Model.UI.ModelObjectSelector ms = new Tekla.Structures.Model.UI.ModelObjectSelector();
            ms.Select(selectList);
        }

        private static List<BoltGroup> GetBoltsFromAssembly(Assembly assembly)
        {
            List<BoltGroup> myBoltsList = new List<BoltGroup>();
            ArrayList secondaries = assembly.GetSecondaries();
            secondaries.Add(assembly.GetMainPart());

            foreach (Tekla.Structures.Model.ModelObject item in secondaries)
            {
                if (item is Part part)
                {
                    ModelObjectEnumerator bolts = part.GetBolts();

                    foreach (var setOfBolts in bolts)
                    {
                        BoltGroup bolt = setOfBolts as BoltGroup;
                        if (bolt != null)
                        {
                            if (bolt.PartToBeBolted.Identifier.GUID == part.Identifier.GUID)
                            {
                                BoltGroup matchingBolt = null;
                                matchingBolt = myBoltsList.Find(x => x.Identifier.GUID == bolt.Identifier.GUID);
                                if (matchingBolt == null)
                                {
                                    myBoltsList.Add(bolt);
                                }
                            }
                        }
                    }
                }
            }
            return myBoltsList;
        }
    }
}
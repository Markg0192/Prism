using Microsoft.Office.Interop.Excel;
using Prism.CustomDialogs;
using Prism.Geometry;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Windows.Forms;
using Tekla.Structures.Model;
using static Prism.Enum;
using Model = Tekla.Structures.Model.Model;

namespace Prism
{
    public class TeklaHelper
    {
        public static List<MyAssembly> GetSelectedSteelInfo(Model model)
        {
            var selectedComponents = new List<MyAssembly>();

            ModelObjectEnumerator moe = new Tekla.Structures.Model.UI.ModelObjectSelector().GetSelectedObjects();

            var allUnsupportedTypes = new HashSet<string>();

            foreach (var obj in moe)
            {
                if (obj is Part part)
                {
                    if (part.Identifier.GUID == part.GetAssembly().GetMainPart().Identifier.GUID)
                    {
                        var result = MyAssembly.CreateMyAssembly(part, model);
                        selectedComponents.Add(result.Assembly);
                        foreach (var type in result.UnsupportedTypes)
                        {
                            allUnsupportedTypes.Add(type);
                        }
                    }
                }
            }

            if (allUnsupportedTypes.Count > 0)
            {
                var message = "The following fitting types are unsupported and have not been processed across assemblies:\n\n" +
                              string.Join(", ", allUnsupportedTypes) +
                              "\n\nContact help for more information.";

                MessageBox.Show(message);
            }

            return selectedComponents;
        }

        public static List<SteelItemBase> CompareSteelLists(List<MyAssembly> oldList, List<MyAssembly> newList, Dictionary<string, int> oldFittingDictionary, Dictionary<string, int> newFittingDictionary)
        {
            var omittedParts = new List<SteelItemBase>();

            var comparisonResult = IdentifyDifferencesInLists(oldFittingDictionary, newFittingDictionary);
            omittedParts.AddRange(comparisonResult.Where(a => a.Modification == ModificationType.Omit || a.Modification == ModificationType.OmitRevise));

            // Filtering out modifications with ModificationType.Unassigned
            //   var modifications = comparisonResult.Modifications.Where(kvp => kvp.Value != ModificationType.Unassigned).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            var (differenceMessages, omittedAssemblies) = DetectPartMarkCountDifferences(oldList, newList, comparisonResult);

            DetectChangesInExistingMembers(oldList, newList, comparisonResult); // this gets me all the changes not related to number of assemblies.

            omittedParts.AddRange(omittedAssemblies);

            return omittedParts;
        }

        private static List<MyFitting> IdentifyDifferencesInLists(Dictionary<string, int> oldPartMarkCounts, Dictionary<string, int> newPartMarkCounts)
        {
            var modifications = new Dictionary<string, ModificationType>();
            var messages = new List<string>();
            var modifiedParts = new List<MyFitting>();
            var addedFittings = new List<MyFitting>();

            foreach (var kvp in oldPartMarkCounts)
            {
                var partMark = kvp.Key;
                var oldCount = kvp.Value;
                newPartMarkCounts.TryGetValue(partMark, out int newCount);

                if (oldCount != newCount)
                {
                    int numberOfChanged = newCount - oldCount;

                    if (newCount == 0)
                    {
                        MyFitting fitting = new MyFitting();
                        fitting.PartMark = partMark;
                        fitting.ChangeMessages.Add($"Fitting number {partMark} has been completely removed.");
                        fitting.Modification = ModificationType.Omit;
                        modifiedParts.Add(fitting);
                    }
                    else if (newCount < oldCount)
                    {
                        MyFitting fitting = new MyFitting();
                        fitting.PartMark = partMark;
                        fitting.ChangeMessages.Add($"Fitting number {partMark} has reduced in number by {numberOfChanged}.");
                        fitting.Modification = ModificationType.OmitRevise;

                        modifications[partMark] = ModificationType.OmitRevise;
                        modifiedParts.Add(fitting);
                    }
                    else // newCount > oldCount
                    {
                        MyFitting fitting = new MyFitting();
                        fitting.PartMark = partMark;
                        fitting.ChangeMessages.Add($"Fitting number {partMark} has increased in number by {numberOfChanged}.");
                        fitting.Modification = ModificationType.AddRevise;
                        modifiedParts.Add(fitting);

                        addedFittings.Add(fitting);
                        modifications[partMark] = ModificationType.Add;
                        messages.Add($"Fitting number {partMark} has increased in number by {numberOfChanged}.");
                    }
                }
            }

            //this area needs tidied, look at maybe need add, and a new mod type, AddRevise, similar to omit revise.

            foreach (var kvp in newPartMarkCounts)
            {
                var partMark = kvp.Key;
                if (!oldPartMarkCounts.ContainsKey(partMark))
                {
                    MyFitting fitting = new MyFitting();
                    fitting.PartMark = partMark;
                    fitting.ChangeMessages.Add($"Fitting number {partMark} is new.");
                    fitting.Modification = ModificationType.Add;
                    modifiedParts.Add(fitting);

                    messages.Add($"Fitting number {partMark} is new.");
                    modifications[partMark] = ModificationType.Add;
                }
            }

            return modifiedParts;


        }

        private static List<string> IdentifyDifferencesInLists(Dictionary<string, int> oldPartMarkCounts, Dictionary<string, int> newPartMarkCounts, MyAssembly assembly)
        {
            var messages = new List<string>();

            foreach (var kvp in oldPartMarkCounts)
            {
                var partMark = kvp.Key;
                var oldCount = kvp.Value;
                newPartMarkCounts.TryGetValue(partMark, out int newCount);

                if (oldCount != newCount)
                {
                    int numberOfChanged = newCount - oldCount;
                    assembly.Modification = ModificationType.Revise;
                    if (newCount == 0)
                    {
                        assembly.ChangeMessages.Add($"Fitting number {partMark} has been completely removed from assembly {assembly.PartMark}.");
                        messages.Add($"Fitting number {partMark} has been completely removed from assembly {assembly.PartMark}.");
                    }
                    else if (newCount < oldCount)
                    {
                        assembly.ChangeMessages.Add($"Fitting number {partMark} has reduced in number by {numberOfChanged} on assembly {assembly.PartMark}.");
                        messages.Add($"Fitting number {partMark} has reduced in number by {numberOfChanged} on assembly {assembly.PartMark}.");
                    }
                    else // newCount > oldCount
                    {
                        assembly.ChangeMessages.Add($"Fitting number {partMark} has increased in number by {numberOfChanged} on assembly {assembly.PartMark}.");
                        messages.Add($"Fitting number {partMark} has increased in number by {numberOfChanged} on assembly {assembly.PartMark}.");
                    }
                }
            }

            foreach (var kvp in newPartMarkCounts)
            {
                var partMark = kvp.Key;
                if (!oldPartMarkCounts.ContainsKey(partMark))
                {
                    messages.Add($"Fitting number {partMark} is new and has been added to assembly {assembly.PartMark}.");
                    assembly.ChangeMessages.Add($"Fitting number {partMark} is new and has been added to assembly");
                    assembly.Modification = ModificationType.Revise;
                }
            }

            return messages;
        }

        public class ComparisonResult
        {
            public List<MyFitting> OmittedFittings = new List<MyFitting>();
            public List<MyFitting> AddedFittings = new List<MyFitting>();
            public Dictionary<string, ModificationType> Modifications { get; set; }
            public List<string> Messages { get; set; }

        }

        private static (List<string> Messages, List<MyAssembly> omittedAssemblies) IdentifyDifferencesInLists(Dictionary<string, int> oldPartMarkCounts, Dictionary<string, int> newPartMarkCounts, List<MyAssembly> newList, List<MyFitting> comparisonResult)
        {
            List<MyAssembly> omittedAssemblies = new List<MyAssembly>();
            var messages = new List<string>();

            List<MyAssembly> newStuff = new List<MyAssembly>();
            // Handle the case where new assemblies are added which were not in the old list
            foreach (var kvp in newPartMarkCounts)
            {
                var partMark = kvp.Key;
                var newCount = kvp.Value;

                if (!oldPartMarkCounts.ContainsKey(partMark))
                {
                    var assembliesToUpdate = newList.Where(x => x.PartMark == partMark);
                    foreach (MyAssembly assembly in assembliesToUpdate)
                    {
                        if(assembly.Fittings.Count > 0) assembly.ChangeMessages.Add($"{newCount} No. added each containing:");
                        foreach (var fitting in assembly.Fittings)
                        {
                            assembly.ChangeMessages.Add($"Fitting - {fitting.PartMark} - {fitting.ProfileString}");
                        }
                        assembly.Modification = ModificationType.Add; // This is a brand new piece so is an add

                        foreach (var fitting in assembly.Fittings)
                        {
                            // Find a matching item in the comparisonList based on PartMark
                            var matchingFitting = comparisonResult.FirstOrDefault(f => f.PartMark == fitting.PartMark);

                            if (matchingFitting != null)
                            {
                                // Set fitting.ChangeMessage to the ChangeMessage of the matching item
                                fitting.ChangeMessages = matchingFitting.ChangeMessages;
                                fitting.Modification = matchingFitting.Modification;
                            }
                        }
                    }
                }
            }

            foreach (var kvp in oldPartMarkCounts)
            {
                var partMark = kvp.Key;
                var oldCount = kvp.Value;

                newPartMarkCounts.TryGetValue(partMark, out int newCount);

                if (oldCount != newCount)
                {
                    if (newCount == 0) //This piece no longer exists so is a full omit
                    {
                        MyAssembly ass = new MyAssembly();
                        ass.PartMark = partMark;
                        ass.ChangeMessages.Add("This member has been completly removed.");
                        omittedAssemblies.Add(ass);

                        messages.Add($"Beam number {partMark} has been completely removed.");
                    }
                    else
                    {
                        var assembliesToUpdate = newList.Where(x => x.PartMark == partMark);
                        var assembly = assembliesToUpdate.First();
                        {
                            int numberOfChange = Math.Abs(newCount - oldCount);
                            if (newCount < oldCount) //The number of pieces has been reduced but not fully removed, an omit that requires a revised drawing
                            {
                                assembly.Modification = ModificationType.Revise;
                                messages.Add($"Beam number {assembly.PartMark} has reduced in number by {numberOfChange}.");

                                MyAssembly ass = new MyAssembly();
                                ass.PartMark = partMark;
                                ass.ChangeMessages.Add($"Member has reduced in number by {numberOfChange}.");                                
                                omittedAssemblies.Add(ass);
                                assembly.ChangeMessages.Add($"Member has reduced in number by {numberOfChange}.");
                            }
                            else // newCount > oldCount more parts than before, an add
                            {
                                assembly.ChangeMessages.Add($"Member has increased in number by {numberOfChange}.");
                                messages.Add($"Beam number {assembly.PartMark} has increased in number by {numberOfChange}.");
                                assembly.Modification = ModificationType.Add;
                            }
                        }
                    }
                }
            }

            return (messages, omittedAssemblies);
        }

        private static (List<string> Messages, List<MyAssembly> omittedAssemblies) DetectPartMarkCountDifferences(List<MyAssembly> oldList, List<MyAssembly> newList, List<MyFitting> comparisonResult)
        {
            var oldPartMarkCounts = oldList.GroupBy(x => x.PartMark).ToDictionary(g => g.Key, g => g.Count());
            var newPartMarkCounts = newList.GroupBy(x => x.PartMark).ToDictionary(g => g.Key, g => g.Count());
            return IdentifyDifferencesInLists(oldPartMarkCounts, newPartMarkCounts, newList, comparisonResult);
        }

        private static List<string> DetectPartMarkCountDifferences(List<MyFitting> oldList, List<MyFitting> newList, MyAssembly assembly)
        {
            var oldPartMarkCounts = oldList.GroupBy(x => x.PartMark).ToDictionary(g => g.Key, g => g.Count());
            var newPartMarkCounts = newList.GroupBy(x => x.PartMark).ToDictionary(g => g.Key, g => g.Count());
            return IdentifyDifferencesInLists(oldPartMarkCounts, newPartMarkCounts, assembly);
        }

        private static IEnumerable<string> DetectChangesInExistingMembers(List<MyAssembly> oldList, List<MyAssembly> newList, List<MyFitting> fittingCollection)
        {
            var messages = new List<string>();

            foreach (var oldAssembly in oldList)
            {
                var matchingNewAssembly = newList.FirstOrDefault(x => x.Guid == oldAssembly.Guid);
                if (matchingNewAssembly != null) //then there is an assembly number in the new list that matches on from the old list.
                {
                    DetectPartMarkCountDifferences(oldAssembly.Fittings, matchingNewAssembly.Fittings, matchingNewAssembly);
                    messages.AddRange(CompareProperties(oldAssembly, matchingNewAssembly, matchingNewAssembly)); //Compare all myAssembly properties

                    messages.AddRange(CompareBoltsWeldsAndCuts(oldAssembly, matchingNewAssembly, matchingNewAssembly));

                    messages.AddRange(DetectChangesInExistingFittings(oldAssembly.Fittings, matchingNewAssembly.Fittings, matchingNewAssembly, fittingCollection));
                }
            }

            return messages;
        }

        private static IEnumerable<string> DetectChangesInExistingFittings(List<MyFitting> oldList, List<MyFitting> newList, MyAssembly newAssembly, List<MyFitting> modifications)
        {
            var messages = new List<string>();

            foreach (var newFitting in newList) // Iterating over newList to check every new fitting against modifications dictionary.
            {
                MyFitting matchingFitting = modifications.FirstOrDefault(x => x.PartMark == newFitting.PartMark);

                if (matchingFitting != null) // Check if the PartMark is in the modifications dictionary
                {
                    newFitting.Modification = matchingFitting.Modification;
                    newFitting.ChangeMessages = matchingFitting.ChangeMessages;// If it is, update the fitting's Modification property with the value from the dictionary
                }
            }

            foreach (var oldFitting in oldList)
            {
                var matchingNewFitting = newList.FirstOrDefault(x => x.Guid == oldFitting.Guid && x.PartMark == oldFitting.PartMark);
                if (matchingNewFitting != null) // Then there is an assembly number in the new list that matches one from the old list.
                {
                    messages.AddRange(CompareProperties(oldFitting, matchingNewFitting, newAssembly, matchingNewFitting)); // Compare all myAssembly properties
                    messages.AddRange(CompareBoltsWeldsAndCuts(oldFitting, matchingNewFitting, newAssembly, matchingNewFitting));
                }
            }

            return messages;
        }

        private static IEnumerable<string> CompareBoltsWeldsAndCuts(SteelItemBase oldObjectForComparison, SteelItemBase newObjectForComparison, SteelItemBase newAssembly, SteelItemBase newFitting = null)
        {
            var messages = new List<string>();

            // Compare bolts and add messages to list
            messages.AddRange(CompareItems(oldObjectForComparison.Bolts, newObjectForComparison.Bolts, (b, id) => b.Guid == id, newAssembly, newFitting));

            //Compare cuts and add messages to list
            messages.AddRange(CompareItems(oldObjectForComparison.Cuts, newObjectForComparison.Cuts, (c, id) => c.Guid == id, newAssembly, newFitting));

            // Compare welds and add messages to list
            messages.AddRange(CompareItems(oldObjectForComparison.Welds, newObjectForComparison.Welds, (w, id) => w.Guid == id, newAssembly));

            return messages;
        }

        private static IEnumerable<string> CompareItems<T>(IEnumerable<T> oldItems, IEnumerable<T> newItems, Func<T, string, bool> idSelector, SteelItemBase newAssembly, SteelItemBase newFitting = null)
        {
            var messages = new List<string>();
            PropertyInfo checkedProperty = typeof(T).GetProperty("Checked");

            foreach (var oldItem in oldItems)
            {
                var id = typeof(T).GetProperty("Guid")?.GetValue(oldItem)?.ToString();
                if (id == null)
                    continue; // or throw an exception depending on your use case

                var matchingNewItem = newItems.FirstOrDefault(item => idSelector(item, id));

                if (matchingNewItem != null)
                {
                    bool checkedStatus = (bool?)checkedProperty?.GetValue(matchingNewItem) ?? false;
                    if (!checkedStatus)
                    {
                        var itemMessages = CompareProperties(oldItem, matchingNewItem, newAssembly, newFitting);
                        if (itemMessages != null)
                        {
                            messages.AddRange(itemMessages);
                        }
                    }
                    checkedProperty.SetValue(matchingNewItem, true);
                }
            }

            return messages;
        }

        private static Dictionary<string, int> CountFittingsByMark(List<MyFitting> fittings)
        {
            return fittings.GroupBy(f => f.PartMark).ToDictionary(g => g.Key, g => g.Count());
        }

        private static IEnumerable<string> CompareProperties<T>(T oldObject, T newObject, SteelItemBase newAssembly, SteelItemBase newFitting = null)
        {
            string teset = oldObject.GetType().Name;
            var messages = new List<string>();
            var properties = typeof(T).GetProperties(); // Get all properties of the type
            double pointMove = 0;
            bool changeMade = false;

            foreach (var property in properties)
            {
                if (property.Name == "Modification")
                    continue;

                if(property.Name.Contains("ContourPoint"))
                {
                    Console.WriteLine("");
                }

                var oldValue = property.GetValue(oldObject);
                var newValue = property.GetValue(newObject);

                if (oldValue == null && newValue == null)
                    continue;

                string type = GetTypeOfProperty(property);

                // If the property is IEnumerable, but not string, handle it separately
                if (property.PropertyType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
                {
                    string test = oldObject.GetType().Name;
                    if(oldObject.GetType().FullName == "")
                    { }
                    var oldEnumerable = oldValue as IEnumerable;
                    var newEnumerable = newValue as IEnumerable;
                    if (oldEnumerable == null && newEnumerable == null) continue;

                    var oldList = oldEnumerable?.Cast<object>().ToList() ?? new List<object>();
                    var newList = newEnumerable?.Cast<object>().ToList() ?? new List<object>();
                     
                    // Determine the type of the elements in the IEnumerable
                    Type elementType = property.PropertyType.IsGenericType
                                        ? property.PropertyType.GetGenericArguments()[0]
                                        : typeof(object);

                    if (!oldList.SequenceEqual(newList))
                    {
                        string partMark = newFitting == null ? newAssembly.PartMark : newFitting.PartMark;

                        // Logic for handling differences in collections
                        string changeMessage = GetMyChangeMessage(property, oldValue, newValue, ref pointMove, type); //$"{type} {property.Name} collection has changed";
                        
                        if (newFitting == null)
                        {
                            newAssembly.ChangeMessages.Add(changeMessage);
                        }
                        else
                        {
                            newFitting.ChangeMessages.Add(changeMessage);
                        }
                        changeMade = true;
                    }
                    continue;
                }

                if ((oldValue == null && newValue != null) ||
                    (oldValue != null && newValue == null) ||
                    (oldValue != null && !oldValue.Equals(newValue)))
                {
                    string changeMessage = GetMyChangeMessage(property, oldValue, newValue, ref pointMove, type);

                    // Add the change message if it's not empty
                    if (!string.IsNullOrEmpty(changeMessage))
                    {
                        if (newFitting == null)
                        {
                            newAssembly.ChangeMessages.Add(changeMessage);
                        }
                        else
                        {
                            newFitting.ChangeMessages.Add(changeMessage);
                        }

                        changeMade = true;
                    }
                }
            }

            if (changeMade)
            {
                newAssembly.Modification = ModificationType.Revise;
                if (newFitting != null) newFitting.Modification = ModificationType.Revise;
            }

            return messages;
        }

        private static string HandleCutChangeProperty(string propertyName)
        {
            switch (propertyName)
            {
                case "CutContourPoints":
                    return "Cut contourPoints have moved.";
                case "BooleanPlane":
                    return "A cut has moved.";
                default:
                    return "A cut property has be changed";
            }
        }

        private static string GetMyChangeMessage(PropertyInfo property, object oldValue, object newValue, ref double pointMove, string type)
        {
            string changeMessage = "";
            string measurement = DefineMeasurement(property.Name);

            if (type.Contains("Bolt"))
            {
                changeMessage = HandleBoltPropertyChange(property.Name, oldValue, newValue, ref pointMove, type, measurement);
            }
            else if(type.Contains("Cut"))
            {
                changeMessage = HandleCutChangeProperty(property.Name);
            }
            else 
            {
                changeMessage = $"{type} {property.Name} has changed from {oldValue ?? "null"}{measurement} to {newValue ?? "null"}{measurement}";
            }
            return changeMessage;
        }

        private static string HandleBoltPropertyChange(string propertyName, object oldValue, object newValue, ref double pointMove, string type, string measurement)
        {
            switch (propertyName)
            {
                case "StartPoint":
                case "EndPoint":
                    pointMove += BoltMoveDistance(oldValue as Point3D, newValue as Point3D);
                    if (propertyName == "EndPoint")
                    {
                        return $"Holes have moved approx {pointMove / 2}mm";
                    }
                    return "";

                case "DistX":
                    return $"The X spacings of a hole array have changed from {oldValue} to {newValue}.";

                case "DistY":
                    return $"The Y spacings of a hole array have changed from {oldValue} to {newValue}.";

                default:
                    return $"Hole {propertyName} has changed from {oldValue ?? "null"}{measurement} to {newValue ?? "null"}{measurement}";
            }
        }



        private static string DefineMeasurement(string propertyName)
        {
            string measurement = "";
            if (propertyName == "Length") measurement = "mm";
            if (propertyName == "Weight") measurement = "kg";
            if (propertyName == "BoltDiameter" || propertyName == "HoleDiameter") measurement = "ø";

            return measurement;
        }

        private static double BoltMoveDistance(Point3D oldPoint, Point3D newPoint)
        {
            return Distances.Point2Point(newPoint, oldPoint);
        }

        private static string GetTypeOfProperty(PropertyInfo prop)
        {
            if (prop.DeclaringType.ToString().Contains("Bolt"))
            {
                return "Bolt";
            }
            if (prop.DeclaringType.ToString().Contains("Cut"))
            {
                return "Cut";
            }
            if (prop.DeclaringType.ToString().Contains("Weld"))
            {
                return "Weld";
            }
            return "";
        }


        /*  private static IEnumerable<string> CompareBolts(SteelItemBase oldPart, SteelItemBase newPart, SteelItemBase newPartsMainPart = null)
          {
              var messages = new List<string>();

              foreach (var oldBolt in oldPart.Bolts)
              {
                  var matchingNewBolt = newPart.Bolts.FirstOrDefault(b => b.Id == oldBolt.Id); // Assuming a unique Id property on bolts

                  if (matchingNewBolt != null)
                  {
                      var boltMessages = CompareBoltProperties(oldBolt, matchingNewBolt, newPart, newPartsMainPart);

                      //   var boltMessages = CompareProperties(oldBolt, matchingNewBolt);

                      if (boltMessages != null)
                      {
                          messages.AddRange(boltMessages);
                      }
                  }
              }

              return messages;
          }

          private static IEnumerable<string> CompareWelds(SteelItemBase oldAssembly, SteelItemBase newAssembly)
          {
              var messages = new List<string>();

              foreach (var oldWeld in oldAssembly.Welds)
              {
                  var matchingNewWeld = newAssembly.Welds.FirstOrDefault(b => b.Id == oldWeld.Id); // Assuming a unique Id property on bolts

                  if (matchingNewWeld != null)
                  {
                      //  var boltMessages = CompareBoltProperties(oldBolt, matchingNewBolt);

                      var weldMessages = CompareProperties(oldWeld, matchingNewWeld);

                      if (weldMessages != null)
                      {
                          messages.AddRange(weldMessages);
                      }
                  }
              }

              return messages;
          }*/

        /* private static IEnumerable<string> DetectPropertyChanges(SteelItemBase oldPart, SteelItemBase newPart, SteelItemBase newPartsMainPart = null)
         {
             var messages = new List<string>();

             // Checking for any changes in Name
             AddIfNotNull(messages, Check.HasNameChanged(oldPart, newPart));

             // Checking for any changes in Length
             AddIfNotNull(messages, Check.HasLengthChanged(oldPart, newPart));

             // Checking for any changes in the Start Point
             AddIfNotNull(messages, Check.HasStartPointChanged(oldPart, newPart));

             // Checking for any changes in the End Point
             AddIfNotNull(messages, Check.HasEndPointChanged(oldPart, newPart));

             // Checking for any changes in Profile String
             AddIfNotNull(messages, Check.HasProfileStringChanged(oldPart, newPart));

             // Checking for any changes in Weight
             AddIfNotNull(messages, Check.HasWeightChanged(oldPart, newPart));

             // Checking for any changes in Material
             AddIfNotNull(messages, Check.HasMaterialChanged(oldPart, newPart));

             // Checking for any changes in Finish
             AddIfNotNull(messages, Check.HasFinishChanged(oldPart, newPart));

             // Checking for any changes in FireDFT
             AddIfNotNull(messages, Check.HasFireDFTChanged(oldPart, newPart));

             // Checking for any changes in FireWFT
             AddIfNotNull(messages, Check.HasFireWFTChanged(oldPart, newPart));

             // Checking for any changes in Execution Class
             AddIfNotNull(messages, Check.HasExecutionClassChanged(oldPart, newPart));

             // Checking for any changes in OnPlane
             AddIfNotNull(messages, Check.HasOnPlaneChanged(oldPart, newPart));

             // Checking for any changes in Rotation
             AddIfNotNull(messages, Check.HasRotationChanged(oldPart, newPart));

             // Checking for any changes in Depth
             AddIfNotNull(messages, Check.HasDepthChanged(oldPart, newPart));

             // Checking for any changes in OnPlaneOffset
             AddIfNotNull(messages, Check.HasOnPlaneOffsetChanged(oldPart, newPart));

             // Checking for any changes in RotationOffset
             AddIfNotNull(messages, Check.HasRotationOffsetChanged(oldPart, newPart));

             // Checking for any changes in DepthOffset
             AddIfNotNull(messages, Check.HasDepthOffsetChanged(oldPart, newPart));

             // Checking for any changes in StartDx, StartDy, StartZ
             AddIfNotNull(messages, Check.HasStartCoordinatesChanged(oldPart, newPart));

             // Checking for any changes in EndDx, EndDy, EndZ
             AddIfNotNull(messages, Check.HasEndCoordinatesChanged(oldPart, newPart));

             // Checking for any changes in StartWarp
             AddIfNotNull(messages, Check.HasStartWarpChanged(oldPart, newPart));

             // Checking for any changes in EndWarp
             AddIfNotNull(messages, Check.HasEndWarpChanged(oldPart, newPart));

             // Checking for any changes in Cambering
             AddIfNotNull(messages, Check.HasCamberingChanged(oldPart, newPart));

             // Checking for any changes in Shortening
             AddIfNotNull(messages, Check.HasShorteningChanged(oldPart, newPart));

             // Checking for any changes in CurvedPlane
             AddIfNotNull(messages, Check.HasCurvedPlaneChanged(oldPart, newPart));

             // Checking for any changes in CurvedRadius
             AddIfNotNull(messages, Check.HasCurvedRadiusChanged(oldPart, newPart));

             // Checking for any changes in CurvedSegments
             AddIfNotNull(messages, Check.HasCurvedSegmentsChanged(oldPart, newPart));

             if (messages.Count > 0)
             {
                 newPart.Modification = ModificationType.Revise;
                 if (newPartsMainPart != null) { newPartsMainPart.Modification = ModificationType.Revise; }
             }

             return messages;
         }

         private static IEnumerable<string> CompareBoltProperties(MyBolts oldBolt, MyBolts newBolt, SteelItemBase newPart, SteelItemBase newPartsMainPart)
         {
             var messages = new List<string>();

             // Checking for any changes in Quantity
             AddIfNotNull(messages, BoltCheck.HasQuantityChanged(oldBolt, newBolt));

             // Checking for any changes in Standard
             AddIfNotNull(messages, BoltCheck.HasStandardChanged(oldBolt, newBolt));

             // Checking for any changes in Type
             AddIfNotNull(messages, BoltCheck.HasTypeChanged(oldBolt, newBolt));

             // Checking for any changes in StartDx
             AddIfNotNull(messages, BoltCheck.HasStartDxChanged(oldBolt, newBolt));

             // Checking for any changes in StartDy
             AddIfNotNull(messages, BoltCheck.HasStartDyChanged(oldBolt, newBolt));

             // Checking for any changes in StartDz
             AddIfNotNull(messages, BoltCheck.HasStartDzChanged(oldBolt, newBolt));

             // Checking for any changes in EndDx
             AddIfNotNull(messages, BoltCheck.HasEndDxChanged(oldBolt, newBolt));

             // Checking for any changes in EndDy
             AddIfNotNull(messages, BoltCheck.HasEndDyChanged(oldBolt, newBolt));

             // Checking for any changes in EndDz
             AddIfNotNull(messages, BoltCheck.HasEndDzChanged(oldBolt, newBolt));

             // Checking for any changes in Shape
             AddIfNotNull(messages, BoltCheck.HasShapeChanged(oldBolt, newBolt));

             // Checking for any changes in DistX
             AddIfNotNull(messages, BoltCheck.HasDistXChanged(oldBolt, newBolt));

             // Checking for any changes in DistY
             AddIfNotNull(messages, BoltCheck.HasDistYChanged(oldBolt, newBolt));

             // Checking for any changes in Diameter
             AddIfNotNull(messages, BoltCheck.HasDiameterChanged(oldBolt, newBolt));

             // Checking for any changes in ConnectAs
             AddIfNotNull(messages, BoltCheck.HasConnectAsChanged(oldBolt, newBolt));

             // Checking for any changes in ThreadInMaterial
             AddIfNotNull(messages, BoltCheck.HasThreadInMaterialChanged(oldBolt, newBolt));

             // Checking for any changes in CutLength
             AddIfNotNull(messages, BoltCheck.HasCutLength(oldBolt, newBolt));

             // Checking for any changes in ExtraLength
             AddIfNotNull(messages, BoltCheck.HasExtraLength(oldBolt, newBolt));

             // Checking for any changes in BoltOn
             AddIfNotNull(messages, BoltCheck.HasBoltOnChanged(oldBolt, newBolt));

             // Checking for any changes in Washer1
             AddIfNotNull(messages, BoltCheck.HasWasher1Changed(oldBolt, newBolt));

             // Checking for any changes in Washer2
             AddIfNotNull(messages, BoltCheck.HasWasher2Changed(oldBolt, newBolt));

             // Checking for any changes in Washer3
             AddIfNotNull(messages, BoltCheck.HasWasher3Changed(oldBolt, newBolt));

             // Checking for any changes in Nut1
             AddIfNotNull(messages, BoltCheck.HasNut1Changed(oldBolt, newBolt));

             // Checking for any changes in Nut2
             AddIfNotNull(messages, BoltCheck.HasNut2Changed(oldBolt, newBolt));

             // Checking for any changes in CircleNumberOfBolts
             AddIfNotNull(messages, BoltCheck.HasCircleNumberOfBoltsChanged(oldBolt, newBolt));

             // Checking for any changes in CircleDiameter
             AddIfNotNull(messages, BoltCheck.HasCircleDiamterChanged(oldBolt, newBolt));

             // Checking for any changes in Tolerance
             AddIfNotNull(messages, BoltCheck.HasToleranceChanged(oldBolt, newBolt));

             // Checking for any changes in PlainHoleType
             AddIfNotNull(messages, BoltCheck.HasPlainHoleTypeChanged(oldBolt, newBolt));

             // Checking for any changes in SlottedHoleX
             AddIfNotNull(messages, BoltCheck.HasSlottedHoleXChanged(oldBolt, newBolt));

             // Checking for any changes in SlottedHoleY
             AddIfNotNull(messages, BoltCheck.HasSlottedHoleYChanged(oldBolt, newBolt));

             // Checking for any changes in RotateSlots
             AddIfNotNull(messages, BoltCheck.HasRotateSlotesChanged(oldBolt, newBolt));

             // Checking for any changes in OnPlane
             AddIfNotNull(messages, BoltCheck.HasOnPlaneChanged(oldBolt, newBolt));

             // Checking for any changes in Rotation
             AddIfNotNull(messages, BoltCheck.HasRotationChanged(oldBolt, newBolt));

             // Checking for any changes in RotationDegree
             AddIfNotNull(messages, BoltCheck.HasRotationDegreeChanged(oldBolt, newBolt));

             // Checking for any changes in StartPoint
             AddIfNotNull(messages, BoltCheck.HasStartPointChanged(oldBolt, newBolt));

             // Checking for any changes in EndPoint
             AddIfNotNull(messages, BoltCheck.HasEndPointChanged(oldBolt, newBolt));

             if (messages.Count > 0)
             {
                 newPart.Modification = ModificationType.Revise;
                 if (newPartsMainPart != null) newPartsMainPart.Modification = ModificationType.Revise;
             }
             return messages;
         }*/

        public static string GetStringAttribute(ModelObject mObj, string attribute)
        {
            string value = "";
            mObj.GetReportProperty(attribute, ref value);
            return value;
        }

        public static double GetDoubleAttribute(ModelObject mObj, string attribute)
        {
            double value = 0.0;
            mObj.GetReportProperty(attribute, ref value);
            return value;
        }

        public static int GetIntAttribute(ModelObject mObj, string attribute)
        {
            int value = 0;
            mObj.GetReportProperty(attribute, ref value);
            return value;
        }

        private static void AddIfNotNull(List<string> messages, string message)
        {
            if (message != null)
            {
                messages.Add(message);
            }
        }


    }
}
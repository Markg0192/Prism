using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tekla.Structures.Drawing;
using Tekla.Structures.Model;
using static Prism.Enums;
using ModelObject = Tekla.Structures.Model.ModelObject;
using Operation = Tekla.Structures.Model.Operations.Operation;
using Part = Tekla.Structures.Model.Part;
using Task = System.Threading.Tasks.Task;

namespace Prism
{
	/// <summary>
	/// The SelecedObjects class gets and stores model objects for our use elsewhere.
	/// </summary>
	public class SelectedObjects
	{
		private ModelObjectEnumerator Moe;
		public DrawingHandler MyDrawingHandler;

		public SelectedObjects(string projectPath, StageTypes stageType, string phaseNum, string issueNum, Model model, bool isSpecialUser, ToolStrip toolStrip = null, ToolStripStatusLabel statusLabel = null)
		{
			Model = model;
			NumbersUpToDate = true;
			MyDrawingHandler = new DrawingHandler();

			PrismParts = new List<PrismPart>();
			PrismDrawings = new List<PrismDrawing>();

			MyMarks = new List<string>();

			bool runNewMethod = true;

		/*	if (isSpecialUser)
			{
				runNewMethod = PrismWarnings.TryTheNewSelectionMethod();
			}*/
			if (runNewMethod)
			{
				CreatePartListFromReport(projectPath, model, phaseNum, issueNum, stageType);
				UpdatePartWeights(PrismParts);
			}
			else
			{
				Moe = new Tekla.Structures.Model.UI.ModelObjectSelector().GetSelectedObjects();
				ProcessModelObjects(stageType, phaseNum, issueNum, toolStrip, statusLabel);
			}

			MainPartWeight = Math.Round(MainPartWeight / 1000, 3);
			FittingWeight = Math.Round(FittingWeight / 1000, 3);
		}

		public SelectedObjects(string projectPath, StageTypes stageType, string phaseNum, string issueNum, Model model, bool isSpecialUser, Action<int, string> progress, ToolStrip toolStrip = null, ToolStripStatusLabel statusLabel = null)
		{
			Model = model;
			NumbersUpToDate = true;
			MyDrawingHandler = new DrawingHandler();

			PrismParts = new List<PrismPart>();
			PrismDrawings = new List<PrismDrawing>();
			MyMarks = new List<string>();

			bool runNewMethod = true;

			if (runNewMethod)
			{
				progress?.Invoke(6, "Creating Tekla selection report...");

				CreatePartListFromReport(projectPath, model, phaseNum, issueNum, stageType, progress);

				progress?.Invoke(11, "Calculating selected part weights...");

				UpdatePartWeights(PrismParts);
			}
			else
			{
				Moe = new Tekla.Structures.Model.UI.ModelObjectSelector().GetSelectedObjects();
				ProcessModelObjects(stageType, phaseNum, issueNum, toolStrip, statusLabel);
			}

			MainPartWeight = Math.Round(MainPartWeight / 1000, 3);
			FittingWeight = Math.Round(FittingWeight / 1000, 3);
		}

		private void CreatePartListFromReport(string packagePath, Model model, string phaseNum, string issueNum, StageTypes stageType, Action<int, string> progress = null)
		{
			Stopwatch totalStopwatch = Stopwatch.StartNew();
			Dictionary<string, long> prismPartTimings = new Dictionary<string, long>();
			Action<string, long> recordPrismPartTiming = delegate (string name, long ticks)
			{
				if (!prismPartTimings.ContainsKey(name))
				{
					prismPartTimings[name] = 0;
				}

				prismPartTimings[name] += ticks;
			};

			string teklaReportLocation = Path.Combine(FirmFolderLoc.ReportTemplates(), "PrismPart_List.rpt");
			string newReportLocation = Path.Combine(packagePath, "PrismPart_List.xsr");

			progress?.Invoke(6, "Generating Tekla selection report...");

			if (!CreateReportAndWait(teklaReportLocation, newReportLocation))
			{
				return;
			}

			progress?.Invoke(7, "Waiting for Tekla report...");

			if (!IfLockedWait(newReportLocation, 15))
			{
				return;
			}

			progress?.Invoke(8, "Reading selected objects...");

			Stopwatch readStopwatch = Stopwatch.StartNew();

			List<string[]> reportObjects = File.ReadLines(newReportLocation)
				.Select(line => line.Split(','))
				.Where(items => items.Length > 0 && (items[0] == " Part" || items[0] == " Bolt"))
				.ToList();

			readStopwatch.Stop();

			int totalObjects = reportObjects.Count;
			int objectCount = 0;
			int partCount = 0;
			int boltCount = 0;
			int lastReportedStep = -1;
			long checkXyzTicks = 0;
			long boltCreationTicks = 0;

			foreach (string[] items in reportObjects)
			{
				objectCount++;

				if (items[0] == " Part")
				{
					PrismPart newPart = new PrismPart(items, model, stageType, recordPrismPartTiming);

					PrismParts.Add(newPart);

					long xyzStarted = Stopwatch.GetTimestamp();
					CheckXYZSize(newPart.Part);
					checkXyzTicks += Stopwatch.GetTimestamp() - xyzStarted;

					partCount++;
				}
				else if (items[0] == " Bolt")
				{
					long boltStarted = Stopwatch.GetTimestamp();
					PrismBoltGroup boltGroup = new PrismBoltGroup(items, phaseNum, issueNum, model, recordPrismPartTiming);
					boltCreationTicks += Stopwatch.GetTimestamp() - boltStarted;

					if (boltGroup != null)
					{
						PrismBoltGroups.Add(boltGroup);
						boltCount++;
					}
				}

				if (totalObjects > 0)
				{
					int completionPercent = (int)((objectCount / (double)totalObjects) * 100);
					int currentStep = completionPercent / 10;

					if (currentStep > lastReportedStep)
					{
						lastReportedStep = currentStep;

						int progressPercentage = 8 + (int)(completionPercent * 0.04);

						progress?.Invoke(
							progressPercentage,
							"Building selected objects... " + completionPercent + "% (" + objectCount + " of " + totalObjects + ")");
					}
				}
			}

			progress?.Invoke(
				12,
				"Selection built - " + partCount + " parts and " + boltCount + " bolt groups.");

			totalStopwatch.Stop();

			WritePrismPartPerformanceLog(
				packagePath,
				stageType,
				partCount,
				boltCount,
				readStopwatch.Elapsed.TotalMilliseconds,
				prismPartTimings,
				checkXyzTicks,
				boltCreationTicks,
				totalStopwatch.Elapsed.TotalMilliseconds);

			File.Delete(newReportLocation);
		}

		private static void WritePrismPartPerformanceLog(string packagePath, StageTypes stageType, int partCount, int boltCount,
			double reportReadMs, Dictionary<string, long> prismPartTimings, long checkXyzTicks, long boltCreationTicks, double totalMs)
		{
			try
			{
				string logPath = Path.Combine(packagePath, "PrismPart_Performance.log");
				double timestampFrequency = Stopwatch.Frequency;

				Func<string, double> getTimingMs = delegate (string name)
				{
					long ticks;
					return prismPartTimings.TryGetValue(name, out ticks) ? ticks * 1000.0 / timestampFrequency : 0;
				};

				string line =
					DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") +
					" | Stage=" + stageType +
					" | Parts=" + partCount +
					" | Bolts=" + boltCount +
					" | ReportRead=" + reportReadMs.ToString("0.0") + "ms" +
					" | CoreFields=" + getTimingMs("Core fields").ToString("0.0") + "ms" +
					" | GetIdentifierByGUID=" + getTimingMs("GetIdentifierByGUID").ToString("0.0") + "ms" +
					" | SelectModelObject=" + getTimingMs("SelectModelObject").ToString("0.0") + "ms" +
					" | GetAssembly=" + getTimingMs("GetAssembly").ToString("0.0") + "ms" +
					" | DrawingFields=" + getTimingMs("Drawing/classification fields").ToString("0.0") + "ms" +
					" | CheckXYZ=" + (checkXyzTicks * 1000.0 / timestampFrequency).ToString("0.0") + "ms" +
					" | BoltFields=" + getTimingMs("Bolt fields").ToString("0.0") + "ms" +
					" | BoltLazySetup=" + getTimingMs("Bolt lazy setup").ToString("0.0") + "ms" +
					" | BoltCreation=" + (boltCreationTicks * 1000.0 / timestampFrequency).ToString("0.0") + "ms" +
					" | Total=" + totalMs.ToString("0.0") + "ms" +
					Environment.NewLine;

				File.AppendAllText(logPath, line);
			}
			catch
			{
				// Performance logging must never interrupt the Prism workflow.
			}
		}
		public Model Model;
		public double SmallestX = 100000000;
		public double SmallestY = 100000000;
		public double SmallestZ = 100000000;
		public double BiggestX = -100000000;
		public double BiggestY = -100000000;
		public double BiggestZ = -100000000;

		public List<PrismPart> PrismParts { get; set; }
		public List<PrismDrawing> PrismDrawings { get; set; }
		public List<PrismBoltGroup> PrismBoltGroups = new List<PrismBoltGroup>();

		public string ErrorMessage { get; set; }

		public double MainPartWeight { get; set; }
		public double FittingWeight { get; set; }

		public bool NumbersUpToDate { get; set; }
		public bool SeversafePresent = false;
		public List<string> MyMarks { get; set; }
		public List<PrismPart> OmittedParts = new List<PrismPart>();

		public List<PrismPart> GetNonSeversafeParts()
		{
			return PrismParts.Where(part => !part.IsSeversafe).ToList();
		}

		public List<PrismPart> GetSeversafeParts()
		{
			return PrismParts.Where(part => part.IsSeversafe).ToList();
		}

		public List<PrismPart> GetFittings()
		{
			return PrismParts.Where(part => part.IsFitting).ToList();
		}

		public List<PrismPart> GetNonLockedParts()
		{
			return PrismParts.Where(part => !part.IsLocked).ToList();
		}

		public List<PrismPart> GetDistinctByPartMark(List<PrismPart> prismParts)
		{
			return prismParts
				.GroupBy(part => part.PartMark)
				.Select(group => group.First())
				.ToList();
		}

		public List<PrismPart> GetLockedParts()
		{
			return PrismParts.Where(part => part.IsLocked).ToList();
		}

		public List<PrismPart> GetFabsecParts()
		{
			return PrismParts.Where(part => part.IsFabsec).ToList();
		}

		public List<PrismPart> GetPartsWithStartNumberError()
		{
			return PrismParts.Where(part => part.StartNumberDoesntMatch).ToList();
		}

		public List<PrismPart> GetPartsWithPhaseNotMatchingError()
		{
			return PrismParts.Where(part => part.PhaseDoesntMatchMain).ToList();
		}

		public List<PrismPart> GetNonFabsecParts()
		{
			return PrismParts.Where(part => !part.IsFabsec).ToList();
		}

		public List<PrismPart> GetMainParts()
		{
			return PrismParts.Where(part => part.IsMainPart).ToList();
		}

		public List<PrismPart> GetSecondaryParts()
		{
			return PrismParts.Where(part => !part.IsMainPart).ToList();
		}

		public List<PrismPart> GetPartsWithOutOfDateNumbers()
		{
			return PrismParts.Where(part => part.NumbersOutOfDate).ToList();
		}

		private void UpdatePartWeights(List<PrismPart> partsList)
		{
			// Using LINQ to calculate MainPartWeight and FittingWeight
			var weightData = partsList
				.GroupBy(part => new[] { "PLT", "FLT", "RSA" }.Any(part.Profile.Contains))
				.Select(group => new
				{
					IsFitting = group.Key, // true if it matches any of the fitting profiles, false otherwise
					TotalWeight = group.Sum(part => part.Weight)
				});

			MainPartWeight = weightData
				.Where(w => !w.IsFitting)
				.Select(w => w.TotalWeight)
				.FirstOrDefault();

			FittingWeight = weightData
				.Where(w => w.IsFitting)
				.Select(w => w.TotalWeight)
				.FirstOrDefault();
		}

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
				if (totalCount > 0) UpdateStatusLabelWithProcessCount(ref currentCount, toolStrip, statusLabel, totalCount);
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

		private void CreatePartListFromReport(string packagePath, Model model, string phaseNum, string issueNum, StageTypes stageType)
		{
			string teklaReportLocation = Path.Combine(FirmFolderLoc.ReportTemplates(), "PrismPart_List.rpt");
			string newReportLocation = Path.Combine(packagePath, "PrismPart_List.xsr");

			if (!CreateReportAndWait(teklaReportLocation, newReportLocation)) return;

			// wait until Tekla Structures has unlocked the file, or timeout
			if (!IfLockedWait(newReportLocation, 15)) return;

			List<PrismDrawing> unfilteredDrawingList = new List<PrismDrawing>();

			// read the report
			using (var reader = new StreamReader(newReportLocation))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					var items = line.Split(',');

					if (items[0] == " Part")
					{
						PrismPart newPart = new PrismPart(items, model, stageType);
						PrismParts.Add(newPart);
						CheckXYZSize(newPart.Part);
						/*if (items[11].TrimEnd(' ').TrimStart(' ') != "") // then assembly drawing information is available
						{
							unfilteredDrawingList.Add(new PrismDrawing(items, true));
						}
						if(items[18].TrimEnd(' ').TrimStart(' ') != "") //then fitting drwaing information is available
						{
							unfilteredDrawingList.Add(new PrismDrawing(items, false));
						}*/
					}
					if (items[0] == " Bolt")
					{
						// we create the bg and check for null because it was failing sometimes..
						PrismBoltGroup bg = new PrismBoltGroup(items, phaseNum, issueNum, model);
						if (bg != null)
						{
							PrismBoltGroups.Add(bg);
						}
					}
				}

			/*	// Remove duplicates based on AssemblyDrawingNumber and PartDrawingNumber
				PrismDrawings = unfilteredDrawingList
					.GroupBy(d => new { d.AssemblyDrawingNumber, d.PartDrawingNumber })
					.Select(g => g.First())
					.ToList();*/
 
			}

			File.Delete(newReportLocation);
		}

		public bool CreateReportAndWait(string teklaReportLocation, string newReportLocation)
		{
			Operation.CreateReportFromSelected(teklaReportLocation, newReportLocation, "", "", "");

			int waitTime = 0;
			const int maxWaitTime = 10000; // 10 seconds

			while (waitTime < maxWaitTime)
			{
				if (File.Exists(newReportLocation))
				{
					return true;
				}

				Thread.Sleep(1000); // wait for 1 second
				waitTime += 1000;
			}

			return false;
		}

		/// <summary>
		/// Waits until a file is properly closed or returns false
		/// </summary>
		/// <param name="fileName"></param>
		/// /// <param name="seconds"></param>
		/// <returns></returns>
		public static bool IfLockedWait(string fileName, int seconds)
		{
			while (true)
			{
				try
				{
					using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
					{
						var readText = new byte[fileStream.Length];
						fileStream.Seek(0, SeekOrigin.Begin);
						var unused = fileStream.Read(readText, 0, (int)fileStream.Length);
					}
					return true;
				}

				catch (IOException)
				{
					// wait one second
					Thread.Sleep(1000);
					seconds--;
					if (seconds == 0)
						return false;
				}
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

			PrismPart myPrismPart = CategorizeAndProcessPart(myPart, isSeversafe);

			UpdatePartWeight(myPrismPart);

			AddPartMark(myPart);

			ProcessAssembly(myPrismPart, phaseNum, issueNum);

			PrismParts.Add(myPrismPart);
		}

		private PrismPart CategorizeAndProcessPart(Part myPart, bool isSeversafe)
		{
			PrismPart myPrismPart = new PrismPart(myPart) { IsSeversafe = isSeversafe };

			if (isSeversafe)
			{
				SeversafePresent = true;
			}
			else
			{
				if (IsLocked(myPart)) myPrismPart.IsLocked = true;
				myPrismPart.IsFabsec = myPart.Profile.ProfileString.StartsWith("PG");
			}
			return myPrismPart;
		}

		private void UpdatePartWeight(PrismPart myPart)
		{
			if (!new[] { "PLT", "FLT", "RSA" }.Any(myPart.Profile.Contains)) //it is not a fitting
			{
				MainPartWeight += myPart.Weight;
			}
			else
			{
				FittingWeight += myPart.Weight;
			}
		}

		private void AddPartMark(Part myPart)
		{
			MyMarks.Add(myPart.GetPartMark());
		}

		private async void ProcessAssembly(PrismPart myPart, string phaseNum, string issueNum)
		{
			if (myPart.IsMainPart)
			{
				var boltsFromAssembly = await GetBoltsFromAssemblyAsync(myPart.Assembly);
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
			foreach (PrismPart part in PrismParts)
			{
				selectList.Add(part.Part);
			}
			Tekla.Structures.Model.UI.ModelObjectSelector ms = new Tekla.Structures.Model.UI.ModelObjectSelector();
			ms.Select(selectList);
		}

		private static async Task<List<BoltGroup>> GetBoltsFromAssemblyAsync(Assembly assembly)
		{
			List<BoltGroup> myBoltsList = new List<BoltGroup>();
			ArrayList secondaries = assembly.GetSecondaries();
			secondaries.Add(assembly.GetMainPart());

			ConcurrentBag<BoltGroup> myBoltsBag = new ConcurrentBag<BoltGroup>();

			List<Task> tasks = new List<Task>();

			foreach (ModelObject item in secondaries)
			{
				if (item is Part part)
				{
					tasks.Add(Task.Run(() =>
					{
						ModelObjectEnumerator bolts = part.GetBolts();
						foreach (var setOfBolts in bolts)
						{
							if (setOfBolts is BoltGroup bolt)
							{
								if (bolt.PartToBeBolted.Identifier.GUID == part.Identifier.GUID)
								{
									if (!myBoltsBag.Any(x => x.Identifier.GUID == bolt.Identifier.GUID))
									{
										myBoltsBag.Add(bolt);
									}
								}
							}
						}
					}));
				}
			}

			await Task.WhenAll(tasks);

			// If you need a List instead of ConcurrentBag
			return myBoltsBag.ToList();
		}

		public static List<ModelObject> GetSelectedParts()
		{
			try
			{
				List<ModelObject> parts = new List<ModelObject>();
				ModelObjectEnumerator moe = new Tekla.Structures.Model.UI.ModelObjectSelector().GetSelectedObjects();
	
				foreach (var item in moe)
				{
					if (item is Part part) { parts.Add(part); }
				}
				return parts;
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
				return null;
			}
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
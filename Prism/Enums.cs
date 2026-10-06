namespace Prism
{
	public class Enums
	{
		public enum StageTypes { Unassigned, Prelim1, Prelim2, Prelim3, Check1, Check2, Check3, FAB, Bolt, PrelimPG, RocketPacket };
		public enum Factory { SNI, SUK, Unknown };
		public enum IgnoreType { AutoFix, Ignore, Stop, Unspecified };

		public enum Error
		{
			Execution,
			Orientation,
			NameAndClass,
			CarcassIsShorterThanOrdered,
			NumberingNotUpToDate,
			PartLocked,
			PreviousStepIncomplete,
			FabsecNotProcessed,
			FabsecCarcassNotOrdered,

			PartNotOrdered,
			FinishMissing,
			IntumescentLoadingMissing,
			SecondaryNumberingMismatch,
			SecondaryPhasingMismatch
		}

		public enum AdvancedSettingType { Default, PrelimPrefix, FabPackType, DirectoryMaterial, DirectoryCarcasses, DirectoryBolts, DirectorySeversafe, DirectoryFabPack, DirectoryVariation, FabsecGreen }
		public enum DrawingFolder { Default, ASS, FIT, PRT, WLD, SHA, PGC, NotRequired, AssNotRequired }
		public enum DrawingClassification { Unclassified, ASS1, ASS2, ASS3, ASS4, ASS5, FIT1, FIT2, FIT3 }

		public enum DrawingCheckRunResult
		{
			Success,
			InitialSetupFailed,
			StageOneFailed,
			StageTwoFailed
		}

		public enum MaterialOrderRunResult
		{
			Success,
			InitialSetupFailed,
			OperationFailed,
			Exception
		}

		public enum FabPackCheckRunResult
		{
			Success,
			InitialSetupFailed,
			DrawingChecksFailed,
			ChecksFailed
		}

		public enum FabPackRunResult
		{
			Success,
			InitialSetupFailed,
			DrawingChecksFailed,
			OperationFailed
		}
	}
}
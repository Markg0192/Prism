namespace Tekla.Structures
{
	public interface ICommonTasks
	{
		void CreateGeneralArrangementDrawingFromTemplate(string name);

		void OpenAssemblyDrawingProperties(string name);

		void OpenAutoDrawingScript(string name);

		void OpenCastUnitDrawingProperties(string name);

		void OpenDrawingList();

		void OpenGeneralArrangementDrawingProperties(string name);

		void OpenNumberingSettings();

		void OpenSinglePartDrawingProperties(string name);

		void PerformNumbering(bool fullNumbering);
	}
}

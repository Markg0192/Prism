using System.IO;
using Tekla.Structures.Filtering;
using Tekla.Structures.Filtering.Categories;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model.UI;

namespace Prism
{
    public static class ViewManager
    {
        public static void CreateFabViewFilter(string phaseNum, string issueNum, PrismProjectData projectData)
        {
            // Creates the filter expressions
            PartFilterExpressions.CustomString PartName = new PartFilterExpressions.CustomString(ModelUDA.FabStampUDA());
            StringConstantFilterExpression phaseInfo = new StringConstantFilterExpression(ModelUDA.FabViewAndFilterStamp(phaseNum));

            PartFilterExpressions.Name name = new PartFilterExpressions.Name();
            StringConstantFilterExpression seversafeName = new StringConstantFilterExpression("SS*");

            // Creates the binary filter expressions
            BinaryFilterExpression Expression1 = new BinaryFilterExpression(PartName, StringOperatorType.STARTS_WITH, phaseInfo);
            BinaryFilterExpression Seversafe = new BinaryFilterExpression(name, StringOperatorType.IS_NOT_EQUAL, seversafeName);

            // Creates the binary filter expression collection
            BinaryFilterExpressionCollection ExpressionCollection = new BinaryFilterExpressionCollection();
            ExpressionCollection.Add(new BinaryFilterExpressionItem(Expression1, BinaryFilterOperatorType.BOOLEAN_AND));
            ExpressionCollection.Add(new BinaryFilterExpressionItem(Seversafe, BinaryFilterOperatorType.BOOLEAN_AND));

            string AttributesPath = Path.Combine(projectData.ProjPath, "attributes");
            string FilterName = Path.Combine(AttributesPath, ModelUDA.FabViewAndFilterStamp(phaseNum));

            Filter Filter = new Filter(ExpressionCollection);
            // Generates the filter file
            Filter.CreateFile(FilterExpressionFileType.OBJECT_GROUP_VIEW, FilterName);
        }

        public static void CreateFabView(string phaseNum, string issueNum, PrismProjectData projectData, SelectedObjects obj)
        {
            CreateFabViewFilter(phaseNum, issueNum, projectData);
            var view = new View();
            view.ViewFilter = ModelUDA.FabViewAndFilterStamp(phaseNum);

            view.DisplayCoordinateSystem.AxisX = new Vector(1, 0, 0);
            view.DisplayCoordinateSystem.AxisY = new Vector(1, 0, 0);
            view.DisplayCoordinateSystem.Origin = new Vector(0, 0, obj.SmallestZ - 50);
            view.DisplayType = View.DisplayOrientationType.DISPLAY_3D;

            view.Name = ModelUDA.FabViewAndFilterStamp(phaseNum);
            view.ViewCoordinateSystem.AxisX = new Vector(1, 0, 0);
            view.ViewCoordinateSystem.AxisY = new Vector(0, 1, 0);
            view.ViewCoordinateSystem.Origin = new Point(0, 0, obj.SmallestZ - 50);
            view.ViewDepthUp = 100000;
            view.ViewDepthDown = 100000;
            view.WorkArea.MinPoint = new Point(obj.SmallestX - 1000, obj.SmallestY - 1000, obj.SmallestZ - 1000);
            view.WorkArea.MaxPoint = new Point(obj.BiggestX + 1000, obj.BiggestY + 1000, obj.BiggestZ + 1000);

            view.SetVisibilitySettings();

            view.SharedView = true;
            view.Insert();
            view.Modify();
        }

		public static void CreateGraveyardView(View current)
		{		
			var view = new View();
			view.DisplayType = View.DisplayOrientationType.DISPLAY_3D;

            view.Name = Constants.OmitGraveyrdViewName;
			view.ViewCoordinateSystem.AxisX = new Vector(1, 0, 0);
			view.ViewCoordinateSystem.AxisY = new Vector(0, 1, 0);
			view.ViewCoordinateSystem.Origin = new Point(0, 0, -100000);
			view.ViewDepthUp = 90000;
			view.ViewDepthDown = 150000;
			view.WorkArea.MinPoint = new Point(current.WorkArea.MinPoint.X - 100000, current.WorkArea.MinPoint.Y - 100000, current.WorkArea.MinPoint.Z - 100000);
			view.WorkArea.MaxPoint = new Point(current.WorkArea.MaxPoint.X + 100000, current.WorkArea.MinPoint.Y + 100000, current.WorkArea.MinPoint.Z + 100000);

			view.SetVisibilitySettings();

			view.SharedView = true;
			view.Insert();
			view.Modify();
		}

		private static void SetVisibilitySettings(this View view)
        {
            view.VisibilitySettings.PartsVisible = true;
            view.VisibilitySettings.PartsVisibleInComponents = true;
            view.VisibilitySettings.GridsVisible = true;
            view.VisibilitySettings.BoltHolesVisible = false;
            view.VisibilitySettings.BoltHolesVisibleInComponents = false;
            view.VisibilitySettings.BoltsVisible = false;
            view.VisibilitySettings.BoltsVisibleInComponents = false;
            view.VisibilitySettings.ComponentsVisible = false;
            view.VisibilitySettings.ComponentsVisibleInComponents = false;
            view.VisibilitySettings.ConstructionLinesVisible = false;
            view.VisibilitySettings.ConstructionPlanesVisible = false;
            view.VisibilitySettings.ConstructionPlanesVisibleInComponents = false;
            view.VisibilitySettings.CutsVisible = false;
            view.VisibilitySettings.CutsVisibleInComponents = false;
            view.VisibilitySettings.FittingsVisible = false;
            view.VisibilitySettings.FittingsVisibleInComponents = false;
            view.VisibilitySettings.LoadsVisible = false;
            view.VisibilitySettings.PointsVisible = false;
            view.VisibilitySettings.PointsVisibleInComponents = false;
            view.VisibilitySettings.PourBreaksVisible = false;
            view.VisibilitySettings.PoursVisible = false;
            view.VisibilitySettings.RebarsVisible = false;
            view.VisibilitySettings.RebarsVisibleInComponents = false;
            view.VisibilitySettings.ReferenceObjectsVisible = false;
            view.VisibilitySettings.SurfaceTreatmentsVisible = false;
            view.VisibilitySettings.WeldsVisible = false;
            view.VisibilitySettings.WeldsVisibleInComponents = false;
        }
    }
}
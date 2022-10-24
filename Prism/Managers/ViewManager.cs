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
           StringConstantFilterExpression phaseInfo = new StringConstantFilterExpression(ModelUDA.FabStamp(phaseNum, issueNum));

           PartFilterExpressions.Name name = new PartFilterExpressions.Name();
           StringConstantFilterExpression seversafeName = new StringConstantFilterExpression("SS*");

           // Creates the binary filter expressions
           BinaryFilterExpression Expression1 = new BinaryFilterExpression(PartName, StringOperatorType.IS_EQUAL, phaseInfo);
           BinaryFilterExpression Seversafe = new BinaryFilterExpression(name, StringOperatorType.IS_NOT_EQUAL, seversafeName);

            // Creates the binary filter expression collection
            BinaryFilterExpressionCollection ExpressionCollection = new BinaryFilterExpressionCollection();
           ExpressionCollection.Add(new BinaryFilterExpressionItem(Expression1, BinaryFilterOperatorType.BOOLEAN_AND));
           ExpressionCollection.Add(new BinaryFilterExpressionItem(Seversafe, BinaryFilterOperatorType.BOOLEAN_AND));

            string AttributesPath = Path.Combine(projectData.ProjPath, "attributes");
           string FilterName = Path.Combine(AttributesPath, ModelUDA.FabStamp(phaseNum, issueNum));

           Filter Filter = new Filter(ExpressionCollection);
           // Generates the filter file
           Filter.CreateFile(FilterExpressionFileType.OBJECT_GROUP_VIEW, FilterName);      
        }

        public static void CreateFabView(string phaseNum, string issueNum, PrismProjectData projectData, SelectedObjects obj)
        {
            CreateFabViewFilter(phaseNum, issueNum, projectData);
            var view = new View();
            view.Name = ModelUDA.FabStamp(phaseNum, issueNum);
            view.ViewCoordinateSystem.AxisX = new Vector(1, 0, 0);
            view.ViewCoordinateSystem.AxisY = new Vector(0, 1, 0);
            view.ViewCoordinateSystem.Origin = new Point(0, 0, 0);
            view.ViewDepthUp = 100000;
            view.ViewDepthDown = 100000;
            view.WorkArea.MinPoint = new Point(obj.SmallestX - 1000, obj.SmallestY - 1000, obj.SmallestZ - 1000);
            view.WorkArea.MaxPoint = new Point(obj.BiggestX + 1000, obj.BiggestY + 1000, obj.BiggestZ + 1000);
            view.DisplayType = View.DisplayOrientationType.DISPLAY_3D;
            view.SetVisibilitySettings();
            view.ViewFilter = ModelUDA.FabStamp(phaseNum, issueNum);
            view.SharedView = true;
            view.Insert();
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
using Tekla.Structures.Model;

namespace Prism
{
    public class MyFitting : SteelItemBase
    {
        public static MyFitting CreateMyNewFitting(Part part, Model model)
        {
            MyFitting myNewFitting = new MyFitting();
            myNewFitting.SetCommonProperties(part, model);

            return myNewFitting;
        }
    }
}
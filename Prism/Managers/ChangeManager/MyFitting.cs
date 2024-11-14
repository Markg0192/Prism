using System.Threading.Tasks;
using Tekla.Structures.Model;

namespace Prism
{
    public class MyFitting : SteelItemBase
    {
        public static MyFitting CreateMyNewFitting(Part part, Model model)
        {
            MyFitting myNewFitting = new MyFitting();
            myNewFitting.SetCommonProperties(part, model);
            myNewFitting.Guid = part.Identifier.GUID.ToString();
            if(myNewFitting.Guid == null)
            {

            }
            return myNewFitting;
        }
    }
}
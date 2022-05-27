using Tekla.Structures.Model;

namespace Prism
{
    public static class TeklaExtensions
    {
        public static SevModelData CreateSevModelData(this Model model)
        {
            return new SevModelData(model);
        }
        
        public static SevFolders CreateSevFolders(this Model model, string phaseNum, string issueNum)
        {
            return new SevFolders(model, phaseNum, issueNum);
        }

        public static SevModelEnumerator CreateSevModelEnumerator(this Model model, string stageType)
        {
            return new SevModelEnumerator(stageType);
        }
        public static ModelModifiers CreateModelMods(this Model model)
        {
            return new ModelModifiers(model);
        }
    }
}
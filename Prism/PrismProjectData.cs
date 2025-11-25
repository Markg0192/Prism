using System;
using System.IO;
using Tekla.Structures.Model;

namespace Prism
{
    /// <summary>
    /// The Prism project data class gathers some project information for us to use in folder naming, report naming and email text.
    /// </summary>
    public class PrismProjectData
    {
        public PrismProjectData(ProjectInfo projectInfo, ModelInfo modelInfo, ExternalService.WebService1 webService)
        {
            pInfo = projectInfo;
            ProjName = projectInfo.Name;
            ProjNumber = projectInfo.ProjectNumber;
            ProjNumberAndGuid = ProjNumber + "-" + projectInfo.GUID;
            ProjNumberAndName = ProjNumber + "-" + ProjName;
            Date = DateTime.Now.ToString("dd/MM/yyyy");
            string[] NameArray = Environment.UserName.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            First = Capitalise(NameArray[0]);
            if (NameArray.Length != 2)
            {
                Last = "";
                Full = First; ;
                Initials = First[0].ToString().ToUpper();
            }
            else
            {
                Last = Capitalise(NameArray[1]);
                Full = First + " " + Last;
                Initials = new string(new char[] { First.ToCharArray()[0], Last.ToCharArray()[0] }).ToUpper();
            }


			ModelName = Path.GetFileNameWithoutExtension(modelInfo.ModelName);

			ProjPath = modelInfo.ModelPath;
            WebService = webService;
        }

        public string VariationNumber { get; set; }
        public readonly string ProjNumberAndName;
        public readonly string ProjNumberAndGuid;
        public readonly string ProjPath;
        public readonly string ProjName;
        public readonly string ProjNumber;
        public readonly string Date;
        public readonly string First;
        public readonly string Last;
        public readonly string Full;
        public readonly string Initials;
        public bool IsVariation = false;
        public ProjectInfo pInfo;
        public ExternalService.WebService1 WebService;
        public string ModelName;

        private string Capitalise(string original)
        {
            char[] chars = original.ToLower().ToCharArray();
            string initial = new string(chars, 0, 1).ToUpper();
            string following = new string(chars, 1, chars.Length - 1);
            return initial + following;
        }
    }
}
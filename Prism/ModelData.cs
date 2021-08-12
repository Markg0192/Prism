using System;
using Tekla.Structures.Model;

namespace Prism
{
    /// <summary>
    /// The model data class gathers all the information that is being read from the model.
    /// This is then used to inform folder and report names.
    /// </summary>
    public class SevModelData
    {     
        public readonly string ProjName;
        public readonly string ProjNumber;
        public readonly string Date;      
        public readonly string First;
        public readonly string Last;
        public readonly string Full;
        public readonly string Initials;

        public SevModelData(Model model)
        {
            ProjectInfo projectName = model.GetProjectInfo();
            ProjName = projectName.Name;
            ProjNumber = projectName.ProjectNumber;
            Date = DateTime.Now.ToString("dd/MM/yyyy");          
            string[] NameArray = Environment.UserName.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            First = Capitalise(NameArray[0]);
            Last = Capitalise(NameArray[1]);
            Full = First + " " + Last;
            Initials = new string(new char[] { First.ToCharArray()[0], Last.ToCharArray()[0] }).ToUpper();
        }

        public string Capitalise(string original)
        {
            char[] chars = original.ToLower().ToCharArray();
            string initial = new string(chars, 0, 1).ToUpper();
            string following = new string(chars, 1, chars.Length - 1);
            return initial + following;
        }
    }
}

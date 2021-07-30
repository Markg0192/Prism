using System;
using Tekla.Structures.Model;

namespace Prism
{
    public static class TeklaExtensions
    {
        public static SevModelData SevModelData(this Model model)
        {
            return new SevModelData(model);
        }
    }
    public class SevModelData
    {     
        public readonly string projName;
        public readonly string projNumber;
        public readonly string date;        
        public readonly string EnvironmentName;
        public readonly string First;
        public readonly string Last;
        public readonly string Full;
        public readonly string Initials;

        public SevModelData(Model model)
        {
            ProjectInfo projectName = model.GetProjectInfo();
            projName = projectName.Name.ToString();
            projNumber = projectName.ProjectNumber.ToString();
            date = DateTime.Now.ToString("dd/MM/yyyy");           
            EnvironmentName = Environment.UserName;
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

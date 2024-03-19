using System.Collections.Generic;
using System.Threading.Tasks;
using Tekla.Structures.Model;
using Task = System.Threading.Tasks.Task;

namespace Prism
{
    public class MyAssembly : SteelItemBase
    {
        public List<MyFitting> Fittings = new List<MyFitting>();

        //Because the MyAssembly is being written to xml it cannot have a constructor that takes variables, this factory method is the work around for that
        public static MyAssembly CreateMyAssembly(Assembly assembly, Model model)
        {
            // Await the task to get the result of the asynchronous operation
            AssemblyProcessingResult assemblyProcessingResult = GetFittingsInAssembly(assembly, model);

            MyAssembly myNewAssembly = new MyAssembly();
            myNewAssembly.SetCommonProperties(assembly.GetMainPart() as Part, model);

            // Now that assemblyProcessingResult is the result of the awaited task, you can access its properties
            myNewAssembly.Fittings = assemblyProcessingResult.Fittings;

            return myNewAssembly;
        }

        private static AssemblyProcessingResult GetFittingsInAssembly(Assembly beam, Model model)
        {
            var result = new AssemblyProcessingResult();

            var secondaryObjects = beam.GetSecondaries();

            foreach (object secondary in secondaryObjects)
            {
                Part secondaryPart = secondary as Part;

                if (secondaryPart != null && beam.GetMainPart().Identifier.GUID != secondaryPart.Identifier.GUID)
                {
                    MyFitting newFitting = CastObjectAndCreateFitting(secondary, model);
                    if (newFitting != null)
                    {
                        result.Fittings.Add(newFitting);
                    }
                    else
                    {
                        result.UnsupportedTypes.Add(secondary.GetType().Name);
                    }
                }
            }

            return result;
        }

        private static MyFitting CastObjectAndCreateFitting(object myObject, Model model)
        {
            if (myObject is Part part)
            {
                return MyFitting.CreateMyNewFitting(part, model);
            }
            return null;
        }

        public override string ToString()
        {
            return PartMark;
        }

        public class AssemblyProcessingResult
        {
            public List<MyFitting> Fittings { get; set; } = new List<MyFitting>();
            public HashSet<string> UnsupportedTypes { get; set; } = new HashSet<string>();
        }

        public class AssemblyCreationResult
        {
            public MyAssembly Assembly { get; set; }
            public HashSet<string> UnsupportedTypes { get; set; } = new HashSet<string>();
        }
    }
}
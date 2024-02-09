using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tekla.Structures.Model;

namespace Prism.Managers
{
    public class ChangedObject
    {
        public Part Part { get; set; }
        public string ChangeMessage { get;set; }
        public Enum.ModificationType ModificationType { get; set; }
    }
}

using System.Collections.Generic;
using Tekla.Structures.Model;
using Tekla.Structures.Plugins;

namespace Prism
{
    public class StructuresData
    {
        [StructuresField("attributeName")]
        public string attname;
    }

    [Plugin("Prism")]
    [PluginUserInterface("Prism.PrismForm")]

    public class PrismPlugin : PluginBase
    {
        private readonly Model privateModel;
        public Model myModel
        {
            get { return privateModel; }
        }

        private readonly StructuresData privateData;

        /// <summary>
        /// This is the constructor for our plugin object?
        /// </summary>
        /// <param name="data"></param>
        public PrismPlugin(StructuresData data)
        {
            privateModel = new Model();
            privateData = data;
        }

        /// <summary>
        /// this is a required method for the inherited template
        /// for prism there is no prior input, the plugin simply starts its own form
        /// </summary>
        /// <returns></returns>
        public override List<InputDefinition> DefineInput()
        {
            return new List<InputDefinition>();
        }

        /// <summary>
        /// this is a required method for the inherited template
        /// for prism the actions will be initiated from its form so this 'run' is empty
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public override bool Run(List<InputDefinition> input)
        {
            return true;
        }
    }
}

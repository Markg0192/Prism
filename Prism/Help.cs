namespace Prism
{
    internal class Help
    {
        /// <summary>
        /// Opens the User Guide
        /// </summary>
        public static void Open(ExternalService.WebService1 service)
        {
            var helpForm = new HelpForm(null, null, service);
            helpForm.Show();
        }

        /// <summary>
        /// Opens the User Guide at the named page 
        /// under the specified parent catagory
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        public static void OpenAt(string name, string parent, ExternalService.WebService1 service)
        {
            var helpForm = new HelpForm(name, parent, service);
            helpForm.Show();
        }
    }
}

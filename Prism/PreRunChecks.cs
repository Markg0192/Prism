using System.Windows.Forms;
using Tekla.Structures.Drawing;
using Tekla.Structures.Model;

namespace Prism
{
    /// <summary>
    /// The pre run checks class is used to check the model for any issues in the model that may cause the program to crash.
    /// These are ran before the main program and warns the user of the problem and closes the app before it crashes.
    /// </summary>
    public class PreRunChecks
    {
        private string[] mark;
        private SevModelEnumerator modelEnum;
        private DrawingUpToDateStatus updateStatus;  

        public PreRunChecks(Model model)
        {
            modelEnum = model.SevModelEnumerator();
        }

        public bool CheckDrawingsAreUpToDate(DrawingEnumerator drawingsList)
        {
            const string notUpToDateMessage = "Some drawings are not up to date, please update and try again";
            const string notUpToDateTitle = "Drawings not up to date";

            foreach (Drawing currentDrawing in drawingsList)
            {
                mark = currentDrawing.Mark.Split(new char[] { '[', '.', ']' });
                string drawingName = "";
                foreach (string s in mark)
                {
                    drawingName = drawingName + s;
                }
                updateStatus = currentDrawing.UpToDateStatus;

                if (modelEnum.MyMarks != null)
                {
                    if (modelEnum.MyMarks.Contains(drawingName) && updateStatus!=DrawingUpToDateStatus.DrawingIsUpToDate)
                    {
                        MessageBox.Show(notUpToDateMessage, notUpToDateTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }                    
                }
            }
            return true;
        }
    }
}


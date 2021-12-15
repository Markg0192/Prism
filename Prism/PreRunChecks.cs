using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using Tekla.Structures.Drawing;
using Tekla.Structures.Model;
using Tekla.Structures.Model.Operations;

namespace Prism
{
    /// <summary>
    /// The pre run checks class is used to check the model for any issues in the model that may cause the program to crash.
    /// These are ran before the main program and warns the user of the problem and closes the app before it crashes.
    /// </summary>
    public class PreRunChecks
    {
        private string[] mark;
        private DrawingUpToDateStatus updateStatus;  

        public PreRunChecks(Model model)
        {
        }

        public bool CheckDrawingsAreUpToDate(List<PrismDrawing> drawingsList)
        {      
            const string notUpToDateMessage = "Some drawings are not up to date, please update and try again";
            const string notUpToDateTitle = "Drawings not up to date";

            foreach (PrismDrawing currentDrawing in drawingsList)
            {    
                updateStatus = currentDrawing.TeklaDrawing.UpToDateStatus;
                    if (updateStatus!=DrawingUpToDateStatus.DrawingIsUpToDate)
                    {
                        MessageBox.Show(notUpToDateMessage, notUpToDateTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }      
            }
            return true;
        }

        public bool CheckNumberingIsUpToDate(ArrayList selectedParts)
        {
            const string notUpToDateMessage = "Your member numbering is not up to date, please update and try again";
            const string notUpToDateTitle = "Numbers not up to date";

            foreach (Tekla.Structures.Model.Part part in selectedParts)
            {                
                if (!Operation.IsNumberingUpToDate(part)) 
                { 
                    MessageBox.Show(notUpToDateMessage, notUpToDateTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
            }
            return true;
        }
    }
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tekla.Structures.Drawing;
using Tekla.Structures.Model;
using Tekla.Structures.Dialog;

namespace Prism
{
    public class PreRunChecks
    {
        string[] mark;
        SevModelEnumerator ModelEnum;
        DrawingUpToDateStatus updateStatus;
        string notUpToDateMessage;
        string notUpToDateTitle;
        MessageBoxIcon warning;
        MessageBoxButtons okButton;

        public PreRunChecks(Model model)
        {
            ModelEnum = model.SevModelEnumerator();
            notUpToDateMessage = "Some drawings are not up to date, please update and try again";
            notUpToDateTitle = "Drawings not up to date";
            warning = MessageBoxIcon.Warning;
            okButton = MessageBoxButtons.OK;
        }

        public bool CheckDrawingsAreUpToDate(DrawingEnumerator drawingsList)
        {
            foreach (Drawing currentDrawing in drawingsList)
            {
                mark = currentDrawing.Mark.Split(new char[] { '[', '.', ']' });
                string drawingName = "";
                foreach (string s in mark) drawingName = drawingName + s;
                updateStatus = currentDrawing.UpToDateStatus;

                if (ModelEnum.myMarks != null)
                {
                    if (ModelEnum.myMarks.Contains(drawingName) && updateStatus.ToString() != "DrawingIsUpToDate")
                    {
                        DialogResult result = MessageBox.Show(notUpToDateMessage, notUpToDateTitle, okButton, warning);
                        if (result == DialogResult.OK)
                        {
                            return false;                            
                        }
                    }                    
                }
            }
            return true;
        }
    }
}


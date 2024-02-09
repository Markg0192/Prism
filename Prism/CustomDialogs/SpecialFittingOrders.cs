using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Tekla.Structures.Model;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using static Prism.Enums;
using System.Windows.Forms.VisualStyles;
//using System.Xml.Linq;
using Tekla.Structures.Drawing;
using ModelObject = Tekla.Structures.Model.ModelObject;
using Part = Tekla.Structures.Model.Part;

namespace Prism.CustomDialogs
{
    public partial class SpecialFittingOrders : Form
    {
        public int orderAction;
        private string phaseNum;
        private string issueNum;

        public SpecialFittingOrders()
        {
            InitializeComponent();
            CenterToScreen();
            TopMost = true;
        }

        private void btn_TagSpecial_Click(object sender, EventArgs e)
        {
            orderAction = ModelModifiers.ChangeSpecialTag("Special", phaseNum, issueNum, out List<ModelObject> objects);
            ModelModifiers.SetPartsBlue(objects);
        }

        private void btn_RemoveSpecialTag_Click(object sender, EventArgs e)
        {
            orderAction = ModelModifiers.ChangeSpecialTag("", phaseNum, issueNum, out List<ModelObject> objects);
            ModelModifiers.SetPartsRed(objects);
        }

        private void btn_OrderTagged_Click(object sender, EventArgs e)
        {
            orderAction = 2;
            Close();
        }

        private void btn_OrderSelected_Click(object sender, EventArgs e)
        {
            orderAction = 3;
            Close();
        }

        private void btn_SpecialClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btn_ShowTagged_Click(object sender, EventArgs e)
        {
            SelectedObjects selectedObjects = new SelectedObjects(StageTypes.Prelim3, phaseNum, issueNum);
            List<ModelObject> objects = new List<ModelObject>();
            foreach (Part p in selectedObjects.SelectedModelParts)
            {
                string tagInfo = "";
                p.GetUserProperty(ModelUDA.SpecialFittingTag(), ref tagInfo);
                if (tagInfo != "")
                {
                    objects.Add(p);
                }
            }
            ModelModifiers.SetPartsBlue(objects);
        }

        private void btn_CreateTaggedDrawings_Click(object sender, EventArgs e)
        {
            orderAction = 4;
            Close();
        }

        private void btn_CreateSelectedDrawings_Click(object sender, EventArgs e)
        {
            orderAction = 5;
            Close();
        }
    }
}
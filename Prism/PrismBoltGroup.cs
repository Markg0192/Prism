using System;
using Tekla.Structures;
using Tekla.Structures.Model;

namespace Prism
{
	public class PrismBoltGroup
	{
		public PrismBoltGroup(BoltGroup boltGroup, string phaseNum, string issueNum)
		{
			isShop = boltGroup.BoltType == BoltGroup.BoltTypeEnum.BOLT_TYPE_WORKSHOP;
			isShearStud = boltGroup.BoltStandard == "SHEAR-STUD";
			isOrdered = IsBoltOrdered(boltGroup, phaseNum, issueNum);
			BoltGroup = boltGroup;
		}

		public PrismBoltGroup(string[] items, string phaseNum, string issueNum, Model model)
		{
			isShop = Convert.ToInt32(Trim(items[1])) == 1;
			isShearStud = Trim(items[2]).Contains("SHEAR-STUD");
			Name = Trim(items[2]);
			isOrdered = IsBoltOrdered(phaseNum, issueNum, Trim(items[3]), Trim(items[4]), Trim(items[5]));
			BoltGroup = model.SelectModelObject(new Identifier(Trim(items[6]))) as BoltGroup;
		}

		public string Name { get; set; }
		public BoltGroup BoltGroup { get; set; }
		public bool isShop { get; set; }
		public bool isOrdered { get; set; }
		public bool isShearStud { get; set; }

		private static bool IsBoltOrdered(BoltGroup boltGroup, string phaseNum, string issueNum)
		{
			string shearTag = "";
			string boltPhaseNo = "";
			string boltIssueNo = "";

			boltGroup.GetReportProperty(ModelUDA.BoltShearStudTag(), ref shearTag);
			boltGroup.GetReportProperty(ModelUDA.BoltOrderPhaseNo(), ref boltPhaseNo);
			boltGroup.GetReportProperty(ModelUDA.BoltOrderIssueNo(), ref boltIssueNo);

			return IsBoltOrdered(phaseNum, issueNum, shearTag, boltPhaseNo, boltIssueNo);
		}

		private static bool IsBoltOrdered(string phaseNum, string issueNum, string boltShearStudTag, string orderPhaseNo, string orderIssueNo)
		{
			if (orderPhaseNo == phaseNum && orderIssueNo == issueNum)
			{
				return true;
			}
			if (boltShearStudTag != "")
			{
				return true;
			}

			return false;
		}

		private static bool BoltIsNotOrdered(bool isShearStud, string phaseNum, string issueNum, string boltShearStudTag, string orderPhaseNo, string orderIssueNo)
		{
			string property = isShearStud ? boltShearStudTag : ModelModifiers.BoltPhaseAndIssue(orderPhaseNo, orderIssueNo);

			string currentString = ModelModifiers.BoltPhaseAndIssue(phaseNum, issueNum);

			bool currentAndExistingMatch = property == currentString;

			bool isNullOrEmpty = string.IsNullOrEmpty(property);

			return isNullOrEmpty || currentAndExistingMatch;
			// Return true if the property is null or empty or if the issue number is being used again effectively nulling the original order
		}

		private string Trim(string s)
		{
			return s.TrimEnd(' ').TrimStart(' ');
		}
	}
}
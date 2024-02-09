using Tekla.Structures.Model;

namespace Prism
{
    public class PrismBoltGroup
    {
        public PrismBoltGroup(BoltGroup boltGroup, string phaseNum, string issueNum)
        {
            isShop = boltGroup.BoltType == BoltGroup.BoltTypeEnum.BOLT_TYPE_WORKSHOP;
            isShearStud = boltGroup.BoltStandard == "SHEAR-STUD";
            isOrdered = !BoltIsNotOrdered(boltGroup, isShearStud, phaseNum, issueNum);
            BoltGroup = boltGroup;
        }

        public BoltGroup BoltGroup { get; set; }
        public bool isShop { get; set; }
        public bool isOrdered { get; set; }
        public bool isShearStud { get; set; }

        private static bool BoltIsNotOrdered(BoltGroup boltGroup, bool isShearStud, string phaseNum, string issueNum)
        {
            string propertyToCheck = isShearStud ? ModelUDA.BoltShearStudTag() : ModelUDA.BoltOrderedAmount();
            string property = "";
            boltGroup.GetReportProperty(propertyToCheck, ref property);

            string currentString = ModelModifiers.BoltPhaseAndIssue(phaseNum, issueNum);

            bool currentAndExistingMatch = property == currentString;
            bool isNullOrEmpty = string.IsNullOrEmpty(property);

            return isNullOrEmpty || currentAndExistingMatch;
            // Return true if the property is null or empty or if the issue number is being used again effectively nulling the original order
        }
    }
}
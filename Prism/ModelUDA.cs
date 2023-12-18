namespace Prism
{
    public static class ModelUDA
    {


        public static string FabsecEngRef()
        {
            return "SEV-UDA-9";
        }

        public static string SpecialFittingTag()
        {
            return "SEV-UDA-129";
        }

        public static string FirstVariationNumber()
        {
            return "SEV-UDA-12";
        }

        public static string SecondVariationNumber()
        {
            return "SEV-UDA-13";
        }

        public static string FireDFT()
        {
            return "FIRE_DFT";
        }
        public static string FireWFT()
        {
            return "FIRE_WFT";
        }

        public static string FabStamp(string phaseNum, string issueNum)
        {
            return $"FAB-PHASE{phaseNum}-ISSUE{issueNum}";
        }

        public static string FabStampUDA()
        {
            return "SEV-UDA-126";
        }

        public static string Weight()
        {
            return "WEIGHT";
        }

        public static string Length()
        {
            return "LENGTH";
        }

        public static string ObjectLock()
        {
            return "OBJECT_LOCKED";
        }

        public static string PreviousStageName(int stageNumber)
        {
            return $"SEV-UDA-{GetUDANoFromStageNumber(stageNumber) - 1}";
        }

        public static string Pre_Ordered()
        {
            return $"SEV-UDA-40";
        }

        public static string ExcecutionClass()
        {
            return "EN1090_EXC_PART";
        }

        public static string PrelimMark()
        {
            return "PRELIM_MARK";
        }

        public static string CurrentStageName(int stageNumber)
        {
            return $"SEV-UDA-{GetUDANoFromStageNumber(stageNumber)}";
        }

        public static string CurrentStageDate(int stageNumber) //stage date uda is always 1 higher than its related name uda so name uda + 1
        {
            return $"SEV-UDA-{GetUDANoFromStageNumber(stageNumber) + 1}";
        }

        private static int GetUDANoFromStageNumber(int stageNumber)
        {
            int udaNumber = 0;
            switch (stageNumber)
            {
                case 1:
                    udaNumber = 110;
                    break;
                case 2:
                    udaNumber = 112;
                    break;
                case 3:
                    udaNumber = 114;
                    break;
                case 4:
                    udaNumber = 116;
                    break;
                case 5:
                    udaNumber = 118;
                    break;
                case 6:
                    udaNumber = 120;
                    break;
                case 7:
                    udaNumber = 122;
                    break;
                case 8:
                    udaNumber = 127;
                    break;
            }
            return udaNumber;
        }

        public static string PartMarkAtFab()
        {
            return "SEV-UDA-124";
        }

        public static string BoltOrderedBy()
        {
            return "SEV-UDA-136";
            return "BOLT_USERFIELD_7";
        }

        public static string BoltOrderedDate()
        {
            return "SEV-UDA-137";
            return "BOLT_USERFIELD_8";
        }

        public static string BoltOrderedAmount()
        {
            return "SEV-UDA-138";
            return "BOLT_USERFIELD_6";
        }

        public static string BoltShearStudTag()
        {
            return "SEV-UDA-139";
            return "BOLT_USERFIELD_6";
        }

        public static string FabsecUniqueNumber()
        {
            return "USER_PHASE";
            //return "SEV-UDA-125";
        }

        public static string FabsecCarcassInfo()
        {
            return "SEV-UDA-132";
        }

        public static string FabsecStartNumber()
        {
            return "SEV-UDA-133";
        }

        public static string FabsecCarcassOrdered()
        {
            return "SEV-UDA-134";
        }

        public static string FabsecOrderLength()
        {
            return "SEV-UDA-135";
        }

        public static string NextFabsecPrefixNumber() //Hidden UDA
        {
            return "PRISM_PG_NEXT_NUMBER";
        }

        public static string LastUsedPrelim()
        {
            return "PRISM_LAST_USED_PRELIM";
        }

        public static string BSWXAttributeName(Enums.StageTypes stage)
        {
            if (stage == Enums.StageTypes.Prelim3)
            {
                return "-SEV_PRELIM";
            }
            if (stage == Enums.StageTypes.FAB)
            {
                return "-SEV_FAB";
            }
            return "";
        }
    }
}
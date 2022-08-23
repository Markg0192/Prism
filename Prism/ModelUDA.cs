namespace Prism
{
    public static class ModelUDA
    {
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

        public static string ObjectLock()
        {
            return "OBJECT_LOCKED";
        }

        public static string PreviousStageName(int stageNumber)
        {
            return $"SEV-UDA-{GetUDANoFromStageNumber(stageNumber) - 1}";
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
            return "BOLT_USERFIELD_7";
        }

        public static string BoltOrderedDate()
        {
            return "BOLT_USERFIELD_8";
        }

        public static string FabsecUniqueNumber()
        {
            //return "USER_PHASE";
            return "SEV-UDA-125";
        }

        public static string NextFabsecPrefixNumber() //Hidden UDA
        {
            return "PRISM_PG_NEXT_NUMBER";
        }

        public static string LastUsedPrelim()
        {
            return "PRISM_LAST_USED_PRELIM";
        }
    }
}
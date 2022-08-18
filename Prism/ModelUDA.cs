namespace Prism
{
    public static class ModelUDA
    {
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
           return $"PRISM-{stageNumber - 1}-NAME";
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
            return $"PRISM-{stageNumber}-NAME";
        }

        public static string CurrentStageDate(int stageNumber)
        {
            return $"PRISM-{stageNumber}-DATE";
        }

        public static string CurrentStageNumber(int stageNumber)
        {
            return $"PRISM-{stageNumber}-NUMBER";
        }

        public static string BoltOrderedBy()
        {
            return "BOLT_USERFIELD_7";
        }

        public static string BoltOrderedDate()
        {
            return "BOLT_USERFIELD_8";
        }

        public static string UserPhase()
        {
            return "USER_PHASE";
        }

        public static string NextFabsecPrefixNumber()
        {
            return "PRISM_PG_NEXT_NUMBER";
        }
    }
}
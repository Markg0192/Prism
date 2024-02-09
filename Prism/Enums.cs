namespace Prism
{
    public class Enums
    {
        public enum StageTypes { Unassigned, Prelim1, Prelim2, Prelim3, Check1, Check2, Check3, FAB, Bolt, PrelimPG, RocketPacket};
        public enum Factory { SNI, SUK, Unknown};
        public enum IgnoreType { AutoFix, Ignore, Stop, Unspecified };
        public enum Error { Execution, Orientation, NameAndClass}
    }
}
namespace UGSSpace
{
    public class UGSActionCategory_InputPreferences : UGSActionCategory
    {
        public override int Order => 0;
        public override FlowBuilderCategory Category => new UGSActionCategory_Inputs();

        public override string NameInBuilderList_Spanish => "Input Preferences";

        public override string NameInBuilderList_English => "Input Preferences";

        public override string DescriptionSpanish => "Input Preferences";

        public override string DescriptionEnglish => "Input Preferences";

        public override string IconName => "d_EventTrigger Icon";
    }
}

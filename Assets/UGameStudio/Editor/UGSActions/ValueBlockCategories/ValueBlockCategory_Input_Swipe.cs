namespace UGSSpace
{
    public class ValueBlockCategory_Input_Swipe : ValueBlockCategory
    {
        public override int Order => 0;
        public override FlowBuilderCategory Category => new ValueBlockCategory_Input();

        public override string NameInBuilderList_Spanish => "Swipe";

        public override string NameInBuilderList_English => "Swipe";

        public override string DescriptionSpanish => "Swipe";

        public override string DescriptionEnglish => "Swipe";

        public override string IconName => "d_AvatarPivot@2x";
    }
}

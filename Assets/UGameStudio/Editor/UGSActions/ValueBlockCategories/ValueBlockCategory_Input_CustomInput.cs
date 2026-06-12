namespace UGSSpace
{
    public class ValueBlockCategory_Input_CustomInput : ValueBlockCategory
    {
        public override int Order => 0;
        public override FlowBuilderCategory Category => new ValueBlockCategory_Input();

        public override string NameInBuilderList_Spanish => "Input Personalizados";

        public override string NameInBuilderList_English => "Custom Input";

        public override string DescriptionSpanish => "En esta categoria encontraras todo lo relacionado con \"Input\" personalizados";

        public override string DescriptionEnglish => "In this category you will find everything related to customized \"Input\".";

        public override string IconName => "d_EventTrigger Icon";
    }
}

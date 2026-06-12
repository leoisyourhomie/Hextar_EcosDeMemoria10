namespace UGSSpace
{
    public class ValueBlockCategory_Input_PlayerInputManager : ValueBlockCategory
    {
        public override int Order => 0;
        public override FlowBuilderCategory Category => new ValueBlockCategory_Input();

        public override string NameInBuilderList_Spanish => "Player Input Manager (Componente)";

        public override string NameInBuilderList_English => "Player Input Manager (Component)";

        public override string DescriptionSpanish => "En esta categoria encontraras todo lo relacionado con el componente PlayerInputManager";

        public override string DescriptionEnglish => "In this category you will find everything related to the management of \"Player Input\".";

        public override string IconName => "d_EventTrigger Icon";
    }
}

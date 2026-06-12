namespace UGSSpace
{
    public class UGSActionCategory_Inputs_PlayerInput : UGSActionCategory
    {
        public override int Order => 0;
        public override FlowBuilderCategory Category => new UGSActionCategory_Inputs();

        public override string NameInBuilderList_Spanish => "Player Input (Componente)";

        public override string NameInBuilderList_English => "Player Input (Component)";

        public override string DescriptionSpanish => "En esta categoria encontraras todo lo relacionado con el componente de PlayerInput.";

        public override string DescriptionEnglish => "In this category you will find everything related to the PlayerInput component.";

        public override string IconName => "d_EventTrigger Icon";
    }
}

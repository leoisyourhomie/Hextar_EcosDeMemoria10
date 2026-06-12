namespace UGSSpace
{
    public class UGSActionCategory_Inputs_PlayerInputManager : UGSActionCategory
    {
        public override int Order => 0;
        public override FlowBuilderCategory Category => new UGSActionCategory_Inputs();

        public override string NameInBuilderList_Spanish => "Player Input Manager (Componente)";

        public override string NameInBuilderList_English => "Player Input Manager (Component)";

        public override string DescriptionSpanish => "En esta categoria encontraras todo lo relacionado con la gestión del PlayerInputManager.";

        public override string DescriptionEnglish => "In this category you will find everything related to the management of the PlayerInputManager.";

        public override string IconName => "d_EventTrigger Icon";
    }
}

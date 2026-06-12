namespace UGSSpace
{
    public class ValueBlockCategory_Input_InputFromAnyPlayer : ValueBlockCategory
    {
        public override int Order => 0;
        public override FlowBuilderCategory Category => new ValueBlockCategory_Input();

        public override string NameInBuilderList_Spanish => "Input Desde Cualquier Jugador (Input Action Asset)";

        public override string NameInBuilderList_English => "Input From Any Player (Input Action Asset)";

        public override string DescriptionSpanish => "En esta categoria encontraras todo lo relacionado con \"Input\" para cualquier jugador";

        public override string DescriptionEnglish => "In this category you will find everything related to \"Input\" for any player.";

        public override string IconName => "d_EventTrigger Icon";
    }
}

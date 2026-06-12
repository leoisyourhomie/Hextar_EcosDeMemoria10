namespace UGSSpace
{
    public class ValueBlockCategory_Input_InputFromPlayer : ValueBlockCategory
    {
        public override int Order => 0;
        public override FlowBuilderCategory Category => new ValueBlockCategory_Input();

        public override string NameInBuilderList_Spanish => "Input Desde Jugador (Input Action Asset)";

        public override string NameInBuilderList_English => "Input From Player (Input Action Asset)";

        public override string DescriptionSpanish => "En esta categoria encontraras todo lo relacionado con \"Input\" para el jugador";

        public override string DescriptionEnglish => "In this category you will find everything related to \"Input\" for the player.";

        public override string IconName => "d_EventTrigger Icon";
    }
}

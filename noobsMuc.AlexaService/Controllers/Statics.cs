namespace noobsMuc.AlexaService.Controllers
{
    public class Statics
    {
        // Intents
        public const string FuerMichIntent = "FuerMichIntent";
        public const string WiesnIntent = "WiesnIntent";

        public const string AmazonNavigateHomeIntent = "AMAZON.NavigateHomeIntent";
        public const string AmazonStopIntent = "AMAZON.StopIntent";
        public const string AmazonCancelIntent = "AMAZON.CancelIntent";
        public const string AmazonHelpIntent = "AMAZON.HelpIntent";


        public static string StopMessage = "Ok. Wir trainieren später.";
        public static string CancelMessage = "Ok. Training abgebrochen.";
        public static string NavigateHomeMessage = "Ok. Geh nach hause.";
        
        public static string HelpMessage = "Lass mich für dich trainieren. sag zum beispiel: laufen oder crossfit: Was soll ich trainieren?";
        public static string WelcomeMessage = "Willkommen beim täglichen Training. Du kannst Laufen, Krafttraining oder einzelne Muskelgruppen trainieren. Was willst du machen?";
        public static string ErrorMessage = "Die Anfrage verstehe ich nicht!";
    }
}

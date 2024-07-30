using Alexa.NET.Response;
using Microsoft.AspNetCore.Mvc;

namespace noobsMuc.AlexaService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrainingController : AlexaBaseController
    {
        private IConfiguration _config;
        private readonly ILogger<TrainingController> _logger;

        public TrainingController(IConfiguration config, ILogger<TrainingController> logger) : base(config, logger)
        {
            _config = config;
            _logger = logger;
        }

        [HttpGet("test")]
        public string Get()
        {
            return "toll";
            //string message = $"Alexa trained at {DateTime.Now.ToString(CultureInfo.InvariantCulture)} in version {GetVersion()}";
            //m_Logger.LogDebug(message);
            //return message;
        }

        [HttpPost]
        public async Task<IActionResult> HandleSkillRequestTrainingAsync()
        {
            return await HandleSkillRequestAsync();
        }


        [HttpGet("version")]
        public string GetVersion()
        {
            var version = _config.GetValue<string>("Version");
            return version;
        }

        [HttpGet("appId")]
        public string GetAppIdResponse()
        {
            return GetAppId();
        }

        internal override Massages GetMessages(Language language)
        {
            if (language == Language.de)
            {
                return new Massages
                {
                    Welcome = "Willkommen beim täglichen Training. Du kannst Laufen, Krafttraining oder einzelne Muskelgruppen trainieren. Was willst du machen?",
                    Stop = "Ok. Wir trainieren später",
                    Cancel = "Ok. Training abgebrochen.",
                    NavigateHome = "Ok. Geh nach hause.",
                    Help = "Lass mich für dich trainieren.sag zum beispiel: laufen oder crossfit: Was soll ich trainieren ? ",
                    Error = "Die Anfrage verstehe ich nicht!"
                };
            }
            else
            {
                return new Massages
                {
                    Welcome = "Welcome to dailyteaining. You can run, strength train or train individual muscle groups. What do you want to do?",
                    Stop = "Ok. We train later",
                    Cancel = "Ok. Training canceled.",
                    NavigateHome = "Ok. Let's go home",
                    Help = "Let me train for you. Say for example: running or crossfit: what should I train? ",
                    Error = "I don't understand the request!"
                };
            }
        }

        internal override SkillAnswer GetSkillAnswer(string intentName, Language language)
        {
            _logger.LogDebug("GetSkillAnswer " + intentName);
            if (intentName.Equals(TrainingSportIntent))
            {
                return GetSportAnswer(language);
            }

            if (intentName.Equals(TrainingLaufenIntent))
            {
                return GetLaufenAnswer(0, language);
            }

            if (intentName.Equals(TrainingRadfahrenIntent))
            {
                return GetRadfahrenAnswer(language);
            }

            if (intentName.Equals(TrainingWiesnIntent))
            {
                return GetBierAnswer(language);
            }

            if (intentName.Equals(TrainingHalbMarathonIntent))
            {
                return GetLaufenAnswer(21, language);
            }


            if (intentName.Equals(TrainingMarathonIntent))
            {
                return GetLaufenAnswer(42, language);
            }


            if (intentName.Equals(TrainingIronVikingIntent))
            {
                return GetVikingAnswer(42, 100, language);
            }


            if (intentName.Equals(TrainingUltraVikingIntent))
            {
                return GetVikingAnswer(60, 135, language);
            }

            //TrainingFürMichIntent
            return GetSportAnswer(language);
        }


        private SkillAnswer GetBierAnswer(Language language)
        {
            Random rnd = new Random();
            int bier = rnd.Next(1, 11);
            if (language == Language.de)
                return new SkillAnswer($"Gute Leistung du hast {bier} Maß getrunken. Training beendet", true);

            return new SkillAnswer($"Good job, you drank {bier} mass. Training finished", true);
        }

        private SkillAnswer GetSportAnswer(Language language)
        {
            Random rnd = new Random();
            int kcal = rnd.Next(133, 3132);

            if(language == Language.de)
                return new SkillAnswer($"Super Leistung. Du hast {kcal} kalorien verbraucht. Training beendet", true);

            return new SkillAnswer($"Fantastic performance. You have consumed {kcal} calories. Training finished", true);
        }

        private SkillAnswer GetLaufenAnswer(int km, Language language)
        {
            Random rnd = new Random();
            if (km == 0)
            {
                km = rnd.Next(3, 42);
            }

            double kmh1 = rnd.Next(4, 22);

            return AnswerWithDurationAndDistance(km, kmh1, rnd, 0, language);
        }

        private SkillAnswer GetRadfahrenAnswer(Language language)
        {
            Random rnd = new Random();
            int km = rnd.Next(10, 250);
            double kmh1 = rnd.Next(10, 50);

            return AnswerWithDurationAndDistance(km, kmh1, rnd, 0,language);
        }


        private  SkillAnswer GetVikingAnswer(int km, int obstacles, Language language)
        {
            Random rnd = new Random();
            double kmh1 = rnd.Next(4, 22);

            return AnswerWithDurationAndDistance(km, kmh1, rnd, obstacles, language);
        }


        private SkillAnswer AnswerWithDurationAndDistance(int km, double kmh1, Random rnd, int obstaclesCount, Language language)
        {
            double kmh2 = rnd.Next(0, 9);
            double duration = km / (kmh1 + (kmh2 / 10));
            TimeSpan timespan = TimeSpan.FromHours(duration);
            int kcalFactor = rnd.Next(17, 30);
            int kcal = (int)Math.Round(timespan.TotalMinutes * kcalFactor, 0);


            if (obstaclesCount > 0)
            {
                if (language == Language.de)
                    return new SkillAnswer(
                        $"Super Leistung. Du hast {km} kilometer und über {obstaclesCount} Hindernisse in {GetTime(timespan, language)} geschafft und dabei {kcal} kalorien verbraucht. Training beendet",
                        true);

                return new SkillAnswer(
                   $"Super performance. You have completed {km} kilometers and over {obstaclesCount} obstacles in {GetTime(timespan, language)} while burning {kcal} calories. Training ended",
                   true);
            }

            if (language == Language.de)
                return new SkillAnswer(
                    $"Super Leistung. Du hast {km} kilometer in {GetTime(timespan, language)} geschafft und dabei {kcal} kalorien verbraucht. Training beendet",
                    true);

            return new SkillAnswer(
                $"Super performance. You have covered {km} kilometers in {GetTime(timespan, language)} and burned {kcal} calories. Training ended",
                true);

        }

        private string GetTime(TimeSpan timespan, Language language)
        {
            if (language == Language.de)
            {
                string hoursText = "Stunden";
                string minutesText = "Minuten";
                if (timespan.Hours < 2)
                {
                    hoursText = "Stunde";
                }

                if (timespan.Hours < 2)
                {
                    minutesText = "Minute";
                }

                return $"{timespan.Hours} {hoursText} und {timespan.Minutes} {minutesText}";
            }

            string hoursTextEn = "hours";
            string minutesTextEn = "minutes";
            if (timespan.Hours < 2)
            {
                hoursTextEn = "hour";
            }

            if (timespan.Hours < 2)
            {
                minutesTextEn = "minute";
            }

            return $"{timespan.Hours} {hoursTextEn} and {timespan.Minutes} {minutesTextEn}";
        }

        internal override string GetAppId()
        {
            return _config.GetValue<string>("SkillApplicationIdTraining");
        }
    }
}

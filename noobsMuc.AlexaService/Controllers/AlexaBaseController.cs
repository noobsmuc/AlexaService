using System.Text;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace noobsMuc.AlexaService.Controllers
{
    public abstract class AlexaBaseController : ControllerBase
    {
        private IConfiguration _config;
        private ILogger _logger;

        public const string TrainingSportIntent = "SportIntent";
        public const string TrainingLaufenIntent = "LaufenIntent";
        public const string TrainingRadfahrenIntent = "RadfahrenIntent";
        public const string TrainingWiesnIntent = "WiesnIntent";
        public const string TrainingHalbMarathonIntent = "TrainingHalbMarathonIntent";
        public const string TrainingMarathonIntent = "TrainingMarathonIntent";
        public const string TrainingIronVikingIntent = "IronVikingIntent";
        public const string TrainingUltraVikingIntent = "UltraVikingIntent";

        public const string TrainingFürMichIntent = "FuerMichIntent";

        public const string AmazonNavigateHomeIntent = "AMAZON.NavigateHomeIntent";
        public const string AmazonStopIntent = "AMAZON.StopIntent";
        public const string AmazonCancelIntent = "AMAZON.CancelIntent";
        public const string AmazonHelpIntent = "AMAZON.HelpIntent";
        public const string AmazonRepeatIntent = "AMAZON.RepeatIntent";

        private string[] ownSkill = {TrainingSportIntent, TrainingLaufenIntent, TrainingRadfahrenIntent, TrainingWiesnIntent,
                                    TrainingHalbMarathonIntent, TrainingMarathonIntent,
                                    TrainingIronVikingIntent, TrainingUltraVikingIntent,
                                    TrainingFürMichIntent };

        public AlexaBaseController(IConfiguration config, ILogger logger)
        {
            _logger = logger;
            _config = config;
        }

        private Language GetLanguage(string locale)
        {
            if (locale.StartsWith("de-"))
                return Language.de;

            return Language.en;
        }

        internal async Task<IActionResult> HandleSkillRequestAsync()
        {
            string body;
            using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                body = await reader.ReadToEndAsync();
                //m_Logger.LogDebug("body: " + body);
            }

            //_logger.LogInformation($"Request in Api with Version={GetVersion()}");
            SkillRequest input = JsonConvert.DeserializeObject<SkillRequest>(body);

            Language language = GetLanguage(input.Request.Locale);

            string signatureCertChainUrl = Request.Headers["SignatureCertChainUrl"];
            string signature = Request.Headers["Signature"];

            _logger.LogDebug("signatureCertChainUrl: " + signatureCertChainUrl);
            _logger.LogDebug("signature: " + signature);

            if (string.IsNullOrEmpty(signatureCertChainUrl) || string.IsNullOrEmpty(signature))
                return BadRequest();

            _logger.LogDebug("HandleSkillRequest: " + input.Session.Application.ApplicationId);

            if (input.Session.Application.ApplicationId != GetAppId()
                && input.Session.Application.ApplicationId != "amzn1.ask.skill.cd670610-a755-4030-af85-a435b0fe3afd")
            {
                _logger.LogDebug("ApplicationId wrong");
                return BadRequest();
            }

            if (!RequestVerification.VerifyCertificateUrl(new Uri(signatureCertChainUrl)))
            {
                _logger.LogDebug("VerifyCertificateUrl: false");
                return BadRequest();
            }

            var certificate = RequestVerification.GetCertificate(new Uri(signatureCertChainUrl)).Result;
            if (!RequestVerification.VerifyChain(certificate))
            {
                _logger.LogDebug("VerifyChain: false");
                return BadRequest();
            }

            if (!RequestVerification.AssertHashMatch(certificate, signature, body))
            {
                _logger.LogDebug("AssertHashMatch: false");
                return BadRequest();
            }

            if (!RequestVerification.RequestTimestampWithinTolerance(input))
            {
                _logger.LogDebug("RequestTimestampWithinTolerance: false");
                return BadRequest();
            }

            Massages messages = GetMessages(language);

            var requestType = input.GetRequestType();
            if (requestType == typeof(IntentRequest))
            {
                _logger.LogInformation("requestType IntentRequest");
                var response = HandleIntents(input, messages, language);

                return Ok(response);
            }

            if (requestType == typeof(LaunchRequest))
            {
                _logger.LogInformation("requestType LaunchRequest");
                return Ok(BuildResponse(messages.Welcome, false));
            }

            _logger.LogWarning("ErrorResponse");
            return Ok(BuildResponse(messages.Error, true));
        }

        private string HandleIntents(SkillRequest input, Massages messages, Language language)
        {
            //return ManuellResponse();

            var intentRequest = input.Request as IntentRequest;
            _logger.LogInformation("intent: " + intentRequest.Intent.Name);

            if (intentRequest.Intent.Name.Equals(AmazonRepeatIntent))
            {
                return BuildResponse(messages.Welcome, true);
            }

            if (intentRequest.Intent.Name.Equals(AmazonStopIntent))
            {
                return BuildResponse(messages.Stop, true);
            }

            if (intentRequest.Intent.Name.Equals(AmazonCancelIntent))
            {
                return BuildResponse(messages.Cancel, true);
            }

            if (intentRequest.Intent.Name.Equals(AmazonNavigateHomeIntent))
            {
                return BuildResponse(messages.NavigateHome, false);
            }

            if (ownSkill.Contains(intentRequest.Intent.Name))
            {
                _logger.LogDebug("ownSkill: " + intentRequest.Intent.Name);

                var answer = GetSkillAnswer(intentRequest.Intent.Name, language);
                return BuildResponse(answer.Answer, answer.EndSession);
            }

            _logger.LogWarning("intent error: " + intentRequest.Intent.Name);
            return BuildResponse(messages.Help, false);
        }

        private static string ManuellResponse()
        {
            // create the speech response
            var speech = new SsmlOutputSpeech();
            speech.Ssml = "<speak>Today is <say-as interpret-as=\"date\">????0922</say-as>.</speak>";

            // create the reprompt speech
            var repromptMessage = new SsmlOutputSpeech();
            repromptMessage.Ssml = "Would you like to know what tomorrow is?";

            // create the reprompt object
            var repromptBody = new Reprompt();
            repromptBody.OutputSpeech = repromptMessage;

            // create the response
            var responseBody = new ResponseBody();
            responseBody.OutputSpeech = speech;
            responseBody.ShouldEndSession = false; // this triggers the reprompt
            responseBody.Reprompt = repromptBody;
            responseBody.Card = new SimpleCard { Title = "Test", Content = "Testing Alexa" };

            var skillResponse = new SkillResponse();
            skillResponse.Response = responseBody;
            skillResponse.Version = "1.0";

            var jsonRespo = JsonConvert.SerializeObject(skillResponse);

            return jsonRespo;
        }

        private string BuildResponse(string message, bool shouldEndSession)
        {
            var speech = new PlainTextOutputSpeech { Text = message };
            var response = new ResponseBody { OutputSpeech = speech, ShouldEndSession = false };
            var skillResponse = new SkillResponse { Response = response, Version = "1.0" };


            _logger.LogDebug("BuildResponse " + message);

            var jsonRespo = JsonConvert.SerializeObject(skillResponse);

            return jsonRespo;
        }

        internal abstract string GetAppId();

        internal abstract Massages GetMessages(Language language);

        internal abstract SkillAnswer GetSkillAnswer(string intentName, Language language);
    }
}

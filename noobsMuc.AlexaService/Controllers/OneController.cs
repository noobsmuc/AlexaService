using System;
using System.Globalization;
using System.IO;
using System.Text;
using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace noobsMuc.AlexaService.Controllers
{
    [Produces("application/json")]
    [Route("api/one")]
    public class OneController : Controller
    {
        private static ILogger<OneController> m_Logger;
        private IConfiguration _config;
        private string _appid;

        public OneController(ILogger<OneController> logger, IConfiguration config)
        {
            m_Logger = logger;
            _config = config;
            _appid = _config.GetValue<string>("SkillApplicationId");
        }

        [HttpGet]
        public string Get()
        {
            string message = "Alexa get alive " + DateTime.Now.ToString(CultureInfo.InvariantCulture);
            m_Logger.LogDebug(message);
            return message;
        }

        [HttpPost]
        public IActionResult HandleSkillRequest()
        {
            string body;
            using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                body = reader.ReadToEnd();
                m_Logger.LogDebug("body: " + body);
            }

            SkillRequest input = JsonConvert.DeserializeObject<SkillRequest>(body);

            string signatureCertChainUrl = Request.Headers["SignatureCertChainUrl"];
            string signature = Request.Headers["Signature"];

            m_Logger.LogDebug("signatureCertChainUrl: " + signatureCertChainUrl);
            m_Logger.LogDebug("signature: " + signature);

            if (string.IsNullOrEmpty(signatureCertChainUrl)  || string.IsNullOrEmpty(signature))
                return BadRequest();

            m_Logger.LogDebug("HandleSkillRequest: " + input.Session.Application.ApplicationId);

            if (input.Session.Application.ApplicationId != _appid)
            {
                m_Logger.LogDebug("ApplicationId wrong");
                return BadRequest();
            }

            if (!RequestVerification.VerifyCertificateUrl(new Uri(signatureCertChainUrl)))
            {
                m_Logger.LogDebug("VerifyCertificateUrl: false");
                return BadRequest();
            }

            var certificate = RequestVerification.GetCertificate(new Uri(signatureCertChainUrl)).Result;
            if (!RequestVerification.VerifyChain(certificate))
            {
                m_Logger.LogDebug("VerifyChain: false");
                return BadRequest();
            }
            
            if (!RequestVerification.AssertHashMatch(certificate, signature, body))
            {
                m_Logger.LogDebug("AssertHashMatch: false");
                return BadRequest();
            }

            if (!RequestVerification.RequestTimestampWithinTolerance(input))
            {
                m_Logger.LogDebug("RequestTimestampWithinTolerance: false");
                return BadRequest();
            }

            var requestType = input.GetRequestType();
            if (requestType == typeof(IntentRequest))
            {
                m_Logger.LogInformation("requestType IntentRequest");
                var response = HandleIntents(input);
                return Ok(response);
            }

            if (requestType == typeof(LaunchRequest))
            {
                m_Logger.LogInformation("requestType LaunchRequest");
                return Ok(BuildResponse(Statics.WelcomeMessage, false));
            }

            m_Logger.LogWarning("ErrorResponse");
            return Ok(BuildResponse(Statics.ErrorMessage, true));
        }


        private SkillResponse HandleIntents(SkillRequest input)
        {
            var intentRequest = input.Request as IntentRequest;
            m_Logger.LogInformation("intent: " + intentRequest.Intent.Name);

            if (intentRequest.Intent.Name.Equals(Statics.AmazonStopIntent))
            {
                return BuildResponse(Statics.StopMessage, true);
            }

            if (intentRequest.Intent.Name.Equals(Statics.AmazonCancelIntent))
            {
                return BuildResponse(Statics.CancelMessage, true);
            }

            if (intentRequest.Intent.Name.Equals(Statics.AmazonNavigateHomeIntent))
            {
                return BuildResponse(Statics.NavigateHomeMessage, false);
            }

            if (intentRequest.Intent.Name.Equals(Statics.FuerMichIntent))
            {
                return BuildResponse(GetKcalMessages(), true);
            }

            if (intentRequest.Intent.Name.Equals(Statics.WiesnIntent))
            {
                return BuildResponse(GetBierMessages(), true);
            }

            m_Logger.LogWarning("intent: error: " + intentRequest.Intent.Name);
            return BuildResponse(Statics.HelpMessage, false);
        }


        private SkillResponse BuildResponse(string message, bool shouldEndSession)
        {
            var speech = new SsmlOutputSpeech { Ssml = "<speak>" + message + "</speak>" };
            var resp = ResponseBuilder.Tell(speech);
            resp.Response.ShouldEndSession = shouldEndSession;
            return resp;
        }

        private static string GetBierMessages()
        {
            Random rnd = new Random();
            int bier = rnd.Next(1, 11);

            return $"Gute Leistung du hast {bier} Maß getrunken. Training beendet";
        }

        private static string GetKcalMessages()
        {
            Random rnd = new Random();
            int kcal = rnd.Next(133, 3132);

            return $"Super Leistung. Du hast {kcal} kalorien verbraucht. Training beendet";
        }
    }
}
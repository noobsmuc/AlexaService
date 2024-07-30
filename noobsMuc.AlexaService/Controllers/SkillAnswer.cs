
namespace noobsMuc.AlexaService.Controllers
{
    public class SkillAnswer
    {
        public SkillAnswer(string answer, bool endSession)
        {
            Answer = answer;
            EndSession = endSession;
        }
        public string Answer { get;  }
        public bool EndSession { get; }
    }
}

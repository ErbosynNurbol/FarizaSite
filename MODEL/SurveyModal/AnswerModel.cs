using System.Collections.Generic;
namespace MODEL.SurveyModal
{
    public class AnswerModel
    {
        public int SurveyId { get; set; }
        public List<int> QuestionIds { get; set; }
        public List<int> QuestionOptionIds { get; set; }
    }
}

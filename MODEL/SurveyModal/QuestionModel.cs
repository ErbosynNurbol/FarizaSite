using System.Collections.Generic;

namespace MODEL.SurveyModal
{
    public class QuestionModel
    {
        public int QuestionId { get; set; }
        public int IsDelete { get; set; }
        public int QuestionIndex { get; set; }
        public string QuestionTitle { get; set; }
        public int QuestionType { get; set; }
        public int TolalVoteCount { get; set; }
        public int OptionGroupId { get; set; }
        public int ChoiceCount { get; set; }
        public List<OptionModel> OptionList { get; set; }
    }
}

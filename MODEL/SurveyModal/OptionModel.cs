using System.Data;

namespace MODEL.SurveyModal
{
    public class OptionModel
    {
        public int IsDelete { get; set; }
        public int QuestionOptionId { get; set; }
        public int OptionId { get; set; }
        public string OptionTitle { get; set; }
        public string OptionDescription { get; set; }
        public int OptionGroupId { get; set; }
        public sbyte CorrectAnswer { get; set; }
        public int ChoiceCount { get; set; }
        public int OptionIndex { get; set; }
    }
}

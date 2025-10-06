using System;
using System.Collections.Generic;
using System.Text;

namespace MODEL.SurveyModal
{
    public class SurveyModel
    {
        public int SurveyId { get; set; }
        public int ChapterId{get;set;}
        public string ThumbnailUrl { get; set; }
        public string KeyWord { get; set; }
        public string Language { get; set; }
        public string SurveyTitle { get; set; }
        public int ShareType { get; set; }
        public string  VideoUrl { get; set; }
        public string ShortDescription { get; set; }
        public string SurveyQuestionArrStr { get; set; }
        public List<QuestionModel> SurveyQuestionList { get; set; }
    }
}

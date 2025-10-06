namespace MODEL.ViewModels;

public class LessonModel
{
        public int Id {get;set;}
        public string ChapterTitle {get;set;}
        
        public string ChapterShortDescription{get;set;}
        public string Title { get; set; }
        public string ShortDescription { get; set; }
        public string ThumbnailUrl {get;set;}
        public string VideoUrl{get;set;}
        public List<QuestionItemModel> Questions {get;set;}
}
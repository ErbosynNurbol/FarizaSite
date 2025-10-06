namespace MODEL.ViewModels;

public class ChapterModel
{
        public string Title { get; set; }
        public string ShortDescription { get; set; }
        
        public List<LessonItemModel> Lessons{get;set;}
}
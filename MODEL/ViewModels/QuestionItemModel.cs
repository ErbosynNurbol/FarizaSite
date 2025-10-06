namespace MODEL.ViewModels;

public class QuestionItemModel
{
        public int Id { get; set; }
        public string Title { get; set; }
        public int Type { get; set; }
        public int OptionGroupId { get; set; }
        public List<OptionItemModel> Options { get; set; }
}
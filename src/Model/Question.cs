namespace multiply_choice_trainer.src.Model
{
    public record Option(int Id, string Text);

    public record Questions(string Question, QuestionType Type, List<Option> Options, List<int> CorrectAnswers, List<int> IncorrectAnswers, int Points, string LeftHeader = "", string RightHeader = "");

    public enum QuestionType
    {
        A,
        P,
        K
    }
}
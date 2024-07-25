using multiply_choice_trainer.src.Model;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace multiply_choice_trainer
{
    public partial class MainPage : ContentPage
    {
        private const string LastFilePathKey = "LastFilePath";
        private List<Questions> _questions = [];
        private int _currentQuestionIndex = 0;

        public MainPage()
        {
            InitializeComponent();
            LoadLastUsedFilePath();
        }

        private void LoadLastUsedFilePath()
        {
            var lastFilePath = Preferences.Get(LastFilePathKey, string.Empty);

            LoadQuestionsFromFile(lastFilePath);
        }

        private async void LoadQuestionsFromFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !Path.Exists(filePath))
                return;

            try
            {
                string json = File.ReadAllText(filePath);
                if (string.IsNullOrWhiteSpace(json))
                    return;
                var options = new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter() }
                };

                _questions = JsonSerializer.Deserialize<List<Questions>>(json, options);

                Preferences.Set(LastFilePathKey, filePath);
                _currentQuestionIndex = 0;
                DisplayQuestion();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Fehler", $"Fehler beim Laden der Datei: {ex.Message}", "OK");
            }
        }

        private void DisplayQuestion()
        {
            if (_questions == null || _questions.Count == 0) return;

            var currentQuestion = _questions[_currentQuestionIndex];
            QuestionLabel.Text = currentQuestion.Question;
            OptionsStackLayout.Children.Clear();

            if (currentQuestion.Type == QuestionType.A || currentQuestion.Type == QuestionType.P)
            {
                foreach (var option in currentQuestion.Options)
                {
                    var grid = new Grid
                    {
                        ColumnDefinitions = {
                    new ColumnDefinition { Width = new GridLength(100) }, // Spalte für CheckBox
                    new ColumnDefinition { Width = GridLength.Star }    // Spalte für Label
                },
                        Margin = new Thickness(0, 5)
                    };

                    var checkBox = new CheckBox { BindingContext = option.Id };
                    var label = new Label
                    {
                        Text = option.Text,
                        Style = (Style)Resources["LabelStyle"],
                        LineBreakMode = LineBreakMode.WordWrap
                    };

                    grid.Children.Add(checkBox);
                    Grid.SetColumn(checkBox, 0);

                    grid.Children.Add(label);
                    Grid.SetColumn(label, 1);

                    OptionsStackLayout.Children.Add(grid);
                }
            }
            else if (currentQuestion.Type == QuestionType.K)
            {
                var headerGrid = new Grid
                {
                    ColumnDefinitions = {
                new ColumnDefinition { Width = new GridLength(100) }, // Spalte für Links-Checkboxen
                new ColumnDefinition { Width = new GridLength(100) },  // Spalte für Rechts-Checkboxen
                new ColumnDefinition { Width = GridLength.Star } // Spalte für Label
            },
                    Margin = new Thickness(0, 5)
                };

                var leftHeader = new Label
                {
                    Text = currentQuestion.LeftHeader,
                    Style = (Style)Resources["LabelStyle"]
                };

                var rightHeader = new Label
                {
                    Text = currentQuestion.RightHeader,
                    Style = (Style)Resources["LabelStyle"]
                };

                headerGrid.Children.Add(leftHeader);
                Grid.SetColumn(leftHeader, 0);

                headerGrid.Children.Add(rightHeader);
                Grid.SetColumn(rightHeader, 1);

                OptionsStackLayout.Children.Add(headerGrid);

                foreach (var option in currentQuestion.Options)
                {
                    var grid = new Grid
                    {
                        ColumnDefinitions = {
                    new ColumnDefinition { Width = new GridLength(100) }, // Spalte für Links-Checkbox
                    new ColumnDefinition { Width = new GridLength(100) },  // Spalte für Rechts-Checkbox
                    new ColumnDefinition { Width = GridLength.Star }    // Spalte für Label
                },
                        Margin = new Thickness(0, 5)
                    };

                    var leftCheckBox = new CheckBox { BindingContext = option.Id };
                    var label = new Label
                    {
                        Text = option.Text,
                        Style = (Style)Resources["LabelStyle"],
                        LineBreakMode = LineBreakMode.WordWrap
                    };
                    var rightCheckBox = new CheckBox { BindingContext = option.Id };

                    grid.Children.Add(leftCheckBox);
                    Grid.SetColumn(leftCheckBox, 0);

                    grid.Children.Add(rightCheckBox);
                    Grid.SetColumn(rightCheckBox, 1);

                    grid.Children.Add(label);
                    Grid.SetColumn(label, 2);

                    OptionsStackLayout.Children.Add(grid);
                }
            }
        }

        private void OnNextClicked(object sender, EventArgs e)
        {
            if (_currentQuestionIndex < _questions.Count - 1)
            {
                _currentQuestionIndex++;
                DisplayQuestion();
            }
        }

        private void OnPreviousClicked(object sender, EventArgs e)
        {
            if (_currentQuestionIndex > 0)
            {
                _currentQuestionIndex--;
                DisplayQuestion();
            }
        }

        private void OnCheckAnswerClicked(object sender, EventArgs e)
        {
            if (_questions == null || _questions.Count == 0)
                return;

            var currentQuestion = _questions[_currentQuestionIndex];

            // Iteriere durch die Kinder des OptionsStackLayout
            foreach (var child in OptionsStackLayout.Children)
            {
                if (child is Grid grid)
                {
                    var checkBoxes = grid.Children.OfType<CheckBox>().ToList();

                    if (currentQuestion.Type == QuestionType.A || currentQuestion.Type == QuestionType.P)
                    {
                        var checkBox = checkBoxes.FirstOrDefault();
                        if (checkBox != null && checkBox.BindingContext is int optionId)
                        {
                            if (currentQuestion.CorrectAnswers.Contains(optionId))
                            {
                                grid.BackgroundColor = Colors.Green;
                            }
                            else
                            {
                                grid.BackgroundColor = checkBox.IsChecked ? Colors.Red : Colors.Transparent;
                            }
                        }
                    }
                    else if (currentQuestion.Type == QuestionType.K)
                    {
                        if (checkBoxes.Count == 2)
                        {
                            var leftCheckBox = checkBoxes[0];
                            var rightCheckBox = checkBoxes[1];

                            if (leftCheckBox.BindingContext is int leftOptionId &&
                                rightCheckBox.BindingContext is int rightOptionId)
                            {
                                bool isLeftCorrect = currentQuestion.CorrectAnswers.Contains(leftOptionId);
                                bool isRightCorrect = currentQuestion.IncorrectAnswers.Contains(rightOptionId);

                                if (leftCheckBox.IsChecked && rightCheckBox.IsChecked)
                                {
                                    grid.BackgroundColor = Colors.Red;
                                    continue;
                                }
                                   

                                grid.BackgroundColor = isLeftCorrect && leftCheckBox.IsChecked || isRightCorrect && rightCheckBox.IsChecked
                                    ? Colors.Green
                                    : Colors.Red;
                            }
                        }
                    }
                }
            }
        }

        private async void OnSelectFileClicked(object sender, EventArgs e)
        {
            try
            {
                var result = await FilePicker.PickAsync(new PickOptions
                {
                    FileTypes = null,
                    PickerTitle = "Bitte wählen Sie eine JSON-Datei aus"
                });

                if (result != null)
                {
                    LoadQuestionsFromFile(result.FullPath);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Fehler", $"Fehler beim Auswählen der Datei: {ex.Message}", "OK");
            }
        }
    }
}
using Quiz.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;

namespace Quiz
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Category> file;
        List<Team> teams = new List<Team>();

        public Team selectedTeam;
        public Question selectedQuestion;

        public int teamCount;
        public int questionCount;
        public int categoryCount;
        //Team name radio buttons

        public MainWindow()
        {

            InitializeComponent();
            parseXML();
            categoryComboBox();

        }

        void parseXML()
        {
            var Categories = new List<Category>();
            var exeLocation = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
            var exeDir = System.IO.Path.GetDirectoryName(exeLocation);
            var xmlLocation = System.IO.Path.Combine(exeDir, "xml\\questions.xml");

            XDocument doc = XDocument.Load(xmlLocation);
            file = doc.Descendants("category")
             .Select(o => new Category()
             {
                 name = o.Attribute("Name").Value,
                 questions = o.Elements("question")
                     .Select(m => new Question()
                     {
                         value = Int32.Parse(m.Attribute("value").Value),
                         question = m.Element("text").Value,
                         options = m.Elements("option")
                         .Select(n => new Option
                         {
                             correct = bool.Parse(n.Attribute("correct").Value),
                             text = n.FirstNode.ToString()
                         }).ToList()
                     }).ToList()
             }).ToList();
            foreach (var cat in file)
            {
                cat.questions = cat.questions.OrderBy(x => x.value).ToList();
            }
        }
        void categoryComboBox()
        {
            for (int i = 0; i < file.Count; i++)
            {
                categoryCountCombo.Items.Add(i + 1);
            }
            categoryCountCombo.SelectedIndex = 0;
        }

        //Initialize teams (team name panel)
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            TeamListBox.Items.Clear();
            try
            {
                var count = Int32.Parse(TeamCount.Text);
                if (count > 0)
                {
                    teamCount = count;
                    StartPanel.Visibility = Visibility.Hidden;
                    CommandNamesPanel.Visibility = Visibility.Visible;
                    teams = new List<Team>();
                    for (int i = 0; i < count; i++)
                    {
                        var team = new Team()
                        {
                            name = "Komanda " + (i + 1),
                            score = 0
                        };
                        teams.Add(team);
                        TeamListBox.Items.Add(team);
                    }
                }
            }
            catch (Exception ex)
            {
                return;
            }

        }

        //Close team name window
        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CommandNamesPanel.Visibility = Visibility.Hidden;
            StartPanel.Visibility = Visibility.Visible;
        }

        //Start game !
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            TeamDockPanel.Items.Clear();
            foreach (var team in teams) {
                TeamDockPanel.Items.Add(new TeamModel() { team = team });
            }
          
            foreach (var t in teams) {
                t.score = 0;
            }
            if (teams.Count != 0)
            {
                try
                {
                    questionCount = Int32.Parse(this.QuestionCount.Text);
                    categoryCount = Int32.Parse(this.categoryCountCombo.SelectedItem.ToString());
                }
                catch (Exception ex)
                {
                    return;
                }
                initQuestionMatrix();
                QuestionPanel.Visibility = Visibility.Visible;
                StartPanel.Visibility = Visibility.Hidden;
            }
        }

        private void Image_MouseLeftButtonUp_1(object sender, MouseButtonEventArgs e)
        {
        }

        private void QuestionPanelBackButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            QuestionPanel.Visibility = Visibility.Hidden;
            StartPanel.Visibility = Visibility.Visible;

        }

        private void initQuestionMatrix()
        {
            QuestionMatrix.RowDefinitions.Clear();
            QuestionMatrix.ColumnDefinitions.Clear();
            QuestionMatrix.Children.Clear();
            QuestionMatrix.ShowGridLines = true;
            for (int i = 0; i < categoryCount; i++)
            {
                var def = new ColumnDefinition();
                def.Width = new GridLength(QuestionMatrix.Width / categoryCount);
                QuestionMatrix.ColumnDefinitions.Add(def);
            }
            var qc = file.FirstOrDefault().questions.Count<questionCount?file.FirstOrDefault().questions.Count:questionCount;
            for (int i = 0; i < questionCount + 1; i++)
            {
                var def = new RowDefinition()
                {
                    Height = new GridLength(QuestionMatrix.Height / (qc + 1))
                };
                QuestionMatrix.RowDefinitions.Add(def);
            }
            generateQuestionMatrix();

        }

        private void generateQuestionMatrix()
        {
            for (int i = 0; i < categoryCount; i++)
            {
                var label = new Label()
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Content = file[i].name
                };
                Grid.SetRow(label, 0);
                Grid.SetColumn(label, i);
                QuestionMatrix.Children.Add(label);
                for (int j = 0; j < questionCount; j++)
                {
                    if (j >= file[i].questions.Count) break;
                    var button = new Button()
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Content = file[i].questions[j],
                    };
                    button.Click += new RoutedEventHandler(questionButtonClick);
                    Grid.SetRow(button, j + 1);
                    Grid.SetColumn(button, i);
                    QuestionMatrix.Children.Add(button);
                }
            }
        }
        //A question was opened
        private void questionButtonClick(object sender, RoutedEventArgs e)
        {
            if (selectedTeam == null) return;//NO team selected
            var question = ((Button)sender).Content as Question;
            ((Button)sender).Visibility = Visibility.Hidden;
            selectedQuestion = question;
            AnswerQuestionPanel.Visibility = Visibility.Visible;
            QuestionPanel.Visibility = Visibility.Hidden;
            QuestionTextLabel.Content = question.question;
            option1.Content = question.options[0];
            option2.Content = question.options[1];
            option3.Content = question.options[2];
            option4.Content = question.options[3];
            //Reset buttons
            option1.IsChecked = option2.IsChecked = option3.IsChecked = option4.IsChecked = false;
            option1.Foreground = option2.Foreground = option3.Foreground = option4.Foreground = Brushes.Black;
            option1.IsEnabled = option2.IsEnabled = option3.IsEnabled = option4.IsEnabled = true;

        }
        //A question was answered
        private void option1_Checked(object sender, RoutedEventArgs e)
        {
            disableControls();
            var list = new List<RadioButton>();
            list.Add(option1);
            list.Add(option2);
            list.Add(option3);
            list.Add(option4);
            var content = ((RadioButton)sender).Content as Option;
            if (content.correct)
            {
                ((RadioButton)sender).Foreground = Brushes.Green;
                selectedTeam.score += selectedQuestion.value;

            }
            else
            {
                ((RadioButton)sender).Foreground = Brushes.Red;
                foreach (var b in list)
                {
                    var cont = b.Content as Option;
                    if (cont.correct)
                    {
                        b.Foreground = Brushes.Green;
                    }
                }
            }

        }

        private void disableControls()
        {
            option1.IsEnabled = option2.IsEnabled = option3.IsEnabled = option4.IsEnabled = false;
        }
        //Close answering question
        private void QuestionOptionsPanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            AnswerQuestionPanel.Visibility = Visibility.Hidden;
            QuestionPanel.Visibility = Visibility.Visible;
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            var b = sender as RadioButton;
            selectedTeam = b.Content as Team;
        }
        //Toggle results
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            ResultMatrix.RowDefinitions.Clear();
            ResultMatrix.Children.Clear();
            for (int i = 0; i < teams.Count; i++)
            {
                ResultMatrix.RowDefinitions.Add(new RowDefinition()
                {
                    Height = new GridLength(ResultMatrix.Height / teams.Count)
                });
                Label teamName = new Label() { Content = teams[i].name, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, Foreground=Brushes.Black };
                Label score = new Label() { Content = teams[i].score, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, Foreground = Brushes.Black };
                Grid.SetColumn(teamName, 0);
                Grid.SetRow(teamName, i);
                ResultMatrix.Children.Add(teamName);
                Grid.SetColumn(score, 1);
                Grid.SetRow(score, i);
                ResultMatrix.Children.Add(score);
            }
            ResultPanel.Visibility = Visibility.Visible;
            QuestionPanel.Visibility = Visibility.Hidden;
        }

        //Back from result
        private void ResultPanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ResultPanel.Visibility = Visibility.Hidden;
            QuestionPanel.Visibility = Visibility.Visible;
        }
    }
}

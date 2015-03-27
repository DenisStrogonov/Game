using Quiz.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;
using System.Windows.Data;
using System.Media;
using System.Reflection;
using System.Windows.Media.Imaging;


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
        public Brush normalBrush;
        //Music shit
        Boolean playing = true;
        SoundPlayer player;
        public MainWindow()
        {

            InitializeComponent();
            normalBrush = botion1.Background;

            parseXML();
            categoryComboBox();
            player = new SoundPlayer(Properties.Resources.Life_of_Riley);
            player.PlayLooping();
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
            TeamNameMatrix.Children.Clear();
            TeamNameMatrix.RowDefinitions.Clear();
            try
            {
                var count = Int32.Parse(TeamCount.Text);
                if (count > 0 && count < 6)
                {
                    teamCount = count;

                    StartPanel.Visibility = System.Windows.Visibility.Hidden;
                    TeamPanel.Visibility = Visibility.Visible;
                    /*
                    CommandNamesPanel.Visibility = System.Windows.Visibility.Visible;*/
                    teams = new List<Team>();
                    for (int i = 0; i < count; i++)
                    {
                        var team = new Team()
                        {
                            name = "Komanda " + (i + 1),
                            score = 0
                        };
                        teams.Add(team);
                        TextBox box = new TextBox()
                        {
                            Margin = new Thickness(15, 15, 15, 15),
                            VerticalContentAlignment = VerticalAlignment.Center
                        };
                        Binding b = new Binding("name")
                        {
                            Source = team,
                            Mode = BindingMode.TwoWay,
                            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                        };
                        box.SetBinding(TextBox.TextProperty, b);
                        Label l = new Label()
                        {
                            Content = "Komanda " + (i + 1) + ":     ",
                            Foreground = Brushes.Black,
                            VerticalContentAlignment = VerticalAlignment.Center,
                            HorizontalContentAlignment = HorizontalAlignment.Right,
                            FontWeight = System.Windows.FontWeights.Bold,
                            FontFamily = new FontFamily("Comic Sans")
                        };
                        RowDefinition def = new RowDefinition();
                        TeamNameMatrix.RowDefinitions.Add(new RowDefinition());
                        Grid.SetColumn(box, 1);
                        Grid.SetRow(box, i);
                        Grid.SetColumn(l, 0);
                        Grid.SetRow(l, i);
                        TeamNameMatrix.Children.Add(box);
                        TeamNameMatrix.Children.Add(l);
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
            CommandNamesPanel.Visibility = System.Windows.Visibility.Hidden;
            StartPanel.Visibility = System.Windows.Visibility.Visible;
        }

        //Start game !
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            selectedTeam = null;
            selectedQuestion = null;
            TeamDockPanel.Items.Clear();
            foreach (var team in teams)
            {
                TeamDockPanel.Items.Add(new TeamModel() { team = team });
            }

            foreach (var t in teams)
            {
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
                QuestionPanel.Visibility = System.Windows.Visibility.Visible;
                StartPanel.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        private void Image_MouseLeftButtonUp_1(object sender, MouseButtonEventArgs e)
        {
        }

        //From question grid to start panel
        private void QuestionPanelBackButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            QuestionPanel.Visibility = System.Windows.Visibility.Hidden;
            StartPanel.Visibility = System.Windows.Visibility.Visible;

        }

        private void initQuestionMatrix()
        {
            QuestionMatrix.RowDefinitions.Clear();
            QuestionMatrix.ColumnDefinitions.Clear();
            QuestionMatrix.Children.Clear();
            for (int i = 0; i < categoryCount; i++)
            {
                var def = new ColumnDefinition();
                def.Width = new GridLength(QuestionMatrix.Width / categoryCount);
                QuestionMatrix.ColumnDefinitions.Add(def);
            }
            var qc = file.FirstOrDefault().questions.Count < questionCount ? file.FirstOrDefault().questions.Count : questionCount;
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
                    Content = file[i].name,
                    Foreground = Brushes.Black
                };
                Grid.SetRow(label, 0);
                Grid.SetColumn(label, i);
                QuestionMatrix.Children.Add(label);
                for (int j = 0; j < questionCount; j++)
                {
                    if (j >= file[i].questions.Count) break;
                    var button = new Button()
                    {
                        /*HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,*/
                        Content = file[i].questions[j],
                        Margin = new System.Windows.Thickness(10, 10, 10, 10)
                    };
                    button.Click += new RoutedEventHandler(questionButtonClick);
                    Grid.SetRow(button, j + 1);
                    Grid.SetColumn(button, i);
                    QuestionMatrix.Children.Add(button);
                }
            }
        }
        //A question is being opened
        private void questionButtonClick(object sender, RoutedEventArgs e)
        {

            if (selectedTeam == null) return;//NO team selected
            CurrentTeamLabel.Content = "Komanda: " + selectedTeam.name;
            var question = ((Button)sender).Content as Question;
            ((Button)sender).Visibility = System.Windows.Visibility.Hidden;
            selectedQuestion = question;
            AnswerQuestionPanel.Visibility = System.Windows.Visibility.Visible;
            QuestionPanel.Visibility = System.Windows.Visibility.Hidden;
            QuestionTextLabel.Text = question.question;
            Option1.Content = question.options[0];
            Option2.Content = question.options[1];
            Option3.Content = question.options[2];
            Option4.Content = question.options[3];
            //Reset boptions
            botion1.IsEnabled = botion2.IsEnabled = botion3.IsEnabled = botion4.IsEnabled = true;
            botion1.Background = botion2.Background = botion3.Background = botion4.Background = normalBrush;
        }


        //Close answering question
        private void QuestionOptionsPanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            AnswerQuestionPanel.Visibility = System.Windows.Visibility.Hidden;
            QuestionPanel.Visibility = System.Windows.Visibility.Visible;
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
                ResultMatrix.RowDefinitions.Add(new RowDefinition());
                Label teamName = new Label()
                {
                    Content = teams[i].name + ":",
                    Foreground = Brushes.Black,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    FontSize = 26
                };
                Label score = new Label()
                {
                    Content = teams[i].score,
                    Foreground = Brushes.Black,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    FontSize = 26
                };
                Grid.SetColumn(teamName, 0);
                Grid.SetRow(teamName, i);
                ResultMatrix.Children.Add(teamName);
                Grid.SetColumn(score, 1);
                Grid.SetRow(score, i);
                ResultMatrix.Children.Add(score);
            }
            ResultPanel.Visibility = System.Windows.Visibility.Visible;
            QuestionPanel.Visibility = System.Windows.Visibility.Hidden;
        }

        //Back from result
        private void ResultPanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ResultPanel.Visibility = System.Windows.Visibility.Hidden;
            QuestionPanel.Visibility = System.Windows.Visibility.Visible;
        }
        //Question was answered
        private void botion1_Click(object sender, RoutedEventArgs e)
        {
            botion1.IsEnabled = botion2.IsEnabled = botion3.IsEnabled = botion4.IsEnabled = false;
            var list = new List<Button>();
            list.Add(botion1);
            list.Add(botion2);
            list.Add(botion3);
            list.Add(botion4);
            var b = sender as Button;
            var option = ((b.Content as Viewbox).Child as Label).Content as Option;
            if (option.correct)
            {
                b.Background = Brushes.ForestGreen;
                selectedTeam.score += selectedQuestion.value;
            }
            else {
                b.Background = Brushes.Red;
                foreach (var button in list) {
                    var opt = ((button.Content as Viewbox).Child as Label).Content as Option;
                    if (opt.correct) button.Background = Brushes.ForestGreen;
                }
            }

        }

        private void TeamPanelBack_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TeamPanel.Visibility = Visibility.Hidden;
            StartPanel.Visibility = Visibility.Visible;
        }

        private void WelcomePanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            WelcomePanel.Visibility = Visibility.Hidden;
            StartPanel.Visibility = Visibility.Visible;
        }

        private void Label_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            WelcomePanel.Visibility = Visibility.Hidden;
            StartPanel.Visibility = Visibility.Visible;
        }


        private void StopPlayback_Click(object sender, RoutedEventArgs e)
        {
            player.Stop();
            StopPlayback.Visibility = Visibility.Hidden;
            StartPlayback.Visibility = Visibility.Visible;
        }

        private void StartPlayback_Click(object sender, RoutedEventArgs e)
        {
            player.PlayLooping();
            StopPlayback.Visibility = Visibility.Visible;
            StartPlayback.Visibility = Visibility.Hidden;
        }
    }

    /// <summary>
    /// This class is a replacement for <c>SoundPlayer</c>
    /// and <c>MediaPlayer</c> classes. It solves 
    /// their shortcomings.
    /// </summary>
   
}

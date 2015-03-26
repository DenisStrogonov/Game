using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Quiz.Classes
{
    public class Option
    {
       public String text { get; set; }
       public bool correct { get; set; }
       public override string ToString()
       {
           return text;
       }
    }
    public class Question {
        public int value { get; set; }
        public String question { get; set; }
        public List<Option> options { get; set; }
        public override string ToString()
        {
            return value.ToString();
        }
    }
    class Category {
        public String name { get; set; }
        public List<Question> questions { get; set; }
    }
    public class Team {
        public String name { get; set; }
        public int score { get; set; }
        public override string ToString()
        {
            return name.ToString();
        }
    }
    class TeamListItem {
        public Team team { get; set; }
        TextBox box { get; set; }
    }
    class TeamModel {
        public Team team { get; set; }
    }
}

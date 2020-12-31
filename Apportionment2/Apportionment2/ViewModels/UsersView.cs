using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Apportionment2.Sqlite;
namespace Apportionment2.ViewModels
{
    public class UsersView : INotifyPropertyChanged
    {
        public UsersView(Users user)
        {
            User = user;
            Color = "#F5F5F5";
        }

        public UsersView(Users user, bool isOdd)
        {
            User = user;
            Color = isOdd ? "#F5F5F5" : "#ffffff";
        }

        public string id => User.id;
        public string Name => User.Name;

        public string Color { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        public Users User;
    }
}

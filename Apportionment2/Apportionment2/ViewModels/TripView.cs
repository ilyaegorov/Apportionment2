using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Apportionment2.Sqlite;

namespace Apportionment2.ViewModels
{
    internal class TripView : INotifyPropertyChanged
    {
        public TripView(Trips trip)
        {
            Trip = trip;
            Color = "#F5F5F5";
        }

        public TripView(Trips trip, bool isOdd)
        {
            Trip = trip;
            Color = isOdd ? "#F5F5F5" : "#ffffff";
        }

        public string id => Trip.id;
        public string Name => Trip.Name;
        public string DateStart => Trip.DateBegin;

        public string Color { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        public Trips Trip;

    }
}

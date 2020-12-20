using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Apportionment2.Sqlite;

namespace Apportionment2.ViewModels
{
    public class CostView : INotifyPropertyChanged
    {
        public CostView(Costs costs)
        {
            Cost = costs;
            Color = "#F5F5F5";
        }

        public CostView(Costs costs, bool isOdd)
        {
            Cost = costs;
            Color = isOdd ? "#F5F5F5" : "#ffffff";
           // Color1 = isOdd ? Xamarin.Forms.Color.FromHex("#F5F5F5") : Xamarin.Forms.Color.LightGray;
        }
        
        public string id => Cost.id;
        public string TripId => Cost.TripId;

        public string CostName
        {
            get { return Cost.CostName; }
           // get { return (Cost.CostName.Length > 60) ? Cost.CostName.Substring(0, 57) + "..." : Cost.CostName; }
            //get { return (_cost.CostName.Length > 25) ? _cost.CostName.Substring(0,23) + "..." :_cost.CostName; }
        }

        public string DateCreate => Cost.DateCreate;

        public string CostsValues
        {
            get
            {
                StringBuilder costValuesStringBuilder = new StringBuilder();

                var costValues = App.Database.Table<CostValues>().Where(n => n.CostId == id)
                    .OrderBy(n => n.CurrencyId)
                    .Select( g => new {g.CurrencyId, Summa = g.Value})
                    .GroupBy(d => d.CurrencyId)
                    .Select( b => new { CurrencyId = b.Key, Summa = b.Sum(x => x.Summa)});
                
                foreach (var value in costValues)
                {
                    //var currencyKod = App.Database.Table<CurrencyDictionary>().FirstOrDefault(n => n.id == value.CurrencyId);
                    //costValuesStringBuilder.Append($"{currencyKod.Code}");

                    string summaString = $"{value.Summa:0.00}";

                    //for (int i = 0; i < 15 - summaString.Length; i++)
                    //    costValuesStringBuilder.Append("_");

                    costValuesStringBuilder.Append(summaString);
                    costValuesStringBuilder.Append($"\n");
                }

                string result = costValuesStringBuilder.ToString();
                
                // Remove last "new line" char.
                if (result != String.Empty)
                    result = result.Substring(0, (result.Length - 1));

                return result;
            }
        }

        public string CurrencyList
        {
            get
            {
                StringBuilder costValuesStringBuilder = new StringBuilder();

                var currencyIds = App.Database.Table<CostValues>().Where(n => n.CostId == id)
                    .OrderBy(n => n.CurrencyId)
                    .Select(g => new { g.CurrencyId, Summa = g.Value })
                    .GroupBy(d => d.CurrencyId)
                    .Select(b => new { CurrencyId = b.Key });

                foreach (var curr in currencyIds)
                {
                    var currency = App.Database.Table<CurrencyDictionary>().FirstOrDefault(n => n.id == curr.CurrencyId);
                    costValuesStringBuilder.Append($"{currency.Code}");
                    costValuesStringBuilder.Append($"\n");
                }

                string result = costValuesStringBuilder.ToString();

                // Remove last "new line" char.
                if (result != String.Empty)
                    result = result.Substring(0, (result.Length - 1));

                return result;
            }
        }

        public string Color { get; set; }
        //public Color Color1 { get; set; }

        public readonly Costs Cost;
        public event PropertyChangedEventHandler PropertyChanged;
    }
}

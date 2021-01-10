using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Apportionment2.Interfaces;
using Apportionment2.Sqlite;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Apportionment2.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TripResultPage : ContentPage
    {
        public TripResultPage()
        {
            InitializeComponent();
        }

        private async void MenuItem_OnClicked(object sender, EventArgs e)
        {
            string[] actionMenuItem = new[]
            {
                Resource.TripResultPageSaveInFileButton,
                Resource.TripResultPageShareFile,
                Resource.TripResultPageSaveDb
            };


            var action = await DisplayActionSheet(null
                , Resource.Cancel
                , null
                , actionMenuItem
            );


            if (action == Resource.TripResultPageSaveInFileButton)
                SaveInFileButton_OnClicked(null, null);
            else if (action == Resource.TripResultPageShareFile)
                ShareFileButton_OnClicked(null, null);
            //else if (action == Resource.CalcApportionPageDebt)
            else if (action == Resource.TripResultPageSaveDb)
                await DependencyService.Get<IHtmlReport>().ShareDb();
        }

        protected override void OnAppearing()
        {
            _trip = (Trips) BindingContext;
            _costs = App.Database.Table<Costs>().Where(n => n.TripId == _trip.id).ToList();
            _users = GetUsers();

            Calculate();
            CreateXml();
            //RefreshSource();
            this.Title = _trip.Name;
            //Calculate();
            // ToolbarItems[0].Icon = string.Format("{0}setting_tools.png", Tools.OnDevicePathToImage());
            //CreateXml();
            //CreateHtml();
            // string LocalUrlPath = DependencyService.Get<IHtmlReport>().GetPath("ReportTemplate.html");
            //string LocalUrlPath = string.Format("{0}ReportTemplate.html", Tools.OnDevicePathLocalHml());
            //UrlWebViewSource urlSource = new UrlWebViewSource();
            //urlSource.Url = LocalUrlPath;
            //WebViewResult.Source = urlSource;
            base.OnAppearing();



            // this.Content = WebViewResult;
        }

        private async void SaveInFileButton_OnClicked(object sender, EventArgs e)
        {
            await DependencyService.Get<IHtmlReport>().
                SaveReportDocAsync(string.Format("{0}_{1}.html", _trip.Name, DateTime.Now.ToString("yy-mm-dd HH:MM")), _htmlResult);
        }

        private async void ShareFileButton_OnClicked(object sender, EventArgs e)
        {
            await DependencyService.Get<IHtmlReport>().ShareHtmlAsync(string.Format("{0}.html", _trip.Name), _htmlResult);
        }

        private void Calculate()
        {
            _results.Clear();

            List <Costs> noShareCosts = new List<Costs>();

            foreach (Costs cost in _costs)
            {
                double costSum = GetSumByCost(cost);
                bool hasCostValues = App.Database.Table<CostValues>().Any(n => (n.CostId == cost.id) && (n.Value != 0)) ;

                if ((costSum == 0) && !hasCostValues)
                    continue;

                if (!App.Database.Table<UserCostShares>().Any(n => (n.CostId == cost.id) && (n.Share != 0)))
                    noShareCosts.Add(cost);

                var results = GetUserWithCostValuesAndShares(cost);
                double shareSum = results.Sum(n => n.UserCostShare);

                foreach (var result in results)
                {
                    result.UserMustPay = (result.UserCostShare / shareSum) * costSum;
                    result.Delta = result.Spend - result.UserMustPay;
                }

                _results.AddRange(results);
            }

            CheckCostShare(noShareCosts);
        }

        private async void CheckCostShare(List<Costs> noShareCosts)
        {
            if (noShareCosts.Count == 0)
                return;

            StringBuilder costsNames = new StringBuilder();

            foreach (Costs cost in noShareCosts)
            {
                costsNames.Append(cost.CostName);
                costsNames.Append($"\n");
            }

            string result = costsNames.ToString();

            // Remove last "new line" char.
            if (result != String.Empty)
                result = result.Substring(0, (result.Length - 1));

            await DisplayAlert(Resource.TripResultPageNoShare
                , result
                , Resource.Ok);
        }

        private List<TripResults> GetUserWithCostValuesAndShares(Costs cost)
        {
            var sql = "Select TripId, CostId, UserId,  sum(share) as UserCostShare, sum(sumSpendByUser) as Spend,  0 as UserMustPay, 0 as Delta " + 
                    "from( Select ucs.TripId, ucs.CostId, ucs.UserId, 0 sumSpendByUser, ucs.Share, 0 delta "+
                    "from UserCostShares ucs "+
                    "where ucs.CostId = '" + cost.id + "'" +
                    "UNION all "+
                    "select cv.TripId, cv.CostId, cv.UserId, sum(cv.Value * IFNULL(cer.Rate, 1)) sumSpendByUser, 0, 0 "+
                    "from CostValues cv "+
                    "left join CurrencyExchangeRate cer on cv.CurrencyId = cer.CurrencyIdFrom and cv.TripId = '" + _trip.id + "' " +
                    "left join CurrencyDictionary cd on cd.Id = cv.CurrencyId "+
                    "where cv.CostId = '" + cost.id + "' GROUP BY cv.TripId, cv.CostId, cv.UserId) "+
                    "GROUP by TripId, CostId, UserId ";

            var cmd = App.Database.CreateCommand(sql);
            return cmd.ExecuteQuery<TripResults>().ToList();
        }

        private double GetSumByCost(Costs cost)
        {
            var sql = "select IFNULL((Select sum(cv.VALUE * IFNULL(cer.Rate, 1)) " +
                      "from CostValues cv " +
                      "left join CurrencyExchangeRate cer on cv.CurrencyId = cer.CurrencyIdFrom and cer.TripId = '" + _trip.id + "' " +
                      "left join CurrencyDictionary cd on cd.Id = cv.CurrencyId " +
                      "where cv.CostId ='" + cost.id + "' ),0)";

            //Select cv.VALUE* IFNULL(cer.Rate, 1), cv.VALUE, cd.Code as costSum
            //from CostValues cv
            //left join CurrencyExchangeRate cer on cv.CurrencyId = cer.CurrencyIdFrom and cv.TripId = 1
            //left join CurrencyDictionary cd on cd.Id = cv.CurrencyId

            var cmd = App.Database.CreateCommand(sql);
            return cmd.ExecuteScalar<double>();
        }

        private List<Users> GetUsers()
        {
            return Utils.GetUsers(_trip.id);
        }

        private async void CreateXml()
        {
            m_HtmlStrings = await DependencyService.Get<IHtmlReport>().LoadFromTemlate("ReportTemplateCurrency.html");

            ReplaceTemplateValues();
            int currencyIndex = m_HtmlStrings.FindIndex(n => n.Contains(Currency));
            int potsDataTableStartIndex = m_HtmlStrings.FindIndex(n => n.Contains(TableBegin));
            int potsDataTableLabelsStringStartIndex = m_HtmlStrings.FindIndex(potsDataTableStartIndex, n => n.Contains(StringTableBegin));
            int costDataTemplateStringStartIndex = m_HtmlStrings.FindIndex(potsDataTableLabelsStringStartIndex + 1, n => n.Contains(StringTableBegin));
            int costDataTemplateStringEndIndex = m_HtmlStrings.FindIndex(costDataTemplateStringStartIndex + 1, n => n.Contains(StringTableEnd));
            int potsDataTabelEndIndex = m_HtmlStrings.FindIndex(potsDataTableStartIndex + 1, n => n.Contains(TableEnd));

            int indexOfSecondTableFirstStringBegin = m_HtmlStrings.FindIndex(potsDataTabelEndIndex + 1, n => n.Contains(StringTableBegin));
            int indexOfSecondTableSecondStringBegin = m_HtmlStrings.FindIndex(indexOfSecondTableFirstStringBegin + 1, n => n.Contains(StringTableBegin));
            int indexOfSecondTableSecondStringEnd = m_HtmlStrings.FindIndex(indexOfSecondTableSecondStringBegin + 1, n => n.Contains(StringTableEnd));
            int indexOfSecondTableEnd = m_HtmlStrings.FindIndex(indexOfSecondTableSecondStringEnd + 1, n => n.Contains(TableEnd));

            int indexOfUserTableFisrtStringEnd = m_HtmlStrings.FindIndex(indexOfSecondTableEnd + 1, n => n.Contains(StringTableEnd));
            int indexOfUserTableSecondStringEnd = m_HtmlStrings.FindIndex(indexOfUserTableFisrtStringEnd + 1, n => n.Contains(StringTableEnd));
            int indexOfUserTableTotalStringEnd = m_HtmlStrings.FindIndex(indexOfUserTableSecondStringEnd + 1, n => n.Contains(StringTableEnd));
            int indexOfUserInsideTableEnd = m_HtmlStrings.FindIndex(indexOfUserTableSecondStringEnd + 1, n => n.Contains(TableEnd));
            int indexOfCharacterTableEnd = m_HtmlStrings.FindIndex(indexOfUserInsideTableEnd + 1, n => n.Contains(TableEnd));
            int indexOfUserTableEnd = m_HtmlStrings.FindIndex(indexOfCharacterTableEnd + 1, n => n.Contains(TableEnd));

            int indexOfMutualSettlementTableFirstStringBegin = m_HtmlStrings.FindIndex(indexOfUserTableEnd + 1, n => n.Contains(StringTableBegin));
            int indexOfMutualSettlementTableFirstStringEnd = m_HtmlStrings.FindIndex(indexOfMutualSettlementTableFirstStringBegin + 1, n => n.Contains(StringTableEnd));
            int indexOfMutualSettlementTableSecondStringBegin = m_HtmlStrings.FindIndex(indexOfMutualSettlementTableFirstStringBegin + 1, n => n.Contains(StringTableBegin));
            int indexOfMutualSettlementTableSecondStringEnd = m_HtmlStrings.FindIndex(indexOfMutualSettlementTableSecondStringBegin + 1, n => n.Contains(StringTableEnd));
            int indexOfMutualSettlementTableEnd = m_HtmlStrings.FindIndex(indexOfMutualSettlementTableSecondStringEnd + 1, n => n.Contains(TableEnd));

            for (int i = 0; i < currencyIndex; i++)
                _htmlResult.Add(m_HtmlStrings[i]);

            InsertCurrenciesLines();
           // InsertTripSumLines();

            for (int i = currencyIndex + 1; i < costDataTemplateStringStartIndex; i++)
                _htmlResult.Add(m_HtmlStrings[i]);

            InsertCostsDataInHtml(costDataTemplateStringStartIndex, costDataTemplateStringEndIndex);

            for (int i = potsDataTabelEndIndex; i < indexOfSecondTableSecondStringBegin; i++)
                _htmlResult.Add(m_HtmlStrings[i]);

            InsertUsersDataStrings(indexOfSecondTableSecondStringBegin, indexOfSecondTableSecondStringEnd);

            _htmlResult.Add(m_HtmlStrings[indexOfSecondTableEnd]);

            foreach (var user in _users)
                InsertUserDataStrings(indexOfSecondTableEnd,
                    indexOfUserTableFisrtStringEnd,
                    user,
                    indexOfUserTableSecondStringEnd,
                    indexOfUserInsideTableEnd,
                    indexOfUserTableTotalStringEnd,
                    indexOfUserTableEnd);

            if (_results.Any())
                InsertMutualSettlementsStrings(indexOfUserTableEnd,
                    indexOfMutualSettlementTableFirstStringEnd,
                    indexOfMutualSettlementTableSecondStringBegin,
                    indexOfMutualSettlementTableSecondStringEnd);

            CreateResultHtml();
        }

        private void InsertMutualSettlementsStrings(
          int indexOfUserTableEnd,
          int indexOfMutualSettlementTableFirstStringEnd,
          int indexOfMutualSettlementTableSecondStringBegin,
          int indexOfMutualSettlementTableSecondStringEnd)
        {
            var aboveZeroUserSums = _results
                .GroupBy(i => i.UserId)
                .Select(g => new { UserId = g.Key, Total = g.Sum(i => i.Delta), })
                .Where(n => n.Total > 0)
                .OrderByDescending(n => n.Total).ToList();

            var lessThanZeroUserSums = _results
                .GroupBy(i => i.UserId)
                .Select(g => new { UserId = g.Key, Total = g.Sum(i => i.Delta), })
                .Where(n => n.Total < 0)
                .OrderBy(n => n.Total).ToList();


            if (aboveZeroUserSums.Any())
            {
                for (int ind = indexOfUserTableEnd + 1; ind <= indexOfMutualSettlementTableFirstStringEnd; ind++)
                    _htmlResult.Add(m_HtmlStrings[ind]);

                int indexDebt = 0;
                double currentLessThanZeroSum = 0;

                for (int ms = 0; ms < aboveZeroUserSums.Count(); ms++)
                {
                    double currentAboveZeroSum = aboveZeroUserSums[ms].Total;

                    for (int b = indexDebt; b < lessThanZeroUserSums.Count(); b++)
                    {
                        currentLessThanZeroSum = (currentLessThanZeroSum < 0)
                            ? currentLessThanZeroSum
                            : lessThanZeroUserSums[b].Total;

                        List<string> stringValue = GetTemplateStrings(indexOfMutualSettlementTableSecondStringBegin,
                            indexOfMutualSettlementTableSecondStringEnd);

                        string aboveZeroUserId = aboveZeroUserSums[ms].UserId;
                        string lessZeroUserId = lessThanZeroUserSums[b].UserId;

                        string userNameAboveZero = App.Database.Table<Users>().FirstOrDefault(n => n.id == aboveZeroUserId).Name;
                        string userNameLessZero = App.Database.Table<Users>().FirstOrDefault(n => n.id == lessZeroUserId).Name;

                        ChangeText(ref stringValue, CredName, userNameAboveZero);
                        ChangeText(ref stringValue, DebName, userNameLessZero);

                        var delta = (currentAboveZeroSum + currentLessThanZeroSum <= 0)
                            ? Math.Abs(currentAboveZeroSum)
                            : Math.Abs(currentLessThanZeroSum);

                        ChangeText(ref stringValue, DebtSum, $"{delta:0.00}");
                        _htmlResult.AddRange(stringValue);

                        currentAboveZeroSum -= delta;

                        if (currentAboveZeroSum <= 0)
                            break;

                        currentLessThanZeroSum += delta;

                        if (currentLessThanZeroSum == 0)
                            indexDebt++;
                    }
                }
            }

            for (int i = indexOfMutualSettlementTableSecondStringEnd + 1; i < m_HtmlStrings.Count; i++)
                _htmlResult.Add(m_HtmlStrings[i]);
        }

        /// <summary>
        /// Inserts string with pots data in result html file.
        /// </summary>
        /// <param name="indexOfBeginPotData">Starting index of sting with template for pot data</param>
        /// <param name="indexOfEndPotData">End index of sting with template for pot data</param>
        private void InsertCostsDataInHtml(int indexOfBeginPotData, int indexOfEndPotData)
        {

            foreach (var cost in _costs)
            {
                if (_results.All(n => n.CostId != cost.id))
                    continue;

                double shareSum = _results.Where(n => n.CostId == cost.id).Sum(n => n.UserCostShare);
                double sumByCost = _results.Where(n => n.CostId == cost.id).Sum(n => n.Spend);
                List<string> stringValue = GetTemplateStrings(indexOfBeginPotData, indexOfEndPotData);
                ChangeText(ref stringValue, PtCostName, cost.CostName);
                ChangeText(ref stringValue, SharesSum, $"{shareSum:0.00000}");
                ChangeText(ref stringValue, SumByCost, GetSumWithCurrencyStrings(cost.id));
                ChangeText(ref stringValue, SumPerUser, $"{sumByCost/ shareSum :0.00}");
                _htmlResult.AddRange(stringValue);
            }
        }

        private List<string> GetTemplateStrings(int begin, int end)
        {
            List<string> l = new List<string>();

            for (int ind = begin; ind <= end; ind++)
                l.Add(m_HtmlStrings[ind]);

            return l;
        }
      
        private void InsertUsersDataStrings(int indexOfSecondTableSecondStringBegin, int indexOfSecondTableSecondStringEnd)
        {
            foreach (var user in _users)
            {
                var costResults = _results.Where(n => n.TripId == _trip.id && n.UserId == user.id).ToList();
                var delta = costResults.Sum(n => n.Delta);

                List<string> stringValue =
                    GetTemplateStrings(indexOfSecondTableSecondStringBegin, indexOfSecondTableSecondStringEnd);

                ChangeText(ref stringValue, UserName, user.Name);
                ChangeText(ref stringValue, SumByUser,GetSumWithCurrencyStrings("", user.id));
                ChangeText(ref stringValue, DeltaPlus, $"{ (delta < 0 ? 0 : delta) : 0.00}");
                ChangeText(ref stringValue, DeltaMinus, $"{(delta > 0 ? 0 : -delta): 0.00}");
                _htmlResult.AddRange(stringValue);
            }
        }

        private void InsertUserDataStrings(int indexOfSecondTableEnd,
           int indexOfUserTableFisrtStringEnd,
           Users user,
           int indexOfUserTableSecondStringEnd,
           int indexOfUserInsideTableEnd,
           int indexOfUserTableTotalStringEnd,
           int indexOfUserTableEnd)
        {
            List<string> stringValues = GetTemplateStrings(indexOfSecondTableEnd + 1, indexOfUserTableFisrtStringEnd);

            ChangeText(ref stringValues, NameTitle, user.Name);

            var costsResults = _results.Where(n => n.TripId == _trip.id && n.UserId == user.id);

            foreach (TripResults costResult in costsResults)
                InsertCostsUserString(ref stringValues, costResult,indexOfUserTableFisrtStringEnd + 1, indexOfUserTableSecondStringEnd);

            InsertTotalCostsUserString(ref stringValues, user.id, indexOfUserTableSecondStringEnd + 1, indexOfUserTableTotalStringEnd);

            for (int ind = indexOfUserInsideTableEnd; ind <= indexOfUserTableEnd; ind++)
                stringValues.Add(m_HtmlStrings[ind]);

            var sumByUser = _results
                .Where(n => n.TripId == _trip.id && n.UserId == user.id)
                .Sum(n => n.Delta);

            string totalUser = $"{((sumByUser < 0) ? Resource.TotalMustPay : Resource.TotalOverpaid)}:{Math.Abs(sumByUser) : 0.00}";
            string colorUser = (sumByUser < 0) ? RedColor : GreenColor;
            int indexTotal = stringValues.FindIndex(n => n.Contains(TotalToPayOrToBack));

            if (indexTotal != -1)
                stringValues[indexTotal] =
                    stringValues[indexTotal].Replace(TotalToPayOrToBack, totalUser).Replace(RedColor, colorUser);

            string textChar = GetCharacteristic(user);

            ChangeText(ref stringValues, CharacteristicText, textChar);
            ChangeText(ref stringValues, Characteristic, Resource.Characteristic);

            _htmlResult.AddRange(stringValues);
        }

        private void InsertCostsUserString(ref List<string> result, TripResults costResult, int startIndex, int endIndex)
        {
            List<string> stringPotResultValues =GetTemplateStrings(startIndex + 1, endIndex);
            Costs cost = App.Database.Table<Costs>().FirstOrDefault(n => n.id == costResult.CostId);
           
            ChangeText(ref stringPotResultValues, CostNameUsT, cost.CostName);
            ChangeText(ref stringPotResultValues, UserShare, $"{costResult.UserCostShare:0.0000}");
            ChangeText(ref stringPotResultValues, UserSpend, $"{costResult.Spend:0.00}");
            ChangeText(ref stringPotResultValues, UserMustPay, $"{costResult.UserMustPay:0.00}");
            ChangeText(ref stringPotResultValues, DeltaUser, $"{(costResult.Delta>0 ?"+" :"")}{costResult.Delta:0.00}");

            result.AddRange(stringPotResultValues);
        }

        private void InsertTotalCostsUserString(ref List<string> result, string userId, int startIndex, int endIndex)
        {
            var spendTotal = _results.Where(n => n.TripId == _trip.id && n.UserId == userId).Sum(n=>n.Spend);
            var sumTotal = _results.Where(n => n.TripId == _trip.id && n.UserId == userId).Sum(n => n.UserMustPay);
            var deltaTotal = _results.Where(n => n.TripId == _trip.id && n.UserId == userId).Sum(n => n.Delta);
            List<string> stringPotResultValues = GetTemplateStrings(startIndex + 1, endIndex);

            ChangeText(ref stringPotResultValues, UserSpendTotal, $"{spendTotal:0.00}");
            ChangeText(ref stringPotResultValues, UserMustPayTotal, $"{sumTotal:0.00}");
            ChangeText(ref stringPotResultValues, DeltaUserTotal, $"{deltaTotal:0.00}");
           

            result.AddRange(stringPotResultValues);
        }

        private string GetCharacteristic(Users u)
        {
            StringBuilder sb = new StringBuilder();

            var spend = _results.Select(n => new { UserId = n.UserId, Sum = n.Spend }).
                GroupBy( p=> p.UserId).Select(g => new {NamUserId = g.Key,Sum = g.Sum(d=>d.Sum)}) ;

            double uSpend = spend.First(n => n.NamUserId == u.id).Sum;

            //Check spend sum.
            if (uSpend == spend.Max(n => n.Sum))
                sb.Append(Resource.CharacteristicSumSpendMax);

            else if (uSpend == spend.Min(n => n.Sum))
                sb.Append(Resource.CharacteristicSumSpendMin);
            else if (uSpend > spend.Average(n => n.Sum))
                sb.Append(Resource.CharacteristicSumSpendAboveMiddle);
            else
                sb.Append(Resource.CharacteristicSumSpendMiddle);

            sb.Append(CheckAlco(u));

            return sb.ToString();
        }

        private string CheckAlco(Users u)
        {
            string alco = Resource.CharacteristicAlco;
            var alcoDict = App.Database.Table<AlcoDict>();
            var userShares = App.Database.Table<UserCostShares>().Where(n => n.UserId == u.id && n.TripId== _trip.id);

            foreach (var userShare in userShares)
            {
                var cost = App.Database.Table<Costs>().FirstOrDefault(n => n.id == userShare.CostId);
                string costName = cost.CostName.ToLower();

                //TODO Add checking of the payment comment
                if (alcoDict.Any(n => costName.Contains(n.Text)))
                    return alco;
            }


            alco = "";
            return alco;
        }

        private void ReplaceTemplateValues()
        {
            ChangeText(ref m_HtmlStrings, TripNameLabel, Resource.CaptionJourney);
            ChangeText(ref m_HtmlStrings, TripName, _trip.Name);
            ChangeText(ref m_HtmlStrings, TripSum, GetSumByTrip());
            ChangeText(ref m_HtmlStrings, LbCostName, Resource.ResultApportionmentPagePotName);
            ChangeText(ref m_HtmlStrings, LbCountShare, "Участников");
            ChangeText(ref m_HtmlStrings, LbTotal, Resource.ResultApportionmentPageItog);
            ChangeText(ref m_HtmlStrings, LbPerUser, Resource.ResultApportionmentPagePerUser);

            ChangeText(ref m_HtmlStrings, LbUserName, Resource.Participants);
            ChangeText(ref m_HtmlStrings, LbUserSpend, Resource.ResultApportionmentPageSpend);
            ChangeText(ref m_HtmlStrings, LbPayBack, Resource.ResultApportionmentPageTotalReturn);
            ChangeText(ref m_HtmlStrings, LbToPay, Resource.ResultApportionmentPageTotalPay);

            ChangeText(ref m_HtmlStrings, UserLabel, Resource.Participant);
            ChangeText(ref m_HtmlStrings, LbCostUsT, Resource.Pot);
            ChangeText(ref m_HtmlStrings, LbShare, Resource.CaptionShare);
            ChangeText(ref m_HtmlStrings, LbSpend, Resource.ResultApportionmentPageSpend);
            ChangeText(ref m_HtmlStrings, LbMustToPay, "Сумма на участника");
            ChangeText(ref m_HtmlStrings, LbDelta, Resource.ResultApportionmentPagePerDelta);

            ChangeText(ref m_HtmlStrings, LbCred, "Получатель");
            ChangeText(ref m_HtmlStrings, LbDeb, "Плательщик");
            ChangeText(ref m_HtmlStrings, LbSum, Resource.Sum);
            ChangeText(ref m_HtmlStrings, TitleScheme, Resource.NettingScheme);
        }

        private void InsertCurrenciesLines()
        {
            int currenciesIndex = m_HtmlStrings.FindIndex(n => n.Contains(Currency));
            var currencies = App.Database.Table<CurrencyExchangeRate>().Where(n => n.TripId == _trip.id);

            foreach (var currency in currencies)
            {
                var currencyFrom = App.Database.Table<CurrencyDictionary>().FirstOrDefault(n => n.id == currency.CurrencyIdFrom);
                var currencyTo = App.Database.Table<CurrencyDictionary>().FirstOrDefault(n => n.id == currency.CurrencyIdTo);
                string currencyString = $"{currencyFrom.Code}-->{currencyTo.Code}:{currency.Rate:0.0000}";
                List<string> stringValue = GetTemplateStrings(currenciesIndex, currenciesIndex);
                ChangeText(ref stringValue, Currency, currencyString);
                _htmlResult.AddRange(stringValue);
            }
        }

        private string GetSumByTrip()
        {
            //StringBuilder costValuesStringBuilder = new StringBuilder();

            //var costValues = App.Database.Table<CostValues>().Where(n => n.TripId == _trip.id)
            //    .OrderBy(n => n.CurrencyId)
            //    .Select(g => new { g.CurrencyId, Summa = g.Value })
            //    .GroupBy(d => d.CurrencyId)
            //    .Select(b => new { CurrencyId = b.Key, Summa = b.Sum(x => x.Summa) });

            //double tripSum = 0;

            //foreach (var value in costValues)
            //{
              
            //    var currency = App.Database.Table<CurrencyDictionary>().FirstOrDefault(n => n.id == value.CurrencyId);

            //    double currencyRate = App.Database.Table<CurrencyExchangeRate>().Any(n => n.id == value.CurrencyId && n.TripId == _trip.id)
            //        ? App.Database.Table<CurrencyExchangeRate>().LastOrDefault(n => n.id == value.CurrencyId && n.TripId == _trip.id).Rate
            //        : 1;

            //    tripSum += currencyRate * value.Summa;
            //    string summaString = $"{currency.Code}     {value.Summa:0.00}";
            //    costValuesStringBuilder.Append(summaString);
            //    costValuesStringBuilder.Append($"<br>");
            //}

            //costValuesStringBuilder.Append($"Total:  {tripSum:0.00}");
            //string result = costValuesStringBuilder.ToString();

            return GetSumWithCurrencyStrings();
        }

        private string GetSumWithCurrencyStrings(string costId = "", string userId = "")
        {
            StringBuilder costValuesStringBuilder = new StringBuilder();

            var costValues = App.Database.Table<CostValues>()
                .Where(n => n.TripId == _trip.id 
                && (n.CostId == costId || costId == string.Empty) 
                && (n.UserId == userId || userId == string.Empty))
                .OrderBy(n => n.CurrencyId)
                .Select(g => new { g.CurrencyId, Summa = g.Value })
                .GroupBy(d => d.CurrencyId)
                .Select(b => new { CurrencyId = b.Key, Summa = b.Sum(x => x.Summa) });

            double totalSumInBaseCurrency = 0;

            foreach (var value in costValues)
            {
                var currency = App.Database.Table<CurrencyDictionary>().FirstOrDefault(n => n.id == value.CurrencyId);

                double currencyRate = App.Database.Table<CurrencyExchangeRate>().
                    Any(n => n.CurrencyIdFrom == value.CurrencyId && n.TripId == _trip.id)
                    ? App.Database.Table<CurrencyExchangeRate>().
                    LastOrDefault(n => n.CurrencyIdFrom == value.CurrencyId && n.TripId == _trip.id).Rate
                    : 1;

                totalSumInBaseCurrency += currencyRate * value.Summa;
                string summaString = $"{currency.Code}     {value.Summa:0.00}";
                costValuesStringBuilder.Append(summaString);
                costValuesStringBuilder.Append($"<br>");
            }

            costValuesStringBuilder.Append($"{Resource.CalcApportionItog}:  {totalSumInBaseCurrency:0.00}");
            string result = costValuesStringBuilder.ToString();

            return result;
        }

        private void ChangeText(ref List<string> htmlStrings, string textIn, string textOut)
        {
            var indexOfnameJourneyLabel = htmlStrings.FindIndex(n => n.Contains(textIn));
            if (indexOfnameJourneyLabel != -1)
                htmlStrings[indexOfnameJourneyLabel] = htmlStrings[indexOfnameJourneyLabel].Replace(textIn, textOut);
        }


        private void CreateResultHtml()
        {
            StringBuilder sb = new StringBuilder();

            foreach (var s in _htmlResult)
                sb.Append(s + "\r");

            var html = sb.ToString();
            var htmlSource = new HtmlWebViewSource();
            htmlSource.Html = html;
            WebViewResult.Source = htmlSource;
        }

        private List<TripResults> _results = new List<TripResults>();
        private Trips _trip;
        private string m_CalculationReport;
        private List<string> m_HtmlStrings;
        private List<string> _htmlResult = new List<string>();
        private List<Costs> _costs;
        private List<Users> _users;

        private const string RedColor = "red";
        private const string GreenColor = "green";
        private const string GreyColor = "#00B050";

        private const string TableBegin = "<table class=MsoTableGrid border=1 cellspacing=0 cellpadding=0";
        private const string TableEnd = "</table>";
        private const string StringTableBegin = "<tr>";
        private const string StringTableEnd = "</tr>";
        private const string TripNameLabel = "TripNameLabel";
        private const string TripName = "TripName";
        private const string LbCostName = "LbCostName";
        private const string LbCountShare = "LbCountShare";
        private const string LbTotal = "LbTotal";
        private const string LbPerUser = "LbPerUser";
        private const string LbUserName = "LbUserName";
        private const string LbUserSpend = "LbUserSpend";
        private const string LbPayBack = "LbPayBack";
        private const string LbToPay = "LbToPay";
        private const string UserLabel = "UserLabel";
        private const string NameTitle = "NameTitle";
        private const string LbCostUsT = "LbCostUsT";
        private const string LbShare = "LbShare";
        private const string LbSpend = "LbSpend";
        private const string LbMustToPay = "LbMustToPay";
        private const string LbDelta = "LbDelta";
        private const string DebtsOrCredits = "DebtsOrCredit";
        private const string TotalToPayOrToBack = "TotalToPayOrToBack";

        private const string PtCostName = "PtCostName";
        private const string SharesSum = "SharesSum";
        private const string SumByCost = "SumByCost";
        private const string SumPerUser = "SumPerUser";
        private const string UserName = "UserName";
        private const string SumByUser = "SumByUser";
        private const string DeltaPlus = "DeltaPlus";
        private const string DeltaMinus = "DeltaMinus";
        private const string CostNameUsT = "CostNameUsT";
        private const string UserShare = "UserShare";
        private const string UserSpend = "UserSpend";
        private const string UserMustPay = "UserMustPay";
        private const string DeltaUser = "DeltaUser";
        private const string CharacteristicText = "Characteristic_text";
        private const string Characteristic = "Characteristic";
        private const string TripSum = "TripSum";
        private const string Currency = "Currency";

        private const string LbCred = "LbCred";
        private const string CredName = "CredName";
        private const string LbDeb = "LbDeb";
        private const string DebName = "DebName";
        private const string LbSum = "LbSum";
        private const string DebtSum = "DebtSum";
        private const string UserSpendTotal = "UserSpendTotal";
        private const string UserMustPayTotal = "UserMustPayTotal";
        private const string DeltaUserTotal = "DeltaUserTotal";

        private const string TitleScheme = "TitleScheme";
    }
}
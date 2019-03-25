using QuantConnect.Brokerages;
using QuantConnect.Data;
using QuantConnect.Data.Consolidators;
using QuantConnect.Data.Market;
using QuantConnect.Indicators;
using QuantConnect.Orders;
using QuantConnect.Securities;
using QuantConnect.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

namespace QuantConnect.Algorithm.CSharp
{
    public partial class BubbleRider : QCAlgorithm
    {
        //Parameters
        private const bool UseCustomData = false;
        private const bool UseBrokerageModel = true;
        private const bool DebugEnabled = false;
        private const string MySymbol = "BTCUSD";
        private const string BaseCurrency = "BTC";
        private const string QuoteCurrency = "USD";
        private const decimal _adxFilterLevel = 25;
        private const int AdxPeriodLevel = 25;
        private const int DonchianPeriods = 7;
        private const decimal AfStart = 0.017m;
        private const decimal AfIncrement = 0.017m;
        private const decimal AfMax = 0.2m;
        private const int HistoryBars = 100;
        private readonly TimeSpan _barTimeSpan = TimeSpan.FromHours(4);
        private readonly Resolution _resolution = Resolution.Minute;

        //Plotting Names
        private const string PriceAndIndicatorsName = "Price + Indicators";
        private const string PriceSeriesName = "Price";
        private const string BuySeriesName = "Buy";
        private const string SellSeriesName = "Sell";
        private const string ParabolicSeriesName = "PSAR";
        private const string DonchianSeriesName = "Donchian";
        private const string AdxChartName = "Plot ADX";
        private const string AdxSeriesName = "ADX";
        private const string Unit = "$";

        //Plotting Colors
        private readonly Color PriceColor = Color.Gray;
        private readonly Color BuyOrdersColor = Color.CornflowerBlue;
        private readonly Color SellOrdersColor = Color.Red;
        private readonly Color ParabolicColor = Color.RosyBrown;
        private readonly Color DonchianColor = Color.MediumPurple;
        private readonly Color AdxColor = Color.CornflowerBlue;

        //Indicators
        private ParabolicStopAndReverse _parabolic;
        private AverageDirectionalIndex _avgDirectionalIndex;
        private DonchianChannel _donchian;

        //Collections
        private RollingWindow<decimal> _histParabolic;
        private RollingWindow<decimal> _histAvgDirectionalIndex;
        private RollingWindow<decimal> _histDonchian;
        private RollingWindow<decimal> _histOpeningValues;
        private RollingWindow<decimal> _histHighValues;
        private RollingWindow<decimal> _histLowValues;
        private RollingWindow<TradeBar> _historicalBar;
        private readonly List<DateTime> _dateOfClosedTrades = new List<DateTime>();

        //Others
        public Logger MyLogger;
        public static decimal QuoteBalance;
        private Security _mySecurity;
        private TradeBuilder _tradeBuilder;

        /// <summary>
        /// Retrieves Quote Currency Balance (rounded)
        /// </summary>
        public static decimal QuoteCurrencyBalance
        {
            get
            {
                return QuoteBalance;
            }
            set
            {
                QuoteBalance = Math.Round(value, 2);
            }
        }

        /// <summary>
        /// Initialise the data and resolution required, as well as the cash and
        /// start-end dates for your algorithm. All algorithms must initialized.
        /// </summary>
        public override void Initialize()
        {
            if (UseCustomData)
            {
                _mySecurity = AddData<Bitcoin>(MySymbol, _resolution);
                SetStartDate(2014, 01, 15);
                SetEndDate(2017, 12, 31);
                SetCash(10000);
            }
            else
            {
                _mySecurity = AddCrypto(MySymbol, _resolution);
                SetStartDate(2015, 02, 01);
                SetEndDate(2018, 03, 31);
                SetCash(10000);

                if (UseBrokerageModel)
                {
                    SetBrokerageModel(BrokerageName.GDAX, AccountType.Cash);
                    BrokerageModel.GetFeeModel(_mySecurity);
                }
            }

            MyLogger = new Logger(this, DebugEnabled);

            TradeBarConsolidator fourHourConsolidator = new TradeBarConsolidator(_barTimeSpan);
            fourHourConsolidator.DataConsolidated += ConsolidatedDataHandler;
            SubscriptionManager.AddConsolidator(_mySecurity.Symbol, fourHourConsolidator);

            _parabolic = new ParabolicStopAndReverse(AfStart, AfIncrement, AfMax);
            _avgDirectionalIndex = new AverageDirectionalIndex(MySymbol, AdxPeriodLevel);
            _donchian = new DonchianChannel("Donchian", DonchianPeriods);

            RegisterIndicator(MySymbol, _parabolic, fourHourConsolidator);
            RegisterIndicator(MySymbol, _avgDirectionalIndex, fourHourConsolidator);
            RegisterIndicator(MySymbol, _donchian, fourHourConsolidator);

            //Create History for Indicator PSAR
            _histParabolic = new RollingWindow<decimal>(HistoryBars);
            _histAvgDirectionalIndex = new RollingWindow<decimal>(HistoryBars);
            _histDonchian = new RollingWindow<decimal>(HistoryBars);

            //Create History For OHLC Prices
            _historicalBar = new RollingWindow<TradeBar>(HistoryBars);
            _histOpeningValues = new RollingWindow<decimal>(HistoryBars);
            _histHighValues = new RollingWindow<decimal>(HistoryBars);
            _histLowValues = new RollingWindow<decimal>(HistoryBars);

            //Must use SetWarmUp to 1 at least
            SetWarmup(1);
            //--Charting and Log
            Chart parabolicPlot = new Chart(PriceAndIndicatorsName);
            parabolicPlot.AddSeries(new Series(PriceSeriesName, SeriesType.Line, Unit, PriceColor));
            parabolicPlot.AddSeries(new Series(BuySeriesName, SeriesType.Scatter, Unit, BuyOrdersColor));
            parabolicPlot.AddSeries(new Series(SellSeriesName, SeriesType.Scatter, Unit, SellOrdersColor));
            parabolicPlot.AddSeries(new Series(ParabolicSeriesName, SeriesType.Line, Unit, ParabolicColor));
            parabolicPlot.AddSeries(new Series(DonchianSeriesName, SeriesType.Line, Unit, DonchianColor));

            Chart plotAvgDirectionalIndex = new Chart(AdxChartName);
            plotAvgDirectionalIndex.AddSeries(new Series(AdxSeriesName, SeriesType.Line, Unit, AdxColor));
            AddChart(parabolicPlot);
            AddChart(plotAvgDirectionalIndex);

            //--For Logging
            QuoteCurrencyBalance = Portfolio.CashBook[QuoteCurrency].Amount;
            MyLogger.ScheduleReport(MySymbol, BaseCurrency, QuoteCurrency);
            MyLogger.InfoSettings($"SETTINGS: " +
                                  $"Use Custom Data: {UseCustomData} | " +
                                  $"Use Brokerage Model: {UseBrokerageModel} | " +
                                  $"Debug: {DebugEnabled} | " +
                                  $"ADX Filter Level: {_adxFilterLevel} | " +
                                  $"ADX Period Level: {AdxPeriodLevel} | " +
                                  $"Symbol: {MySymbol} | " +
                                  $"History Bars: {HistoryBars} | " +
                                  $"Resolution: {_resolution}");


            MyLogger.InfoCustom("Initialize Finished.");
            _tradeBuilder = new TradeBuilder(FillGroupingMethod.FlatToFlat, FillMatchingMethod.FIFO);
            SetTradeBuilder(_tradeBuilder);
        }

        /// <summary>
        /// Called when the algorithm finishes warming up data
        /// (with SetWarmUp() method)
        /// </summary>
        public override void OnWarmupFinished()
        {
            //Must delete the data I filled from SetWarmUp
            _parabolic.Reset();
            _avgDirectionalIndex.Reset();
            _donchian.Reset();

            if (!LiveMode)
            {
                return;
            }

            IEnumerable<TradeBar> history = History(
                _mySecurity.Symbol,
                TimeSpan.FromHours(_barTimeSpan.Hours * 200)
            );
            IEnumerable<TradeBar> customTradeBarHistory = ConsolidateHistory(
                history,
                TimeSpan.FromHours(_barTimeSpan.Hours),
                _resolution
            );

            foreach (TradeBar tradeBar in customTradeBarHistory)
            {
                _parabolic.Update(tradeBar);
                _avgDirectionalIndex.Update(tradeBar);
                _donchian.Update(tradeBar);

                _historicalBar.Add(tradeBar);
                _histOpeningValues.Add(tradeBar.Open);
                _histHighValues.Add(tradeBar.High);
                _histLowValues.Add(tradeBar.Low);

                _histParabolic.Add(_parabolic.Current.Price);
                _histAvgDirectionalIndex.Add(_avgDirectionalIndex.Current.Price);
                _histDonchian.Add(_donchian.LowerBand.Current.Price);

                MyLogger.InfoBar(tradeBar, true);
                MyLogger.InfoIndicator(tradeBar, _parabolic, _avgDirectionalIndex, _donchian, true);
            }

            if (LiveMode
                && customTradeBarHistory.Last().Close > _parabolic.Current.Value
                && _avgDirectionalIndex.Current.Value > _adxFilterLevel)
            {
                OpenPosition();
            }

            MyLogger.InfoCustom("OnWarmupFinished is complete.");
            base.OnWarmupFinished();
        }

        /// <summary>
        /// Event handler for everytime new custom data is processed (UseCustomData == true)
        /// </summary>
        /// <param name="data"></param>
        public void OnData(Bitcoin data)
        {

        }

        public void OnData(Slice slice)
        {
            if (!Portfolio.Invested)
                return;

            if (slice.Values[0].Price <= _histDonchian[0])
                ClosePosition();
        }

        /// <summary>
        /// Handles Four Hour (H4) bar events, everytime a new H4 bar is formed
        /// this method is called.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="bar"></param>
        private void ConsolidatedDataHandler(object sender, TradeBar bar)
        {
            if (!_mySecurity.IsTradable)
            {
                MyLogger.ErrorSymbolNotTradable(MySymbol);
                return;
            }
            if (!_mySecurity.Exchange.ExchangeOpen)
            {
                MyLogger.ErrorExchangeClosed();
                return;
            }
            if (!_parabolic.IsReady || !_avgDirectionalIndex.IsReady || !_donchian.IsReady || IsWarmingUp)
            {
                return;
            }

            _historicalBar.Add(bar);
            _histOpeningValues.Add(bar.Open);
            _histHighValues.Add(bar.High);
            _histLowValues.Add(bar.Low);

            _histParabolic.Add(_parabolic.Current.Price);
            _histAvgDirectionalIndex.Add(_avgDirectionalIndex.Current.Value);
            _histDonchian.Add(_donchian.LowerBand.Current.Value);

            Plot(PriceAndIndicatorsName, PriceSeriesName, _historicalBar[0].Price);
            Plot(PriceAndIndicatorsName, ParabolicSeriesName, _histParabolic[0]);
            Plot(PriceAndIndicatorsName, DonchianSeriesName, _histDonchian[0]);
            Plot(AdxChartName, AdxSeriesName, _histAvgDirectionalIndex[0]);

            MyLogger.InfoBar(bar);
            MyLogger.InfoIndicator(bar, _parabolic, _avgDirectionalIndex, _donchian);

            if (!Portfolio.Invested)
            {
                //Open a position when the criteria meet:
                // - no previous trade within this signal
                // - price > PSAR
                // - price > ADX
                if (DateOfLastSignal > DateOfLastLongEntry
                    && _historicalBar[0].Price > _histParabolic[0]
                    && _histAvgDirectionalIndex[0] > _adxFilterLevel)
                {
                    OpenPosition();
                }
            }
            else if (_histDonchian[0] > _histDonchian[1])
            {
                UpdatePosition();
            }
        }

        /// <summary>
        /// Event handler for when the algorithm ends.
        /// </summary>
        public override void OnEndOfAlgorithm()
        {
            Liquidate(MySymbol);

            decimal peak = 10000m;
            decimal balance = peak;
            decimal valley = 0m;
            decimal balanceDrawdown = 0m;

            foreach (var d in _tradeBuilder.ClosedTrades)
            {
                balance += d.ProfitLoss - d.TotalFees;

                if (balance > peak)
                {
                    peak = balance;
                    valley = peak;
                }
                else
                {
                    valley = balance;
                }

                if ((peak - valley) / peak > balanceDrawdown)
                    balanceDrawdown = (peak - valley) / peak;
            }

            Console.WriteLine($"Balance Drawdown % (From Risk Framework): {balanceDrawdown * 100}%");
        }

        // Override the base class event handler for order events
        public override void OnOrderEvent(OrderEvent orderEvent)
        {
            if (orderEvent.Status == OrderStatus.Invalid)
            {
                MyLogger.ErrorOnOrderEvent(orderEvent);
            }

            if (orderEvent.Direction == OrderDirection.Sell)
            {
                _dateOfClosedTrades.Add(Time);
            }

            if (orderEvent.FillPrice > 0)
            {
                if (orderEvent.Direction == OrderDirection.Sell)
                {
                    Plot(PriceAndIndicatorsName, SellSeriesName, orderEvent.FillPrice);
                    QuoteCurrencyBalance = Portfolio.CashBook[QuoteCurrency].Amount;
                }
                else
                {
                    Plot(PriceAndIndicatorsName, BuySeriesName, orderEvent.FillPrice);
                }
            }

            MyLogger.InfoOrderEvent(orderEvent, QuoteCurrencyBalance, QuoteCurrency);
        }

        /// <summary>
        /// Transforms history from a frequency to another lower frequency,
        /// example: m1 to H4
        /// </summary>
        /// <param name="history">History to transform</param>
        /// <param name="customTime">New bar time length for the new history</param>
        /// <param name="resolution">Original resolution of the current algorithm</param>
        /// <returns></returns>
        public IEnumerable<TradeBar> ConsolidateHistory(
            IEnumerable<TradeBar> history,
            TimeSpan customTime,
            Resolution resolution
        )
        {
            if (resolution == Resolution.Minute)
            {
                TimeSpan minute = TimeSpan.FromMinutes(1);
                int totalMinutes = (int)(minute.TotalMinutes * history.Count());
                int customTimeMinutes = (int)(customTime.TotalMinutes);

                int span = totalMinutes / customTimeMinutes;
                TradeBar[] consolidatedHistory = new TradeBar[span];
                Symbol symbol = history.First().Symbol;

                for (int i = 0; i < span; i++)
                {
                    decimal open = history.Skip(i * customTimeMinutes).
                        Take(customTimeMinutes).First().Open;
                    decimal high = history.Skip(i * customTimeMinutes).
                        Take(customTimeMinutes).Max(item => item.High);
                    decimal low = history.Skip(i * customTimeMinutes).
                        Take(customTimeMinutes).Min(item => item.Low);
                    decimal close = history.Skip(i * customTimeMinutes).
                        Take(customTimeMinutes).Last().Close;
                    DateTime time = history.Skip(i * customTimeMinutes).
                        Take(customTimeMinutes).First().Time;
                    decimal volume = history.Skip(i * customTimeMinutes).
                        Take(customTimeMinutes).Sum(item => item.Volume);
                    consolidatedHistory[i] = new TradeBar(time, symbol, open,
                        high, low, close, volume, customTime);

                    _historicalBar.Add(consolidatedHistory[i]);
                    _histOpeningValues.Add(consolidatedHistory[i].Open);
                    _histHighValues.Add(consolidatedHistory[i].High);
                    _histLowValues.Add(consolidatedHistory[i].Low);
                }
                return consolidatedHistory;
            }
            else
            {
                MyLogger.FatalHistoryConsolidator();
                throw new Exception("History Consolidator was given a wrong resolution parameter.");
            }
        }

        //Retrieves the start-date of the current bull signal
        public DateTime DateOfLastSignal
        {
            get
            {
                int i = 0;
                int maxBars = _historicalBar.Count;

                if (_historicalBar[i].Price > _histParabolic[i])
                {
                    while (_historicalBar[i].Price > _histParabolic[i]
                        && i < maxBars - 1)
                    {
                        i++;
                    }
                }
                return _historicalBar[i].Time;
            }
        }

        //Retrieves the start date of the last closed trade
        public DateTime DateOfLastLongEntry
        {
            get
            {
                if (_dateOfClosedTrades.Count() == 0)
                {
                    return DateTime.MinValue;
                }
                else
                {
                    return _dateOfClosedTrades.Max();
                }
            }
        }

        /// <summary>
        /// returns a string Epoch Tag for an Order
        /// </summary>
        public string TagTrade
        {
            get
            {
                return (
                    Time.ToUniversalTime()
                    - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                ).TotalSeconds.ToString();
            }
        }
    }
}

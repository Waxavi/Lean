using QuantConnect.Brokerages;
using QuantConnect.Data;
using QuantConnect.Data.Consolidators;
using QuantConnect.Data.Market;
using QuantConnect.Indicators;
using QuantConnect.Orders;
using QuantConnect.Securities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QuantConnect.Algorithm.CSharp
{
    public partial class BubbleRiderRenko : QCAlgorithm
    {
        //Parameters
        public bool UseBrokerageModel = true;
        public bool DebugEnabled = true;

        //Indicator declaration
        private ParabolicStopAndReverse Parabolic;
        private AverageDirectionalIndex AvgIndex;
        private DonchianChannel DonchianBands;

        //Indicators parameters
        private const decimal AvgIndexFilterLevel = 25;
        private const int AvgIndexPeriod = 25;
        private const int DonchianPeriod = 20;

        //Others
        public LoggerRenko MyLogger;
        private string MySymbol = "BTCUSD";
        private int HistoryBars = 100;
        private const decimal PercentageChange = 0.01m;

        public static decimal MyDollarBalance;
        public static decimal DollarBalance
        {
            get
            {
                return MyDollarBalance;
            }
            set
            {
                MyDollarBalance = Math.Round(value, 2);
            }
        }

        //Rolling Windows (Historical Values)
        private RollingWindow<decimal> HistParabolic;
        private RollingWindow<decimal> HistAverageDirectionalIndex;
        private RollingWindow<decimal> HistDonchian;
        private RollingWindow<decimal> HistOpeningValues;
        private RollingWindow<decimal> HistHighValues;
        private RollingWindow<decimal> HistLowValues;
        private RollingWindow<RenkoBar> HistRenkoBar;
        private List<DateTime> DateOfClosedTrades = new List<DateTime>();

        private List<DateTime> OrderClosingTimes = new List<DateTime>()
        {
            DateTime.MinValue
        };

        private Resolution Resolution = Resolution.Minute;
        private Security MySecurity;

        private const string TimeFormat = "MM-dd-yyyy hh:mm:ss";

        class RenkoBar
        {
            public decimal Open { get; set; }
            public decimal Close { get; set; }
            public DateTime Time { get; set; }

            public RenkoBar(decimal open, decimal close, DateTime time)
            {
                Open = open;
                Close = close;
                Time = time;
            }

        }

        RenkoBar MyBar;

        /// <summary>
        /// Initialise the data and resolution required, as well as the cash and start-end dates for your algorithm. All algorithms must initialized.
        /// </summary>
        public override void Initialize()
        {
            MySecurity = AddCrypto(MySymbol, Resolution);
            SetStartDate(2015, 02, 01);
            SetEndDate(2018, 05, 01);
            SetCash(10000);

            if (UseBrokerageModel)
            {
                SetBrokerageModel(BrokerageName.GDAX, AccountType.Cash);
                BrokerageModel.GetFeeModel(MySecurity);
            }

            MyLogger = new LoggerRenko(this, DebugEnabled);

            Parabolic = new ParabolicStopAndReverse(0.017m, 0.017m, 0.2m);
            AvgIndex = new AverageDirectionalIndex(MySymbol, AvgIndexPeriod);
            DonchianBands = new DonchianChannel(MySymbol, DonchianPeriod);

            //Create History for Indicator PSAR
            HistParabolic = new RollingWindow<decimal>(HistoryBars);
            HistAverageDirectionalIndex = new RollingWindow<decimal>(HistoryBars);
            HistDonchian = new RollingWindow<decimal>(HistoryBars);

            //Create History For Renko
            HistRenkoBar = new RollingWindow<RenkoBar>(HistoryBars);

            SetWarmUp(10);
            
            //--For Logging
            DollarBalance = Portfolio.CashBook["USD"].Amount;

            var RenkoAndParabolicPlots = new Chart("Renko Chart");
            var openValues = new Series("Open", SeriesType.Line, "$", System.Drawing.Color.FromArgb(50, System.Drawing.Color.CornflowerBlue));
            RenkoAndParabolicPlots.AddSeries(openValues);
            var closeValues = new Series("Close", SeriesType.Line, "$", System.Drawing.Color.FromArgb(50, System.Drawing.Color.Red));
            RenkoAndParabolicPlots.AddSeries(closeValues);
            var psarPrice = new Series("PSAR", SeriesType.Scatter, "$", System.Drawing.Color.FromArgb(50, System.Drawing.Color.Pink));
            RenkoAndParabolicPlots.AddSeries(psarPrice);
            var AverageIndexPlots = new Chart("ADX");
            var adxPrice = new Series("ADX Line",SeriesType.Line, "$", System.Drawing.Color.FromArgb(50,System.Drawing.Color.White));

            Series buyOrders = new Series("Buy", SeriesType.Scatter, 0);
            RenkoAndParabolicPlots.AddSeries(buyOrders);
            Series sellOrders = new Series("Sell", SeriesType.Scatter, 0);
            RenkoAndParabolicPlots.AddSeries(sellOrders);

            Log($"{TimeF} INFO (CUSTOM) Initialize Finished.");
        }

        public override void OnWarmupFinished()
        {
            Parabolic.Reset();
            AvgIndex.Reset();
            DonchianBands.Reset();

            base.OnWarmupFinished();
        }

        public override void OnData(Slice data)
        {
            if (!MySecurity.IsTradable)
            {
                Log($"{TimeF} ERROR BTCUSD Not Tradable.");
                return;
            }

            if (!MySecurity.Exchange.ExchangeOpen)
            {
                Log($"{TimeF} ERROR GDAX is Closed.");
                return;
            }

            UpdateRenkoBar(data.Bars[MySymbol]);
            PlotRenkoAndIndicators();

            if(!Portfolio.Invested)
            {
                if (Time > DateTime.Parse("02/27/2017") && Time < DateTime.Parse("03/15/2017"))
                    return;

                if (DateOfSignal < DateOfLastTrade)
                    return;
                
                if (!(HistRenkoBar[0].Close > HistParabolic[0] && HistAverageDirectionalIndex[0] > AvgIndexFilterLevel))
                    return;

                OpenPosition();
            }
            else if (HistRenkoBar[0].Close > HistParabolic[0])
            {
                //UpdatePosition();
            }
            else
            //else if (HistRenkoBar[0].Close < HistParabolic[0])
            {
                ClosePosition();
            }
        }

        private double GetTimeTag => (Time.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;

        private DateTime DateOfSignal
        {
            get
            {
                int i = 0;
                int maxBars = HistRenkoBar.Count;

                if (HistRenkoBar[i].Close > HistParabolic[i])
                {
                    while (HistRenkoBar[i].Close > HistParabolic[i] && i < maxBars - 1)
                    {
                        i++;
                    }
                }

                return HistRenkoBar[i].Time;
            }
        }

        private DateTime DateOfLastTrade => OrderClosingTimes.Max();

        private void PlotRenkoAndIndicators()
        {
            if (Parabolic.Current.Value == 0)
                return;

            Plot("Renko Chart", "PSAR", (decimal)Parabolic.Current.Value);
            Plot("Renko Chart", "Open", MyBar.Open);
            Plot("Renko Chart", "Close", MyBar.Close);
            Plot("ADX", "ADX Line", AvgIndex.Current.Value);
        }

        public override void OnEndOfAlgorithm()
        {
            Liquidate(MySymbol);
        }

        // Override the base class event handler for order events
        public override void OnOrderEvent(OrderEvent orderEvent)
        {
            //MyLogger.ErrorOnOrderEvent(orderEvent);
            //if (orderEvent.Direction == OrderDirection.Sell)
            //{
            //    DateOfClosedTrades.Add(Time);
            //}
            ////Logs two messages according to orderevent.Direction
            //MyLogger.InfoPositionOrderEvent(orderEvent, MyDollarBalance);

            //if (orderEvent.FillPrice > 0)
            //{
            //    if (orderEvent.Direction == OrderDirection.Sell)
            //    {
            //        Plot("Renko Chart", "Sell", orderEvent.FillPrice);
            //        MyDollarBalance = Portfolio.CashBook["USD"].Amount;
            //    }
            //    else
            //    {
            //        Plot("Renko Chart", "Buy", orderEvent.FillPrice);
            //    }
            //}
        }

        /// <summary>
        /// UTC Format [MM-DD-YYYY hh:mm:ss]
        /// </summary>
        private string TimeF => $"[{Time.ToUniversalTime().ToString(TimeFormat)}]";

        private void UpdateRenkoBar(TradeBar tradeBar)
        {
            if(MyBar == null)
            {
                MyBar = new RenkoBar(tradeBar.Open, tradeBar.Close, tradeBar.Time);
                Parabolic.Update(tradeBar);
                AvgIndex.Update(tradeBar);

                HistAverageDirectionalIndex.Add(AvgIndex.Current.Value);
                HistRenkoBar.Add(MyBar);
                HistParabolic.Add(Parabolic);
                return;
            }

            if (MyBar.Close == MyBar.Open)
            {
                if (tradeBar.Close >= MyBar.Close * (1 + PercentageChange) || tradeBar.Close <= MyBar.Open * (1 - PercentageChange))
                {
                    MyBar.Close = tradeBar.Close;
                    MyBar.Time = tradeBar.Time;
                    Parabolic.Update(tradeBar);
                    AvgIndex.Update(tradeBar);

                    HistAverageDirectionalIndex.Add(AvgIndex.Current.Value);
                    HistRenkoBar.Add(MyBar);
                    HistParabolic.Add(Parabolic);
                }
            }
            else if (MyBar.Close > MyBar.Open)
            {
                if (tradeBar.Close >= MyBar.Close * (1 + PercentageChange))
                {
                    MyBar.Open = MyBar.Close;
                    MyBar.Time = tradeBar.Time;
                    MyBar.Close = tradeBar.Close;
                    Parabolic.Update(tradeBar);
                    AvgIndex.Update(tradeBar);

                    HistAverageDirectionalIndex.Add(AvgIndex.Current.Value);
                    HistRenkoBar.Add(MyBar);
                    HistParabolic.Add(Parabolic);
                }
                else if (tradeBar.Close <= MyBar.Open * (1 - PercentageChange))
                {
                    MyBar.Close = tradeBar.Close;
                    MyBar.Time = tradeBar.Time;
                    Parabolic.Update(tradeBar);
                    AvgIndex.Update(tradeBar);

                    HistAverageDirectionalIndex.Add(AvgIndex.Current.Value);
                    HistRenkoBar.Add(MyBar);
                    HistParabolic.Add(Parabolic);
                }
            }
            else
            {
                if (tradeBar.Close >= MyBar.Open * (1 + PercentageChange))
                {
                    MyBar.Close = tradeBar.Close;
                    MyBar.Time = tradeBar.Time;
                    Parabolic.Update(tradeBar);
                    AvgIndex.Update(tradeBar);

                    HistAverageDirectionalIndex.Add(AvgIndex.Current.Value);
                    HistRenkoBar.Add(MyBar);
                    HistParabolic.Add(Parabolic);
                }
                else if (tradeBar.Close <= MyBar.Close * (1 - PercentageChange))
                {
                    MyBar.Open = MyBar.Close;
                    MyBar.Time = tradeBar.Time;
                    MyBar.Close = tradeBar.Close;
                    Parabolic.Update(tradeBar);
                    AvgIndex.Update(tradeBar);

                    HistAverageDirectionalIndex.Add(AvgIndex.Current.Value);
                    HistRenkoBar.Add(MyBar);
                    HistParabolic.Add(Parabolic);
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
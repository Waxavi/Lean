using QuantConnect.Data.Market;
using QuantConnect.Indicators;
using QuantConnect.Orders;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QuantConnect.Algorithm.CSharp
{
    public class Logger
    {
        private QCAlgorithm QcAlgorithm;
        private bool DebugEnabled;
        private const string TimeFormat = "MM-dd-yyyy HH:mm:ss";

        public Logger(QCAlgorithm qCAlgorithm, bool debugEnabled)
        {
            this.QcAlgorithm = qCAlgorithm;
            this.DebugEnabled = debugEnabled;
        }

        public string CustomTimeFormat
        {
            get
            {
                if (QcAlgorithm.LiveMode)
                {
                    return $"[{DateTime.Now.ToString(TimeFormat)}]";
                }
                else
                {
                    return $"[{QcAlgorithm.Time.ToString(TimeFormat)}]";
                }
            }
        }

        /// <summary>
        /// Logs Initial BBR Settings
        /// </summary>
        public void InfoSettings(string message)
        {
            if (DebugEnabled)
            {
                QcAlgorithm.Debug($"{CustomTimeFormat} DEBUG (CUSTOM) {message}");
            }
            else
            {
                QcAlgorithm.Log($"{CustomTimeFormat} INFO (CUSTOM) {message}");
            }
        }

        /// <summary>
        /// Log OHLC data
        /// </summary>
        /// <param name="tradeBar"></param>
        public void InfoBar(TradeBar tradeBar, bool isWarmUp = false)
        {
            if (DebugEnabled)
            {
                string time = QcAlgorithm.LiveMode
                    ? CustomTimeFormat
                    : "[" + tradeBar.Time.ToUniversalTime().ToString(TimeFormat) + "]";

                QcAlgorithm.Debug($"{time} DEBUG (BAR) OPEN {tradeBar.Open} |" +
                                $" HIGH {tradeBar.High} |" +
                                $" LOW {tradeBar.Low} |" +
                                $" CLOSE {tradeBar.Close}");
            }
            else if (QcAlgorithm.LiveMode)
            {
                string time = isWarmUp
                    ? "[" + tradeBar.Time.ToUniversalTime().ToString(TimeFormat) + "]"
                    : CustomTimeFormat;

                QcAlgorithm.Log($"{time} INFO (BAR) OPEN {tradeBar.Open} |" +
                $" HIGH {tradeBar.High} |" +
                $" LOW {tradeBar.Low} |" +
                $" CLOSE {tradeBar.Close}");
            }
        }

        /// <summary>
        /// Log indicator data
        /// </summary>
        /// <param name="tradeBar"></param>
        /// <param name="parabolic"></param>
        /// <param name="avgDirectionalIndex"></param>
        public void InfoIndicator(
            TradeBar tradeBar,
            ParabolicStopAndReverse parabolic,
            AverageDirectionalIndex avgDirectionalIndex,
            DonchianChannel donchian,
            bool isWarmUp = false
        )
        {
            if (DebugEnabled)
            {
                string time = QcAlgorithm.LiveMode
                    ? CustomTimeFormat
                    : "[" + tradeBar.Time.ToUniversalTime().ToString(TimeFormat) + "]";

                QcAlgorithm.Debug($"{time} DEBUG (INDICATOR) PSAR {Math.Round(parabolic.Current.Price, 2)} | " +
                    $"ADX {Math.Round(avgDirectionalIndex.Current.Price, 2)} | " +
                    $"Donchian {Math.Round(donchian.LowerBand.Current.Price, 2)}");
            }
            else if (QcAlgorithm.LiveMode)
            {
                string time = isWarmUp
                    ? "[" + tradeBar.Time.ToUniversalTime().ToString(TimeFormat) + "]"
                    : CustomTimeFormat;

                QcAlgorithm.Log($"{time} INFO (INDICATOR) PSAR {Math.Round(parabolic.Current.Price, 2)} | " +
                    $"ADX {Math.Round(avgDirectionalIndex.Current.Price, 2)} | " +
                    $"Donchian {Math.Round(donchian.LowerBand.Current.Price, 2)}");
            }
        }

        /// <summary>
        /// Log position data on order events
        /// </summary>
        /// <param name="orderEvent"></param>
        /// <param name="quoteBalance">Quote Balance</param>
        /// <param name="quoteCurrency">Quote Currency i.e. USD in BTCUSD</param>
        public void InfoOrderEvent(
            OrderEvent orderEvent,
            decimal quoteBalance,
            string quoteCurrency
        )
        {
            if (orderEvent.FillPrice > 0 && orderEvent.Status == OrderStatus.Filled)
            {
                    if (DebugEnabled)
                    {
                        QcAlgorithm.Log($"{CustomTimeFormat} DEBUG (ORDER) Action Create |" +
                                        $" Id {orderEvent.OrderId} |" +
                                        $" Type Market |" +
                                        $" Status {orderEvent.Status} |" +
                                        $" Price {orderEvent.FillPrice} |" +
                                        $" Size {orderEvent.FillQuantity} |" +
                                        $" Direction {orderEvent.Direction}");
                    }
                    else if (QcAlgorithm.LiveMode)
                    {
                        QcAlgorithm.Log($"{CustomTimeFormat} INFO (ORDER) Action Create |" +
                                        $" Id {orderEvent.OrderId} |" +
                                        $" Type Market |" +
                                        $" Status {orderEvent.Status} |" +
                                        $" Price {orderEvent.FillPrice} |" +
                                        $" Size {orderEvent.FillQuantity} |" +
                                        $" Direction {orderEvent.Direction}");
                    }
            }
        }

        /// <summary>
        /// Logs Info when a Position is Opened
        /// </summary>
        /// <param name="price"></param>
        /// <param name="quantity"></param>
        /// <param name="quoteCurrency"></param>
        /// <param name="previousQuoteBalance"></param>
        /// <param name="stopLossPrice"></param>
        public void InfoPositionOpen(decimal price, 
            decimal quantity, 
            string quoteCurrency,
            decimal previousQuoteBalance,
            decimal stopLossPrice)
        {
            if (DebugEnabled)
            {
                QcAlgorithm.Debug($"{CustomTimeFormat} DEBUG (POSITION) Action Open |" +
                                  $" Direction Long |" +
                                  $" Price {price} |" +
                                  $" Quantity {quantity} |" +
                                  $" {quoteCurrency} Balance Before {previousQuoteBalance} |" +
                                  $" Stop Loss {stopLossPrice}");
            }
            else if (QcAlgorithm.LiveMode)
            {
                QcAlgorithm.Debug($"{CustomTimeFormat} INFO (POSITION) Action Open |" +
                                  $" Direction Long |" +
                                  $" Price {price} |" +
                                  $" Quantity {quantity} |" +
                                  $" {quoteCurrency} Balance Before {previousQuoteBalance} |" +
                                  $" Stop Loss {stopLossPrice}");
            }
        }

        /// <summary>
        /// Logs info about position
        /// </summary>
        /// <param name="action"></param>
        /// <param name="stopLoss"></param>
        public void InfoPositionUpdate(decimal stopLoss)
        {
            if (DebugEnabled)
            {
                QcAlgorithm.Debug($"{CustomTimeFormat} DEBUG (POSITION) Action Update |" +
                                  $" Stop Loss {stopLoss}");
            }
            else if (QcAlgorithm.LiveMode)
            {
                QcAlgorithm.Debug($"{CustomTimeFormat} INFO (POSITION) Action Update |" +
                                  $" Stop Loss {stopLoss}");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="price"></param>
        /// <param name="quantity"></param>
        /// <param name="quoteCurrency"></param>
        /// <param name="previousQuoteBalance"></param>
        public void InfoPositionClose(
            decimal price,
            decimal quantity,
            string quoteCurrency,
            decimal previousQuoteBalance)
        {
            if (DebugEnabled)
            {
                QcAlgorithm.Debug($"{CustomTimeFormat} DEBUG (POSITION) Action Close |" +
                                  $" Direction Long |" +
                                  $" Price {price} |" +
                                  $" Quantity {quantity} |" +
                                  $" {quoteCurrency} Balance Before {previousQuoteBalance}");
            }
            else if (QcAlgorithm.LiveMode)
            {
                QcAlgorithm.Debug($"{CustomTimeFormat} INFO (POSITION) Action Close |" +
                                  $" Direction Long |" +
                                  $" Price {price} |" +
                                  $" Quantity {quantity} |" +
                                  $" {quoteCurrency} Balance Before {previousQuoteBalance}");
            }
        }

        /// <summary>
        /// Log a custom message
        /// </summary>
        public void InfoCustom(String message)
        {
            if (DebugEnabled)
            {
                QcAlgorithm.Debug($"{CustomTimeFormat} DEBUG (CUSTOM) {message}");
            }
            else
            {
                QcAlgorithm.Log($"{CustomTimeFormat} INFO (CUSTOM) {message}");
            }
        }

        /// <summary>
        /// Log an error if there was a failure canceling an order
        /// </summary>
        /// <param name="response"></param>
        public void ErrorCancelingOrder(OrderResponse response)
        {
            QcAlgorithm.Log($"{CustomTimeFormat} ERROR Canceling Order {response.OrderId}. " +
                $"Error: {response.ErrorMessage}, " +
                $"Code: {response.ErrorCode}");
        }

        /// <summary>
        /// Log an error if th exchange is closed
        /// </summary>
        public void ErrorExchangeClosed()
        {
            QcAlgorithm.Log($"{CustomTimeFormat} ERROR GDAX is Closed.");
        }

        /// <summary>
        /// Log an error on orders mismatch
        /// </summary>
        /// <param name="ordersCount"></param>
        public void ErrorInNumberOfOrdersCanceled(List<OrderTicket> ordersCanceled)
        {
            QcAlgorithm.Log($"{CustomTimeFormat} ERROR in Number of Orders, total: {ordersCanceled.Count}.");

            if (ordersCanceled.Count > 0)
            {
                foreach (var c in ordersCanceled)
                {
                    QcAlgorithm.Log($"{CustomTimeFormat} ERROR Order " +
                        $"Id: {c.OrderId} " +
                        $"Time {c.Time} " +
                        $"Order Closed {c.OrderClosed} " +
                        $"Quantity {c.Quantity} " +
                        $"Status {c.Status}");
                }
            }
        }

        /// <summary>
        /// Log an error on orders mismatch
        /// </summary>
        /// <param name="orders"></param>
        public void ErrorInNumberOfOrders(IEnumerable<Order> orders)
        {
            QcAlgorithm.Log($"{CustomTimeFormat} ERROR in Number of Orders, total: {orders.Count()}.");

            if (orders.Count() > 0)
            {
                foreach (var c in orders)
                {
                    QcAlgorithm.Log($"{CustomTimeFormat} ERROR Order " +
                        $"Id {c.Id} " +
                        $"Time {c.Time} " +
                        $"Quantity {c.Quantity} " +
                        $"Status {c.Status}");
                }
            }
        }

        /// <summary>
        /// Log an error if the order event status is invalid
        /// </summary>
        /// <param name="orderEvent"></param>
        public void ErrorOnOrderEvent(OrderEvent orderEvent)
        {
            QcAlgorithm.Log($"{CustomTimeFormat} ERROR Order {orderEvent.OrderId} " +
                $"Invalid. Message: {orderEvent.Message}. " +
                $"Status: {orderEvent.Status}.");
        }

        /// <summary>
        /// Log an error if the position failed to open
        /// </summary>
        /// <param name="orderStatus"></param>
        public void ErrorOnPositionOpen(OrderStatus orderStatus)
        {
            QcAlgorithm.Log($"{CustomTimeFormat} ERROR Action Open. " +
                $"Status {orderStatus}");
        }

        /// <summary>
        /// Log an error if the position failed to open
        /// </summary>
        /// <param name="orderStatus"></param>
        public void ErrorOnPositionClose(OrderStatus orderStatus)
        {
            QcAlgorithm.Log($"{CustomTimeFormat} ERROR Action Close. " +
                            $"Status {orderStatus}");
        }

        /// <summary>
        /// Log an error if stop loss send fails
        /// </summary>
        /// <param name="response"></param>
        /// <param name="status"></param>
        public void ErrorStopLossSend(OrderResponse response, OrderStatus status)
        {
            QcAlgorithm.Log($"{CustomTimeFormat} ERROR Send Stop Loss, " +
                $"message: {response.ErrorMessage}. " +
                $"Code: {response.ErrorCode}. " +
                $"Status: {status} ");
        }

        /// <summary>
        /// Log an error if stop loss update fails
        /// </summary>
        /// <param name="response"></param>
        /// <param name="status"></param>
        public void ErrorStopLossUpdate(OrderResponse response, OrderStatus status)
        {
            QcAlgorithm.Log($"{CustomTimeFormat} ERROR Update Stop Loss," +
                $" message: {response.ErrorMessage}. " +
                $"Code: {response.ErrorCode}. " +
                $"Status: {status}");
        }

        /// <summary>
        /// Log an error if the trading symbol is not tradable
        /// </summary>
        public void ErrorSymbolNotTradable(string symbol)
        {
            QcAlgorithm.Log($"{CustomTimeFormat} ERROR {symbol} Not Tradable.");
        }

        /// <summary>
        /// Log a FATAL message
        /// </summary>
        public void FatalHistoryConsolidator()
        {
            QcAlgorithm.Error($"{CustomTimeFormat} FATAL History Consolidator was given a wrong resolution parameter.");
        }

        /// <summary>
        /// Reports every 6 hours the following data:
        /// Balance (Quote Currency) | Balance (Base Currency) | Open Orders | Current Position | Position Size
        /// </summary>
        /// <param name="mySymbol">Trading Symbol</param>
        /// <param name="baseCurrency">Base Currency i.e. BTC in BTCUSD </param>
        /// <param name="quoteCurrency">Quote Currency i.e. USD in BTCUSD </param>
        public void ScheduleReport(string mySymbol, string baseCurrency, string quoteCurrency)
        {
            QcAlgorithm.Schedule.On(QcAlgorithm.DateRules.EveryDay(mySymbol), QcAlgorithm.TimeRules.Every(TimeSpan.FromHours(6)), () =>
            {
                if (DebugEnabled || QcAlgorithm.LiveMode)
                {
                    string protocol = DebugEnabled ? "DEBUG" : "INFO";
                    decimal positionSize = QcAlgorithm.Portfolio.CashBook[baseCurrency].Amount;

                    QcAlgorithm.Debug($"{CustomTimeFormat} {protocol} (REPORT) Balance ({quoteCurrency}) {Math.Round(QcAlgorithm.Portfolio.CashBook[quoteCurrency].Amount, 2)} |" +
                                    $" Balance {baseCurrency} {positionSize} |" +
                                    $" Current Position {(positionSize > 0 ? "Long" : "None")} |" +
                                    $" Position Size {positionSize}");
                }
            });
        }
    }
}

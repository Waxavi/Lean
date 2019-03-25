using QuantConnect.Data.Market;
using QuantConnect.Indicators;
using QuantConnect.Orders;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QuantConnect.Algorithm.CSharp
{
    public class LoggerRenko
    {
        private QCAlgorithm QcAlgorithm;
        private bool DebugEnabled;
        private const string TimeFormat = "MM-dd-yyyy hh:mm:ss";

        public LoggerRenko(QCAlgorithm qCAlgorithm, bool debugEnabled)
        {
            this.QcAlgorithm = qCAlgorithm;
            this.DebugEnabled = debugEnabled;
        }

        public string CustomTimeFormat => $"[{DateTime.UtcNow.ToString(TimeFormat)}]";

        /// <summary>
        /// Log OHLC data
        /// </summary>
        /// <param name="tradeBar"></param>
        public void InfoBar(TradeBar tradeBar)
        {
            if (DebugEnabled)
            {
                string time = "[" + tradeBar.Time.ToUniversalTime().ToString(TimeFormat) + "]";
                QcAlgorithm.Debug($"{time} DEBUG (BAR) OPEN {tradeBar.Open} |" +
                                $" HIGH {tradeBar.High} |" +
                                $" LOW {tradeBar.Low} |" +
                                $" CLOSE {tradeBar.Close}");
            }
            else if (QcAlgorithm.LiveMode)
            {
                string time = "[" + tradeBar.Time.ToUniversalTime().ToString(TimeFormat) + "]";
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
            AverageDirectionalIndex avgDirectionalIndex
        )
        {
            if (DebugEnabled)
            {
                string time = "[" + tradeBar.Time.ToUniversalTime().ToString(TimeFormat) + "]";
                QcAlgorithm.Debug($"{time} DEBUG (INDICATOR) PSAR {Math.Round(parabolic.Current.Price, 2)} | " +
                    $"ADX {Math.Round(avgDirectionalIndex.Current.Price, 2)}");
            }
            else if (QcAlgorithm.LiveMode)
            {
                string time = "[" + tradeBar.Time.ToUniversalTime().ToString(TimeFormat) + "]";
                QcAlgorithm.Log($"{time} INFO (INDICATOR) PSAR {Math.Round(parabolic.Current.Price, 2)} | " +
                    $"ADX {Math.Round(avgDirectionalIndex.Current.Price, 2)}");
            }
        }

        /// <summary>
        /// Log position data on order events
        /// </summary>
        /// <param name="orderEvent"></param>
        /// <param name="usdBalance"></param>
        public void InfoPositionOrderEvent(
            OrderEvent orderEvent,
            decimal usdBalance
        )
        {
            if (orderEvent.FillPrice > 0)
            {
                if (orderEvent.Direction == OrderDirection.Sell)
                {
                    QcAlgorithm.Log($"{CustomTimeFormat} INFO (POSITION) Action Close |" +
                                    $" Direction Long |" +
                                    $" Price {orderEvent.FillPrice} |" +
                                    $" Quantity {orderEvent.FillQuantity} |" +
                                    $" USD Balance before {usdBalance}");
                }
                else
                {
                    QcAlgorithm.Log($"{CustomTimeFormat} INFO (POSITION) Action Open |" +
                                     $" Direction Long |" +
                                     $" Price {orderEvent.FillPrice} |" +
                                     $" Quantity {orderEvent.FillQuantity} |" +
                                     $" USD Balance before {usdBalance}");
                }
            }
        }

        /// <summary>
        /// Log info about stop loss cancelation
        /// </summary>
        /// <param name="stopPrice"></param>
        /// <param name="quantity"></param>
        public void InfoStopLossCanceled(OrderTicket order)
        {
            if (DebugEnabled)
            {
                QcAlgorithm.Debug($"{CustomTimeFormat} DEBUG (ORDER) Action Cancel | Id {order.OrderId} | Type Stop Market " +
                    $"| Status {order.Status}");
            }
            else if (QcAlgorithm.LiveMode)
            {
                QcAlgorithm.Log($"{CustomTimeFormat} INFO (ORDER) Action Cancel | Id {order.OrderId} | Type Stop Market " +
                    $"| Status {order.Status}");
            }
        }

        /// <summary>
        /// Log info about stop loss creation
        /// </summary>
        /// <param name="stopPrice"></param>
        /// <param name="quantity"></param>
        public void InfoStopLossSent(OrderTicket order, decimal price)
        {
            if (DebugEnabled)
            {
                QcAlgorithm.Debug($"{CustomTimeFormat} DEBUG (ORDER) Action Create | Id {order.OrderId} | Type Stop Market |" +
                $" Status {order.Status} |" +
                $" Price {price} |" +
                $" Size {order.Quantity} |" +
                $" Direction {OrderDirection.Sell} |");
            }
            else if (QcAlgorithm.LiveMode)
            {
                QcAlgorithm.Log($"{CustomTimeFormat} INFO (ORDER) Action Create | Id {order.OrderId} | Type Stop Market |" +
                $" Status {order.Status} |" +
                $" Price {price} |" +
                $" Size {order.Quantity} |" +
                $" Direction {OrderDirection.Sell} |");
            }
        }

        /// <summary>
        /// Log info about stop loss update
        /// </summary>
        /// <param name="stopPrice"></param>
        /// <param name="quantity"></param>
        public void InfoStopLossUpdate(decimal stopPrice, decimal quantity)
        {
            if (DebugEnabled)
            {
                QcAlgorithm.Debug($"{CustomTimeFormat} DEBUG (POSITION) Action Update |" +
                $" Stop Loss {stopPrice}" +
                $" Quantity {quantity}");
            }
            else if (QcAlgorithm.LiveMode)
            {
                QcAlgorithm.Log($"{CustomTimeFormat} INFO (POSITION) Action Update |" +
                $" Stop Loss {stopPrice}" +
                $" Quantity {quantity}");
            }
        }

        /// <summary>
        /// Log a custom message
        /// </summary>
        public void InfoCustom(String message)
        {
            QcAlgorithm.Log($"{CustomTimeFormat} INFO (CUSTOM) {message}");
        }

        /// <summary>
        /// Log a warning if Selling Power is different from expected value
        /// </summary>
        /// <param name="sellingPower">Selling Power</param>
        /// <param name="openOrders">Number of Open Orders</param>
        public void WarningBadSellingPower(
            decimal sellingPower,
            decimal currentBtcHoldings,
            int openOrders
        )
        {
            QcAlgorithm.Log($"{CustomTimeFormat} WARNING Selling Power is different from expected value. Order History Will Be Logged. " +
                $"Selling Power {sellingPower}. " +
                $"Current BtcHoldings {currentBtcHoldings}. " +
                $"Open Orders {openOrders}.");

            var history = QcAlgorithm.Transactions.GetOrders(x => x.Price > 0);

            foreach (var h in history)
            {
                QcAlgorithm.Log($"{CustomTimeFormat} WARNING Order " +
                    $"Time: {h.Time} " +
                    $"Id: {h.Id} " +
                    $"Status: {h.Status} " +
                    $"Quantity: {h.Quantity} " +
                    $"Price: {h.Price}");
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
            if (orderEvent.Status == OrderStatus.Invalid)
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
        public void ErrorSymbolNotTradable()
        {
            QcAlgorithm.Log($"{CustomTimeFormat} ERROR BTCUSD Not Tradable.");
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
        /// Balance (USD) | Balance (BTC) | Open Orders | Current Position | Position Size
        /// </summary>
        /// <param name="mySymbol">Trading Symbol</param>
        public void ScheduleReport(string mySymbol)
        {
            QcAlgorithm.Schedule.On(QcAlgorithm.DateRules.EveryDay(mySymbol), QcAlgorithm.TimeRules.Every(TimeSpan.FromHours(6)), () =>
            {
                if (DebugEnabled || QcAlgorithm.LiveMode)
                {
                    string protocol = DebugEnabled ? "DEBUG" : "INFO";
                    decimal positionSize = QcAlgorithm.Transactions.GetOrders(q => q.Status == OrderStatus.Filled).Any()
                                            ? QcAlgorithm.Transactions.GetOrders(q => q.Status == OrderStatus.Filled).Last().Quantity
                                            : 0;

                    QcAlgorithm.Debug($"{CustomTimeFormat} {protocol} (REPORT) Balance (USD) {Math.Round(QcAlgorithm.Portfolio.CashBook["USD"].Amount, 2)} |" +
                                    $" Balance BTC {QcAlgorithm.Portfolio.CashBook["BTC"].Amount} |" +
                                    $" Open Orders {QcAlgorithm.Transactions.GetOpenOrders(mySymbol).Count} |" +
                                    $" Current Position {(QcAlgorithm.Transactions.GetOpenOrders(mySymbol).Count > 0 ? "Long" : "None")} |" +
                                    $" Position Size {positionSize}");
                }
            });
        }
    }
}
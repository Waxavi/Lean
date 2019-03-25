using QuantConnect.Orders;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QuantConnect.Algorithm.CSharp
{
    public partial class BubbleRiderRenko
    {
        /// <summary>
        /// Sends a Long Market Order with a Stop Loss at the Parabolic SAR value
        /// </summary>
        public void OpenPosition()
        {
            MyDollarBalance = Portfolio.CashBook["USD"].Amount;

            string tag = TagTrade;
            SetHoldings(MySymbol, 1m, false, tag);
            IEnumerable<Order> orders = Transactions.GetOrders(
                item => item.Symbol == MySymbol &&
                item.Tag == tag
            );

            //if (orders.Count() == 1)
            //{
            //    Order order = orders.First();
            //    int id = order.Id;

            //    if (LiveMode)
            //    {
            //        Transactions.WaitForOrder(id);
            //    }

            //    if (order.Status == OrderStatus.Filled)
            //    {
            //        decimal StopPrice = Math.Round(Parabolic.Current.Value, 2);
            //        decimal sellingPower = Portfolio.GetBuyingPower(MySecurity.Symbol, OrderDirection.Sell);

            //        if (sellingPower != Portfolio[MySymbol].Quantity)
            //        {
            //            MyLogger.WarningBadSellingPower(sellingPower, Portfolio[MySymbol].Quantity, Transactions.GetOpenOrders().Count);
            //        }

            //        OrderTicket StopTrade = StopMarketOrder(MySymbol, -sellingPower, StopPrice);
            //        if (LiveMode)
            //        {
            //            Transactions.WaitForOrder(StopTrade.OrderId);
            //        }
            //        if (StopTrade.Status == OrderStatus.New || StopTrade.Status == OrderStatus.Submitted)
            //        {
            //            MyLogger.InfoStopLossSent(StopTrade, StopPrice);
            //        }
            //        else
            //        {
            //            OrderResponse response = StopTrade.GetMostRecentOrderResponse();
            //            MyLogger.ErrorStopLossSend(response, StopTrade.Status);
            //        }
            //    }
            //    else
            //    {
            //        MyLogger.ErrorOnPositionOpen(order.Status);
            //    }
            //}
            //else
            //{
            //    MyLogger.ErrorInNumberOfOrders(orders);
            //}
        }

        /// <summary>
        /// Updates a Stop Loss Order to the Parabolic SAR value
        /// </summary>
        public void UpdatePosition()
        {
            List<OrderTicket> cancelOrders = Transactions.CancelOpenOrders(MySymbol);

            //DEBUG
            decimal StopPrice = Math.Round(Parabolic.Current.Value, 2);
            decimal sellingPower = Portfolio.GetBuyingPower(MySecurity.Symbol, OrderDirection.Sell);

            OrderTicket StopTrade = StopMarketOrder(MySymbol, -sellingPower, StopPrice);
            //

            //Action sendStopLoss = () =>
            //{
            //    if (LiveMode)
            //    {
            //        System.Threading.Thread.Sleep(1000);
            //    }

            //    decimal StopPrice = Math.Round(Parabolic.Current.Value, 2);
            //    decimal sellingPower = Portfolio.GetBuyingPower(MySecurity.Symbol, OrderDirection.Sell);

            //    if (sellingPower != Portfolio[MySymbol].Quantity)
            //    {
            //        MyLogger.WarningBadSellingPower(sellingPower, Portfolio[MySymbol].Quantity, Transactions.GetOpenOrders().Count);
            //    }

            //    OrderTicket StopTrade = StopMarketOrder(MySymbol, -sellingPower, StopPrice);
            //    if (LiveMode)
            //    {
            //        Transactions.WaitForOrder(StopTrade.OrderId);
            //    }
            //    if (StopTrade.Status == OrderStatus.New || StopTrade.Status == OrderStatus.Submitted)
            //    {
            //        MyLogger.InfoStopLossUpdate(StopPrice, StopTrade.Quantity);
            //    }
            //    else
            //    {
            //        OrderResponse response = StopTrade.GetMostRecentOrderResponse();
            //        MyLogger.ErrorStopLossUpdate(response, StopTrade.Status);
            //    }
            //};

            //if (cancelOrders.Count == 1)
            //{
            //    if (LiveMode)
            //    {
            //        Transactions.WaitForOrder(cancelOrders.First().OrderId);
            //    }
            //    else
            //    {
            //        while (cancelOrders.First().Status == OrderStatus.CancelPending)
            //        {
            //            if (LiveMode)
            //            {
            //                System.Threading.Thread.Sleep(1000);
            //            }
            //        }
            //    }

            //    MyLogger.InfoStopLossCanceled(cancelOrders.First());

            //    if (cancelOrders.First().Status == OrderStatus.Canceled)
            //    {
            //        sendStopLoss.Invoke();
            //    }
            //    else
            //    {
            //        MyLogger.ErrorCancelingOrder(cancelOrders.First().GetMostRecentOrderResponse());
            //    }
            //}
            //else if (cancelOrders.Count == 0)
            //{
            //    MyLogger.ErrorInNumberOfOrdersCanceled(cancelOrders);

            //    sendStopLoss.Invoke();
            //}
            //else
            //{
            //    MyLogger.ErrorInNumberOfOrdersCanceled(cancelOrders);
            //}
        }

        /// <summary>
        /// Closes a Position and Cancels All Open Orders
        /// </summary>
        public void ClosePosition()
        {
            SetHoldings(MySymbol, 0, true);
            //List<OrderTicket> ordersCanceled = Transactions.CancelOpenOrders(MySymbol);

            //if (ordersCanceled.Count == 1)
            //{
            //    if (LiveMode)
            //    {
            //        Transactions.WaitForOrder(ordersCanceled.First().OrderId);
            //    }

            //    MyLogger.InfoStopLossCanceled(ordersCanceled.First());
            //}
            //else
            //{
            //    MyLogger.ErrorInNumberOfOrdersCanceled(ordersCanceled);
            //}

            //decimal sellingPower = Portfolio.GetBuyingPower(MySecurity.Symbol, OrderDirection.Sell);
            //if (sellingPower != Portfolio[MySymbol].Quantity)
            //{
            //    MyLogger.WarningBadSellingPower(sellingPower, Portfolio[MySymbol].Quantity, Transactions.GetOpenOrders().Count);
            //}

            ////Regardless of the status it must try to clear all BTC holdings
            //if (Portfolio.CashBook["BTC"].Amount != 0)
            //{
            //    SetHoldings(MySymbol, 0);
            //}
        }
    }
}
using QuantConnect.Orders;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QuantConnect.Algorithm.CSharp
{
    public partial class BubbleRider
    {
        public decimal StopPrice;
        /// <summary>
        /// Sends a Long Market Order with a Stop Loss at the Parabolic SAR value
        /// </summary>
        public void OpenPosition()
        {
            string tag = TagTrade;
            SetHoldings(MySymbol, 1m, false, tag);
            IEnumerable<Order> orders = Transactions.GetOrders(
                item => item.Symbol == MySymbol &&
                item.Tag == tag
            );

            if (orders.Count() == 1)
            {
                Order order = orders.First();
                int id = order.Id;

                if (LiveMode)
                {
                    Transactions.WaitForOrder(id);
                }

                if (order.Status == OrderStatus.Filled)
                {
                    StopPrice = Math.Round(_histDonchian[0], 2);
                    decimal quantity = Portfolio.CashBook[BaseCurrency].Amount;

                    MyLogger.InfoPositionOpen(order.Price, quantity, QuoteCurrency, QuoteBalance, StopPrice);
                }
                else
                {
                    MyLogger.ErrorOnPositionOpen(order.Status);
                }
            }
            else
            {
                MyLogger.ErrorInNumberOfOrders(orders);
            }
        }

        /// <summary>
        /// Updates a Stop Loss Order to the Donchian Lower Band Value
        /// </summary>
        public void UpdatePosition()
        {
            StopPrice = Math.Round(_histDonchian[0], 2);

            MyLogger.InfoPositionUpdate(StopPrice);
        }

        /// <summary>
        /// Closes a Long Market Order
        /// </summary>
        public void ClosePosition()
        {
            string tag = TagTrade + "Close";
            SetHoldings(MySymbol, 0m, false, tag);
            IEnumerable<Order> orders = Transactions.GetOrders(
                item => item.Symbol == MySymbol &&
                        item.Tag == tag
            );

            if (orders.Count() == 1)
            {
                Order order = orders.First();
                int id = order.Id;

                if (LiveMode)
                {
                    Transactions.WaitForOrder(id);
                }

                if (order.Status == OrderStatus.Filled)
                {
                    MyLogger.InfoPositionClose(order.Price, order.Quantity, QuoteCurrency, QuoteBalance);
                }
                else
                {
                    MyLogger.ErrorOnPositionClose(order.Status);
                }
            }
            else
            {
                MyLogger.ErrorInNumberOfOrders(orders);
            }
        }
    }
}

using QuantConnect.Data;
using QuantConnect.Data.Market;
using System;
using System.Globalization;

namespace QuantConnect
{
    public class Bitcoin : TradeBar
    {
        //Set the defaults:
        public decimal VolumeBTC = 0;
        //public decimal VolumeUSD = 0;
        //public decimal WeightedPrice = 0;

        /// <summary>
        /// 1. DEFAULT CONSTRUCTOR: Custom data types need a default constructor.
        /// We search for a default constructor so please provide one here. It won't be used for data, just to generate the "Factory".
        /// </summary>
        public Bitcoin()
        {
            this.Symbol = "BTCUSD";
            // this is the missing secret sauce
            // tradebar sets this to TradeBar which causes the data to get piped elsewhere
            this.DataType = MarketDataType.Base;
        }

        /// <summary>
        /// 2. RETURN THE STRING URL SOURCE LOCATION FOR YOUR DATA:
        /// This is a powerful and dynamic select source file method. If you have a large dataset, 10+mb we recommend you break it into smaller files. E.g. One zip per year.
        /// We can accept raw text or ZIP files. We read the file extension to determine if it is a zip file.
        /// </summary>
        /// <param name="config">Subscription data, symbol name, data type</param>
        /// <param name="date">Current date we're requesting. This allows you to break up the data source into daily files.</param>
        /// <param name="datafeed">Datafeed type: Backtesting or the Live data broker who will provide live data. You can specify a different source for live trading! </param>
        /// <returns>string URL end point.</returns>
        public override SubscriptionDataSource GetSource(SubscriptionDataConfig config, DateTime date, bool isLiveMode)
        {
            if (isLiveMode)
            {
                throw new NotImplementedException("Define a REST endpoint for live data.");
                //return new SubscriptionDataSource("https://www.bitstamp.net/api/ticker/", SubscriptionTransportMedium.Rest);
            }

            return new SubscriptionDataSource("https://s3.us-east-2.amazonaws.com/fulldata.bitstampedited/bitstamp2018_edited.csv", SubscriptionTransportMedium.RemoteFile);
        }

        /// <summary>
        /// 3. READER METHOD: Read 1 line from data source and convert it into Object.
        /// Each line of the CSV File is presented in here. The backend downloads your file, loads it into memory and then line by line
        /// feeds it into your algorithm
        /// </summary>
        /// <param name="line">string line from the data source file submitted above</param>
        /// <param name="config">Subscription data, symbol name, data type</param>
        /// <param name="date">Current date we're requesting. This allows you to break up the data source into daily files.</param>
        /// <param name="datafeed">Datafeed type - Backtesting or LiveTrading</param>
        /// <returns>New Bitcoin Object which extends BaseData.</returns>
        public override BaseData Reader(SubscriptionDataConfig config, string line, DateTime date, bool isLiveMode)
        {
            //New Bitcoin object
            Bitcoin coin = new Bitcoin();

            try
            {
                string[] data = line.Split(',');
                coin.Time = DateTime.Parse(data[0] + " " + data[1], CultureInfo.InvariantCulture);
                coin.Open = Convert.ToDecimal(data[2], CultureInfo.InvariantCulture);
                coin.High = Convert.ToDecimal(data[3], CultureInfo.InvariantCulture);
                coin.Low = Convert.ToDecimal(data[4], CultureInfo.InvariantCulture);
                coin.Close = Convert.ToDecimal(data[5], CultureInfo.InvariantCulture);
                coin.VolumeBTC = Convert.ToDecimal(data[6], CultureInfo.InvariantCulture);
                coin.Symbol = config.Symbol;
                coin.Value = coin.Close;
            }
            catch { /* Do nothing, skip first title row */ }

            return coin;
        }
    }
}
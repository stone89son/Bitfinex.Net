﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bitfinex.Net.Converters;
using Bitfinex.Net.Objects;
using Bitfinex.Net.Objects.RestV1Objects;
using CryptoExchange.Net;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bitfinex.Net
{
    public class BitfinexClient : ExchangeClient
    {
        #region fields

        private static BitfinexClientOptions defaultOptions = new BitfinexClientOptions();

        private const string GetMethod = "GET";
        private const string PostMethod = "POST";

        private string baseAddress;
        private const string ApiVersion1 = "1";
        private const string ApiVersion2 = "2";

        private const string StatusEndpoint = "platform/status";
        private const string TickersEndpoint = "tickers";
        private const string TradesEndpoint = "trades/{}/hist";
        private const string OrderBookEndpoint = "book/{}/{}";
        private const string StatsEndpoint = "stats1/{}:1m:{}:{}/{}";
        private const string LastCandleEndpoint = "candles/trade:{}:{}/last";
        private const string CandlesEndpoint = "candles/trade:{}:{}/hist";
        private const string MarketAverageEndpoint = "calc/trade/avg";

        private const string WalletsEndpoint = "auth/r/wallets";
        private const string OpenOrdersEndpoint = "auth/r/orders";
        private const string OrderHistoryEndpoint = "auth/r/orders/{}/hist";
        private const string OrderTradesEndpoint = "auth/r/order/{}:{}/trades";
        private const string MyTradesEndpoint = "auth/r/trades/{}/hist";

        private const string ActivePositionsEndpoint = "auth/r/positions";
        private const string ActiveFundingOffersEndpoint = "auth/r/funding/offers/{}";
        private const string FundingOfferHistoryEndpoint = "auth/r/funding/offers/{}/hist";
        private const string FundingLoansEndpoint = "auth/r/funding/loans/{}";
        private const string FundingLoansHistoryEndpoint = "auth/r/funding/loans/{}/hist";
        private const string FundingCreditsEndpoint = "auth/r/funding/credits/{}";
        private const string FundingCreditsHistoryEndpoint = "auth/r/funding/credits/{}/hist";
        private const string FundingTradesEndpoint = "auth/r/funding/trades/{}/hist";
        private const string MaginInfoBaseEndpoint = "auth/r/info/margin/base";
        private const string MaginInfoSymbolEndpoint = "auth/r/info/margin/{}";
        private const string FundingInfoEndpoint = "auth/r/info/funding/{}";

        private const string MovementsEndpoint = "auth/r/movements/{}/hist";
        private const string DailyPerformanceEndpoint = "auth/r/stats/perf:1D/hist";

        private const string AlertListEndpoint = "auth/r/alerts";
        private const string SetAlertEndpoint = "auth/w/alert/set";
        private const string DeleteAlertEndpoint = "auth/w/alert/price:{}:{}/del";

        private const string AccountInfoEndpoint = "account_infos";
        private const string WithdrawalFeeEndpoint = "account_fees";
        private const string PlaceOrderEndpoint = "order/new";
        private const string CancelOrderEndpoint = "order/cancel";
        private const string CancelAllOrderEndpoint = "order/cancel/all";
        private const string OrderStatusEndpoint = "order/status";


        private string nonce => Math.Round((DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds * 10).ToString(CultureInfo.InvariantCulture);
        #endregion

        #region constructor/destructor
        /// <summary>
        /// Create a new instance of BinanceClient using the default options
        /// </summary>
        public BitfinexClient(): this(defaultOptions)
        {
        }

        /// <summary>
        /// Create a new instance of BinanceClient using provided options
        /// </summary>
        /// <param name="options">The options to use for this client</param>
        public BitfinexClient(BitfinexClientOptions options) : base(options, options.ApiCredentials == null ? null : new BitfinexAuthenticationProvider(options.ApiCredentials))
        {
            Configure(options);
        }
        #endregion

        #region methods
        /// <summary>
        /// Sets the default options to use for new clients
        /// </summary>
        /// <param name="options">The options to use for new clients</param>
        public static void SetDefaultOptions(BitfinexClientOptions options)
        {
            defaultOptions = options;
        }

        /// <summary>
        /// Set the API key and secret
        /// </summary>
        /// <param name="apiKey">The api key</param>
        /// <param name="apiSecret">The api secret</param>
        public void SetApiCredentials(string apiKey, string apiSecret)
        {
            SetAuthenticationProvider(new BitfinexAuthenticationProvider(new ApiCredentials(apiKey, apiSecret)));
        }

        #region Version2
        /// <summary>
        /// Synchronized version of the <see cref="GetPlatformStatusAsync"/> method
        /// </summary>
        /// <returns></returns>
        public CallResult<BitfinexPlatformStatus> GetPlatformStatus() => GetPlatformStatusAsync().Result;

        /// <summary>
        /// Gets the platform status
        /// </summary>
        /// <returns>Whether Bitfinex platform is running normally or not</returns>
        public async Task<CallResult<BitfinexPlatformStatus>> GetPlatformStatusAsync()
        {
            return await ExecuteRequest<BitfinexPlatformStatus>(GetUrl(StatusEndpoint, ApiVersion2));
        }

        /// <summary>
        /// Synchronized version of the <see cref="GetTickerAsync"/> method
        /// </summary>
        /// <returns></returns>
        public CallResult<BitfinexMarketOverview[]> GetTicker(params string[] symbols) => GetTickerAsync(symbols).Result;

        /// <summary>
        /// Returns basic market data for the provided smbols
        /// </summary>
        /// <param name="symbols">The symbols to get data for</param>
        /// <returns>Market data</returns>
        public async Task<CallResult<BitfinexMarketOverview[]>> GetTickerAsync(params string[] symbols)
        {
            var parameters = new Dictionary<string, object>()
            {
                {"symbols", string.Join(",", symbols)}
            };

            return await ExecuteRequest<BitfinexMarketOverview[]>(GetUrl(TickersEndpoint, ApiVersion2), GetMethod, parameters);
        }

        /// <summary>
        /// Synchronized version of the <see cref="GetTradesAsync"/> method
        /// </summary>
        /// <returns></returns>
        public CallResult<BitfinexTradeSimple[]> GetTrades(string symbol, int? limit = null, DateTime? startTime = null, DateTime? endTime = null, Sorting? sorting = null) => GetTradesAsync(symbol, limit, startTime, endTime, sorting).Result;

        /// <summary>
        /// Get recent trades for a symbol
        /// </summary>
        /// <param name="symbol">The symbol to get trades for</param>
        /// <param name="limit">The amount of results</param>
        /// <param name="startTime">The starttime to return trades for</param>
        /// <param name="endTime">The endtime to return trades for</param>
        /// <param name="sorting">The way the result is sorted</param>
        /// <returns>Trades for the symbol</returns>
        public async Task<CallResult<BitfinexTradeSimple[]>> GetTradesAsync(string symbol, int? limit = null, DateTime? startTime = null, DateTime? endTime = null, Sorting? sorting = null)
        {
            var parameters = new Dictionary<string, object>();
            parameters.AddOptionalParameter("limit", limit?.ToString());
            parameters.AddOptionalParameter("start", startTime != null ? JsonConvert.SerializeObject(startTime, new TimestampConverter(false)) : null);
            parameters.AddOptionalParameter("end", endTime != null ? JsonConvert.SerializeObject(endTime, new TimestampConverter(false)) : null);
            parameters.AddOptionalParameter("sort", sorting != null ? JsonConvert.SerializeObject(sorting, new SortingConverter(false)) : null);

            return await ExecuteRequest<BitfinexTradeSimple[]>(GetUrl(FillPathParameter(TradesEndpoint, symbol), ApiVersion2), GetMethod, parameters);
        }

        /// <summary>
        /// Synchronized version of the <see cref="GetOrderBookAsync"/> method
        /// </summary>
        /// <returns></returns>
        public CallResult<BitfinexOrderBookEntry[]> GetOrderBook(string symbol, Precision precision, int? limit = null) => GetOrderBookAsync(symbol, precision, limit).Result;

        /// <summary>
        /// Gets the orderbook for a symbol
        /// </summary>
        /// <param name="symbol">The symbol to get the order book for</param>
        /// <param name="precision">The precision of the data</param>
        /// <param name="limit">The amount of results in the book</param>
        /// <returns>The orderbook for the symbol</returns>
        public async Task<CallResult<BitfinexOrderBookEntry[]>> GetOrderBookAsync(string symbol, Precision precision, int? limit = null)
        {
            if (limit != null && (limit != 25 && limit != 100))
                return new CallResult<BitfinexOrderBookEntry[]>(null, new ArgumentError("Limit should be either 25 or 100"));

            var parameters = new Dictionary<string, object>();
            parameters.AddOptionalParameter("len", limit?.ToString());

            return await ExecuteRequest<BitfinexOrderBookEntry[]>(GetUrl(FillPathParameter(OrderBookEndpoint, symbol, precision.ToString()), ApiVersion2), GetMethod, parameters);
        }

        /// <summary>
        /// Synchronized version of the <see cref="GetStatsAsync"/> method
        /// </summary>
        /// <returns></returns>
        public CallResult<BitfinexStats> GetStats(string symbol, StatKey key, StatSide side, StatSection section, Sorting? sorting = null) => GetStatsAsync(symbol, key, side, section, sorting).Result;

        /// <summary>
        /// Get various stats for the symbol
        /// </summary>
        /// <param name="symbol">The symbol to request stats for</param>
        /// <param name="key">The type of stats</param>
        /// <param name="side">Side of the stats</param>
        /// <param name="section">Section of the stats</param>
        /// <param name="sorting">The way the result should be sorted</param>
        /// <returns></returns>
        public async Task<CallResult<BitfinexStats>> GetStatsAsync(string symbol, StatKey key, StatSide side, StatSection section, Sorting? sorting)
        {
            var parameters = new Dictionary<string, object>();
            parameters.AddOptionalParameter("sort", sorting != null ? JsonConvert.SerializeObject(sorting, new SortingConverter(false)) : null);

            var endpoint = FillPathParameter(StatsEndpoint,
                JsonConvert.SerializeObject(key, new StatKeyConverter(false)),
                symbol,
                JsonConvert.SerializeObject(side, new StatSideConverter(false)),
                JsonConvert.SerializeObject(section, new StatSectionConverter(false)));

            return await ExecuteRequest<BitfinexStats>(GetUrl(endpoint, ApiVersion2), GetMethod, parameters);
        }

        /// <summary>
        /// Synchronized version of the <see cref="GetLastCandleAsync"/> method
        /// </summary>
        /// <returns></returns>
        public CallResult<BitfinexCandle> GetLastCandle(TimeFrame timeFrame, string symbol)
            => GetLastCandleAsync(timeFrame, symbol).Result;

        /// <summary>
        /// Get the last candle for a symbol
        /// </summary>
        /// <param name="timeFrame">The timeframe of the candle</param>
        /// <param name="symbol">The symbol to get the candle for</param>
        /// <returns>The last candle for the symbol</returns>
        public async Task<CallResult<BitfinexCandle>> GetLastCandleAsync(TimeFrame timeFrame, string symbol)
        {
            var endpoint = FillPathParameter(LastCandleEndpoint, JsonConvert.SerializeObject(timeFrame, new TimeFrameConverter(false)), symbol);

            return await ExecuteRequest<BitfinexCandle>(GetUrl(endpoint, ApiVersion2));
        }

        /// <summary>
        /// Synchronized version of the <see cref="GetCandlesAsync"/> method
        /// </summary>
        /// <returns></returns>
        public CallResult<BitfinexCandle[]> GetCandles(TimeFrame timeFrame, string symbol, int? limit = null, DateTime? startTime = null, DateTime? endTime = null, Sorting? sorting = null) 
            => GetCandlesAsync(timeFrame, symbol, limit, startTime, endTime, sorting).Result;

        /// <summary>
        /// Gets candles for a symbol
        /// </summary>
        /// <param name="timeFrame">The timeframe of the candles</param>
        /// <param name="symbol">The symbol to get the candles for</param>
        /// <param name="limit">The amount of results</param>
        /// <param name="startTime">The start time of the candles</param>
        /// <param name="endTime">The end time of the candles</param>
        /// <param name="sorting">The way the result is sorted</param>
        /// <returns></returns>
        public async Task<CallResult<BitfinexCandle[]>> GetCandlesAsync(TimeFrame timeFrame, string symbol,int? limit = null, DateTime? startTime = null, DateTime? endTime = null, Sorting? sorting = null)
        {
            var parameters = new Dictionary<string, object>();
            parameters.AddOptionalParameter("len", limit?.ToString());
            parameters.AddOptionalParameter("start", startTime != null ? JsonConvert.SerializeObject(startTime, new TimestampConverter(false)) : null);
            parameters.AddOptionalParameter("end", endTime != null ? JsonConvert.SerializeObject(endTime, new TimestampConverter(false)) : null);
            parameters.AddOptionalParameter("sort", sorting != null ? JsonConvert.SerializeObject(sorting, new SortingConverter(false)) : null);

            var endpoint = FillPathParameter(CandlesEndpoint,
                JsonConvert.SerializeObject(timeFrame, new TimeFrameConverter(false)),
                symbol);

            return await ExecuteRequest<BitfinexCandle[]>(GetUrl(endpoint, ApiVersion2), GetMethod, parameters);
        }

        /// <summary>
        /// Synchronized version of the <see cref="GetMarketAveragePriceAsync"/> method
        /// </summary>
        /// <returns></returns>
        public CallResult<BitfinexMarketAveragePrice> GetMarketAveragePrice(string symbol, decimal amount, decimal rateLimit, int? period = null) => GetMarketAveragePriceAsync(symbol, amount, rateLimit, period).Result;
        
        /// <summary>
        /// Calculate the average execution price
        /// </summary>
        /// <param name="symbol">The symbol to calculate for</param>
        /// <param name="amount">The amount to execute</param>
        /// <param name="rateLimit">Limit to price</param>
        /// <param name="period">Maximum period for margin funding</param>
        /// <returns>The average price at which the execution would happen</returns>
        public async Task<CallResult<BitfinexMarketAveragePrice>> GetMarketAveragePriceAsync(string symbol, decimal amount, decimal? rateLimit = null, int? period = null)
        {
            var parameters = new Dictionary<string, object>()
            {
                { "symbol", symbol },
                { "amount", amount.ToString(CultureInfo.InvariantCulture) },
            };
            parameters.AddOptionalParameter("period", period?.ToString());
            parameters.AddOptionalParameter("rate_limit", rateLimit?.ToString(CultureInfo.InvariantCulture));

            return await ExecuteRequest<BitfinexMarketAveragePrice>(GetUrl(MarketAverageEndpoint, ApiVersion2), PostMethod, parameters);
        }

        /// <summary>
        /// Synchronized version of the <see cref="GetWalletsAsync"/> method
        /// </summary>
        /// <returns></returns>
        public CallResult<BitfinexWallet[]> GetWallets() => GetWalletsAsync().Result;

        /// <summary>
        /// Get all funds
        /// </summary>
        /// <returns></returns>
        public async Task<CallResult<BitfinexWallet[]>> GetWalletsAsync()
        {
            return await ExecuteRequest<BitfinexWallet[]>(GetUrl(WalletsEndpoint, ApiVersion2), PostMethod, null, true);
        }

        /// <summary>
        /// Synchronized version of the <see cref="GetActiveOrdersAsync"/> method
        /// </summary>
        /// <returns></returns>
        public CallResult<BitfinexOrder[]> GetActiveOrders() => GetActiveOrdersAsync().Result;

        /// <summary>
        /// Get the active orders
        /// </summary>
        /// <returns></returns>
        public async Task<CallResult<BitfinexOrder[]>> GetActiveOrdersAsync()
        {
            return await ExecuteRequest<BitfinexOrder[]>(GetUrl(OpenOrdersEndpoint, ApiVersion2), PostMethod, null, true);
        }

        /// <summary>
        /// Synchronized version of the <see cref="GetOrderHistoryAsync"/> method
        /// </summary>
        /// <returns></returns>
        public CallResult<BitfinexOrder[]> GetOrderHistory(string symbol, DateTime? startTime = null, DateTime? endTime = null, int? limit = null) => GetOrderHistoryAsync(symbol, startTime, endTime, limit).Result;

        /// <summary>
        /// Get the order history for a symbol for this account
        /// </summary>
        /// <param name="symbol">The symbol to get the history for</param>
        /// <param name="startTime">Start time of the data to return</param>
        /// <param name="endTime">End time of the data to return</param>
        /// <param name="limit">Max amount of results</param>
        /// <returns></returns>
        public async Task<CallResult<BitfinexOrder[]>> GetOrderHistoryAsync(string symbol, DateTime? startTime = null, DateTime? endTime = null, int? limit = null)
        {
            var parameters = new Dictionary<string, object>();
            parameters.AddOptionalParameter("len", limit?.ToString());
            parameters.AddOptionalParameter("start", startTime != null ? JsonConvert.SerializeObject(startTime, new TimestampConverter(false)) : null);
            parameters.AddOptionalParameter("end", endTime != null ? JsonConvert.SerializeObject(endTime, new TimestampConverter(false)) : null);

            return await ExecuteRequest<BitfinexOrder[]>(GetUrl(FillPathParameter(OrderHistoryEndpoint, symbol), ApiVersion2), PostMethod, parameters, true);
        }

        /// <summary>
        /// Synchronized version of the <see cref="GetTradesForOrderAsync"/> method
        /// </summary>
        /// <returns></returns>
        public CallResult<BitfinexTradeDetails[]> GetTradesForOrder(string symbol, long orderId) => GetTradesForOrderAsync(symbol, orderId).Result;

        /// <summary>
        /// Get the individual trades for an order
        /// </summary>
        /// <param name="symbol">The symbol of the order</param>
        /// <param name="orderId">The order Id</param>
        /// <returns></returns>
        public async Task<CallResult<BitfinexTradeDetails[]>> GetTradesForOrderAsync(string symbol, long orderId)
        {
            return await ExecuteRequest<BitfinexTradeDetails[]>(GetUrl(FillPathParameter(OrderTradesEndpoint, symbol, orderId.ToString()), ApiVersion2), PostMethod, null, true);
        }

        /// <summary>
        /// Synchronized version of the <see cref="GetTradeHistoryAsync"/> method
        /// </summary>
        /// <returns></returns>
        public CallResult<BitfinexTradeDetails[]> GetTradeHistory(string symbol, DateTime? startTime = null, DateTime? endTime = null, int? limit = null) => GetTradeHistoryAsync(symbol, startTime, endTime, limit).Result;

        /// <summary>
        /// Get the trade history for a symbol
        /// </summary>
        /// <param name="symbol">The symbol to get history for</param>
        /// <param name="startTime">Start time of the data to return</param>
        /// <param name="endTime">End time of the data to return</param>
        /// <param name="limit">Max amount of results</param>
        /// <returns></returns>
        public async Task<CallResult<BitfinexTradeDetails[]>> GetTradeHistoryAsync(string symbol, DateTime? startTime = null, DateTime? endTime = null, int? limit = null)
        {
            var parameters = new Dictionary<string, object>();
            parameters.AddOptionalParameter("len", limit?.ToString());
            parameters.AddOptionalParameter("start", startTime != null ? JsonConvert.SerializeObject(startTime, new TimestampConverter(false)) : null);
            parameters.AddOptionalParameter("end", endTime != null ? JsonConvert.SerializeObject(endTime, new TimestampConverter(false)) : null);

            return await ExecuteRequest<BitfinexTradeDetails[]>(GetUrl(FillPathParameter(MyTradesEndpoint, symbol), ApiVersion2), PostMethod, parameters, true);
        }

        /// <summary>
        /// Synchronized version of the <see cref="GetActivePositionsAsync"/> method
        /// </summary>
        /// <returns></returns>
        public CallResult<BitfinexPosition[]> GetActivePositions() => GetActivePositionsAsync().Result;

        /// <summary>
        /// Get the active positions
        /// </summary>
        /// <returns></returns>
        public async Task<CallResult<BitfinexPosition[]>> GetActivePositionsAsync()
        {
            return await ExecuteRequest<BitfinexPosition[]>(GetUrl(ActivePositionsEndpoint, ApiVersion2), PostMethod, null, true);
        }

        /// <summary>
        /// Synchronized version of the <see cref="GetActiveFundingOffersAsync"/> method
        /// </summary>
        /// <returns></returns>
        public CallResult<BitfinexFundingOffer[]> GetActiveFundingOffers(string symbol) => GetActiveFundingOffersAsync(symbol).Result;

        /// <summary>
        /// Get the active funding offers
        /// </summary>
        /// <param name="symbol">The symbol to return the funding offer for</param>
        /// <returns></returns>
        public async Task<CallResult<BitfinexFundingOffer[]>> GetActiveFundingOffersAsync(string symbol)
        {
            return await ExecuteRequest<BitfinexFundingOffer[]>(GetUrl(FillPathParameter(ActiveFundingOffersEndpoint, symbol), ApiVersion2), PostMethod, null, true);
        }

        /// <summary>
        /// Synchronized version of the <see cref="GetFundingOfferHistoryAsync"/> method
        /// </summary>
        /// <returns></returns>
        public CallResult<BitfinexFundingOffer[]> GetFundingOfferHistory(string symbol, DateTime? startTime = null, DateTime? endTime = null, int? limit = null) => GetFundingOfferHistoryAsync(symbol, startTime, endTime, limit).Result;
        
        /// <summary>
        /// Get the funding offer history
        /// </summary>
        /// <param name="symbol">The symbol to get history for</param>
        /// <param name="startTime">Start time of the data to return</param>
        /// <param name="endTime">End time of the data to return</param>
        /// <param name="limit">Max amount of results</param>
        /// <returns></returns>
        public async Task<CallResult<BitfinexFundingOffer[]>> GetFundingOfferHistoryAsync(string symbol, DateTime? startTime = null, DateTime? endTime = null, int? limit = null)
        {
            var parameters = new Dictionary<string, object>();
            parameters.AddOptionalParameter("len", limit?.ToString());
            parameters.AddOptionalParameter("start", startTime != null ? JsonConvert.SerializeObject(startTime, new TimestampConverter(false)) : null);
            parameters.AddOptionalParameter("end", endTime != null ? JsonConvert.SerializeObject(endTime, new TimestampConverter(false)) : null);

            return await ExecuteRequest<BitfinexFundingOffer[]>(GetUrl(FillPathParameter(FundingOfferHistoryEndpoint, symbol), ApiVersion2), PostMethod, parameters, true);
        }

        /// <summary>
        /// Synchronized version of the <see cref="GetFundingLoansAsync"/> method
        /// </summary>
        /// <returns></returns>
        public CallResult<BitfinexFundingLoan[]> GetFundingLoans(string symbol) => GetFundingLoansAsync(symbol).Result;

        /// <summary>
        /// Get the funding loans
        /// </summary>
        /// <param name="symbol">The symbol to get the funding loans for</param>
        /// <returns></returns>
        public async Task<CallResult<BitfinexFundingLoan[]>> GetFundingLoansAsync(string symbol)
        {
            return await ExecuteRequest<BitfinexFundingLoan[]>(GetUrl(FillPathParameter(FundingLoansEndpoint, symbol), ApiVersion2), PostMethod, null, true);
        }

        /// <summary>
        /// Synchronized version of the <see cref="GetFundingLoansHistoryAsync"/> method
        /// </summary>
        /// <returns></returns>
        public CallResult<BitfinexFundingLoan[]> GetFundingLoansHistory(string symbol, DateTime? startTime = null, DateTime? endTime = null, int? limit = null) => GetFundingLoansHistoryAsync(symbol, startTime, endTime, limit).Result;

        /// <summary>
        /// Get the funding loan history
        /// </summary>
        /// <param name="symbol">The symbol to get history for</param>
        /// <param name="startTime">Start time of the data to return</param>
        /// <param name="endTime">End time of the data to return</param>
        /// <param name="limit">Max amount of results</param>
        /// <returns></returns>
        public async Task<CallResult<BitfinexFundingLoan[]>> GetFundingLoansHistoryAsync(string symbol, DateTime? startTime = null, DateTime? endTime = null, int? limit = null)
        {
            var parameters = new Dictionary<string, object>();
            parameters.AddOptionalParameter("len", limit?.ToString());
            parameters.AddOptionalParameter("start", startTime != null ? JsonConvert.SerializeObject(startTime, new TimestampConverter(false)) : null);
            parameters.AddOptionalParameter("end", endTime != null ? JsonConvert.SerializeObject(endTime, new TimestampConverter(false)) : null);

            return await ExecuteRequest<BitfinexFundingLoan[]>(GetUrl(FillPathParameter(FundingLoansHistoryEndpoint, symbol), ApiVersion2), PostMethod, parameters, true);
        }

        /// <summary>
        /// Synchronized version of the <see cref="GetFundingCreditsAsync"/> method
        /// </summary>
        /// <returns></returns>
        public CallResult<BitfinexFundingCredit[]> GetFundingCredits(string symbol) => GetFundingCreditsAsync(symbol).Result;

        /// <summary>
        /// Get the funding credits
        /// </summary>
        /// <param name="symbol">The symbol to get the funding credits for</param>
        /// <returns></returns>
        public async Task<CallResult<BitfinexFundingCredit[]>> GetFundingCreditsAsync(string symbol)
        {
            return await ExecuteRequest<BitfinexFundingCredit[]>(GetUrl(FillPathParameter(FundingCreditsEndpoint, symbol), ApiVersion2), PostMethod, null, true);
        }

        /// <summary>
        /// Synchronized version of the <see cref="GetFundingCreditsHistoryAsyncTask"/> method
        /// </summary>
        /// <returns></returns>
        public CallResult<BitfinexFundingCredit[]> GetFundingCreditsHistory(string symbol, DateTime? startTime = null, DateTime? endTime = null, int? limit = null) => GetFundingCreditsHistoryAsyncTask(symbol, startTime, endTime, limit).Result;

        /// <summary>
        /// Get the funding credits history
        /// </summary>
        /// <param name="symbol">The symbol to get history for</param>
        /// <param name="startTime">Start time of the data to return</param>
        /// <param name="endTime">End time of the data to return</param>
        /// <param name="limit">Max amount of results</param>
        /// <returns></returns>
        public async Task<CallResult<BitfinexFundingCredit[]>> GetFundingCreditsHistoryAsyncTask(string symbol, DateTime? startTime = null, DateTime? endTime = null, int? limit = null)
        {
            var parameters = new Dictionary<string, object>();
            parameters.AddOptionalParameter("len", limit?.ToString());
            parameters.AddOptionalParameter("start", startTime != null ? JsonConvert.SerializeObject(startTime, new TimestampConverter(false)) : null);
            parameters.AddOptionalParameter("end", endTime != null ? JsonConvert.SerializeObject(endTime, new TimestampConverter(false)) : null);

            return await ExecuteRequest<BitfinexFundingCredit[]>(GetUrl(FillPathParameter(FundingCreditsHistoryEndpoint, symbol), ApiVersion2), PostMethod, parameters, true);
        }

        /// <summary>
        /// Synchronized version of the <see cref="GetFundingTradesHistoryAsync"/> method
        /// </summary>
        /// <returns></returns>
        public CallResult<BitfinexFundingTrade[]> GetFundingTradesHistory(string symbol, DateTime? startTime = null, DateTime? endTime = null, int? limit = null) => GetFundingTradesHistoryAsync(symbol, startTime, endTime, limit).Result;

        /// <summary>
        /// Get the funding trades history
        /// </summary>
        /// <param name="symbol">The symbol to get history for</param>
        /// <param name="startTime">Start time of the data to return</param>
        /// <param name="endTime">End time of the data to return</param>
        /// <param name="limit">Max amount of results</param>
        /// <returns></returns>
        public async Task<CallResult<BitfinexFundingTrade[]>> GetFundingTradesHistoryAsync(string symbol, DateTime? startTime = null, DateTime? endTime = null, int? limit = null)
        {
            var parameters = new Dictionary<string, object>();
            parameters.AddOptionalParameter("len", limit?.ToString());
            parameters.AddOptionalParameter("start", startTime != null ? JsonConvert.SerializeObject(startTime, new TimestampConverter(false)) : null);
            parameters.AddOptionalParameter("end", endTime != null ? JsonConvert.SerializeObject(endTime, new TimestampConverter(false)) : null);

            return await ExecuteRequest<BitfinexFundingTrade[]>(GetUrl(FillPathParameter(FundingTradesEndpoint, symbol), ApiVersion2), PostMethod);
        }

        /// <summary>
        /// Synchronized version of the <see cref="GetBaseMarginInfoAsync"/> method
        /// </summary>
        /// <returns></returns>
        public CallResult<BitfinexMarginBase> GetBaseMarginInfo() => GetBaseMarginInfoAsync().Result;

        /// <summary>
        /// Get the base margin info
        /// </summary>
        /// <returns></returns>
        public async Task<CallResult<BitfinexMarginBase>> GetBaseMarginInfoAsync()
        {
            return await ExecuteRequest<BitfinexMarginBase>(GetUrl(MaginInfoBaseEndpoint, ApiVersion2), PostMethod, null, true);
        }

        /// <summary>
        /// Synchronized version of the <see cref="GetSymbolMarginInfoAsync"/> method
        /// </summary>
        /// <returns></returns>
        public CallResult<BitfinexMarginSymbol> GetSymbolMarginInfo(string symbol) => GetSymbolMarginInfoAsync(symbol).Result;

        /// <summary>
        /// Get the margin info for a symbol
        /// </summary>
        /// <param name="symbol">The symbol to get the info for</param>
        /// <returns></returns>
        public async Task<CallResult<BitfinexMarginSymbol>> GetSymbolMarginInfoAsync(string symbol)
        {
            return await ExecuteRequest<BitfinexMarginSymbol>(GetUrl(FillPathParameter(MaginInfoSymbolEndpoint, symbol), ApiVersion2), PostMethod, null, true);
        }

        /// <summary>
        /// Synchronized version of the <see cref="GetFundingInfoAsync"/> method
        /// </summary>
        /// <returns></returns>
        public CallResult<BitfinexFundingInfo> GetFundingInfo(string symbol) => GetFundingInfoAsync(symbol).Result;

        /// <summary>
        /// Get funding info for a symbol
        /// </summary>
        /// <param name="symbol">The symbol to get the info for</param>
        /// <returns></returns>
        public async Task<CallResult<BitfinexFundingInfo>> GetFundingInfoAsync(string symbol)
        {
            return await ExecuteRequest<BitfinexFundingInfo>(GetUrl(FillPathParameter(FundingInfoEndpoint, symbol), ApiVersion2), PostMethod, null, true);
        }

        /// <summary>
        /// Synchronized version of the <see cref="GetMovementsAsync"/> method
        /// </summary>
        /// <returns></returns>
        public CallResult<BitfinexMovement[]> GetMovements(string symbol) => GetMovementsAsync(symbol).Result;

        /// <summary>
        /// Get the withdrawal/deposit history
        /// </summary>
        /// <param name="symbol">Symbol to get history for</param>
        /// <returns></returns>
        public async Task<CallResult<BitfinexMovement[]>> GetMovementsAsync(string symbol)
        {
            return await ExecuteRequest<BitfinexMovement[]>(GetUrl(FillPathParameter(MovementsEndpoint, symbol), ApiVersion2), PostMethod, null, true);
        }
        
        public CallResult<BitfinexPerformance> GetDailyPerformance() => GetDailyPerformanceAsync().Result;
        public async Task<CallResult<BitfinexPerformance>> GetDailyPerformanceAsync()
        {
            // TODO doesn't work?
            return await ExecuteRequest<BitfinexPerformance>(GetUrl(DailyPerformanceEndpoint, ApiVersion2), PostMethod, null, true);
        }

        /// <summary>
        /// Synchronized version of the <see cref="GetAlertListAsync"/> method
        /// </summary>
        /// <returns></returns>
        public CallResult<BitfinexAlert[]> GetAlertList() => GetAlertListAsync().Result;
        
        /// <summary>
        /// Get the list of alerts
        /// </summary>
        /// <returns></returns>
        public async Task<CallResult<BitfinexAlert[]>> GetAlertListAsync()
        {
            var parameters = new Dictionary<string, object>()
            {
                { "type", "price" } 
            };

            return await ExecuteRequest<BitfinexAlert[]>(GetUrl(AlertListEndpoint, ApiVersion2), PostMethod, parameters, true);
        }

        /// <summary>
        /// Synchronized version of the <see cref="SetAlertAsync"/> method
        /// </summary>
        /// <returns></returns>
        public CallResult<BitfinexAlert> SetAlert(string symbol, decimal price) => SetAlertAsync(symbol, price).Result;

        /// <summary>
        /// Set an alert
        /// </summary>
        /// <param name="symbol">The symbol to set the alert for</param>
        /// <param name="price">The price to set the alert for</param>
        /// <returns></returns>
        public async Task<CallResult<BitfinexAlert>> SetAlertAsync(string symbol, decimal price)
        {
            var parameters = new Dictionary<string, object>()
            {
                { "type", "price" },
                { "symbol", symbol },
                { "price", price.ToString(CultureInfo.InvariantCulture) }
            };

            return await ExecuteRequest<BitfinexAlert>(GetUrl(SetAlertEndpoint, ApiVersion2), PostMethod, parameters, true);
        }

        /// <summary>
        /// Synchronized version of the <see cref="DeleteAlertAsync"/> method
        /// </summary>
        /// <returns></returns>
        public CallResult<BitfinexSuccessResult> DeleteAlert(string symbol, decimal price) => DeleteAlertAsync(symbol, price).Result;

        /// <summary>
        /// Delete an existing alert
        /// </summary>
        /// <param name="symbol">The symbol of the alert to delete</param>
        /// <param name="price">The price of the alert to delete</param>
        /// <returns></returns>
        public async Task<CallResult<BitfinexSuccessResult>> DeleteAlertAsync(string symbol, decimal price)
        {
            return await ExecuteRequest<BitfinexSuccessResult>(GetUrl(FillPathParameter(DeleteAlertEndpoint, symbol, price.ToString(CultureInfo.InvariantCulture)), ApiVersion2), PostMethod, null, true);
        }
        #endregion

        #region Version1
        /// <summary>
        /// Synchronized version of the <see cref="GetAccountInfoAsync"/> method
        /// </summary>
        /// <returns></returns>
        public CallResult<BitfinexAccountInfo> GetAccountInfo() => GetAccountInfoAsync().Result;

        /// <summary>
        /// Get information about your account
        /// </summary>
        /// <returns></returns>
        public async Task<CallResult<BitfinexAccountInfo>> GetAccountInfoAsync()
        {
            var result = await ExecuteRequest<BitfinexAccountInfo[]>(GetUrl(AccountInfoEndpoint, ApiVersion1), PostMethod, null, true);
            return result.Success ? new CallResult<BitfinexAccountInfo>(result.Data[0], null) : new CallResult<BitfinexAccountInfo>(null, result.Error);
        }

        /// <summary>
        /// Synchronized version of the <see cref="GetWithdrawalFeesAsync"/> method
        /// </summary>
        /// <returns></returns>
        public CallResult<BitfinexWithdrawalFees> GetWithdrawalFees() => GetWithdrawalFeesAsync().Result;

        /// <summary>
        /// Get withdrawal fees for this account
        /// </summary>
        /// <returns></returns>
        public async Task<CallResult<BitfinexWithdrawalFees>> GetWithdrawalFeesAsync()
        {
            return await ExecuteRequest<BitfinexWithdrawalFees>(GetUrl(WithdrawalFeeEndpoint, ApiVersion1), PostMethod, null, true);
        }

        /// <summary>
        /// Synchronized version of the <see cref="PlaceOrderAsync"/> method
        /// </summary>
        /// <returns></returns>
        public CallResult<BitfinexPlacedOrder> PlaceOrder(string symbol, OrderSide side, OrderTypeV1 type, decimal amount, decimal price) => PlaceOrderAsync(symbol, side, type, amount, price).Result;

        /// <summary>
        /// Place a new order
        /// </summary>
        /// <param name="symbol">Symbol to place order for</param>
        /// <param name="side">Side of the order</param>
        /// <param name="type">Type of the order</param>
        /// <param name="amount">The amount of the order</param>
        /// <param name="price">The price for the order</param>
        /// <returns></returns>
        public async Task<CallResult<BitfinexPlacedOrder>> PlaceOrderAsync(string symbol, OrderSide side, OrderTypeV1 type, decimal amount, decimal price)
        {
            var parameters = new Dictionary<string, object>()
            {
                { "symbol", symbol },
                { "amount", amount.ToString(CultureInfo.InvariantCulture) },
                { "price", price.ToString(CultureInfo.InvariantCulture) },
                { "exchange", "bitfinex" },
                { "side", JsonConvert.SerializeObject(side, new OrderSideConverter(false)) },
                { "type", JsonConvert.SerializeObject(type, new OrderTypeV1Converter(false)) },
            };
            
            return await ExecuteRequest<BitfinexPlacedOrder>(GetUrl(PlaceOrderEndpoint, ApiVersion1), PostMethod, parameters, true);
        }

        /// <summary>
        /// Synchronized version of the <see cref="CancelOrderAsync"/> method
        /// </summary>
        /// <returns></returns>
        public CallResult<BitfinexPlacedOrder> CancelOrder(long orderId) => CancelOrderAsync(orderId).Result;

        /// <summary>
        /// Cancel a specific order
        /// </summary>
        /// <param name="orderId">The id of the order to cancel</param>
        /// <returns></returns>
        public async Task<CallResult<BitfinexPlacedOrder>> CancelOrderAsync(long orderId)
        {
            var parameters = new Dictionary<string, object>()
            {
                { "order_id", orderId }
            };

            return await ExecuteRequest<BitfinexPlacedOrder>(GetUrl(CancelOrderEndpoint, ApiVersion1), PostMethod, parameters, true);
        }

        /// <summary>
        /// Synchronized version of the <see cref="CancelAllOrdersAsync"/> method
        /// </summary>
        /// <returns></returns>
        public CallResult<BitfinexResult> CancelAllOrders() => CancelAllOrdersAsync().Result;

        /// <summary>
        /// Cancels all open orders
        /// </summary>
        /// <returns></returns>
        public async Task<CallResult<BitfinexResult>> CancelAllOrdersAsync()
        {
            return await ExecuteRequest<BitfinexResult>(GetUrl(CancelAllOrderEndpoint, ApiVersion1), PostMethod, null, true);
        }

        /// <summary>
        /// Synchronized version of the <see cref="GetOrderAsync"/> method
        /// </summary>
        /// <returns></returns>
        public CallResult<BitfinexPlacedOrder> GetOrder(long orderId) => GetOrderAsync(orderId).Result;

        /// <summary>
        /// Get the status of a specific order
        /// </summary>
        /// <param name="orderId">The order id of the order to get</param>
        /// <returns></returns>
        public async Task<CallResult<BitfinexPlacedOrder>> GetOrderAsync(long orderId)
        {
            var parameters = new Dictionary<string, object>()
            {
                { "order_id", orderId }
            };

            return await ExecuteRequest<BitfinexPlacedOrder>(GetUrl(OrderStatusEndpoint, ApiVersion1), PostMethod, parameters, true);
        }

        #endregion

        #region private methods
        protected override IRequest ConstructRequest(Uri uri, string method, Dictionary<string, object> parameters, bool signed)
        {
            var uriString = uri.ToString();
            if (!signed && parameters != null)
            {
                if (!uriString.EndsWith("?"))
                    uriString += "?";

                uriString += $"{string.Join("&", parameters.Select(s => $"{s.Key}={s.Value}"))}";
            }

            var request = RequestFactory.Create(uriString);
            request.Method = method;
            request.ContentType = "application/json";
            request.Accept = "application/json";

            if (signed)
            {
                if (uri.ToString().Contains("v2"))
                {
                    if (parameters == null)
                        parameters = new Dictionary<string, object>();

                    var json = JsonConvert.SerializeObject(parameters);
                    var data = Encoding.UTF8.GetBytes(json);

                    var n = nonce;
                    var signature = $"/api{uri.PathAndQuery}{n}{json}";
                    var signedData = authProvider.Sign(signature);
                    request.Headers.Add($"bfx-nonce: {n}");
                    request.Headers.Add($"bfx-apikey: {authProvider.Credentials.Key}");
                    request.Headers.Add($"bfx-signature: {signedData.ToLower()}");

                    using (var stream = request.GetRequestStream().Result)
                        stream.Write(data, 0, data.Length);
                }
                else
                {
                    var path = uri.PathAndQuery;
                    var n = nonce;

                    var signature = new JObject
                    {
                        ["request"] = path,
                        ["nonce"] = n
                    };
                    if (parameters != null)
                        foreach (var keyvalue in parameters)
                            signature.Add(keyvalue.Key, JToken.FromObject(keyvalue.Value));
                    
                    var payload = Convert.ToBase64String(Encoding.ASCII.GetBytes(signature.ToString()));
                    var signedData = authProvider.Sign(payload);
                    
                    request.Headers.Add($"X-BFX-APIKEY: {authProvider.Credentials.Key}");
                    request.Headers.Add($"X-BFX-PAYLOAD: {payload}");
                    request.Headers.Add($"X-BFX-SIGNATURE: {signedData.ToLower()}");
                }
            }

            return request;
        }

        protected override Error ParseErrorResponse(string data)
        {
            var error = JArray.Parse(data).ToObject<BitfinexError>();
            return new ServerError(error.ErrorCode, error.ErrorMessage);
        }

        private Uri GetUrl(string endpoint, string version)
        {
            var result = $"{baseAddress}/v{version}/{endpoint}";
            return new Uri(result);
        }

        private string FillPathParameter(string endpoint, params string[] values)
        {
            foreach (var value in values)
            {
                int index = endpoint.IndexOf("{}", StringComparison.Ordinal);
                if (index >= 0)
                {
                    endpoint = endpoint.Remove(index, 2);
                    endpoint = endpoint.Insert(index, value);
                }
            }
            return endpoint;
        }
        
        private void Configure(BitfinexClientOptions options)
        {
            base.Configure(options);

            baseAddress = options.BaseAddress;
        }
        #endregion
        #endregion
    }
}

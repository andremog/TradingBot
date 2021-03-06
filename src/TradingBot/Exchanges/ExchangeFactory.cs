﻿using System.Collections.Generic;
using System.Linq;
using Common.Log;
using AzureStorage;
using AzureStorage.Tables;
using Lykke.SettingsReader;
using TradingBot.Communications;
using TradingBot.Exchanges.Abstractions;
using TradingBot.Exchanges.Concrete.HistoricalData;
using TradingBot.Exchanges.Concrete.Icm;
using TradingBot.Exchanges.Concrete.Kraken;
using TradingBot.Exchanges.Concrete.LykkeExchange;
using TradingBot.Exchanges.Concrete.StubImplementation;
using TradingBot.Handlers;
using TradingBot.Infrastructure.Configuration;
using TradingBot.Trading;

namespace TradingBot.Exchanges
{
    public static class ExchangeFactory
    {
	    public static List<Exchange> CreateExchanges(
            AppSettings config,
            TranslatedSignalsRepository translatedSignalsRepository,
            IReloadingManager<TradingBotSettings> settingsManager,
            INoSQLTableStorage<FixMessageTableEntity> fixMessagesStorage,
            ILog log)
	    {
            var exchanges = CreateExchanges(config.Exchanges, translatedSignalsRepository, fixMessagesStorage, log);

		    if (config.AzureStorage.Enabled)
			    foreach (var exchange in exchanges.Where(x => x.Config.SaveQuotesToAzure))
                {
                    var exchangeStorage = AzureTableStorage<PriceTableEntity>.Create(
                        settingsManager.ConnectionString(i => i.TradingBot.AzureStorage.StorageConnectionString), exchange.Name, new LogToConsole());
                    exchange.AddTickPriceHandler(new AzureTablePricesPublisher(exchangeStorage, exchange.Name));
                }

            if (config.RabbitMq.Enabled)
		    {
			    var pricesHandler =
				    new RabbitMqHandler<InstrumentTickPrices>(config.RabbitMq.GetConnectionString(), config.RabbitMq.RatesExchange);

			    var tradesHandler =
				    new RabbitMqHandler<ExecutedTrade>(config.RabbitMq.GetConnectionString(), config.RabbitMq.TradesExchange);


			    foreach (var exchange in exchanges.Where(x => x.Config.PubQuotesToRabbit))
			    {
				    exchange.AddTickPriceHandler(pricesHandler);
				    exchange.AddExecutedTradeHandler(tradesHandler);
			    }
		    }

		    return exchanges;
	    }

        private static List<Exchange> CreateExchanges(
            ExchangesConfiguration config,
            TranslatedSignalsRepository translatedSignalsRepository,
            INoSQLTableStorage<FixMessageTableEntity> tableStorage,
            ILog log)
        {
	        var result = new List<Exchange>();

	        if (config.Icm.Enabled)
		        result.Add(new IcmExchange(config.Icm, translatedSignalsRepository, tableStorage, log));
	        
	        if (config.Kraken.Enabled)
		        result.Add(new KrakenExchange(config.Kraken, translatedSignalsRepository));
	        
	        if (config.Stub.Enabled)
		        result.Add(new StubExchange(config.Stub, translatedSignalsRepository));
	        
	        if (config.HistoricalData.Enabled)
		        result.Add(new HistoricalDataExchange(config.HistoricalData, translatedSignalsRepository));
	        
	        if (config.Lykke.Enabled)
		        result.Add(new LykkeExchange(config.Lykke, translatedSignalsRepository));

	        return result;
        }
    }
}

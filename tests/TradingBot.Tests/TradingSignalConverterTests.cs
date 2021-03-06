using System;
using TradingBot.Trading;
using Xunit;

namespace TradingBot.Tests
{
    public class TradingSignalConverterTests
    
    {
        [Fact]
        public void SerializeAndDeserializeTradingSignal()
        {
            var converter = new GenericRabbitModelConverter<InstrumentTradingSignals>();
            var signal = new TradingSignal("", OrderCommand.Create, TradeType.Buy, 100.2m, 10.1m, DateTime.Now, OrderType.Limit);
            var instrumentSignals = new InstrumentTradingSignals(new Instrument("Exchange", "EURUSD"), new [] { signal });

            var serialized = converter.Serialize(instrumentSignals);
            Assert.NotNull(serialized);

            var deserialized = converter.Deserialize(serialized).TradingSignals[0];

            Assert.Equal(signal.TradeType, deserialized.TradeType);
            Assert.Equal(signal.Volume, deserialized.Volume);
            Assert.Equal(signal.OrderType, deserialized.OrderType);
            Assert.Equal(signal.Time, deserialized.Time);
            Assert.Equal(signal.OrderId, deserialized.OrderId);
            Assert.Equal(signal.Command, deserialized.Command);
        }

        [Fact]
        public void TradingSignal_IsTimeInThreshold()
        {
            var signal = new TradingSignal("", OrderCommand.Create, TradeType.Buy, 100m, 100m, DateTime.UtcNow.AddMinutes(-5));
            
            Assert.True(signal.IsTimeInThreshold(TimeSpan.FromMinutes(6)));
            Assert.False(signal.IsTimeInThreshold(TimeSpan.FromMinutes(4)));
            
            var signalInFuture = new TradingSignal("", OrderCommand.Create, TradeType.Buy, 100m, 100m, DateTime.UtcNow.AddMinutes(5));
            
            Assert.True(signalInFuture.IsTimeInThreshold(TimeSpan.FromMinutes(6)));
            Assert.False(signalInFuture.IsTimeInThreshold(TimeSpan.FromMinutes(4)));
        }
    }
}
﻿using System;
using Newtonsoft.Json;

namespace TradingBot.Trading
{
    public enum ExecutionStatus
    {
        Fill,
        PartialFill,
        Cancelled,
        Rejected,
        New,
        Pending
    }

    public class ExecutedTrade
    {
        [JsonConstructor]
        public ExecutedTrade(Instrument instrument, DateTime time, decimal price, decimal volume, TradeType type, string orderId, ExecutionStatus status)
        {
            Instrument = instrument;
            Time = time;
            Price = price;
            Volume = volume;
            Type = type;
            Fee = 0; // TODO
            OrderId = orderId;
            Status = status;
        }
        
        public Instrument Instrument { get; }
        
        public TradeType Type { get; }
        
        public DateTime Time { get; }
        
        public decimal Price { get; }
        
        public decimal Volume { get; }
        
        public decimal Fee { get; }
        
        public string OrderId { get; }
        
        public ExecutionStatus Status { get; }
        
        public string Message { get; set; }

        public override string ToString()
        {
            return $"OrderId: {OrderId} for {Instrument}. {Type} at {Time}. Price: {Price}. Volume: {Volume}";
        }
    }
}
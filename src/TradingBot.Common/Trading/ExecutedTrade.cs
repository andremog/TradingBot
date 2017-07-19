﻿using System;
using Newtonsoft.Json;

namespace TradingBot.Common.Trading
{
    public enum ExecutionStatus
    {
        Fill,
        PartialFill,
        Cancelled
    }

    public class ExecutedTrade
    {
        [JsonConstructor]
        public ExecutedTrade(DateTime time, decimal price, decimal volume, TradeType type, long orderId, ExecutionStatus status)
        {
            Time = time;
            Price = price;
            Volume = volume;
            Type = type;
            Fee = 0; // TODO
            OrderId = orderId;
            Status = status;
        }
        
        public TradeType Type { get; }
        
        public DateTime Time { get; }
        
        public decimal Price { get; }
        
        public decimal Volume { get; }
        
        public decimal Fee { get; }
        
        public long OrderId { get; }
        
        public ExecutionStatus Status { get; }

        public override string ToString()
        {
            return $"OrderId: {OrderId}. {Type} at {Time}. Price: {Price}. Volume: {Volume}";
        }
    }
}
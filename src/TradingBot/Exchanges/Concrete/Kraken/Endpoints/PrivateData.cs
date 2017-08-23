﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TradingBot.Exchanges.Abstractions;
using TradingBot.Exchanges.Concrete.Kraken.Entities;
using TradingBot.Exchanges.Concrete.Kraken.Requests;
using TradingBot.Exchanges.Concrete.Kraken.Responses;
using TradingBot.Infrastructure.Exceptions;

namespace TradingBot.Exchanges.Concrete.Kraken.Endpoints
{
    public class PrivateData
    {
        private readonly string endpointUrl = $"{Urls.ApiBase}/0/private";
        private const string ApiKeyHeader = "API-Key";
        private const string ApiSignHeader = "API-Sign";

        private readonly ApiClient apiClient;
        private readonly string apiKey;
        private readonly string apiPrivateKey;
        private long nonce;

        public PrivateData(ApiClient apiClient, string apiKey, string apiPrivateKey, long startingNonce = 0)
        {
            this.apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));

            this.apiKey = apiKey;
            this.apiPrivateKey = apiPrivateKey;
            this.nonce = startingNonce;
        }

        public Task<Dictionary<string, decimal>> GetAccountBalance(CancellationToken cancellationToken)
        {
            var content = CreateStringContent(new AccountBalanceRequest(), ++nonce, $"/0/private/Balance");
            
            return MakePostRequestAsync<Dictionary<string, decimal>>("Balance", content, cancellationToken);
        }
        
        private async Task<T> MakePostRequestAsync<T>(string url, HttpContent content, CancellationToken cancellationToken)
        {
            var response = await apiClient.MakePostRequestAsync<ResponseBase<T>>($"{endpointUrl}/{url}", content, cancellationToken);

            if (response.Error.Any())
            {
                throw new ApiException(string.Join("; ", response.Error));
            }

            return response.Result;
        }

        private HttpContent CreateStringContent(IKrakenRequest requestData, long nonce, string uriPath)
        {
            var pathBytes = Encoding.UTF8.GetBytes(uriPath);
            var props = "nonce=" + nonce + string.Join("", requestData.FormData.Select(x => $"&{x.Key}={x.Value}"));
            var np = nonce + Convert.ToChar(0) + props;
            var hash = Sha256(np);

            var z = new byte[pathBytes.Length + hash.Length];
            pathBytes.CopyTo(z, 0);
            hash.CopyTo(z, pathBytes.Length);

            var signature = HmacSha512(Convert.FromBase64String(apiPrivateKey), z);
            var sigString = Convert.ToBase64String(signature);
            
            var data = new List<KeyValuePair<string, string>>();
            data.Add(new KeyValuePair<string, string>("nonce", nonce.ToString()));
            data.AddRange(requestData.FormData);
            
            var content = new FormUrlEncodedContent(data);
            content.Headers.Add(ApiKeyHeader, new [] { apiKey });
            content.Headers.Add(ApiSignHeader, new [] { sigString });

            return content;
        }

        private byte[] Sha256(string value)
        {
            using (var sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(Encoding.UTF8.GetBytes(value));
            }
        }

        private byte[] HmacSha512(byte[] key, byte[] message)
        {
            using (var hmac = new HMACSHA512(key))
            {
                return hmac.ComputeHash(message);
            }
        }
    }
}
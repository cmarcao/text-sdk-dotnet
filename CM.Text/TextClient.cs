﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CM.Text.BusinessMessaging;
using CM.Text.BusinessMessaging.Model;
using JetBrains.Annotations;

namespace CM.Text
{
    /// <summary>
    /// 
    /// </summary>
    public interface ITextClient
    {
        /// <summary>
        ///     Sends a message asynchronous.
        /// </summary>
        /// <param name="messageText">The message text.</param>
        /// <param name="from">
        ///     This is the sender name. The maximum length is 11 alphanumerical characters or 16 digits. Example:
        ///     'CM Telecom'.<br/>
        ///     For Twitter: use the Twitter Snowflake ID of the account you want to use as sender.
        /// </param>
        /// <param name="to">
        ///     These are the destination mobile numbers. Restrictions: this value should be in international format.
        ///     Example: '00447911123456'.<br/>
        ///     For Twitter: use the Twitter Snowflake ID
        /// </param>
        /// <param name="reference">
        ///     Here you can include your message reference. This information will be returned in a status
        ///     report so you can match the message and it's status. Restrictions: 1 - 32 alphanumeric characters and reference
        ///     will not work for demo accounts.
        /// </param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<TextClientResult> SendMessageAsync(
            string messageText,
            string from,
            IEnumerable<string> to,
            [CanBeNull] string reference,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        ///     Sends a message asynchronous.
        /// </summary>
        /// <param name="messages">The message to send.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<TextClientResult> SendMessageAsync(
            Message[] messages,
            CancellationToken cancellationToken = default(CancellationToken));
    }

    /// <summary>
    ///     This class provides methods to send text messages.
    /// </summary>
    [PublicAPI]
    public class TextClient : ITextClient
    {
        private static readonly Lazy<HttpClient> ClientSingletonLazy = new Lazy<HttpClient>();
        private readonly Guid _apiKey;
        private readonly HttpClient _httpClient;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TextClient" /> class.
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        [PublicAPI]
        public TextClient(Guid apiKey) : this(apiKey, ClientSingletonLazy.Value)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TextClient" /> class.
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        /// <param name="httpClient">An optional HTTP client.</param>
        [PublicAPI]
        public TextClient(Guid apiKey, HttpClient httpClient)
        {
            this._apiKey = apiKey;
            this._httpClient = httpClient;
        }

        /// <summary>
        ///     Sends a message asynchronous.
        /// </summary>
        /// <param name="messageText">The message text.</param>
        /// <param name="from">
        ///     This is the sender name. The maximum length is 11 alphanumerical characters or 16 digits. Example:
        ///     'CM Telecom'.<br/>
        ///     For Twitter: use the Twitter Snowflake ID of the account you want to use as sender.
        /// </param>
        /// <param name="to">
        ///     These are the destination mobile numbers. Restrictions: this value should be in international format.
        ///     Example: '00447911123456'.<br/>
        ///     For Twitter: use the Twitter Snowflake ID
        /// </param>
        /// <param name="reference">
        ///     Here you can include your message reference. This information will be returned in a status
        ///     report so you can match the message and it's status. Restrictions: 1 - 32 alphanumeric characters and reference
        ///     will not work for demo accounts.
        /// </param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [PublicAPI]
        public async Task<TextClientResult> SendMessageAsync(
            string messageText,
            string from,
            IEnumerable<string> to,
            [CanBeNull] string reference,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var request = new HttpRequestMessage(
                HttpMethod.Post,
                new Uri(BusinessMessagingApi.Constant.BusinessMessagingGatewayJsonEndpoint)
            ))
            {
                request.Content = new StringContent(
                    BusinessMessagingApi.GetHttpPostBody(this._apiKey, messageText, from, to, reference),
                    Encoding.UTF8,
                    BusinessMessagingApi.Constant.BusinessMessagingGatewayMediaTypeJson
                );

                using (var requestResult = await this._httpClient.SendAsync(request, cancellationToken)
                    .ConfigureAwait(false))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    return BusinessMessagingApi.GetTextApiResult(
                        await requestResult.Content.ReadAsStringAsync()
                            .ConfigureAwait(false)
                    );
                }
            }
        }

        /// <summary>
        ///     Sends a message asynchronous.
        /// </summary>
        /// <param name="messages">The message to send.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [PublicAPI]
        public async Task<TextClientResult> SendMessageAsync(
            Message[] messages,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var request = new HttpRequestMessage(
                HttpMethod.Post,
                new Uri(BusinessMessagingApi.Constant.BusinessMessagingGatewayJsonEndpoint)
            ))
            {
                request.Content = new StringContent(
                    BusinessMessagingApi.GetHttpPostBody(this._apiKey, messages),
                    Encoding.UTF8,
                    BusinessMessagingApi.Constant.BusinessMessagingGatewayMediaTypeJson
                );

                using (var requestResult = await this._httpClient.SendAsync(request, cancellationToken)
                    .ConfigureAwait(false))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    return BusinessMessagingApi.GetTextApiResult(
                        await requestResult.Content.ReadAsStringAsync()
                            .ConfigureAwait(false)
                    );
                }
            }
        }
    }
}

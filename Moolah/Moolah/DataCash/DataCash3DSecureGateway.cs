﻿using System;
using System.Xml.Linq;

namespace Moolah.DataCash
{
    public class DataCash3DSecureGateway : I3DSecurePaymentGateway
    {
        private readonly DataCash3DSecureConfiguration _configuration;
        private readonly IHttpClient _httpClient;
        private readonly IDataCashPaymentRequestBuilder _paymentPaymentRequestBuilder;
        private readonly IDataCashAuthorizeRequestBuilder _authorizeRequestBuilder;
        private readonly IDataCash3DSecureResponseParser _responseParser;
        private readonly IRefundGateway _refundGateway;

        public DataCash3DSecureGateway()
            : this(MoolahConfiguration.Current.DataCash3DSecure)
        {
        }

        public DataCash3DSecureGateway(DataCash3DSecureConfiguration configuration)
            : this(configuration, new HttpClient(), new DataCash3DSecureRequestBuilder(configuration), new DataCash3DSecureAuthorizeRequestBuilder(configuration), new DataCash3DSecureResponseParser(), new RefundGateway(configuration))
        {
        }

        /// <summary>
        /// TODO: Make internal and visible to Moolah.Specs
        /// </summary>
        public DataCash3DSecureGateway(
            DataCash3DSecureConfiguration configuration, 
            IHttpClient httpClient, 
            IDataCashPaymentRequestBuilder paymentRequestBuilder, 
            IDataCashAuthorizeRequestBuilder authorizeRequestBuilder,
            IDataCash3DSecureResponseParser responseParser,
            IRefundGateway refundGateway)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");
            if (httpClient == null) throw new ArgumentNullException("httpClient");
            if (paymentRequestBuilder == null) throw new ArgumentNullException("paymentRequestBuilder");
            if (authorizeRequestBuilder == null) throw new ArgumentNullException("authorizeRequestBuilder");
            if (responseParser == null) throw new ArgumentNullException("responseParser");
            if (refundGateway == null) throw new ArgumentNullException("refundGateway");
            _configuration = configuration;
            _httpClient = httpClient;
            _paymentPaymentRequestBuilder = paymentRequestBuilder;
            _authorizeRequestBuilder = authorizeRequestBuilder;
            _responseParser = responseParser;
            _refundGateway = refundGateway;
        }

        public I3DSecureResponse Payment(string merchantReference, decimal amount, CardDetails card, BillingAddress billingAddress = null, string currencyCode = null)
        {
            if (string.IsNullOrWhiteSpace(merchantReference)) throw new ArgumentNullException("merchantReference");

            var requestDocument = _paymentPaymentRequestBuilder.Build(merchantReference, amount, currencyCode, card, billingAddress);
            var httpResponse = _httpClient.Post(_configuration.Host, requestDocument.ToString(SaveOptions.DisableFormatting));
            var response = _responseParser.Parse(httpResponse);
            if (response.CanAuthorize())
                response = Authorise(response.TransactionReference, null);
            return response;
        }

        public I3DSecureResponse Authorise(string transactionReference, string PARes)
        {
            if (string.IsNullOrWhiteSpace(transactionReference)) throw new ArgumentNullException("transactionReference");

            var requestDocument = _authorizeRequestBuilder.Build(transactionReference, PARes);
            var response = _httpClient.Post(_configuration.Host, requestDocument.ToString(SaveOptions.DisableFormatting));
            return _responseParser.Parse(response);
        }

        /// <summary>
        /// Merchant vTID number
        /// </summary>
        public string MerchantId
        {
            get { return _configuration.MerchantId; }
        }

        public IRefundTransactionResponse RefundTransaction(string originalTransactionReference, decimal amount)
        {
            return _refundGateway.Refund(originalTransactionReference, amount);
        }
    }
}
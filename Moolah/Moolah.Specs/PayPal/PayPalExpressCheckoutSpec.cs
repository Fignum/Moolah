﻿using System.Collections.Specialized;
using System.Web;
using Machine.Fakes;
using Machine.Specifications;
using Moolah.PayPal;

namespace Moolah.Specs.PayPal
{
    public abstract class PayPalExpressCheckoutContext : WithFakes
    {
        Establish context = () =>
        {
            Configuration = new PayPalConfiguration(PaymentEnvironment.Test);
            HttpClient = An<IHttpClient>();
            HttpClient.WhenToldTo(x => x.Post(Param<string>.IsNotNull, Request))
                .Return(Response);
            HttpClient.WhenToldTo(x => x.Get(Configuration.Host + "?" + Request))
                .Return(Response);
            RequestBuilder = An<IPayPalRequestBuilder>();
            ResponseParser = An<IPayPalResponseParser>();
            SUT = new PayPalExpressCheckout(Configuration, HttpClient, RequestBuilder, ResponseParser);
        };

        protected static PayPalExpressCheckout SUT;
        protected static PayPalConfiguration Configuration;
        protected static IHttpClient HttpClient;
        protected static IPayPalRequestBuilder RequestBuilder;
        protected static IPayPalResponseParser ResponseParser;

        protected const string Request = "Test=Request";
        protected const string Response = "Test=Response";
    }

    [Subject(typeof(PayPalExpressCheckout))]
    public class When_set_express_checkout_is_called : PayPalExpressCheckoutContext
    {
        It should_return_the_correct_result = () =>
            Result.ShouldEqual(ExpectedResult);

        Establish context = () =>
        {
            ExpectedResult = new PayPalExpressCheckoutToken();
            RequestBuilder.WhenToldTo(x => x.SetExpressCheckout(Amount, CancelUrl, ConfirmationUrl))
                .Return(HttpUtility.ParseQueryString(Request));
            ResponseParser.WhenToldTo(x => x.SetExpressCheckout(Param<NameValueCollection>.Matches(r => r.ToString() == Response)))
                .Return(ExpectedResult);
        };

        Because of = () =>
            Result = SUT.SetExpressCheckout(Amount, CancelUrl, ConfirmationUrl);

        static PayPalExpressCheckoutToken Result;
        static PayPalExpressCheckoutToken ExpectedResult;
        const decimal Amount = 10m;
        const string CancelUrl = "http://www.yourdomain.com/cancel.html";
        const string ConfirmationUrl = "http://www.yourdomain.com/success.html";
    }

    [Subject(typeof(PayPalExpressCheckout))]
    public class When_get_express_checkout_details_is_called : PayPalExpressCheckoutContext
    {
        It should_return_the_correct_result = () =>
            Result.ShouldEqual(ExpectedResult);

        Establish context = () =>
        {
            ExpectedResult = new PayPalExpressCheckoutDetails(new NameValueCollection());
            RequestBuilder.WhenToldTo(x => x.GetExpressCheckoutDetails(Token))
                .Return(HttpUtility.ParseQueryString(Request));
            ResponseParser.WhenToldTo(x => x.GetExpressCheckoutDetails(Param<NameValueCollection>.Matches(r => r.ToString() == Response)))
                .Return(ExpectedResult);
        };

        Because of = () =>
            Result = SUT.GetExpressCheckoutDetails(Token);

        static PayPalExpressCheckoutDetails Result;
        static PayPalExpressCheckoutDetails ExpectedResult;
        const string Token = "tokenValue";
    }

    [Subject(typeof(PayPalExpressCheckout))]
    public class When_do_express_checkout_payment_is_called : PayPalExpressCheckoutContext
    {
        It should_return_the_correct_result = () =>
            Result.ShouldEqual(ExpectedResult);

        Establish context = () =>
        {
            ExpectedResult = new PayPalPaymentResponse(new NameValueCollection());
            RequestBuilder.WhenToldTo(x => x.DoExpressCheckoutPayment(Amount, Token, PayerId))
                .Return(HttpUtility.ParseQueryString(Request));
            ResponseParser.WhenToldTo(x => x.DoExpressCheckoutPayment(Param<NameValueCollection>.Matches(r => r.ToString() == Response)))
                .Return(ExpectedResult);
        };

        Because of = () =>
            Result = SUT.DoExpressCheckoutPayment(Amount, Token, PayerId);

        static IPaymentResponse Result;
        static PayPalPaymentResponse ExpectedResult;
        const decimal Amount = 12.99m;
        const string Token = "tokenValue";
        const string PayerId = "payerId";
    }
}
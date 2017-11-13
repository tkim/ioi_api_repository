/* Copyright 2017. Bloomberg Finance L.P.
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to
 * deal in the Software without restriction, including without limitation the
 * rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
 * sell copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:  The above
 * copyright notice and this permission notice shall be included in all copies
 * or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
 * IN THE SOFTWARE.
 */

using Name = Bloomberglp.Blpapi.Name;
using SessionOptions = Bloomberglp.Blpapi.SessionOptions;
using Session = Bloomberglp.Blpapi.Session;
using Service = Bloomberglp.Blpapi.Service;
using Request = Bloomberglp.Blpapi.Request;
using Element = Bloomberglp.Blpapi.Element;
using CorrelationID = Bloomberglp.Blpapi.CorrelationID;
using Event = Bloomberglp.Blpapi.Event;
using Message = Bloomberglp.Blpapi.Message;
using EventHandler = Bloomberglp.Blpapi.EventHandler;
using Subscription = Bloomberglp.Blpapi.Subscription;
using Identity = Bloomberglp.Blpapi.Identity;
using System;
using System.Collections.Generic;

namespace com.bloomberg.ioi.samples
{
    public class SubscribeIOI
    {

        private static readonly Name IOI_DATA = new Name("Ioidata");

        // ADMIN
        private static readonly Name SLOW_CONSUMER_WARNING = new Name("SlowConsumerWarning");
        private static readonly Name SLOW_CONSUMER_WARNING_CLEARED = new Name("SlowConsumerWarningCleared");

        // SESSION_STATUS
        private static readonly Name SESSION_STARTED = new Name("SessionStarted");
        private static readonly Name SESSION_TERMINATED = new Name("SessionTerminated");
        private static readonly Name SESSION_STARTUP_FAILURE = new Name("SessionStartupFailure");
        private static readonly Name SESSION_CONNECTION_UP = new Name("SessionConnectionUp");
        private static readonly Name SESSION_CONNECTION_DOWN = new Name("SessionConnectionDown");

        // SERVICE_STATUS
        private static readonly Name SERVICE_OPENED = new Name("ServiceOpened");
        private static readonly Name SERVICE_OPEN_FAILURE = new Name("ServiceOpenFailure");

        // SUBSCRIPTION_STATUS + SUBSCRIPTION_DATA
        private static readonly Name SUBSCRIPTION_FAILURE = new Name("SubscriptionFailure");
        private static readonly Name SUBSCRIPTION_STARTED = new Name("SubscriptionStarted");
        private static readonly Name SUBSCRIPTION_TERMINATED = new Name("SubscriptionTerminated");

        private static readonly Name AUTHORIZATION_SUCCESS = new Name("AuthorizationSuccess");
        private static readonly Name AUTHORIZATION_FAILURE = new Name("AuthorizationFailure");

        private Subscription ioiSubscription;
        private CorrelationID ioiSubscriptionID;

        private String d_ioi;
        private String d_host;
        private int d_port;

        private string d_auth;
        private string d_user;
        private string d_ip;

        private Identity identity;

        public static void Main(String[] args)
        {
            System.Console.WriteLine("Bloomberg - IOI API Example - SubscribeIOI\n");
            System.Console.WriteLine("Press ENTER at anytime to quit");

            SubscribeIOI example = new SubscribeIOI();
            example.run(args);

            System.Console.ReadKey();

        }

        public SubscribeIOI()
        {

            // Define the service required, in this case the beta service, 
            // and the values to be used by the SessionOptions object
            // to identify IP/port of the back-end process.

            d_ioi = "//blp-test/ioisub-beta";
            d_host = "10.136.8.125";
            d_port = 8294;

            d_auth = "//blp/apiauth";
            d_user = "CORP\\rclegg2";
            d_ip = "10.136.8.125";

        }


        private void run(String[] args)
        {

            SessionOptions d_sessionOptions = new SessionOptions();
            d_sessionOptions.ServerHost = d_host;
            d_sessionOptions.ServerPort = d_port;

            Session session = new Session(d_sessionOptions, new EventHandler(processEvent));

            session.StartAsync();

        }

        public void processEvent(Event evt, Session session)
        {
            System.Console.WriteLine("Event: " + evt.Type);
            try
            {
                switch (evt.Type)
                {
                    case Event.EventType.ADMIN:
                        processAdminEvent(evt, session);
                        break;
                    case Event.EventType.SESSION_STATUS:
                        processSessionEvent(evt, session);
                        break;
                    case Event.EventType.SERVICE_STATUS:
                        processServiceEvent(evt, session);
                        break;
                    case Event.EventType.AUTHORIZATION_STATUS:
                        processAuthorizationStatusEvent(evt, session);
                        break;
                    case Event.EventType.RESPONSE:
                        processResponseEvent(evt, session);
                        break;
                    case Event.EventType.SUBSCRIPTION_STATUS:
                        processSubscriptionStatusEvent(evt, session);
                        break;
                    case Event.EventType.SUBSCRIPTION_DATA:
                        processSubscriptionDataEvent(evt, session);
                        break;
                    default:
                        processMiscEvents(evt, session);
                        break;
                }
            }
            catch (Exception e)
            {
                System.Console.Error.WriteLine(e);
            }
        }

        private void processAdminEvent(Event evt, Session session)
        {
            System.Console.WriteLine("Processing " + evt.Type);

            foreach (Message msg in evt)
            {
                if (msg.MessageType.Equals(SLOW_CONSUMER_WARNING))
                {
                    System.Console.Error.WriteLine("Warning: Entered Slow Consumer status");
                }
                else if (msg.MessageType.Equals(SLOW_CONSUMER_WARNING_CLEARED))
                {
                    System.Console.WriteLine("Slow consumer status cleared");
                }
            }
        }

        private void processSessionEvent(Event evt, Session session)
        {
            System.Console.WriteLine("\nProcessing " + evt.Type);

            foreach (Message msg in evt)
            {
                if (msg.MessageType.Equals(SESSION_STARTED))
                {
                    System.Console.WriteLine("Session started...");
                    session.OpenServiceAsync(d_auth);
                }
                else if (msg.MessageType.Equals(SESSION_STARTUP_FAILURE))
                {
                    System.Console.WriteLine("Error: Session startup failed");
                }
                else if (msg.MessageType.Equals(SESSION_TERMINATED))
                {
                    System.Console.WriteLine("Session has been terminated");
                }
                else if (msg.MessageType.Equals(SESSION_CONNECTION_UP))
                {
                    System.Console.WriteLine("Session connection is up");
                }
                else if (msg.MessageType.Equals(SESSION_CONNECTION_DOWN))
                {
                    System.Console.WriteLine("Session connection is down");
                }
            }
        }

        private void processServiceEvent(Event evt, Session session)
        {

            System.Console.WriteLine("\nProcessing " + evt.Type);

            foreach (Message msg in evt)
            {
                if (msg.MessageType.Equals(SERVICE_OPENED))
                {
                    String serviceName = msg.AsElement.GetElementAsString("serviceName");

                    System.Console.WriteLine("Service opened [" + serviceName + "]...");

                    if (serviceName == d_auth)
                    {
                        System.Console.WriteLine("Auth Service opened... Opening IOI service...");
                        session.OpenServiceAsync(d_ioi);
                    }
                    else if (serviceName == d_ioi)
                    {
                        System.Console.WriteLine("IOI Service opened... Sending Authorization requests");
                        sendAuthRequest(session, d_user, d_ip);

                    }
                }
                else if (msg.MessageType.Equals(SERVICE_OPEN_FAILURE))
                {
                    System.Console.Error.WriteLine("Error: Service failed to open");
                }
            }
        }

        private void processAuthorizationStatusEvent(Event evt, Session session)
        {
            System.Console.WriteLine("\nProcessing " + evt.Type);

            foreach (Message msg in evt)
            {
                System.Console.WriteLine("AUTHORIZATION_STATUS message: " + msg.ToString());
            }
        }

        private void processResponseEvent(Event evt, Session session)
        {
            System.Console.WriteLine("Received Event: " + evt.Type);

            foreach (Message msg in evt)
            {
                if (msg.MessageType.Equals(AUTHORIZATION_SUCCESS))
                {
                    System.Console.WriteLine("Authorization successfull...");

                    createIOISubscription(session);
                }
                else if (msg.MessageType.Equals(AUTHORIZATION_FAILURE))
                {
                    System.Console.WriteLine("Authorisation failed...");
                    System.Console.WriteLine(msg.ToString());

                    // Here you can insert code to automatically retry the authorisation if required
                }
                else
                {
                    System.Console.WriteLine("Unexpected message...");
                    System.Console.WriteLine(msg.ToString());
                }
            }
        }

        private void processSubscriptionStatusEvent(Event evt, Session session)
        {
            System.Console.WriteLine("Received Event: " + evt.Type);

            foreach (Message msg in evt)
            {

                if (msg.MessageType.Equals(SUBSCRIPTION_STARTED))
                {
                    if (msg.CorrelationID == ioiSubscriptionID)
                    {
                        System.Console.WriteLine("IOI subscription started successfully");
                    }
                }
                else if (msg.MessageType.Equals(SUBSCRIPTION_FAILURE))
                {
                    System.Console.WriteLine("Error: IOI subscription failed");
                    System.Console.WriteLine("MESSAGE: " + msg);
                }
                else if (msg.MessageType.Equals(SUBSCRIPTION_TERMINATED))
                {
                    System.Console.WriteLine("IOI subscription terminated");
                    System.Console.WriteLine("MESSAGE: " + msg);
                }
            }
        }

        private void processSubscriptionDataEvent(Event evt, Session session)
        {
            System.Console.WriteLine("Received Event: " + evt.Type);

            foreach (Message msg in evt)
            {
                // Processing the field values in the subscription data...

                System.Console.Error.WriteLine("Message:\n" + msg.ToString());

                if (msg.MessageType.Equals(IOI_DATA))
                {
                    String ioi_instrument_type = msg.HasElement("ioi_instrument_type") ? msg.GetElementAsString("ioi_instrument_type") : "";
                    int ioi_instrument_option_legs_count = msg.HasElement("ioi_instrument_option_legs_count") ? msg.GetElementAsInt32("ioi_instrument_option_legs_count") : 0;

                    Double ioi_instrument_option_legs_0_strike = msg.HasElement("ioi_instrument_option_legs_0_strike") ? msg.GetElementAsFloat64("ioi_instrument_option_legs_0_strike") : 0;
                    DateTime ioi_instrument_option_legs_0_expiry = msg.HasElement("ioi_instrument_option_legs_0_expiry") ? msg.GetElementAsDatetime("ioi_instrument_option_legs_0_expiry").ToSystemDateTime() : Convert.ToDateTime(null);
                    String ioi_instrument_option_legs_0_type = msg.HasElement("ioi_instrument_option_legs_0_type") ? msg.GetElementAsString("ioi_instrument_option_legs_0_type") : "";
                    Double ioi_instrument_option_legs_0_ratio = msg.HasElement("ioi_instrument_option_legs_0_ratio") ? msg.GetElementAsFloat64("ioi_instrument_option_legs_0_ratio") : 0;
                    String ioi_instrument_option_legs_0_underlying_type = msg.HasElement("ioi_instrument_option_legs_0_underlying_type") ? msg.GetElementAsString("ioi_instrument_option_legs_0_underlying_type") : "";
                    String ioi_instrument_option_legs_0_underlying_ticker = msg.HasElement("ioi_instrument_option_legs_0_underlying_ticker") ? msg.GetElementAsString("ioi_instrument_option_legs_0_underlying_ticker") : "";
                    String ioi_instrument_option_legs_0_underlying_figi = msg.HasElement("ioi_instrument_option_legs_0_underlying_figi") ? msg.GetElementAsString("ioi_instrument_option_legs_0_underlying_figi") : "";
                    String ioi_instrument_option_legs_0_exchange = msg.HasElement("ioi_instrument_option_legs_0_exchange") ? msg.GetElementAsString("ioi_instrument_option_legs_0_exchange") : "";
                    String ioi_instrument_option_legs_0_style = msg.HasElement("ioi_instrument_option_legs_0_style") ? msg.GetElementAsString("ioi_instrument_option_legs_0_style") : "";
                    DateTime ioi_instrument_option_legs_0_futureRefDate = msg.HasElement("ioi_instrument_option_legs_0_futureRefDate") ? msg.GetElementAsDatetime("ioi_instrument_option_legs_0_futureRefDate").ToSystemDateTime() : Convert.ToDateTime(null);
                    Double ioi_instrument_option_legs_0_delta = msg.HasElement("ioi_instrument_option_legs_0_delta") ? msg.GetElementAsFloat64("ioi_instrument_option_legs_0_delta") : 0;

                    Double ioi_instrument_option_legs_1_strike = msg.HasElement("ioi_instrument_option_legs_1_strike") ? msg.GetElementAsFloat64("ioi_instrument_option_legs_1_strike") : 0;
                    DateTime ioi_instrument_option_legs_1_expiry = msg.HasElement("ioi_instrument_option_legs_1_expiry") ? msg.GetElementAsDatetime("ioi_instrument_option_legs_1_expiry").ToSystemDateTime() : Convert.ToDateTime(null);
                    String ioi_instrument_option_legs_1_type = msg.HasElement("ioi_instrument_option_legs_1_type") ? msg.GetElementAsString("ioi_instrument_option_legs_1_type") : "";
                    Double ioi_instrument_option_legs_1_ratio = msg.HasElement("ioi_instrument_option_legs_1_ratio") ? msg.GetElementAsFloat64("ioi_instrument_option_legs_1_ratio") : 0;
                    String ioi_instrument_option_legs_1_underlying_type = msg.HasElement("ioi_instrument_option_legs_1_underlying_type") ? msg.GetElementAsString("ioi_instrument_option_legs_1_underlying_type") : "";
                    String ioi_instrument_option_legs_1_underlying_ticker = msg.HasElement("ioi_instrument_option_legs_1_underlying_ticker") ? msg.GetElementAsString("ioi_instrument_option_legs_1_underlying_ticker") : "";
                    String ioi_instrument_option_legs_1_underlying_figi = msg.HasElement("ioi_instrument_option_legs_1_underlying_figi") ? msg.GetElementAsString("ioi_instrument_option_legs_1_underlying_figi") : "";
                    String ioi_instrument_option_legs_1_exchange = msg.HasElement("ioi_instrument_option_legs_1_exchange") ? msg.GetElementAsString("ioi_instrument_option_legs_1_exchange") : "";
                    String ioi_instrument_option_legs_1_style = msg.HasElement("ioi_instrument_option_legs_1_style") ? msg.GetElementAsString("ioi_instrument_option_legs_1_style") : "";
                    DateTime ioi_instrument_option_legs_1_futureRefDate = msg.HasElement("ioi_instrument_option_legs_1_futureRefDate") ? msg.GetElementAsDatetime("ioi_instrument_option_legs_1_futureRefDate").ToSystemDateTime() : Convert.ToDateTime(null);
                    Double ioi_instrument_option_legs_1_delta = msg.HasElement("ioi_instrument_option_legs_1_delta") ? msg.GetElementAsFloat64("ioi_instrument_option_legs_1_delta") : 0;

                    Double ioi_instrument_option_legs_2_strike = msg.HasElement("ioi_instrument_option_legs_2_strike") ? msg.GetElementAsFloat64("ioi_instrument_option_legs_2_strike") : 0;
                    DateTime ioi_instrument_option_legs_2_expiry = msg.HasElement("ioi_instrument_option_legs_2_expiry") ? msg.GetElementAsDatetime("ioi_instrument_option_legs_2_expiry").ToSystemDateTime() : Convert.ToDateTime(null);
                    String ioi_instrument_option_legs_2_type = msg.HasElement("ioi_instrument_option_legs_2_type") ? msg.GetElementAsString("ioi_instrument_option_legs_2_type") : "";
                    Double ioi_instrument_option_legs_2_ratio = msg.HasElement("ioi_instrument_option_legs_2_ratio") ? msg.GetElementAsFloat64("ioi_instrument_option_legs_2_ratio") : 0;
                    String ioi_instrument_option_legs_2_underlying_type = msg.HasElement("ioi_instrument_option_legs_2_underlying_type") ? msg.GetElementAsString("ioi_instrument_option_legs_2_underlying_type") : "";
                    String ioi_instrument_option_legs_2_underlying_ticker = msg.HasElement("ioi_instrument_option_legs_2_underlying_ticker") ? msg.GetElementAsString("ioi_instrument_option_legs_2_underlying_ticker") : "";
                    String ioi_instrument_option_legs_2_underlying_figi = msg.HasElement("ioi_instrument_option_legs_2_underlying_figi") ? msg.GetElementAsString("ioi_instrument_option_legs_2_underlying_figi") : "";
                    String ioi_instrument_option_legs_2_exchange = msg.HasElement("ioi_instrument_option_legs_2_exchange") ? msg.GetElementAsString("ioi_instrument_option_legs_2_exchange") : "";
                    String ioi_instrument_option_legs_2_style = msg.HasElement("ioi_instrument_option_legs_2_style") ? msg.GetElementAsString("ioi_instrument_option_legs_2_style") : "";
                    DateTime ioi_instrument_option_legs_2_futureRefDate = msg.HasElement("ioi_instrument_option_legs_2_futureRefDate") ? msg.GetElementAsDatetime("ioi_instrument_option_legs_2_futureRefDate").ToSystemDateTime() : Convert.ToDateTime(null);
                    Double ioi_instrument_option_legs_2_delta = msg.HasElement("ioi_instrument_option_legs_2_delta") ? msg.GetElementAsFloat64("ioi_instrument_option_legs_2_delta") : 0;

                    Double ioi_instrument_option_legs_3_strike = msg.HasElement("ioi_instrument_option_legs_3_strike") ? msg.GetElementAsFloat64("ioi_instrument_option_legs_3_strike") : 0;
                    DateTime ioi_instrument_option_legs_3_expiry = msg.HasElement("ioi_instrument_option_legs_3_expiry") ? msg.GetElementAsDatetime("ioi_instrument_option_legs_3_expiry").ToSystemDateTime() : Convert.ToDateTime(null);
                    String ioi_instrument_option_legs_3_type = msg.HasElement("ioi_instrument_option_legs_3_type") ? msg.GetElementAsString("ioi_instrument_option_legs_3_type") : "";
                    Double ioi_instrument_option_legs_3_ratio = msg.HasElement("ioi_instrument_option_legs_3_ratio") ? msg.GetElementAsFloat64("ioi_instrument_option_legs_3_ratio") : 0;
                    String ioi_instrument_option_legs_3_underlying_type = msg.HasElement("ioi_instrument_option_legs_3_underlying_type") ? msg.GetElementAsString("ioi_instrument_option_legs_3_underlying_type") : "";
                    String ioi_instrument_option_legs_3_underlying_ticker = msg.HasElement("ioi_instrument_option_legs_3_underlying_ticker") ? msg.GetElementAsString("ioi_instrument_option_legs_3_underlying_ticker") : "";
                    String ioi_instrument_option_legs_3_underlying_figi = msg.HasElement("ioi_instrument_option_legs_3_underlying_figi") ? msg.GetElementAsString("ioi_instrument_option_legs_3_underlying_figi") : "";
                    String ioi_instrument_option_legs_3_exchange = msg.HasElement("ioi_instrument_option_legs_3_exchange") ? msg.GetElementAsString("ioi_instrument_option_legs_3_exchange") : "";
                    String ioi_instrument_option_legs_3_style = msg.HasElement("ioi_instrument_option_legs_3_style") ? msg.GetElementAsString("ioi_instrument_option_legs_3_style") : "";
                    DateTime ioi_instrument_option_legs_3_futureRefDate = msg.HasElement("ioi_instrument_option_legs_3_futureRefDate") ? msg.GetElementAsDatetime("ioi_instrument_option_legs_3_futureRefDate").ToSystemDateTime() : Convert.ToDateTime(null);
                    Double ioi_instrument_option_legs_3_delta = msg.HasElement("ioi_instrument_option_legs_3_delta") ? msg.GetElementAsFloat64("ioi_instrument_option_legs_3_delta") : 0;

                    String ioi_instrument_option_structure = msg.HasElement("ioi_instrument_option_structure") ? msg.GetElementAsString("ioi_instrument_option_structure") : "";
                    String ioi_instrument_stock_security_ticker = msg.HasElement("ioi_instrument_stock_security_ticker") ? msg.GetElementAsString("ioi_instrument_stock_security_ticker") : "";
                    String ioi_instrument_stock_security_figi = msg.HasElement("ioi_instrument_stock_security_figi") ? msg.GetElementAsString("ioi_instrument_stock_security_figi") : "";
                    DateTime ioi_goodUntil = msg.HasElement("ioi_goodUntil") ? msg.GetElementAsDatetime("ioi_goodUntil").ToSystemDateTime() : Convert.ToDateTime(null);

                    String ioi_bid_price_type = msg.HasElement("ioi_bid_price_type") ? msg.GetElementAsString("ioi_bid_price_type") : "";
                    Double ioi_bid_price_fixed_price = msg.HasElement("ioi_bid_price_fixed_price") ? msg.GetElementAsFloat64("ioi_bid_price_fixed_price") : 0;
                    String ioi_bid_price_fixed_currency = msg.HasElement("ioi_bid_price_fixed_currency") ? msg.GetElementAsString("ioi_bid_price_fixed_currency") : "";
                    Double ioi_bid_price_pegged_offsetAmount = msg.HasElement("ioi_bid_price_pegged_offsetAmount") ? msg.GetElementAsFloat64("ioi_bid_price_pegged_offsetAmount") : 0;
                    String ioi_bid_price_pegged_offsetFrom = msg.HasElement("ioi_bid_price_pegged_offsetFrom") ? msg.GetElementAsString("ioi_bid_price_pegged_offsetFrom") : "";
                    Double ioi_bid_price_pegged_limitPrice = msg.HasElement("ioi_bid_price_pegged_limitPrice") ? msg.GetElementAsFloat64("ioi_bid_price_pegged_limitPrice") : 0;
                    String ioi_bid_price_reference = msg.HasElement("ioi_bid_price_reference") ? msg.GetElementAsString("ioi_bid_price_reference") : "";
                    Double ioi_bid_price_moneyness = msg.HasElement("ioi_bid_price_moneyness") ? msg.GetElementAsFloat64("ioi_bid_price_moneyness") : 0;
                    String ioi_bid_size_type = msg.HasElement("ioi_bid_size_type") ? msg.GetElementAsString("ioi_bid_size_type") : "";
                    long ioi_bid_size_quantity = msg.HasElement("ioi_bid_size_quantity") ? msg.GetElementAsInt64("ioi_bid_size_quantity") : 0;
                    String ioi_bid_size_quality = msg.HasElement("ioi_bid_size_quality") ? msg.GetElementAsString("ioi_bid_size_quality") : "";
                    Double ioi_bid_referencePrice_price = msg.HasElement("ioi_bid_referencePrice_price") ? msg.GetElementAsFloat64("ioi_bid_referencePrice_price") : 0;
                    String ioi_bid_referencePrice_currency = msg.HasElement("ioi_bid_referencePrice_currency") ? msg.GetElementAsString("ioi_bid_referencePrice_currency") : "";
                    Double ioi_bid_volatility = msg.HasElement("ioi_bid_volatility") ? msg.GetElementAsFloat64("ioi_bid_volatility") : 0;
                    String ioi_bid_notes = msg.HasElement("ioi_bid_notes") ? msg.GetElementAsString("ioi_bid_notes") : "";
                    int ioi_bid_qualifiers_count = msg.HasElement("ioi_bid_qualifiers_count") ? msg.GetElementAsInt32("ioi_bid_qualifiers_count") : 0;
                    String ioi_bid_qualifiers_0 = msg.HasElement("ioi_bid_qualifiers_0") ? msg.GetElementAsString("ioi_bid_qualifiers_0") : "";
                    String ioi_bid_qualifiers_1 = msg.HasElement("ioi_bid_qualifiers_1") ? msg.GetElementAsString("ioi_bid_qualifiers_1") : "";
                    String ioi_bid_qualifiers_2 = msg.HasElement("ioi_bid_qualifiers_2") ? msg.GetElementAsString("ioi_bid_qualifiers_2") : "";
                    String ioi_bid_qualifiers_3 = msg.HasElement("ioi_bid_qualifiers_3") ? msg.GetElementAsString("ioi_bid_qualifiers_3") : "";
                    String ioi_bid_qualifiers_4 = msg.HasElement("ioi_bid_qualifiers_4") ? msg.GetElementAsString("ioi_bid_qualifiers_4") : "";

                    String ioi_offer_price_type = msg.HasElement("ioi_offer_price_type") ? msg.GetElementAsString("ioi_offer_price_type") : "";
                    Double ioi_offer_price_fixed_price = msg.HasElement("ioi_offer_price_fixed_price") ? msg.GetElementAsFloat64("ioi_offer_price_fixed_price") : 0;
                    String ioi_offer_price_fixed_currency = msg.HasElement("ioi_offer_price_fixed_currency") ? msg.GetElementAsString("ioi_offer_price_fixed_currency") : "";
                    Double ioi_offer_price_pegged_offsetAmount = msg.HasElement("ioi_offer_price_pegged_offsetAmount") ? msg.GetElementAsFloat64("ioi_offer_price_pegged_offsetAmount") : 0;
                    String ioi_offer_price_pegged_offsetFrom = msg.HasElement("ioi_offer_price_pegged_offsetFrom") ? msg.GetElementAsString("ioi_offer_price_pegged_offsetFrom") : "";
                    Double ioi_offer_price_pegged_limitPrice = msg.HasElement("ioi_offer_price_pegged_limitPrice") ? msg.GetElementAsFloat64("ioi_offer_price_pegged_limitPrice") : 0;
                    String ioi_offer_price_reference = msg.HasElement("ioi_offer_price_reference") ? msg.GetElementAsString("ioi_offer_price_reference") : "";
                    Double ioi_offer_price_moneyness = msg.HasElement("ioi_offer_price_moneyness") ? msg.GetElementAsFloat64("ioi_offer_price_moneyness") : 0;
                    String ioi_offer_size_type = msg.HasElement("ioi_offer_size_type") ? msg.GetElementAsString("ioi_offer_size_type") : "";
                    long ioi_offer_size_quantity = msg.HasElement("ioi_offer_size_quantity") ? msg.GetElementAsInt64("ioi_offer_size_quantity") : 0;
                    String ioi_offer_size_quality = msg.HasElement("ioi_offer_size_quality") ? msg.GetElementAsString("ioi_offer_size_quality") : "";
                    Double ioi_offer_referencePrice_price = msg.HasElement("ioi_offer_referencePrice_price") ? msg.GetElementAsFloat64("ioi_offer_referencePrice_price") : 0;
                    String ioi_offer_referencePrice_currency = msg.HasElement("ioi_offer_referencePrice_currency") ? msg.GetElementAsString("ioi_offer_referencePrice_currency") : "";
                    Double ioi_offer_volatility = msg.HasElement("ioi_offer_volatility") ? msg.GetElementAsFloat64("ioi_offer_volatility") : 0;
                    String ioi_offer_notes = msg.HasElement("ioi_offer_notes") ? msg.GetElementAsString("ioi_offer_notes") : "";
                    int ioi_offer_qualifiers_count = msg.HasElement("ioi_offer_qualifiers_count") ? msg.GetElementAsInt32("ioi_offer_qualifiers_count") : 0;
                    String ioi_offer_qualifiers_0 = msg.HasElement("ioi_offer_qualifiers_0") ? msg.GetElementAsString("ioi_offer_qualifiers_0") : "";
                    String ioi_offer_qualifiers_1 = msg.HasElement("ioi_offer_qualifiers_1") ? msg.GetElementAsString("ioi_offer_qualifiers_1") : "";
                    String ioi_offer_qualifiers_2 = msg.HasElement("ioi_offer_qualifiers_2") ? msg.GetElementAsString("ioi_offer_qualifiers_2") : "";
                    String ioi_offer_qualifiers_3 = msg.HasElement("ioi_offer_qualifiers_3") ? msg.GetElementAsString("ioi_offer_qualifiers_3") : "";
                    String ioi_offer_qualifiers_4 = msg.HasElement("ioi_offer_qualifiers_4") ? msg.GetElementAsString("ioi_offer_qualifiers_4") : "";

                    String ioi_routing_strategy_name = msg.HasElement("ioi_routing_strategy_name") ? msg.GetElementAsString("ioi_routing_strategy_name") : "";
                    String ioi_routing_strategy_brief = msg.HasElement("ioi_routing_strategy_brief") ? msg.GetElementAsString("ioi_routing_strategy_brief") : "";
                    String ioi_routing_strategy_detailed = msg.HasElement("ioi_routing_strategy_detailed") ? msg.GetElementAsString("ioi_routing_strategy_detailed") : "";
                    String ioi_routing_customId = msg.HasElement("ioi_routing_customId") ? msg.GetElementAsString("ioi_routing_customId") : "";
                    String ioi_routing_broker = msg.HasElement("ioi_routing_broker") ? msg.GetElementAsString("ioi_routing_broker") : "";

                    DateTime ioi_sentTime = msg.HasElement("ioi_sentTime") ? msg.GetElementAsDatetime("ioi_sentTime").ToSystemDateTime() : Convert.ToDateTime(null);

                    String change = msg.HasElement("change") ? msg.GetElementAsString("change") : "";

                    System.Console.WriteLine("IOI MESSAGE: CorrelationID(" + msg.CorrelationID + ")");

                    System.Console.WriteLine("ioi_instrument_type: " + ioi_instrument_type);
                    System.Console.WriteLine("ioi_instrument_option_legs_count: " + ioi_instrument_option_legs_count);
                    System.Console.WriteLine("ioi_instrument_option_legs_0_strike: " + ioi_instrument_option_legs_0_strike);
                    System.Console.WriteLine("ioi_instrument_option_legs_0_expiry: " + ioi_instrument_option_legs_0_expiry);
                    System.Console.WriteLine("ioi_instrument_option_legs_0_type: " + ioi_instrument_option_legs_0_type);
                    System.Console.WriteLine("ioi_instrument_option_legs_0_ratio: " + ioi_instrument_option_legs_0_ratio);
                    System.Console.WriteLine("ioi_instrument_option_legs_0_underlying_type: " + ioi_instrument_option_legs_0_underlying_type);
                    System.Console.WriteLine("ioi_instrument_option_legs_0_underlying_ticker: " + ioi_instrument_option_legs_0_underlying_ticker);
                    System.Console.WriteLine("ioi_instrument_option_legs_0_underlying_figi: " + ioi_instrument_option_legs_0_underlying_figi);
                    System.Console.WriteLine("ioi_instrument_option_legs_0_exchange: " + ioi_instrument_option_legs_0_exchange);
                    System.Console.WriteLine("ioi_instrument_option_legs_0_style: " + ioi_instrument_option_legs_0_style);
                    System.Console.WriteLine("ioi_instrument_option_legs_0_futureRefDate: " + ioi_instrument_option_legs_0_futureRefDate);
                    System.Console.WriteLine("ioi_instrument_option_legs_0_delta: " + ioi_instrument_option_legs_0_delta);
                    System.Console.WriteLine("ioi_instrument_option_legs_1_strike: " + ioi_instrument_option_legs_1_strike);
                    System.Console.WriteLine("ioi_instrument_option_legs_1_expiry: " + ioi_instrument_option_legs_1_expiry);
                    System.Console.WriteLine("ioi_instrument_option_legs_1_type: " + ioi_instrument_option_legs_1_type);
                    System.Console.WriteLine("ioi_instrument_option_legs_1_ratio: " + ioi_instrument_option_legs_1_ratio);
                    System.Console.WriteLine("ioi_instrument_option_legs_1_underlying_type: " + ioi_instrument_option_legs_1_underlying_type);
                    System.Console.WriteLine("ioi_instrument_option_legs_1_underlying_ticker: " + ioi_instrument_option_legs_1_underlying_ticker);
                    System.Console.WriteLine("ioi_instrument_option_legs_1_underlying_figi: " + ioi_instrument_option_legs_1_underlying_figi);
                    System.Console.WriteLine("ioi_instrument_option_legs_1_exchange: " + ioi_instrument_option_legs_1_exchange);
                    System.Console.WriteLine("ioi_instrument_option_legs_1_style: " + ioi_instrument_option_legs_1_style);
                    System.Console.WriteLine("ioi_instrument_option_legs_1_futureRefDate: " + ioi_instrument_option_legs_1_futureRefDate);
                    System.Console.WriteLine("ioi_instrument_option_legs_1_delta: " + ioi_instrument_option_legs_1_delta);
                    System.Console.WriteLine("ioi_instrument_option_legs_2_strike: " + ioi_instrument_option_legs_2_strike);
                    System.Console.WriteLine("ioi_instrument_option_legs_2_expiry: " + ioi_instrument_option_legs_2_expiry);
                    System.Console.WriteLine("ioi_instrument_option_legs_2_type: " + ioi_instrument_option_legs_2_type);
                    System.Console.WriteLine("ioi_instrument_option_legs_2_ratio: " + ioi_instrument_option_legs_2_ratio);
                    System.Console.WriteLine("ioi_instrument_option_legs_2_underlying_type: " + ioi_instrument_option_legs_2_underlying_type);
                    System.Console.WriteLine("ioi_instrument_option_legs_2_underlying_ticker: " + ioi_instrument_option_legs_2_underlying_ticker);
                    System.Console.WriteLine("ioi_instrument_option_legs_2_underlying_figi: " + ioi_instrument_option_legs_2_underlying_figi);
                    System.Console.WriteLine("ioi_instrument_option_legs_2_exchange: " + ioi_instrument_option_legs_2_exchange);
                    System.Console.WriteLine("ioi_instrument_option_legs_2_style: " + ioi_instrument_option_legs_2_style);
                    System.Console.WriteLine("ioi_instrument_option_legs_2_futureRefDate: " + ioi_instrument_option_legs_2_futureRefDate);
                    System.Console.WriteLine("ioi_instrument_option_legs_2_delta: " + ioi_instrument_option_legs_2_delta);
                    System.Console.WriteLine("ioi_instrument_option_legs_3_strike: " + ioi_instrument_option_legs_3_strike);
                    System.Console.WriteLine("ioi_instrument_option_legs_3_expiry: " + ioi_instrument_option_legs_3_expiry);
                    System.Console.WriteLine("ioi_instrument_option_legs_3_type: " + ioi_instrument_option_legs_3_type);
                    System.Console.WriteLine("ioi_instrument_option_legs_3_ratio: " + ioi_instrument_option_legs_3_ratio);
                    System.Console.WriteLine("ioi_instrument_option_legs_3_underlying_type: " + ioi_instrument_option_legs_3_underlying_type);
                    System.Console.WriteLine("ioi_instrument_option_legs_3_underlying_ticker: " + ioi_instrument_option_legs_3_underlying_ticker);
                    System.Console.WriteLine("ioi_instrument_option_legs_3_underlying_figi: " + ioi_instrument_option_legs_3_underlying_figi);
                    System.Console.WriteLine("ioi_instrument_option_legs_3_exchange: " + ioi_instrument_option_legs_3_exchange);
                    System.Console.WriteLine("ioi_instrument_option_legs_3_style: " + ioi_instrument_option_legs_3_style);
                    System.Console.WriteLine("ioi_instrument_option_legs_3_futureRefDate: " + ioi_instrument_option_legs_3_futureRefDate);
                    System.Console.WriteLine("ioi_instrument_option_legs_3_delta: " + ioi_instrument_option_legs_3_delta);
                    System.Console.WriteLine("ioi_instrument_option_structure: " + ioi_instrument_option_structure);
                    System.Console.WriteLine("ioi_instrument_stock_security_ticker: " + ioi_instrument_stock_security_ticker);
                    System.Console.WriteLine("ioi_instrument_stock_security_figi: " + ioi_instrument_stock_security_figi);
                    System.Console.WriteLine("ioi_goodUntil: " + ioi_goodUntil);
                    System.Console.WriteLine("ioi_bid_price_type: " + ioi_bid_price_type);
                    System.Console.WriteLine("ioi_bid_price_fixed_price: " + ioi_bid_price_fixed_price);
                    System.Console.WriteLine("ioi_bid_price_fixed_currency: " + ioi_bid_price_fixed_currency);
                    System.Console.WriteLine("ioi_bid_price_pegged_offsetAmount: " + ioi_bid_price_pegged_offsetAmount);
                    System.Console.WriteLine("ioi_bid_price_pegged_offsetFrom: " + ioi_bid_price_pegged_offsetFrom);
                    System.Console.WriteLine("ioi_bid_price_pegged_limitPrice: " + ioi_bid_price_pegged_limitPrice);
                    System.Console.WriteLine("ioi_bid_price_reference: " + ioi_bid_price_reference);
                    System.Console.WriteLine("ioi_bid_price_moneyness: " + ioi_bid_price_moneyness);
                    System.Console.WriteLine("ioi_bid_size_type: " + ioi_bid_size_type);
                    System.Console.WriteLine("ioi_bid_size_quantity: " + ioi_bid_size_quantity);
                    System.Console.WriteLine("ioi_bid_size_quality: " + ioi_bid_size_quality);
                    System.Console.WriteLine("ioi_bid_referencePrice_price: " + ioi_bid_referencePrice_price);
                    System.Console.WriteLine("ioi_bid_referencePrice_currency: " + ioi_bid_referencePrice_currency);
                    System.Console.WriteLine("ioi_bid_volatility: " + ioi_bid_volatility);
                    System.Console.WriteLine("ioi_bid_notes: " + ioi_bid_notes);
                    System.Console.WriteLine("ioi_bid_qualifiers_count: " + ioi_bid_qualifiers_count);
                    System.Console.WriteLine("ioi_bid_qualifiers_0: " + ioi_bid_qualifiers_0);
                    System.Console.WriteLine("ioi_bid_qualifiers_1: " + ioi_bid_qualifiers_1);
                    System.Console.WriteLine("ioi_bid_qualifiers_2: " + ioi_bid_qualifiers_2);
                    System.Console.WriteLine("ioi_bid_qualifiers_3: " + ioi_bid_qualifiers_3);
                    System.Console.WriteLine("ioi_bid_qualifiers_4: " + ioi_bid_qualifiers_4);
                    System.Console.WriteLine("ioi_offer_price_type: " + ioi_offer_price_type);
                    System.Console.WriteLine("ioi_offer_price_fixed_price: " + ioi_offer_price_fixed_price);
                    System.Console.WriteLine("ioi_offer_price_fixed_currency: " + ioi_offer_price_fixed_currency);
                    System.Console.WriteLine("ioi_offer_price_pegged_offsetAmount: " + ioi_offer_price_pegged_offsetAmount);
                    System.Console.WriteLine("ioi_offer_price_pegged_offsetFrom: " + ioi_offer_price_pegged_offsetFrom);
                    System.Console.WriteLine("ioi_offer_price_pegged_limitPrice: " + ioi_offer_price_pegged_limitPrice);
                    System.Console.WriteLine("ioi_offer_price_reference: " + ioi_offer_price_reference);
                    System.Console.WriteLine("ioi_offer_price_moneyness: " + ioi_offer_price_moneyness);
                    System.Console.WriteLine("ioi_offer_size_type: " + ioi_offer_size_type);
                    System.Console.WriteLine("ioi_offer_size_quantity: " + ioi_offer_size_quantity);
                    System.Console.WriteLine("ioi_offer_size_quality: " + ioi_offer_size_quality);
                    System.Console.WriteLine("ioi_offer_referencePrice_price: " + ioi_offer_referencePrice_price);
                    System.Console.WriteLine("ioi_offer_referencePrice_currency: " + ioi_offer_referencePrice_currency);
                    System.Console.WriteLine("ioi_offer_volatility: " + ioi_offer_volatility);
                    System.Console.WriteLine("ioi_offer_notes: " + ioi_offer_notes);
                    System.Console.WriteLine("ioi_offer_qualifiers_count: " + ioi_offer_qualifiers_count);
                    System.Console.WriteLine("ioi_offer_qualifiers_0: " + ioi_offer_qualifiers_0);
                    System.Console.WriteLine("ioi_offer_qualifiers_1: " + ioi_offer_qualifiers_1);
                    System.Console.WriteLine("ioi_offer_qualifiers_2: " + ioi_offer_qualifiers_2);
                    System.Console.WriteLine("ioi_offer_qualifiers_3: " + ioi_offer_qualifiers_3);
                    System.Console.WriteLine("ioi_offer_qualifiers_4: " + ioi_offer_qualifiers_4);
                    System.Console.WriteLine("ioi_routing_strategy_name: " + ioi_routing_strategy_name);
                    System.Console.WriteLine("ioi_routing_strategy_brief: " + ioi_routing_strategy_brief);
                    System.Console.WriteLine("ioi_routing_strategy_detailed: " + ioi_routing_strategy_detailed);
                    System.Console.WriteLine("ioi_routing_customId: " + ioi_routing_customId);
                    System.Console.WriteLine("ioi_routing_broker: " + ioi_routing_broker);
                    System.Console.WriteLine("ioi_sentTime: " + ioi_sentTime);
                    System.Console.WriteLine("change: " + change);
                }
                else
                {
                    System.Console.Error.WriteLine("Error: Unexpected message");
                }

            }
        }

        private void processMiscEvents(Event evt, Session session)
        {
            System.Console.WriteLine("Processing " + evt.Type);

            foreach (Message msg in evt)
            {
                System.Console.WriteLine("MESSAGE: " + msg);
            }
        }

        private void sendAuthRequest(Session session, String emrsUser, String ipAddress)
        {
            Service authService = session.GetService(d_auth);
            Request authReq = authService.CreateAuthorizationRequest();

            authReq.Set("emrsId", emrsUser);
            authReq.Set("ipAddress", ipAddress);

            this.identity = session.CreateIdentity();

            try
            {
                System.Console.WriteLine("Sending Auth Request:" + authReq.ToString());
                session.SendAuthorizationRequest(authReq, this.identity, new CorrelationID(identity));
                System.Console.WriteLine("Sent Auth request.");
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine("Unable to send authorization request: " + e.Message);
            }
        }

        private void createIOISubscription(Session session)
        {
            System.Console.WriteLine("Create IOI subscription");

            // Create the topic string for the ioi subscription.
            String ioiTopic = d_ioi + "/ioi";

            ioiSubscriptionID = new CorrelationID();

            ioiSubscription = new Subscription(ioiTopic, ioiSubscriptionID);
            System.Console.WriteLine("IOI Topic: " + ioiTopic);
            List<Subscription> subscriptions = new List<Subscription>();
            subscriptions.Add(ioiSubscription);

            try
            {
                System.Console.WriteLine("Submitting subscription request...");
                session.Subscribe(subscriptions,this.identity);
            }
            catch (Exception ex)
            {
                System.Console.Error.WriteLine("Failed to create IOI subscription: " + ex.Message);
            }

        }
    }
}
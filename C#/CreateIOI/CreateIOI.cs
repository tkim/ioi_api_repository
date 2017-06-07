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
using System;

namespace com.bloomberg.emsx.samples
{
    public class CreateIOI    {

        private static readonly Name SESSION_STARTED = new Name("SessionStarted");
        private static readonly Name SESSION_STARTUP_FAILURE = new Name("SessionStartupFailure");
        private static readonly Name SERVICE_OPENED = new Name("ServiceOpened");
        private static readonly Name SERVICE_OPEN_FAILURE = new Name("ServiceOpenFailure");

        private string d_service;
        private string d_host;
        private int d_port;

        private static bool quit = false;

        private CorrelationID requestID;

        public static void Main(String[] args)
        {
            System.Console.WriteLine("Bloomberg - EMSX API Example - CreateIOI\n");

            CreateIOI example = new CreateIOI();
            example.run(args);

            while (!quit) { };

            System.Console.WriteLine("Press Any Key...");
            System.Console.ReadKey();
        }

        public CreateIOI()
        {

            // Define the service required, in this case the beta service, 
            // and the values to be used by the SessionOptions object
            // to identify IP/port of the back-end process.

            d_service = "//blp/ioiapi-beta-request";
            d_host = "localhost";
            d_port = 8194;

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
            try
            {
                switch (evt.Type)
                {
                    case Event.EventType.SESSION_STATUS:
                        processSessionEvent(evt, session);
                        break;
                    case Event.EventType.SERVICE_STATUS:
                        processServiceEvent(evt, session);
                        break;
                    case Event.EventType.RESPONSE:
                        processResponseEvent(evt, session);
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

        private void processSessionEvent(Event evt, Session session)
        {
            System.Console.WriteLine("\nProcessing " + evt.Type);

            foreach (Message msg in evt)
            {
                if (msg.MessageType.Equals(SESSION_STARTED))
                {
                    System.Console.WriteLine("Session started...");
                    session.OpenServiceAsync(d_service);
                }
                else if (msg.MessageType.Equals(SESSION_STARTUP_FAILURE))
                {
                    System.Console.Error.WriteLine("Error: Session startup failed");
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
                    System.Console.WriteLine("Service opened...");

                    Service service = session.GetService(d_service);

                    Request request = service.CreateRequest("createIoi");

                    Element ioi = request.GetElement("ioi");

                    // Set the good-until time of this option to 15 minutes from now
                    ioi.SetElement("goodUntil", new Bloomberglp.Blpapi.Datetime(DateTime.Now.AddSeconds(900)));

                    // Create the option
                    Element option = ioi.GetElement("instrument").SetChoice("option");

                    option.SetElement("structure", "CallSpread");

                    // This option has two legs. Create the first leg
                    Element leg1 = option.GetElement("legs").AppendElement();
                    leg1.SetElement("type", "Call");
                    leg1.SetElement("strike", 230);
                    leg1.SetElement("expiry", "2017-10-01T00:00:00.000+00:00");
                    leg1.SetElement("style", "European");
                    leg1.SetElement("ratio", +1.00);
                    leg1.SetElement("exchange", "LN");
                    leg1.GetElement("underlying").SetChoice("ticker");
                    leg1.GetElement("underlying").SetElement("ticker", "VOD LN Equity");

                    // Create the second leg
                    Element leg2 = option.GetElement("legs").AppendElement();
                    leg1.SetElement("type", "Call");
                    leg2.SetElement("strike", 240);
                    leg2.SetElement("expiry", "2017-10-01T00:00:00.000+00:00");
                    leg2.SetElement("style", "European");
                    leg2.SetElement("ratio", -1.25);
                    leg2.SetElement("exchange", "LN");
                    leg2.GetElement("underlying").SetChoice("ticker");
                    leg2.GetElement("underlying").SetElement("ticker", "VOD LN Equity");

                    // Create a quote consisting of a bid and an offer
                    Element bid = ioi.GetElement("bid");
                    bid.GetElement("delta").SetValue(.0041);
                    bid.GetElement("size").GetElement("quantity").SetValue(1000);
                    bid.GetElement("referencePrice").SetElement("price", 202.15);
                    bid.GetElement("referencePrice").SetElement("currency", "GBp");
                    bid.SetElement("notes", "bid notes");

                    // Set the offer
                    Element offer = ioi.GetElement("offer");
                    offer.GetElement("price").SetChoice("fixed");
                    offer.GetElement("price").GetElement("fixed").GetElement("price").SetValue(83.64);
                    offer.GetElement("size").SetChoice("quantity");
                    offer.GetElement("size").GetElement("quantity").SetValue(2000);
                    offer.GetElement("referencePrice").SetElement("price", 202.15);
                    offer.GetElement("referencePrice").SetElement("currency", "GBP");
                    offer.SetElement("notes", "offer notes");

                    // Set targets
                    Element includes = ioi.GetElement("targets").GetElement("includes");

                    Element t1 = includes.AppendElement();
                    t1.SetChoice("acronym");
                    t1.SetElement("acronym", "BLPA");

                    Element t2 = includes.AppendElement();
                    t2.SetChoice("acronym");
                    t2.SetElement("acronym", "BLPB");

                    System.Console.WriteLine("Request: " + request.ToString());

                    requestID = new CorrelationID();

                    // Submit the request
                    try
                    {
                        session.SendRequest(request, requestID);
                    }
                    catch (Exception ex)
                    {
                        System.Console.Error.WriteLine("Failed to send the request: " + ex.Message);
                    }

                }
                else if (msg.MessageType.Equals(SERVICE_OPEN_FAILURE))
                {
                    System.Console.Error.WriteLine("Error: Service failed to open");
                }
            }
        }

        private void processResponseEvent(Event evt, Session session)
        {
            System.Console.WriteLine("Received Event: " + evt.Type);

            foreach (Message msg in evt)
            {

                System.Console.WriteLine("MESSAGE: " + msg.ToString());
                System.Console.WriteLine("CORRELATION ID: " + msg.CorrelationID);

                if (evt.Type == Event.EventType.RESPONSE && msg.CorrelationID == requestID)
                {
                    System.Console.WriteLine("Message Type: " + msg.MessageType);
                    if (msg.MessageType.Equals("handle"))
                    {
                        String val= msg.GetElementAsString("value");
                        System.Console.WriteLine("Response: Value=" + val);
                    }

                    quit = true;
                    session.Stop();
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
    }
}
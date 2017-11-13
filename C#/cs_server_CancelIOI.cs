﻿/* Copyright 2017. Bloomberg Finance L.P.
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
using Identity = Bloomberglp.Blpapi.Identity;
using System;

namespace com.bloomberg.ioi.samples
{
    public class CancelIOI
    {

        private static readonly Name SLOW_CONSUMER_WARNING = new Name("SlowConsumerWarning");
        private static readonly Name SLOW_CONSUMER_WARNING_CLEARED = new Name("SlowConsumerWarningCleared");
        private static readonly Name SESSION_STARTED = new Name("SessionStarted");
        private static readonly Name SESSION_STARTUP_FAILURE = new Name("SessionStartupFailure");
        private static readonly Name SERVICE_OPENED = new Name("ServiceOpened");
        private static readonly Name SERVICE_OPEN_FAILURE = new Name("ServiceOpenFailure");
        private static readonly Name AUTHORIZATION_SUCCESS = new Name("AuthorizationSuccess");
        private static readonly Name AUTHORIZATION_FAILURE = new Name("AuthorizationFailure");
        private static readonly Name ERROR_INFO = new Name("ErrorInfo");
        private static readonly Name CANCEL_IOI = new Name("CancelIOI");

        private string d_ioi;
        private string d_host;
        private int d_port;

        private string d_auth;
        private string d_user;
        private string d_ip;

        private Identity identity;

        private static bool quit = false;

        private CorrelationID requestID;

        public static void Main(String[] args)
        {
            System.Console.WriteLine("Bloomberg - IOI API Example - Server - CancelIOI\n");

            CancelIOI example = new CancelIOI();
            example.run(args);

            while (!quit) { };

            System.Console.WriteLine("Press Any Key...");
            System.Console.ReadKey();
        }

        public CancelIOI()
        {

            // Define the service required, in this case the beta service, 
            // and the values to be used by the SessionOptions object
            // to identify IP/port of the back-end process.

            // We are also creating the values need for authentication on the server

            d_ioi = "//blp/ioiapi-beta-request";
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

                    sendCancelIOI(session, d_ioi);
                }
                else if (msg.MessageType.Equals(AUTHORIZATION_FAILURE))
                {
                    System.Console.WriteLine("Authorisation failed...");
                    System.Console.WriteLine(msg.ToString());

                    // Here you can insert code to automatically retry the authorisation if required
                }
                else if (msg.CorrelationID == requestID)
                {
                    System.Console.WriteLine(msg.MessageType + ">>" + msg.ToString());

                    if (msg.MessageType.Equals("handle"))
                    {
                        String val = msg.GetElementAsString("value");
                        System.Console.WriteLine("Response: Value=" + val);
                    }
                    else
                    {
                        System.Console.WriteLine("Unexpected message...");
                        System.Console.WriteLine(msg.ToString());
                    }

                    quit = true;
                }
                else
                {
                    System.Console.WriteLine("Unexpected message...");
                    System.Console.WriteLine(msg.ToString());
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


        private void sendCancelIOI(Session session, String ioiSvc)
        {
            Service service = session.GetService(ioiSvc);
            Request request = service.CreateRequest("cancelIoi");

            Element handle = request.GetElement("handle");
            handle.SetElement("value", "a9276721-12c3-4980-8487-97de558b6c71");

            System.Console.WriteLine("Sending Request: " + request.ToString());

            requestID = new CorrelationID();

            // Submit the request
            try
            {
                session.SendRequest(request, identity, requestID);
                System.Console.WriteLine("Request Sent.");
            }
            catch (Exception ex)
            {
                System.Console.Error.WriteLine("Failed to send the request: " + ex.Message);
            }
        }
    }
}


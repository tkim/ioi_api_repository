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

package com.bloomberg.ioi.samples;

import com.bloomberglp.blpapi.Name;
import com.bloomberglp.blpapi.SessionOptions;
import com.bloomberglp.blpapi.Session;
import com.bloomberglp.blpapi.Service;
import com.bloomberglp.blpapi.Request;
import com.bloomberglp.blpapi.Element;
import com.bloomberglp.blpapi.CorrelationID;
import com.bloomberglp.blpapi.Event;
import com.bloomberglp.blpapi.Message;
import com.bloomberglp.blpapi.MessageIterator;
import com.bloomberglp.blpapi.EventHandler;
import java.io.IOException;
import java.time.Instant;

public class Java_dapi_CancelIOI
{

    private static final Name SLOW_CONSUMER_WARNING = new Name("SlowConsumerWarning");
    private static final Name SLOW_CONSUMER_WARNING_CLEARED = new Name("SlowConsumerWarningCleared");
    private static final Name SESSION_STARTED = new Name("SessionStarted");
    private static final Name SESSION_STARTUP_FAILURE = new Name("SessionStartupFailure");
    private static final Name SERVICE_OPENED = new Name("ServiceOpened");
    private static final Name SERVICE_OPEN_FAILURE = new Name("ServiceOpenFailure");

    private String d_ioi;
    private String d_host;
    private int d_port;

    private static volatile boolean quit = false;

    private CorrelationID requestID;

    public static void main(String[] args) throws IOException
    {
        System.out.println("Bloomberg - IOI API Example - DesktopAPI - CancelIOI\n");

        Java_dapi_CancelIOI example = new Java_dapi_CancelIOI();
        example.run(args);

        while (!quit) { };

        System.out.println("Press Enter to terminate...");
        System.in.read();
    }

    public Java_dapi_CancelIOI()
    {

        // Define the service required, in this case the beta service, 
        // and the values to be used by the SessionOptions object
        // to identify IP/port of the back-end process.

        d_ioi = "//blp/ioiapi-beta-request";
        d_host = "localhost";
        d_port = 8194;
    }

    private void run(String[] args) throws IOException
    {

        SessionOptions d_sessionOptions = new SessionOptions();
        d_sessionOptions.setServerHost(d_host);
        d_sessionOptions.setServerPort(d_port);

        Session session = new Session(d_sessionOptions, new IOIEventHandler(this));

        session.startAsync();

    }

    class IOIEventHandler implements EventHandler 
    {

    	Java_dapi_CancelIOI example;
    	
    	public IOIEventHandler(Java_dapi_CancelIOI example) {
    		this.example = example;
    	}
    	
	    public void processEvent(Event evt, Session session)
	    {
	        try
	        {
	            switch (evt.eventType().intValue())
	            {
	                case Event.EventType.Constants.ADMIN:
	                    processAdminEvent(evt, session);
	                    break;
	                case Event.EventType.Constants.SESSION_STATUS:
	                    processSessionEvent(evt, session);
	                    break;
	                case Event.EventType.Constants.SERVICE_STATUS:
	                    processServiceEvent(evt, session);
	                    break;
	                case Event.EventType.Constants.RESPONSE:
	                    processResponseEvent(evt, session);
	                    break;
	                default:
	                    processMiscEvents(evt, session);
	                    break;
	            }
	        }
	        catch (Exception e)
	        {
	            System.err.println(e);
	        }
	    }
	
	    private void processAdminEvent(Event evt, Session session)
	    {
	    	System.out.println("Processing " + evt.eventType().toString());
	
			MessageIterator msgIter = evt.messageIterator();

			while (msgIter.hasNext()) {
	            
				Message msg = msgIter.next();

				if (msg.messageType().equals(SLOW_CONSUMER_WARNING))
	            {
					System.err.println("Warning: Entered Slow Consumer status");
	            }
	            else if (msg.messageType().equals(SLOW_CONSUMER_WARNING_CLEARED))
	            {
	            	System.out.println("Slow consumer status cleared");
	            }
	        }
	    }
	
	    private void processSessionEvent(Event evt, Session session) throws IOException
	    {
	    	System.out.println("Processing " + evt.eventType().toString());
	
			MessageIterator msgIter = evt.messageIterator();

			while (msgIter.hasNext()) {
	            
				Message msg = msgIter.next();

				if (msg.messageType().equals(SESSION_STARTED))
	            {
					System.out.println("Session started...");
	                session.openServiceAsync(d_ioi);
	            }
	            else if (msg.messageType().equals(SESSION_STARTUP_FAILURE))
	            {
	            	System.err.println("Error: Session startup failed");
	            }
	        }
	    }
	
	    private void processServiceEvent(Event evt, Session session)
	    {
	
	    	System.out.println("Processing " + evt.eventType().toString());
	
			MessageIterator msgIter = evt.messageIterator();

			while (msgIter.hasNext()) {
	            
				Message msg = msgIter.next();

				if (msg.messageType().equals(SERVICE_OPENED))
	            {
					System.out.println("IOIAPI Service opened... Sending request");
	                sendCancelIOI(session, d_ioi);
	            }
	            else if (msg.messageType().equals(SERVICE_OPEN_FAILURE))
	            {
	            	System.err.println("Error: Service failed to open");
	            }
	        }
	    }
	
	    private void processResponseEvent(Event evt, Session session)
	    {
	    	System.out.println("Processing " + evt.eventType().toString());
	    	
			MessageIterator msgIter = evt.messageIterator();

			while (msgIter.hasNext()) {
	            
				Message msg = msgIter.next();

				if (msg.correlationID() == requestID)
	            {
					System.out.println(msg.messageType().toString() + ">>" + msg.toString());
	
	                if (msg.messageType().equals("handle"))
	                {
	                    String val = msg.getElementAsString("value");
	                    System.out.println("Response: Value=" + val);
	                }
	                else
	                {
	                	System.out.println("Unexpected message...");
	                	System.out.println(msg.toString());
	                }
	
	                this.example.quit = true;
	            }
	        }
	    }
	
	    private void processMiscEvents(Event evt, Session session)
	    {
	    	System.out.println("Processing " + evt.eventType().toString());
	    	
			MessageIterator msgIter = evt.messageIterator();

			while (msgIter.hasNext()) {
	            
				Message msg = msgIter.next();

				System.out.println("MESSAGE: " + msg.toString());
	        }
	    }
	
	    private void sendCancelIOI(Session session, String ioiSvc)
	    {
	        Service service = session.getService(ioiSvc);
	        Request request = service.createRequest("cancelIoi");
	
            Element handle = request.getElement("handle");
            handle.setElement("value", "485caead-184f-45b6-bee7-c8999822d34a");

	        System.out.println("Sending Request: " + request.toString());
	
	        requestID = new CorrelationID();
	
	        // Submit the request
	        try
	        {
	            session.sendRequest(request, requestID);
	            System.out.println("Request Sent.");
	        }
	        catch (Exception ex)
	        {
	            System.err.println("Failed to send the request: " + ex.getMessage());
	        }
	    }
	}
}

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
#include "BlpThreadUtil.h"

#include <blpapi_correlationid.h>
#include <blpapi_element.h>
#include <blpapi_event.h>
#include <blpapi_message.h>
#include <blpapi_name.h>
#include <blpapi_session.h>
#include <blpapi_subscriptionlist.h>
#include <blpapi_datetime.h>

#include <cassert>
#include <iostream>
#include <set>
#include <sstream>
#include <string>
#include <time.h>
#include <vector>

using namespace BloombergLP;
using namespace blpapi;

namespace {

	Name SESSION_STARTED("SessionStarted");
	Name SESSION_STARTUP_FAILURE("SessionStartupFailure");
	Name SERVICE_OPENED("ServiceOpened");
	Name SERVICE_OPEN_FAILURE("ServiceOpenFailure");

	const std::string d_service("//blp/ioiapi-beta-request");
	CorrelationId requestID;
}

class ConsoleOut
{
private:
	std::ostringstream  d_buffer;
	Mutex              *d_consoleLock;
	std::ostream&       d_stream;

	// NOT IMPLEMENTED
	ConsoleOut(const ConsoleOut&);
	ConsoleOut& operator=(const ConsoleOut&);

public:
	explicit ConsoleOut(Mutex         *consoleLock,
		std::ostream&  stream = std::cout)
		: d_consoleLock(consoleLock)
		, d_stream(stream)
	{}

	~ConsoleOut() {
		MutexGuard guard(d_consoleLock);
		d_stream << d_buffer.str();
		d_stream.flush();
	}

	template <typename T>
	std::ostream& operator<<(const T& value) {
		return d_buffer << value;
	}

	std::ostream& stream() {
		return d_buffer;
	}
};

struct SessionContext
{
	Mutex            d_consoleLock;
	Mutex            d_mutex;
	bool             d_isStopped;
	SubscriptionList d_subscriptions;

	SessionContext()
		: d_isStopped(false)
	{
	}
};

class EMSXEventHandler : public EventHandler
{
	bool                     d_isSlow;
	SubscriptionList         d_pendingSubscriptions;
	std::set<CorrelationId>  d_pendingUnsubscribe;
	SessionContext          *d_context_p;
	Mutex                   *d_consoleLock_p;

	bool processSessionEvent(const Event &event, Session *session)
	{

		ConsoleOut(d_consoleLock_p) << "Processing SESSION_EVENT" << std::endl;

		MessageIterator msgIter(event);
		while (msgIter.next()) {

			Message msg = msgIter.message();

			if (msg.messageType() == SESSION_STARTED) {
				ConsoleOut(d_consoleLock_p) << "Session started..." << std::endl;
				session->openServiceAsync(d_service.c_str());
			}
			else if (msg.messageType() == SESSION_STARTUP_FAILURE) {
				ConsoleOut(d_consoleLock_p) << "Session startup failed" << std::endl;
				return false;
			}
		}
		return true;
	}

	bool processServiceEvent(const Event &event, Session *session)
	{

		ConsoleOut(d_consoleLock_p) << "Processing SERVICE_EVENT" << std::endl;

		MessageIterator msgIter(event);
		while (msgIter.next()) {

			Message msg = msgIter.message();

			if (msg.messageType() == SERVICE_OPENED) {
				ConsoleOut(d_consoleLock_p) << "Service opened..." << std::endl;

				Service service = session->getService(d_service.c_str());

				Request request = service.createRequest("createIoi");

				Element ioi = request.getElement("ioi");

				// Set the good-until time of this option to 15 minutes from now
				ioi.setElement("goodUntil", "2017-06-06T16:19:00.000+01:00");

				// Create the option
				Element option = ioi.getElement("instrument").setChoice("option");

				option.setElement("structure", "CallSpread");

				// This option has two legs. Create the first leg
				Element leg1 = option.getElement("legs").appendElement();
				leg1.setElement("type", "Call");
				leg1.setElement("strike", 230);
				leg1.setElement("expiry", "2017-10-01T00:00:00.000+00:00");
				leg1.setElement("style", "European");
				leg1.setElement("ratio", +1.00);
				leg1.setElement("exchange", "LN");
				leg1.getElement("underlying").setChoice("ticker");
				leg1.getElement("underlying").setElement("ticker", "VOD LN Equity");

				// Create the second leg
				Element leg2 = option.getElement("legs").appendElement();
				leg1.setElement("type", "Call");
				leg2.setElement("strike", 240);
				leg2.setElement("expiry", "2017-10-01T00:00:00.000+00:00");
				leg2.setElement("style", "European");
				leg2.setElement("ratio", -1.25);
				leg2.setElement("exchange", "LN");
				leg2.getElement("underlying").setChoice("ticker");
				leg2.getElement("underlying").setElement("ticker", "VOD LN Equity");

				// Create a quote consisting of a bid and an offer
				Element bid = ioi.getElement("bid");
				bid.getElement("delta").setValue(.0041);
				bid.getElement("size").getElement("quantity").setValue(1000);
				bid.getElement("referencePrice").setElement("price", 202.15);
				bid.getElement("referencePrice").setElement("currency", "GBp");
				bid.setElement("notes", "bid notes");

				// Set the offer
				Element offer = ioi.getElement("offer");
				offer.getElement("price").setChoice("fixed");
				offer.getElement("price").getElement("fixed").getElement("price").setValue(83.64);
				offer.getElement("size").setChoice("quantity");
				offer.getElement("size").getElement("quantity").setValue(2000);
				offer.getElement("referencePrice").setElement("price", 202.15);
				offer.getElement("referencePrice").setElement("currency", "GBP");
				offer.setElement("notes", "offer notes");

				// Set targets
				Element includes = ioi.getElement("targets").getElement("includes");

				Element t1 = includes.appendElement();
				t1.setChoice("acronym");
				t1.setElement("acronym", "BLPA");

				Element t2 = includes.appendElement();
				t2.setChoice("acronym");
				t2.setElement("acronym", "BLPB");

				ConsoleOut(d_consoleLock_p) << "Request: " << request << std::endl;

				requestID = CorrelationId();

				session->sendRequest(request, requestID);

			}
			else if (msg.messageType() == SERVICE_OPEN_FAILURE) {
				ConsoleOut(d_consoleLock_p) << "Error: Service failed to open" << std::endl;
				return false;
			}

		}
		return true;
	}

	bool processResponseEvent(const Event &event, Session *session)
	{
		ConsoleOut(d_consoleLock_p) << "Processing RESPONSE_EVENT" << std::endl;

		MessageIterator msgIter(event);
		while (msgIter.next()) {

			Message msg = msgIter.message();

			ConsoleOut(d_consoleLock_p) << "MESSAGE: " << msg << std::endl;

			if (msg.messageType() == "handle") {

				std::string val = msg.getElementAsString("value");

				ConsoleOut(d_consoleLock_p) << "Response: Value=" << val << std::endl;
			}

		}
		return true;
	}

	bool processMiscEvents(const Event &event)
	{
		ConsoleOut(d_consoleLock_p) << "Processing UNHANDLED event" << std::endl;

		MessageIterator msgIter(event);
		while (msgIter.next()) {
			Message msg = msgIter.message();
			ConsoleOut(d_consoleLock_p) << msg.messageType().string() << "\n" << msg << std::endl;
		}
		return true;
	}

public:
	EMSXEventHandler(SessionContext *context)
		: d_isSlow(false)
		, d_context_p(context)
		, d_consoleLock_p(&context->d_consoleLock)
	{
	}

	bool processEvent(const Event &event, Session *session)
	{
		try {
			switch (event.eventType()) {
			case Event::SESSION_STATUS: {
				MutexGuard guard(&d_context_p->d_mutex);
				return processSessionEvent(event, session);
			} break;
			case Event::SERVICE_STATUS: {
				MutexGuard guard(&d_context_p->d_mutex);
				return processServiceEvent(event, session);
			} break;
			case Event::RESPONSE: {
				MutexGuard guard(&d_context_p->d_mutex);
				return processResponseEvent(event, session);
			} break;
			default: {
				return processMiscEvents(event);
			}  break;
			}
		}
		catch (Exception &e) {
			ConsoleOut(d_consoleLock_p)
				<< "Library Exception !!!"
				<< e.description() << std::endl;
		}
		return false;
	}
};

class CreateIOI
{

	SessionOptions            d_sessionOptions;
	Session                  *d_session;
	EMSXEventHandler		 *d_eventHandler;
	SessionContext            d_context;

	bool createSession() {
		ConsoleOut(&d_context.d_consoleLock)
			<< "Connecting to " << d_sessionOptions.serverHost()
			<< ":" << d_sessionOptions.serverPort() << std::endl;

		d_eventHandler = new EMSXEventHandler(&d_context);
		d_session = new Session(d_sessionOptions, d_eventHandler);

		d_session->startAsync();

		return true;
	}

public:

	CreateIOI()
		: d_session(0)
		, d_eventHandler(0)
	{


		d_sessionOptions.setServerHost("localhost");
		d_sessionOptions.setServerPort(8194);
		d_sessionOptions.setMaxEventQueueSize(10000);
	}

	~CreateIOI()
	{
		if (d_session) delete d_session;
		if (d_eventHandler) delete d_eventHandler;
	}

	void run(int argc, char **argv)
	{
		if (!createSession()) return;

		// wait for enter key to exit application
		ConsoleOut(&d_context.d_consoleLock)
			<< "\nPress ENTER to quit" << std::endl;
		char dummy[2];
		std::cin.getline(dummy, 2);
		{
			MutexGuard guard(&d_context.d_mutex);
			d_context.d_isStopped = true;
		}
		d_session->stop();
		ConsoleOut(&d_context.d_consoleLock) << "\nExiting..." << std::endl;
	}
};

int main(int argc, char **argv)
{
	std::cout << "Bloomberg - IOI API Example - CreateIOI" << std::endl;
	CreateIOI createIOI;
	try {
		createIOI.run(argc, argv);
	}
	catch (Exception &e) {
		std::cout << "Library Exception!!!" << e.description() << std::endl;
	}
	// wait for enter key to exit application
	std::cout << "Press ENTER to quit" << std::endl;
	char dummy[2];
	std::cin.getline(dummy, 2);
	return 0;
}

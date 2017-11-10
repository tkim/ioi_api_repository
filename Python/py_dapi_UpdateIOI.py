# py_dapi_UpdateIOI.py

import sys
import blpapi
import datetime
import time

SESSION_STARTED                 = blpapi.Name("SessionStarted")
SESSION_STARTUP_FAILURE         = blpapi.Name("SessionStartupFailure")
SERVICE_OPENED                  = blpapi.Name("ServiceOpened")
SERVICE_OPEN_FAILURE            = blpapi.Name("ServiceOpenFailure")
SLOW_CONSUMER_WARNING           = blpapi.Name("SlowConsumerWarning")
SLOW_CONSUMER_WARNING_CLEARED   = blpapi.Name("SlowConsumerWarningCleared")
HANDLE                          = blpapi.Name("handle")


d_ioi = "//blp/ioiapi-beta-request"
d_host = "localhost"
d_port = 8194

bEnd=False

class SessionEventHandler():
    
    def sendCreateIOI(self, session):

        service = session.getService(d_ioi)
        request = service.createRequest("updateIoi")

        handle = request.getElement("handle")
        handle.setElement("value", "5f20228a-bef6-41bb-81eb-6abe0b21a00e")

        ioi = request.getElement("ioi")

        # Set the good-until time of this option to 15 minutes from now
        ioi.setElement("goodUntil", datetime.datetime.utcnow() + datetime.timedelta(0,900))

        # Create the option
        option = ioi.getElement("instrument").setChoice("option")
        option.setElement("structure", "CallSpread")

        # This option has two legs. Create the first leg
        leg1 = option.getElement("legs").appendElement()
        leg1.setElement("type","Call")
        leg1.setElement("strike", 230)
        leg1.setElement("expiry", datetime.datetime(2017,12,15,12))
        leg1.setElement("style", "European")
        leg1.setElement("ratio", +1.00)
        leg1.setElement("exchange", "LN")
        leg1.getElement("underlying").setChoice("ticker")
        leg1.getElement("underlying").setElement("ticker", "VOD LN Equity")

        # Create the second leg
        leg2 = option.getElement("legs").appendElement()
        leg1.setElement("type","Call")
        leg2.setElement("strike", 240)
        leg2.setElement("expiry", datetime.datetime(2017,12,15,12))
        leg2.setElement("style", "European")
        leg2.setElement("ratio", -1.25)
        leg2.setElement("exchange", "LN")
        leg2.getElement("underlying").setChoice("ticker")
        leg2.getElement("underlying").setElement("ticker", "VOD LN Equity")

        # Create a quote consisting of a bid and an offer
        bid = ioi.getElement("bid")
        bid.getElement("price").setChoice("fixed")
        bid.getElement("price").getElement("fixed").getElement("price").setValue(83.63)
        bid.getElement("size").getElement("quantity").setValue(1000)
        bid.getElement("referencePrice").setElement("price", 202.15)
        bid.getElement("referencePrice").setElement("currency", "GBp")
        bid.setElement("notes", "bid notes")

        # Set the offer
        offer = ioi.getElement("offer")
        offer.getElement("price").setChoice("fixed")
        offer.getElement("price").getElement("fixed").getElement("price").setValue(83.64)
        offer.getElement("size").setChoice("quantity")
        offer.getElement("size").getElement("quantity").setValue(2000)
        offer.getElement("referencePrice").setElement("price", 202.15)
        offer.getElement("referencePrice").setElement("currency", "GBp")
        offer.setElement("notes", "offer notes")

        # Set targets
        includes = ioi.getElement("targets").getElement("includes")
        for acronym in ["BLPA", "BLPB"]:
            target = includes.appendElement()
            target.setChoice("acronym")
            target.setElement("acronym", acronym)
                    
        
        print("Sending Request: %s" % request.toString())

        self.requestID = session.sendRequest(request)
        print("UpdateIOI request sent.")

    def processAdminEvent(self,event):  
        print("Processing ADMIN event")

        for msg in event:
            if msg.messageType() == SLOW_CONSUMER_WARNING:
                print("Warning: Entered Slow Consumer status")
                
            elif msg.messageType() == SLOW_CONSUMER_WARNING_CLEARED:
                sys.stderr.write("Slow consumer status cleared")
                
            else:
                print(msg)


    def processSessionStatusEvent(self,event,session):  
        print("Processing SESSION_STATUS event")

        for msg in event:
            if msg.messageType() == SESSION_STARTED:
                print("Session started...")
                session.openServiceAsync(d_ioi)
                
            elif msg.messageType() == SESSION_STARTUP_FAILURE:
                sys.stderr.write("Error: Session startup failed")
                
            else:
                print(msg)
                

    def processServiceStatusEvent(self,event,session):
        print ("Processing SERVICE_STATUS event")
        
        for msg in event:
            
            if msg.messageType() == SERVICE_OPENED:
                
                print("IOIAPI service opened... Sending request...")
                self.sendCreateIOI(session)
                
            elif msg.messageType() == SERVICE_OPEN_FAILURE:
                    print("Error: Service Failed to open")
                
                
    def processResponseEvent(self, event, session):
        print("Processing RESPONSE event")
        
        for msg in event:
            
            print("MESSAGE: %s" % msg.toString())
            print("CORRELATION ID: %d" % msg.correlationIds()[0].value())

            if msg.correlationIds()[0].value() == self.requestID.value():
                print("MESSAGE TYPE: %s" % msg.messageType())
                
                if msg.messageType() == HANDLE:
                    val = msg.getElementAsString("value")
                    print("Response: Value=%s" % (val))
                
                else:
                    print ("Unexpected message...")
                    
                global bEnd
                bEnd = True
            
            else:
                print ("Unexpected message...")
                print (msg)
                    
            
    def processMiscEvents(self, event):
        
        print("Processing " + event.eventType() + " event")
        
        for msg in event:

            print("MISC MESSAGE: %s" % (msg.tostring()))


    def processEvent(self, event, session):
        try:
            
            if event.eventType() == blpapi.Event.ADMIN:
                self.processAdminEvent(event)
            
            if event.eventType() == blpapi.Event.SESSION_STATUS:
                self.processSessionStatusEvent(event,session)

            elif event.eventType() == blpapi.Event.SERVICE_STATUS:
                self.processServiceStatusEvent(event,session)

            elif event.eventType() == blpapi.Event.RESPONSE:
                self.processResponseEvent(event,session)
            
            else:
                self.processMiscEvents(event)
                
        except Exception as e:
            print("Exception:  %s" % str(e))
            
        return False

                
def main():
    
    sessionOptions = blpapi.SessionOptions()
    sessionOptions.setServerHost(d_host)
    sessionOptions.setServerPort(d_port)

    print("Connecting to %s:%d" % (d_host,d_port))

    eventHandler = SessionEventHandler()

    session = blpapi.Session(sessionOptions, eventHandler.processEvent)

    if not session.startAsync():
        print("Failed to start session.")
        return
    
    global bEnd
    while bEnd==False:
        pass
    
    print ("Terminating...")
    
    session.stop()

if __name__ == "__main__":
    print("Bloomberg - IOI API Example - DesktopAPI - UpdateIOI")
    try:
        main()
    except KeyboardInterrupt:
        print("Ctrl+C pressed. Stopping...")


__copyright__ = """
Copyright 2017. Bloomberg Finance L.P.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to
deal in the Software without restriction, including without limitation the
rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
sell copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:  The above
copyright notice and this permission notice shall be included in all copies
or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
IN THE SOFTWARE.
"""

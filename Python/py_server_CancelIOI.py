# CancelIOI.py

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
AUTHORIZATION_SUCCESS           = blpapi.Name("AuthorizationSuccess")
AUTHORIZATION_FAILURE           = blpapi.Name("AuthorizationFailure")
HANDLE                          = blpapi.Name("handle")


d_emsx = "//blp/ioiapi-beta-request"
d_auth = "//blp/apiauth"
d_host = "mbp-nj-beta.bdns.bloomberg.com"
d_port = 8294
d_user = "my EMRS ID" #EMRSID or AuthID of the Server
d_ip = "0.0.0.0" #IP Address of the server

bEnd=False


class SessionEventHandler():
    
    def sendAuthRequest(self,session):
                
        authService = session.getService(d_auth)
        authReq = authService.createAuthorizationRequest()
        authReq.set("emrsId",d_user)
        authReq.set("ipAddress", d_ip)
        self.identity = session.createIdentity()
        
        print ("Sending authorization request: %s" % (authReq))
        
        session.sendAuthorizationRequest(authReq, self.identity)
        
        print ("Authorization request sent.")

    
    def sendCancelIOI(self, session):

        service = session.getService(d_emsx)

        request = service.createRequest("cancelIoi")
        
        handle = request.getElement("handle")
        handle.setElement("value", "c877ff82-3996-4840-86d1-e970f2cd6840")

        print("Sending Request: %s" % request.toString())

        self.requestID = session.sendRequest(request, identity=self.identity)
        print("CancelIOI request sent.")

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
                session.openServiceAsync(d_auth)
                
            elif msg.messageType() == SESSION_STARTUP_FAILURE:
                sys.stderr.write("Error: Session startup failed")
                
            else:
                print(msg)
                

    def processServiceStatusEvent(self,event,session):
        print ("Processing SERVICE_STATUS event")
        
        for msg in event:
            
            if msg.messageType() == SERVICE_OPENED:
                
                serviceName = msg.asElement().getElementAsString("serviceName");
                
                print("Service opened [%s]" % (serviceName))

                if serviceName==d_auth:
                    
                    print("Auth service opened... Opening EMSX service...")
                    session.openServiceAsync(d_emsx)
                
                elif serviceName==d_emsx:
                    
                    print("EMSX service opened... Sending authorization request...")
                    
                    self.sendAuthRequest(session)
                
            elif msg.messageType() == SERVICE_OPEN_FAILURE:
                    print("Error: Service Failed to open")
                
                
    
    def processAuthorizationStatusEvent(self,event):
        
        print ("Processing AUTHORIZATION_STATUS event")

        for msg in event:

            print("AUTHORIZATION_STATUS message: %s" % (msg))


                
    def processResponseEvent(self, event, session):
        print("Processing RESPONSE event")
        
        for msg in event:
            
            print("MESSAGE: %s" % msg.toString())
            print("CORRELATION ID: %d" % msg.correlationIds()[0].value())

            if msg.messageType() == AUTHORIZATION_SUCCESS:
                print("Authorization successful....")
                print ("SeatType: %s" % (self.identity.getSeatType()))
                self.sendCancelIOI(session)

            elif msg.messageType() == AUTHORIZATION_FAILURE:
                print("Authorization failed....")
                # insert code here to automatically retry authorization...

            elif msg.correlationIds()[0].value() == self.requestID.value():
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

            elif event.eventType() == blpapi.Event.AUTHORIZATION_STATUS:
                self.processAuthorizationStatusEvent(event)

            elif event.eventType() == blpapi.Event.RESPONSE:
                self.processResponseEvent(event,session)
            
            else:
                self.processMiscEvents(event)
                
        except:
            print("Exception:  %s" % sys.exc_info()[0])
            
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
    
    #global bEnd
    #while bEnd==False:
    #    pass
    while True: 
        time.sleep(1)
    
if __name__ == "__main__":
    print("Bloomberg - IOI API Example - CancelIOI")
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

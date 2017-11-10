# py_dapi_SubscribeIOI.py

import sys
import blpapi
import datetime
import time

SESSION_STARTED                 = blpapi.Name("SessionStarted")
SESSION_TERMINATED              = blpapi.Name("SessionTerminated")
SESSION_STARTUP_FAILURE         = blpapi.Name("SessionStartupFailure")
SESSION_CONNECTION_UP           = blpapi.Name("SessionConnectionUp")
SESSION_CONNECTION_DOWN         = blpapi.Name("SessionConnectionDown")
SERVICE_OPENED                  = blpapi.Name("ServiceOpened")
SERVICE_OPEN_FAILURE            = blpapi.Name("ServiceOpenFailure")
SLOW_CONSUMER_WARNING           = blpapi.Name("SlowConsumerWarning")
SLOW_CONSUMER_WARNING_CLEARED   = blpapi.Name("SlowConsumerWarningCleared")
SUBSCRIPTION_FAILURE            = blpapi.Name("SubscriptionFailure")
SUBSCRIPTION_STARTED            = blpapi.Name("SubscriptionStarted")
SUBSCRIPTION_TERMINATED         = blpapi.Name("SubscriptionTerminated")

IOI_DATA                        = blpapi.Name("Ioidata")


d_ioi = "//blp-test/ioisub-beta"
d_host = "localhost"
d_port = 8194
ioiSubscriptionID=blpapi.CorrelationId(1)
bEnd=False

class SessionEventHandler():
    
    def createIOISubscription(self, session):

        print("Create IOI subscription")
    
        #Create the topic string for the ioi subscription.
        ioiTopic = d_ioi + "/ioi"
    
        subscriptions = blpapi.SubscriptionList()
        
        subscriptions.add(topic=ioiTopic,correlationId=ioiSubscriptionID)

        print("Sending subscription...")
        session.subscribe(subscriptions)

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
                
            elif msg.messageType() == SESSION_CONNECTION_UP:
                print("Session connection is up")

            elif msg.messageType() == SESSION_CONNECTION_DOWN:
                print("Session connection is down")

            else:
                print(msg)
                

    def processServiceStatusEvent(self,event,session):
        print ("Processing SERVICE_STATUS event")
        
        for msg in event:
            
            if msg.messageType() == SERVICE_OPENED:
                
                print("IOIAPI service opened... Sending request...")
                self.createIOISubscription(session)
                
            elif msg.messageType() == SERVICE_OPEN_FAILURE:
                    print("Error: Service Failed to open")
                
                
    def processSubscriptionStatusEvent(self,event):

        print ("Processing SUBSCRIPTION_STATUS event")
        
        for msg in event:
            
            if msg.messageType() == SUBSCRIPTION_STARTED:
                print("IOIAPI subscription started...")
                
            elif msg.messageType() == SERVICE_OPEN_FAILURE:
                print("Error: IOI subscription failed")
                print(msg)

            
    def processSubscriptionDataEvent(self,event):
        
        print ("Processing SUBSCRIPTION_DATA event")
        
        for msg in event:
            
            #print (msg)
            
            if msg.messageType() == IOI_DATA:

                print ("IOI MESSAGE: CorrelationID(%s)" % (msg.correlationIds()[0].value()))
                
                ioi_instrument_type = msg.getElementAsString("ioi_instrument_type") if msg.hasElement("ioi_instrument_type")  else  ""
                ioi_instrument_option_legs_count = msg.getElementAsInteger("ioi_instrument_option_legs_count") if msg.hasElement("ioi_instrument_option_legs_count") else 0
                
                ioi_instrument_option_legs_0_strike = msg.getElementAsFloat("ioi_instrument_option_legs_0_strike") if msg.hasElement("ioi_instrument_option_legs_0_strike") else 0
                ioi_instrument_option_legs_0_expiry = msg.getElementAsString("ioi_instrument_option_legs_0_expiry") if msg.hasElement("ioi_instrument_option_legs_0_expiry") else ""
                ioi_instrument_option_legs_0_type = msg.getElementAsString("ioi_instrument_option_legs_0_type") if msg.hasElement("ioi_instrument_option_legs_0_type") else ""
                ioi_instrument_option_legs_0_ratio = msg.getElementAsFloat("ioi_instrument_option_legs_0_ratio") if msg.hasElement("ioi_instrument_option_legs_0_ratio") else 0
                ioi_instrument_option_legs_0_underlying_type = msg.getElementAsString("ioi_instrument_option_legs_0_underlying_type") if msg.hasElement("ioi_instrument_option_legs_0_underlying_type") else ""
                ioi_instrument_option_legs_0_underlying_ticker = msg.getElementAsString("ioi_instrument_option_legs_0_underlying_ticker") if msg.hasElement("ioi_instrument_option_legs_0_underlying_ticker") else ""
                ioi_instrument_option_legs_0_underlying_figi = msg.getElementAsString("ioi_instrument_option_legs_0_underlying_figi") if msg.hasElement("ioi_instrument_option_legs_0_underlying_figi") else ""
                ioi_instrument_option_legs_0_exchange = msg.getElementAsString("ioi_instrument_option_legs_0_exchange") if msg.hasElement("ioi_instrument_option_legs_0_exchange") else  ""
                ioi_instrument_option_legs_0_style = msg.getElementAsString("ioi_instrument_option_legs_0_style") if msg.hasElement("ioi_instrument_option_legs_0_style") else ""
                ioi_instrument_option_legs_0_futureRefDate = msg.getElementAsString("ioi_instrument_option_legs_0_futureRefDate") if msg.hasElement("ioi_instrument_option_legs_0_futureRefDate") else ""
                ioi_instrument_option_legs_0_delta = msg.getElementAsFloat("ioi_instrument_option_legs_0_delta") if msg.hasElement("ioi_instrument_option_legs_0_delta") else 0
    
                ioi_instrument_option_legs_1_strike = msg.getElementAsFloat("ioi_instrument_option_legs_1_strike") if msg.hasElement("ioi_instrument_option_legs_1_strike") else 0
                ioi_instrument_option_legs_1_expiry = msg.getElementAsString("ioi_instrument_option_legs_1_expiry") if msg.hasElement("ioi_instrument_option_legs_1_expiry") else ""
                ioi_instrument_option_legs_1_type = msg.getElementAsString("ioi_instrument_option_legs_1_type") if msg.hasElement("ioi_instrument_option_legs_1_type") else ""
                ioi_instrument_option_legs_1_ratio = msg.getElementAsFloat("ioi_instrument_option_legs_1_ratio") if msg.hasElement("ioi_instrument_option_legs_1_ratio") else 0
                ioi_instrument_option_legs_1_underlying_type = msg.getElementAsString("ioi_instrument_option_legs_1_underlying_type") if msg.hasElement("ioi_instrument_option_legs_1_underlying_type") else ""
                ioi_instrument_option_legs_1_underlying_ticker = msg.getElementAsString("ioi_instrument_option_legs_1_underlying_ticker") if msg.hasElement("ioi_instrument_option_legs_1_underlying_ticker") else ""
                ioi_instrument_option_legs_1_underlying_figi = msg.getElementAsString("ioi_instrument_option_legs_1_underlying_figi") if msg.hasElement("ioi_instrument_option_legs_1_underlying_figi") else ""
                ioi_instrument_option_legs_1_exchange = msg.getElementAsString("ioi_instrument_option_legs_1_exchange") if msg.hasElement("ioi_instrument_option_legs_1_exchange") else ""
                ioi_instrument_option_legs_1_style = msg.getElementAsString("ioi_instrument_option_legs_1_style") if msg.hasElement("ioi_instrument_option_legs_1_style") else ""
                ioi_instrument_option_legs_1_futureRefDate = msg.getElementAsString("ioi_instrument_option_legs_1_futureRefDate") if msg.hasElement("ioi_instrument_option_legs_1_futureRefDate") else ""
                ioi_instrument_option_legs_1_delta = msg.getElementAsFloat("ioi_instrument_option_legs_1_delta") if msg.hasElement("ioi_instrument_option_legs_1_delta") else 0
    
                ioi_instrument_option_legs_2_strike = msg.getElementAsFloat("ioi_instrument_option_legs_2_strike") if msg.hasElement("ioi_instrument_option_legs_2_strike") else 0
                ioi_instrument_option_legs_2_expiry = msg.getElementAsString("ioi_instrument_option_legs_2_expiry") if msg.hasElement("ioi_instrument_option_legs_2_expiry") else ""
                ioi_instrument_option_legs_2_type = msg.getElementAsString("ioi_instrument_option_legs_2_type") if msg.hasElement("ioi_instrument_option_legs_2_type") else ""
                ioi_instrument_option_legs_2_ratio = msg.getElementAsFloat("ioi_instrument_option_legs_2_ratio") if msg.hasElement("ioi_instrument_option_legs_2_ratio") else 0
                ioi_instrument_option_legs_2_underlying_type = msg.getElementAsString("ioi_instrument_option_legs_2_underlying_type") if msg.hasElement("ioi_instrument_option_legs_2_underlying_type") else ""
                ioi_instrument_option_legs_2_underlying_ticker = msg.getElementAsString("ioi_instrument_option_legs_2_underlying_ticker") if msg.hasElement("ioi_instrument_option_legs_2_underlying_ticker") else ""
                ioi_instrument_option_legs_2_underlying_figi = msg.getElementAsString("ioi_instrument_option_legs_2_underlying_figi") if msg.hasElement("ioi_instrument_option_legs_2_underlying_figi") else ""
                ioi_instrument_option_legs_2_exchange = msg.getElementAsString("ioi_instrument_option_legs_2_exchange") if msg.hasElement("ioi_instrument_option_legs_2_exchange") else ""
                ioi_instrument_option_legs_2_style = msg.getElementAsString("ioi_instrument_option_legs_2_style") if msg.hasElement("ioi_instrument_option_legs_2_style") else ""
                ioi_instrument_option_legs_2_futureRefDate = msg.getElementAsString("ioi_instrument_option_legs_2_futureRefDate") if msg.hasElement("ioi_instrument_option_legs_2_futureRefDate") else ""
                ioi_instrument_option_legs_2_delta = msg.getElementAsFloat("ioi_instrument_option_legs_2_delta") if msg.hasElement("ioi_instrument_option_legs_2_delta") else 0
    
                ioi_instrument_option_legs_3_strike = msg.getElementAsFloat("ioi_instrument_option_legs_3_strike") if msg.hasElement("ioi_instrument_option_legs_3_strike") else 0
                ioi_instrument_option_legs_3_expiry = msg.getElementAsString("ioi_instrument_option_legs_3_expiry") if msg.hasElement("ioi_instrument_option_legs_3_expiry") else ""
                ioi_instrument_option_legs_3_type = msg.getElementAsString("ioi_instrument_option_legs_3_type") if msg.hasElement("ioi_instrument_option_legs_3_type") else ""
                ioi_instrument_option_legs_3_ratio = msg.getElementAsFloat("ioi_instrument_option_legs_3_ratio") if msg.hasElement("ioi_instrument_option_legs_3_ratio") else 0
                ioi_instrument_option_legs_3_underlying_type = msg.getElementAsString("ioi_instrument_option_legs_3_underlying_type") if msg.hasElement("ioi_instrument_option_legs_3_underlying_type") else ""
                ioi_instrument_option_legs_3_underlying_ticker = msg.getElementAsString("ioi_instrument_option_legs_3_underlying_ticker") if msg.hasElement("ioi_instrument_option_legs_3_underlying_ticker") else ""
                ioi_instrument_option_legs_3_underlying_figi = msg.getElementAsString("ioi_instrument_option_legs_3_underlying_figi") if msg.hasElement("ioi_instrument_option_legs_3_underlying_figi") else ""
                ioi_instrument_option_legs_3_exchange = msg.getElementAsString("ioi_instrument_option_legs_3_exchange") if msg.hasElement("ioi_instrument_option_legs_3_exchange") else ""
                ioi_instrument_option_legs_3_style = msg.getElementAsString("ioi_instrument_option_legs_3_style") if msg.hasElement("ioi_instrument_option_legs_3_style") else ""
                ioi_instrument_option_legs_3_futureRefDate = msg.getElementAsString("ioi_instrument_option_legs_3_futureRefDate") if msg.hasElement("ioi_instrument_option_legs_3_futureRefDate") else ""
                ioi_instrument_option_legs_3_delta = msg.getElementAsFloat("ioi_instrument_option_legs_3_delta") if msg.hasElement("ioi_instrument_option_legs_3_delta") else 0
    
                ioi_instrument_option_structure = msg.getElementAsString("ioi_instrument_option_structure") if msg.hasElement("ioi_instrument_option_structure") else ""
                ioi_instrument_stock_security_ticker = msg.getElementAsString("ioi_instrument_stock_security_ticker") if msg.hasElement("ioi_instrument_stock_security_ticker") else ""
                ioi_instrument_stock_security_figi = msg.getElementAsString("ioi_instrument_stock_security_figi") if msg.hasElement("ioi_instrument_stock_security_figi") else ""
                ioi_goodUntil = msg.getElementAsString("ioi_goodUntil") if msg.hasElement("ioi_goodUntil") else ""
    
                ioi_bid_price_type = msg.getElementAsString("ioi_bid_price_type") if msg.hasElement("ioi_bid_price_type") else ""
                ioi_bid_price_fixed_price = msg.getElementAsFloat("ioi_bid_price_fixed_price") if msg.hasElement("ioi_bid_price_fixed_price") else 0
                ioi_bid_price_fixed_currency = msg.getElementAsString("ioi_bid_price_fixed_currency") if msg.hasElement("ioi_bid_price_fixed_currency") else ""
                ioi_bid_price_pegged_offsetAmount = msg.getElementAsFloat("ioi_bid_price_pegged_offsetAmount") if msg.hasElement("ioi_bid_price_pegged_offsetAmount") else 0
                ioi_bid_price_pegged_offsetFrom = msg.getElementAsString("ioi_bid_price_pegged_offsetFrom") if msg.hasElement("ioi_bid_price_pegged_offsetFrom") else ""
                ioi_bid_price_pegged_limitPrice = msg.getElementAsFloat("ioi_bid_price_pegged_limitPrice") if msg.hasElement("ioi_bid_price_pegged_limitPrice") else 0
                ioi_bid_price_reference = msg.getElementAsString("ioi_bid_price_reference") if msg.hasElement("ioi_bid_price_reference") else ""
                ioi_bid_price_moneyness = msg.getElementAsFloat("ioi_bid_price_moneyness") if msg.hasElement("ioi_bid_price_moneyness") else 0
                ioi_bid_size_type = msg.getElementAsString("ioi_bid_size_type") if msg.hasElement("ioi_bid_size_type") else ""
                ioi_bid_size_quantity = msg.getElementAsInteger("ioi_bid_size_quantity") if msg.hasElement("ioi_bid_size_quantity") else 0
                ioi_bid_size_quality = msg.getElementAsString("ioi_bid_size_quality") if msg.hasElement("ioi_bid_size_quality") else ""
                ioi_bid_referencePrice_price = msg.getElementAsFloat("ioi_bid_referencePrice_price") if msg.hasElement("ioi_bid_referencePrice_price") else 0
                ioi_bid_referencePrice_currency = msg.getElementAsString("ioi_bid_referencePrice_currency") if msg.hasElement("ioi_bid_referencePrice_currency") else ""
                ioi_bid_volatility = msg.getElementAsFloat("ioi_bid_volatility") if msg.hasElement("ioi_bid_volatility") else 0
                ioi_bid_notes = msg.getElementAsString("ioi_bid_notes") if msg.hasElement("ioi_bid_notes") else ""
                ioi_bid_qualifiers_count = msg.getElementAsInteger("ioi_bid_qualifiers_count") if msg.hasElement("ioi_bid_qualifiers_count") else 0
                ioi_bid_qualifiers_0 = msg.getElementAsString("ioi_bid_qualifiers_0") if msg.hasElement("ioi_bid_qualifiers_0") else ""
                ioi_bid_qualifiers_1 = msg.getElementAsString("ioi_bid_qualifiers_1") if msg.hasElement("ioi_bid_qualifiers_1") else ""
                ioi_bid_qualifiers_2 = msg.getElementAsString("ioi_bid_qualifiers_2") if msg.hasElement("ioi_bid_qualifiers_2") else ""
                ioi_bid_qualifiers_3 = msg.getElementAsString("ioi_bid_qualifiers_3") if msg.hasElement("ioi_bid_qualifiers_3") else ""
                ioi_bid_qualifiers_4 = msg.getElementAsString("ioi_bid_qualifiers_4") if msg.hasElement("ioi_bid_qualifiers_4") else ""
    
                ioi_offer_price_type = msg.getElementAsString("ioi_offer_price_type") if msg.hasElement("ioi_offer_price_type") else ""
                ioi_offer_price_fixed_price = msg.getElementAsFloat("ioi_offer_price_fixed_price") if msg.hasElement("ioi_offer_price_fixed_price") else 0
                ioi_offer_price_fixed_currency = msg.getElementAsString("ioi_offer_price_fixed_currency") if msg.hasElement("ioi_offer_price_fixed_currency") else ""
                ioi_offer_price_pegged_offsetAmount = msg.getElementAsFloat("ioi_offer_price_pegged_offsetAmount") if msg.hasElement("ioi_offer_price_pegged_offsetAmount") else 0
                ioi_offer_price_pegged_offsetFrom = msg.getElementAsString("ioi_offer_price_pegged_offsetFrom") if msg.hasElement("ioi_offer_price_pegged_offsetFrom") else ""
                ioi_offer_price_pegged_limitPrice = msg.getElementAsFloat("ioi_offer_price_pegged_limitPrice") if msg.hasElement("ioi_offer_price_pegged_limitPrice") else 0
                ioi_offer_price_reference = msg.getElementAsString("ioi_offer_price_reference") if msg.hasElement("ioi_offer_price_reference") else ""
                ioi_offer_price_moneyness = msg.getElementAsFloat("ioi_offer_price_moneyness") if msg.hasElement("ioi_offer_price_moneyness") else 0
                ioi_offer_size_type = msg.getElementAsString("ioi_offer_size_type") if msg.hasElement("ioi_offer_size_type") else ""
                ioi_offer_size_quantity = msg.getElementAsInteger("ioi_offer_size_quantity") if msg.hasElement("ioi_offer_size_quantity") else 0
                ioi_offer_size_quality = msg.getElementAsString("ioi_offer_size_quality") if msg.hasElement("ioi_offer_size_quality") else ""
                ioi_offer_referencePrice_price = msg.getElementAsFloat("ioi_offer_referencePrice_price") if msg.hasElement("ioi_offer_referencePrice_price") else 0
                ioi_offer_referencePrice_currency = msg.getElementAsString("ioi_offer_referencePrice_currency") if msg.hasElement("ioi_offer_referencePrice_currency") else ""
                ioi_offer_volatility = msg.getElementAsFloat("ioi_offer_volatility") if msg.hasElement("ioi_offer_volatility") else 0
                ioi_offer_notes = msg.getElementAsString("ioi_offer_notes") if msg.hasElement("ioi_offer_notes") else ""
                ioi_offer_qualifiers_count = msg.getElementAsInteger("ioi_offer_qualifiers_count") if msg.hasElement("ioi_offer_qualifiers_count") else 0
                ioi_offer_qualifiers_0 = msg.getElementAsString("ioi_offer_qualifiers_0") if msg.hasElement("ioi_offer_qualifiers_0") else ""
                ioi_offer_qualifiers_1 = msg.getElementAsString("ioi_offer_qualifiers_1") if msg.hasElement("ioi_offer_qualifiers_1") else ""
                ioi_offer_qualifiers_2 = msg.getElementAsString("ioi_offer_qualifiers_2") if msg.hasElement("ioi_offer_qualifiers_2") else ""
                ioi_offer_qualifiers_3 = msg.getElementAsString("ioi_offer_qualifiers_3") if msg.hasElement("ioi_offer_qualifiers_3") else ""
                ioi_offer_qualifiers_4 = msg.getElementAsString("ioi_offer_qualifiers_4") if msg.hasElement("ioi_offer_qualifiers_4") else ""
    
                ioi_routing_strategy_name = msg.getElementAsString("ioi_routing_strategy_name") if msg.hasElement("ioi_routing_strategy_name") else ""
                ioi_routing_strategy_brief = msg.getElementAsString("ioi_routing_strategy_brief") if msg.hasElement("ioi_routing_strategy_brief") else ""
                ioi_routing_strategy_detailed = msg.getElementAsString("ioi_routing_strategy_detailed") if msg.hasElement("ioi_routing_strategy_detailed") else ""
                ioi_routing_customId = msg.getElementAsString("ioi_routing_customId") if msg.hasElement("ioi_routing_customId") else ""
                ioi_routing_broker = msg.getElementAsString("ioi_routing_broker") if msg.hasElement("ioi_routing_broker") else ""
    
                ioi_sentTime = msg.getElementAsString("ioi_sentTime") if msg.hasElement("ioi_sentTime") else ""
    
                change = msg.getElementAsString("change") if msg.hasElement("change") else ""
                
                print("ioi_instrument_type: %s" % ioi_instrument_type)
                print("ioi_instrument_option_legs_count: %d" % ioi_instrument_option_legs_count)
                
                print("ioi_instrument_option_legs_0_strike: %d" % ioi_instrument_option_legs_0_strike)
                print("ioi_instrument_option_legs_0_expiry: %s" % ioi_instrument_option_legs_0_expiry)
                print("ioi_instrument_option_legs_0_type: %s" % ioi_instrument_option_legs_0_type)
                print("ioi_instrument_option_legs_0_ratio: %d" % ioi_instrument_option_legs_0_ratio)
                print("ioi_instrument_option_legs_0_underlying_type: %s" % ioi_instrument_option_legs_0_underlying_type)
                print("ioi_instrument_option_legs_0_underlying_ticker: %s" % ioi_instrument_option_legs_0_underlying_ticker)
                print("ioi_instrument_option_legs_0_underlying_figi: %s" % ioi_instrument_option_legs_0_underlying_figi)
                print("ioi_instrument_option_legs_0_exchange: %s" % ioi_instrument_option_legs_0_exchange)
                print("ioi_instrument_option_legs_0_style: %s" % ioi_instrument_option_legs_0_style)
                print("ioi_instrument_option_legs_0_futureRefDate: %s" % ioi_instrument_option_legs_0_futureRefDate)
                print("ioi_instrument_option_legs_0_delta: %d" % ioi_instrument_option_legs_0_delta)

                print("ioi_instrument_option_legs_1_strike: %d" % ioi_instrument_option_legs_1_strike)
                print("ioi_instrument_option_legs_1_expiry: %s" % ioi_instrument_option_legs_1_expiry)
                print("ioi_instrument_option_legs_1_type: %s" % ioi_instrument_option_legs_1_type)
                print("ioi_instrument_option_legs_1_ratio: %d" % ioi_instrument_option_legs_1_ratio)
                print("ioi_instrument_option_legs_1_underlying_type: %s" % ioi_instrument_option_legs_1_underlying_type)
                print("ioi_instrument_option_legs_1_underlying_ticker: %s" % ioi_instrument_option_legs_1_underlying_ticker)
                print("ioi_instrument_option_legs_1_underlying_figi: %s" % ioi_instrument_option_legs_1_underlying_figi)
                print("ioi_instrument_option_legs_1_exchange: %s" % ioi_instrument_option_legs_1_exchange)
                print("ioi_instrument_option_legs_1_style: %s" % ioi_instrument_option_legs_1_style)
                print("ioi_instrument_option_legs_1_futureRefDate: %s" % ioi_instrument_option_legs_1_futureRefDate)
                print("ioi_instrument_option_legs_1_delta: %d" % ioi_instrument_option_legs_1_delta)
                
                print("ioi_instrument_option_legs_2_strike: %d" % ioi_instrument_option_legs_2_strike)
                print("ioi_instrument_option_legs_2_expiry: %s" % ioi_instrument_option_legs_2_expiry)
                print("ioi_instrument_option_legs_2_type: %s" % ioi_instrument_option_legs_2_type)
                print("ioi_instrument_option_legs_2_ratio: %d" % ioi_instrument_option_legs_2_ratio)
                print("ioi_instrument_option_legs_2_underlying_type: %s" % ioi_instrument_option_legs_2_underlying_type)
                print("ioi_instrument_option_legs_2_underlying_ticker: %s" % ioi_instrument_option_legs_2_underlying_ticker)
                print("ioi_instrument_option_legs_2_underlying_figi: %s" % ioi_instrument_option_legs_2_underlying_figi)
                print("ioi_instrument_option_legs_2_exchange: %s" % ioi_instrument_option_legs_2_exchange)
                print("ioi_instrument_option_legs_2_style: %s" % ioi_instrument_option_legs_2_style)
                print("ioi_instrument_option_legs_2_futureRefDate: %s" % ioi_instrument_option_legs_2_futureRefDate)
                print("ioi_instrument_option_legs_2_delta: %d" % ioi_instrument_option_legs_2_delta)
                
                print("ioi_instrument_option_legs_3_strike: %d" % ioi_instrument_option_legs_3_strike)
                print("ioi_instrument_option_legs_3_expiry: %s" % ioi_instrument_option_legs_3_expiry)
                print("ioi_instrument_option_legs_3_type: %s" % ioi_instrument_option_legs_3_type)
                print("ioi_instrument_option_legs_3_ratio: %d" % ioi_instrument_option_legs_3_ratio)
                print("ioi_instrument_option_legs_3_underlying_type: %s" % ioi_instrument_option_legs_3_underlying_type)
                print("ioi_instrument_option_legs_3_underlying_ticker: %s" % ioi_instrument_option_legs_3_underlying_ticker)
                print("ioi_instrument_option_legs_3_underlying_figi: %s" % ioi_instrument_option_legs_3_underlying_figi)
                print("ioi_instrument_option_legs_3_exchange: %s" % ioi_instrument_option_legs_3_exchange)
                print("ioi_instrument_option_legs_3_style: %s" % ioi_instrument_option_legs_3_style)
                print("ioi_instrument_option_legs_3_futureRefDate: %s" % ioi_instrument_option_legs_3_futureRefDate)
                print("ioi_instrument_option_legs_3_delta: %d" % ioi_instrument_option_legs_3_delta)
                
                print("ioi_instrument_option_structure: %s" % ioi_instrument_option_structure)
                print("ioi_instrument_stock_security_ticker: %s" % ioi_instrument_stock_security_ticker)
                print("ioi_instrument_stock_security_figi: %s" % ioi_instrument_stock_security_figi)
                print("ioi_goodUntil: %s" % ioi_goodUntil)
                print("ioi_bid_price_type: %s" % ioi_bid_price_type)
                print("ioi_bid_price_fixed_price: %d" % ioi_bid_price_fixed_price)
                print("ioi_bid_price_fixed_currency: %s" % ioi_bid_price_fixed_currency)
                print("ioi_bid_price_pegged_offsetAmount: %d" % ioi_bid_price_pegged_offsetAmount)
                print("ioi_bid_price_pegged_offsetFrom: %s" % ioi_bid_price_pegged_offsetFrom)
                print("ioi_bid_price_pegged_limitPrice: %d" % ioi_bid_price_pegged_limitPrice)
                print("ioi_bid_price_reference: %s" % ioi_bid_price_reference)
                print("ioi_bid_price_moneyness: %d" % ioi_bid_price_moneyness)
                print("ioi_bid_size_type: %s" % ioi_bid_size_type)
                print("ioi_bid_size_quantity: %d" % ioi_bid_size_quantity)
                print("ioi_bid_size_quality: %s" % ioi_bid_size_quality)
                print("ioi_bid_referencePrice_price: %d" % ioi_bid_referencePrice_price)
                print("ioi_bid_referencePrice_currency: %s" % ioi_bid_referencePrice_currency)
                print("ioi_bid_volatility: %d" % ioi_bid_volatility)
                print("ioi_bid_notes: %s" % ioi_bid_notes)
                print("ioi_bid_qualifiers_count: %d" % ioi_bid_qualifiers_count)
                print("ioi_bid_qualifiers_0: %s" % ioi_bid_qualifiers_0)
                print("ioi_bid_qualifiers_1: %s" % ioi_bid_qualifiers_1)
                print("ioi_bid_qualifiers_2: %s" % ioi_bid_qualifiers_2)
                print("ioi_bid_qualifiers_3: %s" % ioi_bid_qualifiers_3)
                print("ioi_bid_qualifiers_4: %s" % ioi_bid_qualifiers_4)
                print("ioi_offer_price_type: %s" % ioi_offer_price_type)
                print("ioi_offer_price_fixed_price: %d" % ioi_offer_price_fixed_price)
                print("ioi_offer_price_fixed_currency: %s" % ioi_offer_price_fixed_currency)
                print("ioi_offer_price_pegged_offsetAmount: %d" % ioi_offer_price_pegged_offsetAmount)
                print("ioi_offer_price_pegged_offsetFrom: %s" % ioi_offer_price_pegged_offsetFrom)
                print("ioi_offer_price_pegged_limitPrice: %d" % ioi_offer_price_pegged_limitPrice)
                print("ioi_offer_price_reference: %s" % ioi_offer_price_reference)
                print("ioi_offer_price_moneyness: %d" % ioi_offer_price_moneyness)
                print("ioi_offer_size_type: %s" % ioi_offer_size_type)
                print("ioi_offer_size_quantity: %d" % ioi_offer_size_quantity)
                print("ioi_offer_size_quality: %s" % ioi_offer_size_quality)
                print("ioi_offer_referencePrice_price: %d" % ioi_offer_referencePrice_price)
                print("ioi_offer_referencePrice_currency: %s" % ioi_offer_referencePrice_currency)
                print("ioi_offer_volatility: %d" % ioi_offer_volatility)
                print("ioi_offer_notes: %s" % ioi_offer_notes)
                print("ioi_offer_qualifiers_count: %d" % ioi_offer_qualifiers_count)
                print("ioi_offer_qualifiers_0: %s" % ioi_offer_qualifiers_0)
                print("ioi_offer_qualifiers_1: %s" % ioi_offer_qualifiers_1)
                print("ioi_offer_qualifiers_2: %s" % ioi_offer_qualifiers_2)
                print("ioi_offer_qualifiers_3: %s" % ioi_offer_qualifiers_3)
                print("ioi_offer_qualifiers_4: %s" % ioi_offer_qualifiers_4)
                print("ioi_routing_strategy_name: %s" % ioi_routing_strategy_name)
                print("ioi_routing_strategy_brief: %s" % ioi_routing_strategy_brief)
                print("ioi_routing_strategy_detailed: %s" % ioi_routing_strategy_detailed)
                print("ioi_routing_customId: %s" % ioi_routing_customId)
                print("ioi_routing_broker: %s" % ioi_routing_broker)
                print("ioi_sentTime: %s" % ioi_sentTime)
                print("change: %s" % change)

            else:
                print("Error: Unexpected Message")

                
    def processMiscEvents(self, event):
        print("Processing Unknown event")
        
        for msg in event:

            print("MISC MESSAGE: %s" % (msg))


    def processEvent(self, event, session):
        
        try:
            
            if event.eventType() == blpapi.Event.ADMIN:
                self.processAdminEvent(event)
            
            if event.eventType() == blpapi.Event.SESSION_STATUS:
                self.processSessionStatusEvent(event,session)

            elif event.eventType() == blpapi.Event.SERVICE_STATUS:
                self.processServiceStatusEvent(event,session)

            elif event.eventType() == blpapi.Event.SUBSCRIPTION_STATUS:
                self.processSubscriptionStatusEvent(event)
            
            elif event.eventType() == blpapi.Event.SUBSCRIPTION_DATA:
                self.processSubscriptionDataEvent(event)

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
    print("Bloomberg - IOI API Example - DesktopAPI - SubscribeIOI")
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

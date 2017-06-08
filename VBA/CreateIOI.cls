VERSION 1.0 CLASS
BEGIN
  MultiUse = -1  'True
END
Attribute VB_Name = "CreateIOI"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = False
Attribute VB_Exposed = False
' Copyright 2017. Bloomberg Finance L.P.
'
' Permission is hereby granted, free of charge, to any person obtaining a copy
' of this software and associated documentation files (the "Software"), to
' deal in the Software without restriction, including without limitation the
' rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
' sell copies of the Software, and to permit persons to whom the Software is
' furnished to do so, subject to the following conditions:  The above
' copyright notice and this permission notice shall be included in all copies
' or substantial portions of the Software.
'
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
' IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
' FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
' AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
' LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
' FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
' IN THE SOFTWARE.

Option Explicit

Private WithEvents m_BBG_EMSX As blpapicomLib2.Session
Attribute m_BBG_EMSX.VB_VarHelpID = -1
Public running As Boolean
Private svc As blpapicomLib2.service
Private emsxService As String
Private requestID As blpapicomLib2.CorrelationId

Private Sub Class_Initialize()

    log "Bloomberg - IOI API Example - CreateIOI"

    emsxService = "//blp/ioiapi-beta-request"
    
    Set m_BBG_EMSX = New blpapicomLib2.Session
    
    running = True
    
    m_BBG_EMSX.QueueEvents = True
    m_BBG_EMSX.Start
    

End Sub

Private Sub Class_Terminate()
    Set m_BBG_EMSX = Nothing
End Sub

Private Sub m_BBG_EMSX_ProcessEvent(ByVal obj As Object)

    On Error GoTo errHandler

    Dim eventObj As blpapicomLib2.Event
    
     '   Assign the returned data to a Bloomberg type event
    Set eventObj = obj
    
    If Application.Ready Then
    
        Select Case eventObj.EventType
        
            Case SESSION_STATUS
                processSessionEvent eventObj
                
            Case BLPSERVICE_STATUS
                processServiceEvent eventObj
                
            Case RESPONSE
                processResponseEvent eventObj
                
        End Select
        
    End If

    Exit Sub

errHandler:
    Dim errmsg As Variant
    errmsg = Err.Description
    log (errmsg)
    running = False

End Sub


Private Sub processSessionEvent(evt As blpapicomLib2.Event)

    log "Processing SESSION_STATUS event"
    
    Dim it As blpapicomLib2.MessageIterator
    
    Set it = evt.CreateMessageIterator()

    ' Loop while we have messages remaining
    Do While it.Next()
              
        Dim msg As Message
        
        '   Pick up message
        Set msg = it.Message
        
        log "MessageType: " + msg.MessageTypeAsString
        
        If msg.MessageTypeAsString = "SessionStarted" Then
            log "Session started..."
            m_BBG_EMSX.OpenService emsxService
        ElseIf msg.MessageTypeAsString = "SessionStartupFailure" Then
            log "Error: Session startup failed"
            running = False
        End If
        
    Loop

End Sub

Private Sub processServiceEvent(evt As blpapicomLib2.Event)

    Dim req As REQUEST
    Dim service As service
    Dim it As blpapicomLib2.MessageIterator
    Dim ioi As Element
    Dim opt As Element
    Dim leg1 As Element
    Dim leg2 As Element
    Dim bid As Element
    Dim offer As Element
    Dim includes As Element
    Dim t1 As Element
    Dim t2 As Element
    
    On Error GoTo failed
    
    log "Processing SERVICE_STATUS event"
    
    Set it = evt.CreateMessageIterator()

    ' Loop while we have messages remaining
    Do While it.Next()
              
        Dim msg As Message
        
        '   Pick up message
        Set msg = it.Message
        
        log "MessageType: " + msg.MessageTypeAsString
        
        If msg.MessageTypeAsString = "ServiceOpened" Then
    
            ' Get the service
            Set service = m_BBG_EMSX.GetService(emsxService)
    
            'First, create our request object
            Set req = service.CreateRequest("createIoi")
    
            Set ioi = req.GetElement("ioi")
                    
            ' Set the good-until time of this option
            ioi.SetElement "goodUntil", "2017-06-06T17:35:00.000+01:00"
                    
            ' Create the option
            Set opt = ioi.GetElement("instrument").SetChoice("option")
                    
            opt.SetElement "structure", "CallSpread"
                    
            ' This option has two legs. Create the first leg
            Set leg1 = opt.GetElement("legs").AppendElment()
            leg1.SetElement "type", "Call"
            leg1.SetElement "strike", 230
            leg1.SetElement "expiry", "2017-10-01T00:00:00.000+00:00"
            leg1.SetElement "style", "European"
            leg1.SetElement "ratio", 1#
            leg1.SetElement "exchange", "LN"
            leg1.GetElement("underlying").SetChoice "ticker"
            leg1.GetElement("underlying").SetElement "ticker", "VOD LN Equity"

            ' Create the second leg
            Set leg2 = opt.GetElement("legs").AppendElment()
            leg2.SetElement "type", "Call"
            leg2.SetElement "strike", 240
            leg2.SetElement "expiry", "2017-10-01T00:00:00.000+00:00"
            leg2.SetElement "style", "European"
            leg2.SetElement "ratio", -1.25
            leg2.SetElement "exchange", "LN"
            leg2.GetElement("underlying").SetChoice "ticker"
            leg2.GetElement("underlying").SetElement "ticker", "VOD LN Equity"
                    
            ' Create a quote consisting of a bid and an offer
            Set bid = ioi.GetElement("bid")
            bid.GetElement("delta").SetValue 0.0041
            bid.GetElement("size").GetElement("quantity").SetValue 1000
            bid.GetElement("referencePrice").SetElement "price", 202.15
            bid.GetElement("referencePrice").SetElement "currency", "GBp"
            bid.SetElement "notes", "bid notes"

            ' Set the offer
            Set offer = ioi.GetElement("offer")
            offer.GetElement("price").SetChoice "fixed"
            offer.GetElement("price").GetElement("fixed").GetElement("price").SetValue 83.64
            offer.GetElement("size").SetChoice "quantity"
            offer.GetElement("size").GetElement("quantity").SetValue 2000
            offer.GetElement("referencePrice").SetElement "price", 202.15
            offer.GetElement("referencePrice").SetElement "currency", "GBP"
            offer.SetElement "notes", "offer notes"

            ' Set targets
            Set includes = ioi.GetElement("targets").GetElement("includes")
            
            Set t1 = includes.AppendElment()
            t1.SetChoice "acronym"
            t1.SetElement "acronym", "BLPA"
                    
            Set t2 = includes.AppendElment()
            t2.SetChoice "acronym"
            t2.SetElement "acronym", "BLPB"
            
            log "Request: " & req.Print
            
            ' Send the request
            Set requestID = m_BBG_EMSX.SendRequest(req)

        ElseIf msg.MessageTypeAsString = "ServiceOpenFailure" Then
        
            log "Error: Service failed to open"
            running = False
            
        End If
        
    Loop

    Exit Sub
    
failed:

    log "Failed to send the request: " + Err.Description
    
    running = False
    Exit Sub
    
End Sub

Private Sub processResponseEvent(evt As blpapicomLib2.Event)

    log "Processing RESPONSE event"
    
    Dim it As blpapicomLib2.MessageIterator
    Dim i As Integer
    Dim successful As Element
    Dim failed As Element
    Dim order As Element
    Dim numValues As Integer
    Dim errorCode As Long
    Dim errorMessage As String
    Dim success As Boolean

    Set it = evt.CreateMessageIterator()

    ' Loop while we have messages remaining
    Do While it.Next()
              
        Dim msg As Message
        
        '   Pick up message
        Set msg = it.Message
        
        log "MessageType: " + msg.MessageTypeAsString
        
        If evt.EventType = RESPONSE And msg.CorrelationId.Value = requestID.Value Then
        
            log msg.Print
            
            If msg.MessageTypeAsString = "handle" Then
                
                Dim val As String
                
                val = msg.GetElement("value")
                
                log "Response: Value=" & val
                
                m_BBG_EMSX.Stop
                running = False
            
            End If
        End If
    Loop

End Sub



Imports HSPI_ENVIRACOM_HVAC
' Just a name to use when reporting information and errors through the HomeSeer log
Const ScriptName = "enviracomextras.vb"
Const HomeseerHouseCode = "\"

Public Sub CmdSetHeat(ByVal parms As Object)
    Dim EV As Object
    EV = hs.Plugin("Enviracom Manager")
    EV.CmdSetHeat(1, 32.0, 1)
    EV = Nothing
End Sub
' Set the cool setpoint temperature (If supported)
Public Sub CmdSetCool(ByVal parms As Object)
    Dim EV As Object
    EV = hs.Plugin("Enviracom Manager")
    EV.CmdSetCool(1, 44.0, 1)
    EV = Nothing
End Sub
' Set the thermostat operating mode.
Public Sub CmdSetMode(ByVal parms As Object)
    Dim EV As Object
    EV = hs.Plugin("Enviracom Manager")
    EV.CmdSetMode(1, 1, 1)
    EV = Nothing
End Sub
' Set the thermostat fan operating mode.
Public Sub CmdSetFan(ByVal parms As Object)
    Dim EV As Object
    EV = hs.Plugin("Enviracom Manager")
    EV.CmdSetFan(1, 1, 1)
    EV = Nothing
End Sub
' Set the thermostat hold mode. (If supported)
Public Sub CmdSetHold(ByVal parms As Object)
    Dim EV As Object
    EV = hs.Plugin("Enviracom Manager")
    EV.CmdSetHold(7, 1, 0)
    EV = Nothing
End Sub
Public Sub SendProgramMessage(ByVal parms As Object)
    Dim EV As Object
    EV = hs.Plugin("Enviracom Manager")
    EV.CmdSetHold(7, 0, 0)
    EV = Nothing
End Sub
' Send a set time message / command to the HVAC panel
Public Sub SendSetTimeMessage(ByVal parms As Object)
    Dim EV As Object
    EV = hs.Plugin("Enviracom Manager")
    EV.SendSetTimeMessageToUnits()
    EV = Nothing
End Sub
' Send a test message / command to the HVAC panel
Public Sub SendTestMessage(ByVal parms As Object)
    Dim EV As Object
    EV = hs.Plugin("Enviracom Manager")
    EV.SendTestMessageToAllUnits()
    EV = Nothing
End Sub
' Send a test message / command to the HVAC panel
Public Sub SendQueryTempMessage(ByVal parms As Object)
    Dim EV As Object
    EV = hs.Plugin("Enviracom Manager")
    EV.SendQueryTempMessageToAllZones()
    EV = Nothing
End Sub
' Send a test message / command to the HVAC panel
Public Sub SendQuerySystemSwitchMessage(ByVal parms As Object)
    Dim EV As Object
    EV = hs.Plugin("Enviracom Manager")
    EV.SendQuerySystemSwitchMessageToAllZones()
    EV = Nothing
End Sub
' Send a test message / command to the HVAC panel
Public Sub SendQuerySystemStatesMessage(ByVal parms As Object)
    Dim EV As Object
    EV = hs.Plugin("Enviracom Manager")
    EV.SendQuerySystemStateMessageToAllZones()
    EV = Nothing
End Sub
' Send a query filter message / command to the HVAC panel
Public Sub SendFilterMessage(ByVal parms As Object)
    Dim EV As Object
    EV = hs.Plugin("Enviracom Manager")
    EV.SendFilterMessageToAllUnits()
    EV = Nothing
End Sub
' Send a query set points message / command to the HVAC panel
Public Sub SendQuerySetPointsMessage(ByVal parms As Object)
    Dim EV As Object
    EV = hs.Plugin("Enviracom Manager")
    EV.SendQuerySetPointsMessageToAllZones()
    EV = Nothing
End Sub
' Send a query set point limits message / command to the HVAC panel
Public Sub SendQuerySetPointLimitsMessage(ByVal parms As Object)
    Dim EV As Object
    EV = hs.Plugin("Enviracom Manager")
    EV.SendQuerySetPointLimitsMessageToAllZones()
    EV = Nothing
End Sub
' Send a query all schedules message / command to the HVAC panel
Public Sub SendQueryAllSchedulesMessage(ByVal parms As Object)
    Dim EV As Object
    EV = hs.Plugin("Enviracom Manager")
    EV.SendQueryAllSchedulesMessageToAllUnits()
    EV = Nothing
End Sub
' Send a message string to the HVAC panel
Public Sub SendMessageToPanel_0(ByVal parms As Object)
    Dim EV As Object
    Dim szMessage As String
    EV = hs.Plugin("Enviracom Manager")
    szMessage = parms & " " & CalcCheckSum(parms)
    hs.WriteLog(ScriptName, "Received parameter " & parms & " calculate checksum " & CalcCheckSum(parms))
    EV.SendMessageToPanel(0, szMessage)
    EV = Nothing
End Sub
Public Sub SendMessageToPanel_1(ByVal parms As Object)
    Dim EV As Object
    Dim szMessage As String
    EV = hs.Plugin("Enviracom Manager")
    szMessage = parms & " " & CalcCheckSum(parms)
    hs.WriteLog(ScriptName, "Received parameter " & parms & " calculate checksum " & CalcCheckSum(parms))
    EV.SendMessageToPanel(1, szMessage)
    EV = Nothing
End Sub
Public Sub SendMessageToPanel_2(ByVal parms As Object)
    Dim EV As Object
    Dim szMessage As String
    EV = hs.Plugin("Enviracom Manager")
    szMessage = parms & " " & CalcCheckSum(parms)
    hs.WriteLog(ScriptName, "Received parameter " & parms & " calculate checksum " & CalcCheckSum(parms))
    EV.SendMessageToPanel(2, szMessage)
    EV = Nothing
End Sub
Public Sub SendMessageToPanel(ByVal parms As Object)
    Dim EV As Object
    Dim szMessage As String
    EV = hs.Plugin("Enviracom Manager")
    szMessage = parms & " " & CalcCheckSum(parms)
    hs.WriteLog(ScriptName, "Received parameters params: " & parms & " calculate checksum " & CalcCheckSum(parms))
    '    EV.SendMessageToPanel(unit, szMessage)
    EV = Nothing
End Sub
' this only need to be run once to add the button
Sub AddEnviracomExtraBtns(ByVal parms As Object)
    hs.devicebuttonRemove(HomeseerHouseCode + "99", "Set Time")
    hs.devicebuttonAdd(HomeseerHouseCode + "99", "enviracomextras.vb(""SendSetTimeMessage"", """")", "Set Time" + CHR(4))
    hs.devicebuttonRemove(HomeseerHouseCode + "99", "Query Temps")
    hs.devicebuttonAdd(HomeseerHouseCode + "99", "enviracomextras.vb(""SendQueryTempMessage"", """")", "Query Temps" + CHR(4))
    hs.devicebuttonRemove(HomeseerHouseCode + "99", "Query System Switch")
    hs.devicebuttonAdd(HomeseerHouseCode + "99", "enviracomextras.vb(""SendQuerySystemSwitchMessage"", """")", "Query System Switch" + CHR(4))
    hs.devicebuttonRemove(HomeseerHouseCode + "99", "Query Filters")
    hs.devicebuttonAdd(HomeseerHouseCode + "99", "enviracomextras.vb(""SendFilterMessage"", """")", "Query Filters" + CHR(4))
    hs.devicebuttonRemove(HomeseerHouseCode + "99", "Query Set Points")
    hs.devicebuttonAdd(HomeseerHouseCode + "99", "enviracomextras.vb(""SendQuerySetPointsMessage"", """")", "Query Set Points" + CHR(4))
    hs.devicebuttonRemove(HomeseerHouseCode + "99", "Query Set Point Limits")
    hs.devicebuttonAdd(HomeseerHouseCode + "99", "enviracomextras.vb(""SendQuerySetPointLimitsMessage"", """")", "Query Set Point Limits" + CHR(4))
    hs.devicebuttonRemove(HomeseerHouseCode + "99", "Query All Schedules")
    hs.devicebuttonAdd(HomeseerHouseCode + "99", "enviracomextras.vb(""SendQueryAllSchedulesMessage"", """")", "Query All Schedules" + CHR(4))
    hs.devicebuttonRemove(HomeseerHouseCode + "99", "Query System States")
    hs.devicebuttonAdd(HomeseerHouseCode + "99", "enviracomextras.vb(""SendQuerySystemStatesMessage"", """")", "Query System States" + CHR(4))
    hs.devicebuttonRemove(HomeseerHouseCode + "99", "Test")
    hs.devicebuttonRemove(HomeseerHouseCode + "99", "CmdSetHold")
    hs.devicebuttonAdd(HomeseerHouseCode + "99", "enviracomextras.vb(""CmdSetHold"", """")", "CmdSetHold" + CHR(4))
    hs.devicebuttonRemove(HomeseerHouseCode + "99", "CmdSetProgram")
    hs.devicebuttonAdd(HomeseerHouseCode + "99", "enviracomextras.vb(""SendProgramMessage"", """")", "CmdSetProgram" + CHR(4))

    hs.devicebuttonRemove(HomeseerHouseCode + "98", "Query Temp Outdoor")
    hs.devicebuttonAdd(HomeseerHouseCode + "98", "enviracomextras.vb(""SendMessageToPanel_0"", ""M 1290 00 Q 00"")", "Query Temp Outdoor" + CHR(4))
    hs.devicebuttonRemove(HomeseerHouseCode + "98", "Query Blower")
    hs.devicebuttonAdd(HomeseerHouseCode + "98", "enviracomextras.vb(""SendMessageToPanel_0"", ""M 3E70 10 Q 00"")", "Query Blower" + CHR(4))
    hs.devicebuttonRemove(HomeseerHouseCode + "98", "Query Circulate")
    hs.devicebuttonAdd(HomeseerHouseCode + "98", "enviracomextras.vb(""SendMessageToPanel_0"", ""M 31F0 00 Q 00"")", "Query Circulate" + CHR(4))
    hs.devicebuttonRemove(HomeseerHouseCode + "98", "Set Heat Mode")
    hs.devicebuttonAdd(HomeseerHouseCode + "98", "enviracomextras.vb(""SendMessageToPanel_0"", ""M 22D0 04 C 01 01"")", "Set Heat Mode" + CHR(4))
    hs.devicebuttonRemove(HomeseerHouseCode + "97", "Query Temp Outdoor")
    hs.devicebuttonAdd(HomeseerHouseCode + "97", "enviracomextras.vb(""SendMessageToPanel_1"", ""M 1290 00 Q 00"")", "Query Temp Outdoor" + CHR(4))
    hs.devicebuttonRemove(HomeseerHouseCode + "97", "Query Blower")
    hs.devicebuttonAdd(HomeseerHouseCode + "97", "enviracomextras.vb(""SendMessageToPanel_1"", ""M 3E70 10 Q 00"")", "Query Blower" + CHR(4))
    hs.devicebuttonRemove(HomeseerHouseCode + "97", "Query Circulate")
    hs.devicebuttonAdd(HomeseerHouseCode + "97", "enviracomextras.vb(""SendMessageToPanel_1"", ""M 31F0 00 Q 00"")", "Query Circulate" + CHR(4))
    hs.devicebuttonRemove(HomeseerHouseCode + "97", "Set Filter")
    hs.devicebuttonAdd(HomeseerHouseCode + "97", "enviracomextras.vb(""SendMessageToPanel_1"", ""M 10D0 00 C 02 7E 78"")", "Set Filter" + CHR(4))
    hs.devicebuttonRemove(HomeseerHouseCode + "96", "Query Temp Outdoor")
    hs.devicebuttonAdd(HomeseerHouseCode + "96", "enviracomextras.vb(""SendMessageToPanel_2"", ""M 1290 00 Q 00"")", "Query Temp Outdoor" + CHR(4))
    hs.devicebuttonRemove(HomeseerHouseCode + "96", "Query Blower")
    hs.devicebuttonAdd(HomeseerHouseCode + "96", "enviracomextras.vb(""SendMessageToPanel_2"", ""M 3E70 10 Q 00"")", "Query Blower" + CHR(4))
    hs.devicebuttonRemove(HomeseerHouseCode + "96", "Query Circulate")
    hs.devicebuttonAdd(HomeseerHouseCode + "96", "enviracomextras.vb(""SendMessageToPanel_2"", ""M 31F0 00 Q 00"")", "Query Circulate" + CHR(4))

End Sub


Private Function CalcChecksum(ByVal szMessage As String) As String
    Dim Chksum As Byte = 0
    Dim sList As String()
    sList = szMessage.Split(" ")

    Try

        ' process the priority character
        Select Case sList(0)
            Case "H"
                Chksum = &H80
                Exit Select

            Case "M"
                Chksum = &H40
                Exit Select

            Case "L"
                Chksum = &H0
                Exit Select
            Case Else
                hs.WriteLog(ScriptName, "Bad priority character in message: '" + szMessage + "'")
                Return &HFF
                'break;
        End Select

        Dim temp As UInt16 = Convert.ToUInt16(sList(1), 16)
        ' process the message class id as two seperate bytes
        Chksum = Chksum Xor CByte(((temp And &HFF00) >> 8))
        Chksum = Chksum Xor CByte((temp And &HFF))

        ' process the instance number
        Chksum = Chksum Xor Convert.ToByte(sList(2), 16)
        ' process the service
        Select Case sList(3)
            Case "C"
                Chksum = Chksum Xor &H80
                Exit Select

            Case "R"
                Chksum = Chksum Xor &H40
                Exit Select

            Case "Q"
                Chksum = Chksum Xor &H0
                Exit Select
            Case Else

                hs.WriteLog(ScriptName, "Bad service character in message: '" + szMessage + "'")
                Return &HFF
                'break;
        End Select

        ' process the count of databytes
        Chksum = Chksum Xor Convert.ToByte(sList(4), 16)
        ' process the data bytes
        For i As Integer = 5 To (sList.GetUpperBound(0))
            ' process each databyte
            Chksum = Chksum Xor Convert.ToByte(sList(i), 16)

        Next
    Catch ex As Exception
        ' This isn't good, log it appropriately
        hs.WriteLog(ScriptName, "CheckSum: Reporting exception: " & ex.ToString() & " during checksum generation")
        Return &HFF
    End Try
    Return Chksum.ToString("X2")
End Function

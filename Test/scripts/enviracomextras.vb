'
' LAST UPDATE: 19 Feb 2012, Mitch Mitchell
'
'
'      hs.Plugin("Enviracom Manager").NThermostats()
'      hs.Plugin("Enviracom Manager").ThermLoc(HSTermostat)
'      hs.Plugin("Enviracom Manager").ThermName(HSTermostat)
'      hs.Plugin("Enviracom Manager").NTemps(HSTermostat)
'      hs.Plugin("Enviracom Manager").SupportsStat(HSTermostat, 0)
'      hs.Plugin("Enviracom Manager").SupportsCoolSet(HSTermostat, 0)
'      hs.Plugin("Enviracom Manager").SupportsDirect(HSTermostat, 0)
'      hs.Plugin("Enviracom Manager").SupportsHold(HSTermostat, 0)
'      hs.Plugin("Enviracom Manager").SupportsHoldOverride(HSTermostat, 0)
'      hs.Plugin("Enviracom Manager").NumSetbacks(HSTermostat, 0)
'      hs.Plugin("Enviracom Manager").SupportsAux(HSTermostat, 0)
'      hs.Plugin("Enviracom Manager").SupportsOperating(HSTermostat, 0)
'      hs.Plugin("Enviracom Manager").ThermAddress(HSTermostat)
'      hs.Plugin("Enviracom Manager").GetTemp(HSTermostat, 0)
'      hs.Plugin("Enviracom Manager").GetHeatSet(HSTermostat, 0)
'      hs.Plugin("Enviracom Manager").GetCoolSet(HSTermostat, 0)
'      hs.Plugin("Enviracom Manager").GetModeSet(HSTermostat, 0)
'      hs.Plugin("Enviracom Manager").GetFanMode(HSTermostat, 0)
'      hs.Plugin("Enviracom Manager").GetHoldMode(HSTermostat, 0)
'      hs.Plugin("Enviracom Manager").GetCurrentMode(HSTermostat, 0)
'      hs.Plugin("Enviracom Manager").GetOperating(HSTermostat, 0)
'      hs.Plugin("Enviracom Manager").CmdSetHeat(int HSThermostat, double Temperature, 0)
'      hs.Plugin("Enviracom Manager").CmdSetCool(int HSThermostat, double Temperature, 0)
'      hs.Plugin("Enviracom Manager").CmdSetMode(int HSThermostat, int Mode, 0)
'      hs.Plugin("Enviracom Manager").CmdSetFan(int HSThermostat, int Mode, 0)
'      hs.Plugin("Enviracom Manager").CmdSetHold(int HSThermostat, int Mode, 0)
'      hs.Plugin("Enviracom Manager").GetOutdoorTemp(HSTermostat, 0)
'      hs.Plugin("Enviracom Manager").GetHumidity(HSTermostat, 0)
'      hs.Plugin("Enviracom Manager").GetThermostatByName(string name)
'      hs.Plugin("Enviracom Manager").GetThermostatByCode(string dc, string hc)
'      hs.Plugin("Enviracom Manager").CmdSetRecirc(int HSThermostat, bool bReCirculateOn, 0)
'      hs.Plugin("Enviracom Manager").CmdSetClock()
'      hs.Plugin("Enviracom Manager").CmdQuerySchedules(HSTermostat)
'
'

Private Const g_szScriptName As String = "enviracomextras.vb"
Private Const g_szENVMGR_HouseCode As String = "\"           ' We are going to use this House Code


Public Function NThermostats(ByVal parms As String) As Integer
    Try
        hs.WriteLog(g_szScriptName, "NThermostats: " & parms)
        NThermostats = hs.Plugin("Enviracom Manager").NThermostats()
    Catch ex As Exception
        NThermostats = -1
        hs.WriteLog(g_szScriptName, "Error in NThermostats: " & ex.ToString)
    End Try

End Function

Public Function ThermLoc(ByVal parms As String) As String
    Dim HSThermostat As Integer
    Try
        HSThermostat = GetThermostatByName(parms)
        ThermLoc = hs.Plugin("Enviracom Manager").ThermLoc(HSThermostat)
    Catch ex As Exception
        ThermLoc = ""
        hs.WriteLog(g_szScriptName, "Error in ThermLoc: " & ex.ToString)
    End Try

End Function

Public Function ThermName(ByVal parms As String) As String
    Dim HSThermostat As Integer
    Try
        HSThermostat = GetThermostatByName(parms)
        ThermName = hs.Plugin("Enviracom Manager").ThermName(HSThermostat)
    Catch ex As Exception
        ThermName = ""
        hs.WriteLog(g_szScriptName, "Error in ThermName: " & ex.ToString)
    End Try
End Function

Public Function NTemps(ByVal parms As String) As Integer
    Dim HSThermostat As Integer
    Try
        HSThermostat = GetThermostatByName(parms)
        NTemps = hs.Plugin("Enviracom Manager").NTemps(HSThermostat)
    Catch ex As Exception
        NTemps = ""
        hs.WriteLog(g_szScriptName, "Error in NTemps: " & ex.ToString)
    End Try
End Function

Public Function SupportsStat(ByVal parms As String) As Boolean
    Dim HSThermostat As Integer
    Try
        HSThermostat = GetThermostatByName(parms)
        SupportsStat = hs.Plugin("Enviracom Manager").SupportsStat(HSThermostat, 0)
    Catch ex As Exception
        SupportsStat = ""
        hs.WriteLog(g_szScriptName, "Error in SupportsStat: " & ex.ToString)
    End Try
End Function

Public Function SupportsCoolSet(ByVal parms As String) As Boolean
    Dim HSThermostat As Integer
    Try
        HSThermostat = GetThermostatByName(parms)
        SupportsCoolSet = hs.Plugin("Enviracom Manager").SupportsCoolSet(HSThermostat, 0)
    Catch ex As Exception
        SupportsCoolSet = ""
        hs.WriteLog(g_szScriptName, "Error in SupportsCoolSet: " & ex.ToString)
    End Try
End Function

Public Function SupportsDirect(ByVal parms As String) As Boolean
    Dim HSThermostat As Integer
    Try
        HSThermostat = GetThermostatByName(parms)
        SupportsDirect = hs.Plugin("Enviracom Manager").SupportsDirect(HSThermostat, 0)
    Catch ex As Exception
        SupportsDirect = ""
        hs.WriteLog(g_szScriptName, "Error in SupportsDirect: " & ex.ToString)
    End Try
End Function

Public Function SupportsHold(ByVal parms As String) As Boolean
    Dim HSThermostat As Integer
    Try
        HSThermostat = GetThermostatByName(parms)
        SupportsHold = hs.Plugin("Enviracom Manager").SupportsHold(HSThermostat, 0)
    Catch ex As Exception
        SupportsHold = ""
        hs.WriteLog(g_szScriptName, "Error in SupportsHold: " & ex.ToString)
    End Try
End Function

Public Function SupportsHoldOverride(ByVal parms As String) As Boolean
    Dim HSThermostat As Integer
    Try
        HSThermostat = GetThermostatByName(parms)
        SupportsHoldOverride = hs.Plugin("Enviracom Manager").SupportsHoldOverride(HSThermostat, 0)
    Catch ex As Exception
        SupportsHoldOverride = ""
        hs.WriteLog(g_szScriptName, "Error in SupportsHoldOverride: " & ex.ToString)
    End Try
End Function

Public Function NumSetbacks(ByVal parms As String) As Integer
    Dim HSThermostat As Integer
    Try
        HSThermostat = GetThermostatByName(parms)
        NumSetBacks = hs.Plugin("Enviracom Manager").NumSetbacks(HSThermostat, 0)
    Catch ex As Exception
        NumSetBacks = -1
        hs.WriteLog(g_szScriptName, "Error in NumSetbacks: " & ex.ToString)
    End Try
End Function

Public Function SupportsAux(ByVal parms As String) As Boolean
    Dim HSThermostat As Integer
    Try
        HSThermostat = GetThermostatByName(parms)
        SupportsAux = hs.Plugin("Enviracom Manager").SupportsAux(HSThermostat, 0)
    Catch ex As Exception
        SupportsAux = False
        hs.WriteLog(g_szScriptName, "Error in SupportsAux: " & ex.ToString)
    End Try
End Function

Public Function SupportsOperating(ByVal parms As String) As Boolean
    Dim HSThermostat As Integer
    Try
        HSThermostat = GetThermostatByName(parms)
        SupportsOperating = hs.Plugin("Enviracom Manager").SupportsOperating(HSThermostat, 0)
    Catch ex As Exception
        SupportsOperating = False
        hs.WriteLog(g_szScriptName, "Error in TSupportsOperating: " & ex.ToString)
    End Try
End Function

Public Function ThermAddress(ByVal parms As String) As String
    Dim HSThermostat As Integer
    Try
        HSThermostat = GetThermostatByName(parms)
        ThermAddress = hs.Plugin("Enviracom Manager").ThermAddress(HSThermostat)
    Catch ex As Exception
        ThermAddress = ""
        hs.WriteLog(g_szScriptName, "Error in ThermAddress: " & ex.ToString)
    End Try
    hs.WriteLog(g_szScriptName, "ThermAddress: " & ThermAddress)
End Function

Public Function GetTemp(ByVal parms As String) As Double
    Dim HSThermostat As Integer
    Try
        HSThermostat = GetThermostatByName(parms)
        GetTemp = hs.Plugin("Enviracom Manager").GetTemp(HSThermostat, 0)
    Catch ex As Exception
        GetTemp = -1
        hs.WriteLog(g_szScriptName, "Error in GetTemp: " & ex.ToString)
    End Try
    hs.WriteLog(g_szScriptName, "GetTemp: " & GetTemp)
End Function

Public Function GetHeatSet(ByVal parms As String) As Double
    Dim HSThermostat As Integer
    Try
        HSThermostat = GetThermostatByName(parms)
        GetHeatSet = hs.Plugin("Enviracom Manager").GetHeatSet(HSThermostat, 0)
    Catch ex As Exception
        GetHeatSet = -1
        hs.WriteLog(g_szScriptName, "Error in GetHeatSet: " & ex.ToString)
    End Try
    hs.WriteLog(g_szScriptName, "GetHeatSet: " & GetHeatSet)
End Function

Public Function GetCoolSet(ByVal parms As String) As Double
    Dim HSThermostat As Integer
    Try
        HSThermostat = GetThermostatByName(parms)
        GetCoolSet = hs.Plugin("Enviracom Manager").GetCoolSet(HSThermostat, 0)
    Catch ex As Exception
        GetCoolSet = -1
        hs.WriteLog(g_szScriptName, "Error in GetCoolSet: " & ex.ToString)
    End Try
    hs.WriteLog(g_szScriptName, "GetCoolSet: " & GetCoolSet)
End Function

Public Function GetModeSet(ByVal parms As String) As Integer
    Dim HSThermostat As Integer
    Try
        HSThermostat = GetThermostatByName(parms)
        GetModeSet = hs.Plugin("Enviracom Manager").GetModeSet(HSThermostat, 0)
    Catch ex As Exception
        GetModeSet = -1
        hs.WriteLog(g_szScriptName, "Error in GetModeSet: " & ex.ToString)
    End Try
    hs.WriteLog(g_szScriptName, "GetModeSet: " & GetModeSet)
End Function

Public Function GetFanMode(ByVal parms As String) As Integer
    Dim HSThermostat As Integer
    Try
        HSThermostat = GetThermostatByName(parms)
        GetFanMode = hs.Plugin("Enviracom Manager").GetFanMode(HSThermostat, 0)
    Catch ex As Exception
        GetFanMode = -1
        hs.WriteLog(g_szScriptName, "Error in GetFanMode: " & ex.ToString)
    End Try
    hs.WriteLog(g_szScriptName, "GetFanMode: " & GetFanMode)
End Function

Public Function GetHoldMode(ByVal parms As String) As Integer
    Dim HSThermostat As Integer
    Try
        HSThermostat = GetThermostatByName(parms)
        GetHoldMode = hs.Plugin("Enviracom Manager").GetHoldMode(HSThermostat, 0)
    Catch ex As Exception
        GetHoldMode = -1
        hs.WriteLog(g_szScriptName, "Error in GetHoldMode: " & ex.ToString)
    End Try
    hs.WriteLog(g_szScriptName, "GetHoldMode: " & GetHoldMode)
End Function

Public Function GetCurrentMode(ByVal parms As String) As Integer
    Dim HSThermostat As Integer
    Try
        HSThermostat = GetThermostatByName(parms)
        GetCurrentMode = hs.Plugin("Enviracom Manager").GetCurrentMode(HSThermostat, 0)
    Catch ex As Exception
        GetCurrentMode = -1
        hs.WriteLog(g_szScriptName, "Error in GetCurrentMode: " & ex.ToString)
    End Try
    hs.WriteLog(g_szScriptName, "GetCurrentMode: " & GetCurrentMode)
End Function

Public Function GetOperating(ByVal parms As String) As Boolean
    Dim HSThermostat As Integer
    Try
        HSThermostat = GetThermostatByName(parms)
        GetOperating = hs.Plugin("Enviracom Manager").GetOperating(HSThermostat, 0)
    Catch ex As Exception
        GetOperating = False
        hs.WriteLog(g_szScriptName, "Error in GetOperating: " & ex.ToString)
    End Try
    hs.WriteLog(g_szScriptName, "GetOperating: " & GetOperating)
End Function

Public Sub CmdSetHeat(ByVal parms As String)
    Dim name As String
    Dim HSThermostat As Integer
    Dim Temperature As Double
    Dim Param() As String
    Param = parms.Split(";")
    Try
        name = Param(0)
        Temperature = Param(1)
        HSThermostat = GetThermostatByName(name)
        hs.WriteLog(g_szScriptName, "CmdSetHeat: " & parms.ToString)
        hs.Plugin("Enviracom Manager").CmdSetHeat(HSThermostat, Temperature, 0)
    Catch ex As Exception
        hs.WriteLog(g_szScriptName, "Error in CmdSetHeat: " & ex.ToString)
    End Try
End Sub

Public Sub CmdSetCool(ByVal parms As String)
    Dim name As String
    Dim HSThermostat As Integer
    Dim Temperature As Double
    Dim Param() As String
    Param = parms.Split(";")
    Try
        name = Param(0)
        Temperature = Param(1)
        HSThermostat = GetThermostatByName(name)
        hs.WriteLog(g_szScriptName, "CmdSetCool: " & parms.ToString)
        hs.Plugin("Enviracom Manager").CmdSetCool(HSThermostat, Temperature, 0)
    Catch ex As Exception
        hs.WriteLog(g_szScriptName, "Error in CmdSetCool: " & ex.ToString)
    End Try
End Sub

Public Sub CmdSetMode(ByVal parms As String)
    Dim name As String
    Dim HSThermostat As Integer
    Dim Mode As Integer
    Dim Param() As String
    Param = parms.Split(";")
    Try
        name = Param(0)
        Mode = Param(1)
        HSThermostat = GetThermostatByName(name)
        hs.WriteLog(g_szScriptName, "CmdSetMode: " & parms.ToString)
        hs.Plugin("Enviracom Manager").CmdSetMode(HSThermostat, Mode, 0)
    Catch ex As Exception
        hs.WriteLog(g_szScriptName, "Error in CmdSetMode: " & ex.ToString)
    End Try
End Sub

Public Sub CmdSetFan(ByVal parms As String)
    Dim name As String
    Dim HSThermostat As Integer
    Dim Mode As Integer
    Dim Param() As String
    Param = parms.Split(";")
    Try
        name = Param(0)
        Mode = Param(1)
        HSThermostat = GetThermostatByName(name)
        hs.WriteLog(g_szScriptName, "CmdSetFan: " & parms.ToString)
        hs.Plugin("Enviracom Manager").CmdSetFan(HSThermostat, Mode, 0)
    Catch ex As Exception
        hs.WriteLog(g_szScriptName, "Error in CmdSetFan: " & ex.ToString)
    End Try
End Sub

Public Sub CmdSetHold(ByVal parms As String)
    Dim name As String
    Dim HSThermostat As Integer
    Dim Mode As Integer
    Dim Param() As String
    Param = parms.Split(";")
    Try
        name = Param(0)
        Mode = Param(1)
        HSThermostat = GetThermostatByName(name)
        hs.WriteLog(g_szScriptName, "CmdSetHold: " & parms.ToString & " Stat# " & HSThermostat.ToString)
        hs.Plugin("Enviracom Manager").CmdSetHold(HSThermostat, Mode, 0)
    Catch ex As Exception
        hs.WriteLog(g_szScriptName, "Error in CmdSetHold: " & ex.ToString)
    End Try
End Sub

Public Sub GetOutdoorTemp(ByVal parms As String)
    Dim name As String
    Dim HSThermostat As Integer
    Dim Param() As String
    Param = parms.Split(";")
    Try
        name = Param(0)
        HSThermostat = GetThermostatByName(name)
        hs.WriteLog(g_szScriptName, "GetOutdoorTemp: " & parms.ToString)
        hs.Plugin("Enviracom Manager").GetOutdoorTemp(HSThermostat, 0)
    Catch ex As Exception
        hs.WriteLog(g_szScriptName, "Error in GetOutdoorTemp: " & ex.ToString)
    End Try
End Sub

Public Sub GetHumidity(ByVal parms As String)
    Dim name As String
    Dim HSThermostat As Integer
    Dim Param() As String
    Param = parms.Split(";")
    Try
        name = Param(0)
        HSThermostat = GetThermostatByName(name)
        hs.WriteLog(g_szScriptName, "GetHumidity: " & parms.ToString)
        hs.Plugin("Enviracom Manager").GetHumidity(HSThermostat, 0)
    Catch ex As Exception
        hs.WriteLog(g_szScriptName, "Error in GetHumidity: " & ex.ToString)
    End Try
End Sub

Public Sub CmdSetRecirc(ByVal parms As String)
    Dim name As String
    Dim HSThermostat As Integer
    Dim bReCirculateOn As Boolean
    Dim Param() As String
    Param = parms.Split(";")
    Try
        name = Param(0)
        bReCirculateOn = Param(1)
        HSThermostat = GetThermostatByName(name)
        hs.WriteLog(g_szScriptName, "CmdSetRecirc: " & parms.ToString)
        hs.Plugin("Enviracom Manager").CmdSetRecirc(HSThermostat, bReCirculateOn, 0)
    Catch ex As Exception
        hs.WriteLog(g_szScriptName, "Error in CmdSetRecirc: " & ex.ToString)
    End Try
End Sub

Public Sub CmdSetClock(ByVal parms As String)
    Try
        hs.Plugin("Enviracom Manager").CmdSetClock()
    Catch ex As Exception
        hs.WriteLog(g_szScriptName, "Error in CmdSetClock: " & ex.ToString)
    End Try
End Sub

Public Sub CmdQuerySchedules(ByVal parms As String)
    Dim name As String
    Dim HSThermostat As Integer
    Dim Param() As String
    Param = parms.Split(";")
    Try
        name = Param(0)
        HSThermostat = GetThermostatByName(name)
        hs.WriteLog(g_szScriptName, "CmdQuerySchedules: " & parms.ToString)
        hs.Plugin("Enviracom Manager").CmdQuerySchedules(HSThermostat)
    Catch ex As Exception
        hs.WriteLog(g_szScriptName, "Error in CmdQuerySchedules: " & ex.ToString)
    End Try
End Sub

'Get the homeseer thermostat number given the device name

Public Function GetThermostatByName(name As String) As Integer
    Try
        hs.WriteLog(g_szScriptName, "GetThermostatByName: " & name)
        GetThermostatByName = hs.Plugin("Enviracom Manager").GetThermostatByName(name)
        hs.WriteLog(g_szScriptName, "GetThermostatByName: " & name & " Result: " & GetThermostatByName)
    Catch ex As Exception
        GetThermostatByName = -1
        hs.WriteLog(g_szScriptName, "Error in GetThermostatByName: " & ex.ToString)
    End Try
End Function


'Get the homeseer thermostat number given the house code and device code

Public Function GetThermostatByCode(parms As String) As Integer
    Dim hc, dc As String
    Dim Param() As String
    Param = parms.Split(";")

    Try
        hc = Param(0)
        dc = Param(1)
        hs.WriteLog(g_szScriptName, "GetThermostatByCode: House Code: " & hc & "Device Code " & dc)
        GetThermostatByCode = hs.Plugin("Enviracom Manager").GetThermostatByCode(dc, hc)
    Catch ex As Exception
        GetThermostatByCode = -1
        hs.WriteLog(g_szScriptName, "Error in GetThermostatByCode: " & ex.ToString)
    End Try
End Function

' this only need to be run once to add the button

Sub AddEnviracomExtraBtns(ByVal parms As String)
    hs.devicebuttonRemove(g_szENVMGR_HouseCode + "99", "CmdSetClock")
    hs.devicebuttonAdd(g_szENVMGR_HouseCode + "99", "enviracomextras.vb(""CmdSetClock"", """")", "CmdSetClock" + CHR(4))
    hs.devicebuttonRemove(g_szENVMGR_HouseCode + "99", "CmdQuerySchedules")
    hs.devicebuttonAdd(g_szENVMGR_HouseCode + "99", "enviracomextras.vb(""CmdQuerySchedules"", """")", "CmdQuerySchedules" + CHR(4))
    hs.devicebuttonRemove(g_szENVMGR_HouseCode + "99", "CmdSetHold On Z5")
    hs.devicebuttonAdd(g_szENVMGR_HouseCode + "99", "enviracomextras.vb(""CmdSetHold"", ""Unit 1 Thermostat 5;1"")", "CmdSetHold On Z5" + CHR(4))
    hs.devicebuttonRemove(g_szENVMGR_HouseCode + "99", "CmdSetHold Off Z5")
    hs.devicebuttonAdd(g_szENVMGR_HouseCode + "99", "enviracomextras.vb(""CmdSetHold"", ""Unit 1 Thermostat 5;0"")", "CmdSetHold Off Z5" + CHR(4))
    hs.devicebuttonRemove(g_szENVMGR_HouseCode + "99", "CmdSetHold On Z4")
    hs.devicebuttonAdd(g_szENVMGR_HouseCode + "99", "enviracomextras.vb(""CmdSetHold"", ""Unit 1 Thermostat 4;1"")", "CmdSetHold On Z4" + CHR(4))
    hs.devicebuttonRemove(g_szENVMGR_HouseCode + "99", "CmdSetHold Off Z4")
    hs.devicebuttonAdd(g_szENVMGR_HouseCode + "99", "enviracomextras.vb(""CmdSetHold"", ""Unit 1 Thermostat 4;0"")", "CmdSetHold Off Z4" + CHR(4))
    hs.devicebuttonRemove(g_szENVMGR_HouseCode + "99", "CmdSetHold On Z3")
    hs.devicebuttonAdd(g_szENVMGR_HouseCode + "99", "enviracomextras.vb(""CmdSetHold"", ""Unit 1 Thermostat 3;1"")", "CmdSetHold On Z3" + CHR(4))
    hs.devicebuttonRemove(g_szENVMGR_HouseCode + "99", "CmdSetHold Off Z3")
    hs.devicebuttonAdd(g_szENVMGR_HouseCode + "99", "enviracomextras.vb(""CmdSetHold"", ""Unit 1 Thermostat 3;0"")", "CmdSetHold Off Z3" + CHR(4))
    hs.devicebuttonRemove(g_szENVMGR_HouseCode + "99", "CmdSetHold On Z2")
    hs.devicebuttonAdd(g_szENVMGR_HouseCode + "99", "enviracomextras.vb(""CmdSetHold"", ""Unit 1 Thermostat 2;1"")", "CmdSetHold On Z2" + CHR(4))
    hs.devicebuttonRemove(g_szENVMGR_HouseCode + "99", "CmdSetHold Off Z2")
    hs.devicebuttonAdd(g_szENVMGR_HouseCode + "99", "enviracomextras.vb(""CmdSetHold"", ""Unit 1 Thermostat 2;0"")", "CmdSetHold Off Z2" + CHR(4))
    hs.devicebuttonRemove(g_szENVMGR_HouseCode + "99", "CmdSetHold On Z1")
    hs.devicebuttonAdd(g_szENVMGR_HouseCode + "99", "enviracomextras.vb(""CmdSetHold"", ""Unit 1 Thermostat 1;1"")", "CmdSetHold On Z1" + CHR(4))
    hs.devicebuttonRemove(g_szENVMGR_HouseCode + "99", "CmdSetHold Off Z1")
    hs.devicebuttonAdd(g_szENVMGR_HouseCode + "99", "enviracomextras.vb(""CmdSetHold"", ""Unit 1 Thermostat 1;0"")", "CmdSetHold Off Z1" + CHR(4))
End Sub


Private Function ENVMGRCalcChecksum(ByVal szMessage As String) As String
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
                hs.WriteLog(g_szScriptName, "Bad priority character in message: '" + szMessage + "'")
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

                hs.WriteLog(g_szScriptName, "Bad service character in message: '" + szMessage + "'")
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
        hs.WriteLog(g_szScriptName, "CheckSum: Reporting exception: " & ex.ToString() & " during checksum generation")
        Return &HFF
    End Try
    Return Chksum.ToString("X2")
End Function
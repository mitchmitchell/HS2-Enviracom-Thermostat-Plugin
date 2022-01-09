Imports System.ComponentModel
Imports System.Configuration.Install
Imports System.IO


Public Class SettingsInstaller
    Inherits System.Configuration.Install.Installer

    ' enable the setting type for your plugin
    Dim PlugInType As String = "io_interfaces"              ' for generic plugins
    'Dim PlugInType As String = "gSelectedIRInterface"      ' for Infrared plugins
    'Dim PlugInType As String = "gSelectedX10Interface"     ' for X10 plugins

    Dim InstallDir As String    ' directory where HomeSeer is installed
    Dim PluginName As String    ' name of plugin


    Public Overrides Sub Install(ByVal stateSaver As System.Collections.IDictionary)
        MyBase.Install(stateSaver)

        InstallDir = Me.Context.Parameters("targ")
        PluginName = Me.Context.Parameters("plugin_name")               ' full name like "HomeSeer Plug-in iTunes"
        PluginName = Replace(PluginName, "HomeSeer Plug-in", "").Trim   ' get the name of the plugin only like "iTunes"

        InstallOptions()

    End Sub

    Private Sub InstallOptions()
        Dim options As New DlgOptions
        Try
            options.Text = PluginName & " Plug-In Installation Options"
            options.ShowDialog()

            If options._Activate Then
                ' edit settings.ini and enable this plugin

                Dim csetting As String
                csetting = GetConfigSetting("Settings", PlugInType, InstallDir & "config\settings.ini")

                If csetting.Contains(PluginName) Then
                    ' already active
                Else
                    If csetting = "" Then
                        ' no plugins are set, just set this one
                        csetting &= PluginName
                    Else
                        ' append this plugin to the list
                        csetting &= "," & PluginName
                    End If
                End If
                SaveConfigSetting("Settings", PlugInType, csetting, InstallDir & "config\settings.ini")
            Else
                ' do not enable plugin
            End If

            ' handle COM Port setting
            ' HomeSeer maintains a com port setting for I/O devices in the INI setting "comport"
            ' You may store your com port setting in your own INI setting, change that here
            If options._ComPort <> "" Then
                SaveConfigSetting("Settings", "comport", options._ComPort, InstallDir & "config\settings.ini")
            End If
        Catch ex As Exception

        End Try

        

        ' handle other INI file options here
    End Sub

    Public Overrides Sub Uninstall(ByVal savedState As System.Collections.IDictionary)
        MyBase.Uninstall(savedState)

        Try
            InstallDir = Me.Context.Parameters("targ")
            PluginName = Me.Context.Parameters("plugin_name")
            PluginName = Replace(PluginName, "HomeSeer Plug-in", "").Trim   ' get the name of the plugin only

            ' remove the INI settings
            Dim csetting As String
            csetting = GetConfigSetting("Settings", PlugInType, InstallDir & "config\settings.ini")

            csetting = Replace(csetting, PluginName, "")
            SaveConfigSetting("Settings", PlugInType, csetting, InstallDir & "config\settings.ini")

            ' no need to remove any COM Port settings

            ' handle any other INI settings here
        Catch ex As Exception

        End Try
        
    End Sub

    Public Sub New()
        MyBase.New()

        'This call is required by the Component Designer.
        InitializeComponent()

        'Add initialization code after the call to InitializeComponent

    End Sub




    Public Function GetConfigSetting(ByVal section As String, ByVal key As String, ByVal inifile As String) As String
        Dim ini As New CiniFile

        Try
            ini.INIFile = inifile
            Return ini.GetFile(section, key)
        Catch ex As Exception

        End Try
        Return ""
    End Function

    Public Sub SaveConfigSetting(ByVal section As String, ByVal key As String, ByVal Value As String, ByVal inifile As String)
        Dim ini As New CiniFile

        Try
            ini.INIFile = inifile
            ini.WriteFile(section, key, Value)
        Catch ex As Exception
        End Try

    End Sub

End Class

#Region "Support Functions"
Public Class CiniFile

    '// Private member that holds a reference to
    '// the path of our ini file
    Private strInI As String

    '// Win API Declares
    Private Declare Function WritePrivateProfileString Lib "kernel32" Alias "WritePrivateProfileStringA" _
                            (ByVal lpApplicationName As String, _
                             ByVal lpKeyName As String, _
                             ByVal lpString As String, _
                             ByVal lpFileName As String) As Integer

    Private Declare Function GetPrivateProfileString Lib "kernel32" Alias "GetPrivateProfileStringA" _
                            (ByVal lpApplicationName As String, _
                             ByVal lpKeyName As String, _
                             ByVal lpDefault As String, _
                             ByVal lpReturnedString As String, _
                             ByVal nSize As Integer, _
                             ByVal lpFileName As String) As Integer

    Private Declare Function WritePrivateProfileSection Lib "kernel32" Alias "WritePrivateProfileSectionA" _
                            (ByVal lpAppName As String, _
                             ByVal lpString As String, _
                             ByVal lpFileName As String) As Integer

    Private Declare Function GetPrivateProfileSection Lib "kernel32" Alias "GetPrivateProfileSectionA" _
                            (ByVal lpAppName As String, _
                             ByVal lpReturnedString As String, _
                             ByVal nSize As Integer, _
                             ByVal lpFileName As String) As Integer


    ' get a section
    Public Function GetSection(ByRef Name As String, ByRef FileName As String) As String
        Dim st As String = ""
        Dim items As Object
        Dim count As Integer
        Dim i As Integer
        Dim s As String = ""
        On Error Resume Next

        st = New String(Chr(0), 16000)

        ' returns all values seperated with a null character
        ' string is terminated with two nulls
        count = GetPrivateProfileSection(Name, st, 16000, FileName)
        If count <> 0 Then
            s = Mid(st, 1, count - 1)
        End If
        Return s
    End Function

    ' clear a section
    Public Sub ClearSection(ByRef Name As String, ByRef FileName As String)
        WritePrivateProfileSection(Name, "", FileName)
    End Sub



    Public Sub WriteFile(ByRef strSection As String, ByRef strKey As String, ByRef strValue As String)

        Dim R As Integer

        '// Write to strINI
        R = WritePrivateProfileString(strSection, strKey, strValue, strInI)
        'WriteMon("clsCiniFile", "Writing section " & strSection & ", key " & strKey & ", Value " & strValue & " to INI file " & strInI)

    End Sub

    Public Function GetFile(ByRef strSection As String, ByRef strKey As String) As String

        Dim strTmp As String = ""
        Dim lngRet As Integer


        strTmp = New String(Chr(0), 500)       'Try it with a small buffer first
        lngRet = GetPrivateProfileString(strSection, strKey, "", strTmp, 500, strInI)

        If lngRet >= 499 Then         'oops, we needed a bigger buffer, so try again
            strTmp = New String(Chr(0), 16000)
            lngRet = GetPrivateProfileString(strSection, strKey, "", strTmp, 16000, strInI)
        End If

        If lngRet = 0 Then
            GetFile = ""
        Else
            GetFile = Left(strTmp, lngRet)
        End If

    End Function


    Public Property INIFile() As String
        Get

            '// Returns the current ini path
            INIFile = strInI

        End Get
        Set(ByVal Value As String)

            '// Sets the new ini path
            strInI = Value

        End Set
    End Property

End Class
#End Region


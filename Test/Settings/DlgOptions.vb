Imports System.Windows.Forms

' by default, the com port setting is hidden. If your plugin uses a COM port, set the comport label and the comport
' text box visible property to True

Public Class DlgOptions

    Public _Activate As Boolean
    Public _ComPort As String = ""

    Public Event Complete()

    Private Sub OK_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK_Button.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.OK
        If ChkActivate.Checked Then
            _Activate = True
        Else
            _Activate = False
        End If
        If TxtComPort.Visible Then
            _ComPort = TxtComPort.Text
        End If
        RaiseEvent Complete()
        Me.Close()
    End Sub

    Private Sub Cancel_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub

End Class

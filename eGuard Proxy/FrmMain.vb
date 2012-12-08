Public Class FrmMain

    Private isListening As Boolean = False
    Private CListener As System.Net.Sockets.TcpListener
    Public MyCn As New System.Data.OleDb.OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" & Application.StartupPath & "\eGProxy.mdb;Persist Security Info=True")
    Private curFrm As New Windows.Forms.Form

#Region "Start Proxy Server"
    Private Sub FormLoaded_StartNow(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Start_Proxy_Server_Now(Nothing, Nothing)
    End Sub
    Private Sub Start_Proxy_Server_Now(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles BtnStart.Click
        If IsNumeric(TextBox1.Text) Then
            CListener = New Net.Sockets.TcpListener(Net.Dns.GetHostEntry("localhost").AddressList(0), Val(TextBox1.Text))
            CListener.Start()
            ClientAccepter.Enabled = True
            isListening = True
            ClientAccepter.Start()
            'Enviormental Settings..
            TextBox1.Enabled = False
            Me.TrayIcon.Icon = My.Resources.Started
            BtnStart.Enabled = False
            BtnStop.Enabled = True
        Else
            MsgBox("Invalid Port Number..!")
        End If
    End Sub

    Private Sub Start_OR_Stop_Tray(ByVal sender As System.Object, ByVal e As System.EventArgs)
        If String.Compare(sender.ToString, "Stop Proxy Server", False) Then
            Start_Proxy_Server_Now(Nothing, Nothing)
        Else
            Stop_Proxy_Server(Nothing, Nothing)
        End If
    End Sub
#End Region

#Region "Stop Proxy Server"
    Private Sub Stop_Proxy_Server(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles BtnStop.Click
        If isListening Then
            Try
                ClientAccepter.Stop()
                ClientAccepter.Enabled = False
                CListener.Stop()
                'Enviornmental Settings..
                BtnStart.Enabled = True
                TextBox1.Enabled = True
                BtnStop.Enabled = False
                Me.TrayIcon.Icon = My.Resources.Stopped
                isListening = False
            Catch ex As Exception
                MsgBox(ex.Message)
            End Try
        End If
    End Sub
#End Region

#Region "Manage Blocked Addresses"

    Private Sub Show_Blocking_CPanel(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button6.Click
        If Not curFrm Is Nothing Then
            curFrm.Dispose()
        End If
        curFrm = New FrmSiteBlocker
        curFrm.ShowDialog()
    End Sub

#End Region

#Region "Accecpt Client Requests"
    Private Sub Client_Conn_Accept(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ClientAccepter.Tick
        If CListener.Pending Then
            Dim CurClient As System.Net.Sockets.Socket = CListener.AcceptSocket
            Dim obj As New Req_Processor
            obj.ClientConnection = CurClient
            Dim C_Server As Threading.Thread = New Threading.Thread(AddressOf obj.ProcessThis)
            C_Server.Start()
        End If
    End Sub

#End Region

#Region "Exit Application"
    Private Sub Exit_Proxy(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        Stop_Proxy_Server(Nothing, Nothing)
        TrayIcon.Visible = False
        Application.Exit()
    End Sub
#End Region

#Region "Show Aboutbox"

    Private Sub Show_AboutBox(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        If Not curFrm Is Nothing Then
            curFrm.Dispose()
        End If
        curFrm = New AbouteGUARD
        curFrm.ShowDialog()
    End Sub

#End Region

#Region "Hide / Display Settings"
    Private Sub TrayIcon_MouseClick(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles TrayIcon.MouseClick
        If Not Me.Visible Then
            If e.Button = Windows.Forms.MouseButtons.Right Then
                Me.Show()
            ElseIf e.Button = Windows.Forms.MouseButtons.Left Then
                If isListening Then
                    TrayIcon.ShowBalloonTip(5000, "eGuard Proxy Server..", "Proxy Server is currently ON.", ToolTipIcon.Info)
                Else
                    TrayIcon.ShowBalloonTip(5000, "eGuard Proxy Server..", "Proxy Server is currently OFF.", ToolTipIcon.Info)
                End If
            End If
        End If
    End Sub

#End Region

#Region "Show Help..."
    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click
        Help.ShowHelp(sender, My.Application.Info.DirectoryPath & "/Manual.CHM", HelpNavigator.TableOfContents)
    End Sub
#End Region

End Class

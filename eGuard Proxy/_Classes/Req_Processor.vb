Public Class Req_Processor

    Private _cSocket As System.Net.Sockets.Socket
    Private RequestedURL, _RequestedHOSTName As String
    Private _RequestedPort As Integer = 80

    Public WriteOnly Property ClientConnection() As System.Net.Sockets.Socket
        Set(ByVal value As System.Net.Sockets.Socket)
            _cSocket = value
        End Set
    End Property

    Sub ProcessThis()
        Dim data(8192) As Byte
        Dim blen As Integer
        If Not _cSocket Is Nothing Then
            Try
                blen = _cSocket.Receive(data)
                Dim HTTP_GET_Request As String = System.Text.Encoding.ASCII.GetString(data)
                '^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
                ExtractURL(HTTP_GET_Request)
                '^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
                If isblocked(RequestedURL) Then
                    'This URL is Blocked by Netwok Administrator...
                    _cSocket.Send(System.Text.Encoding.ASCII.GetBytes("<h2>eGuard Proxy Server : Blocked Site</h2><hr><p>The web page requested is <b>BANNED</b> by Network Administrator..</p>"))
                Else
                    'This URL is Not Blocked... Now Site Will be Loaded...
                    Load_Requested_Site(HTTP_GET_Request)
                End If
            Catch ex As Exception
                Try
                    _cSocket.Send(System.Text.Encoding.ASCII.GetBytes("<h2>eGuard Proxy Server : Crytical Error</h2><hr><p>Please Inform your Network Administrator. <b>" & ex.Message & "</b></p>"))
                Catch ex1 As Exception
                End Try
            End Try
            _cSocket.Close()
        End If

    End Sub

    Private Sub Load_Requested_Site(ByVal siteURL As String)
        Dim IPhost As System.Net.IPHostEntry
        Dim IPEP As System.Net.IPEndPoint
        Dim NetSock As System.Net.Sockets.Socket
        Try
            '=============================================='
            'Send Request to Remote Web Server at _Port
            '=============================================='
            IPhost = System.Net.Dns.GetHostEntry(_RequestedHOSTName)
            IPEP = New System.Net.IPEndPoint(IPhost.AddressList(0), _RequestedPort)
            NetSock = New System.Net.Sockets.Socket(IPEP.AddressFamily, System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp)
            NetSock.Connect(IPEP)
            Dim i As Integer
            i = NetSock.Send(New System.Text.ASCIIEncoding().GetBytes(siteURL))
            '=============================================='
            'Receive From Remote Web Server at _Port and send it to client..
            '=============================================='
            Dim barray(8192) As Byte
            Dim retval As Integer
            retval = NetSock.Receive(barray)
            _cSocket.Send(barray)
        Catch ex As Exception
            Try
                _cSocket.Send(System.Text.Encoding.ASCII.GetBytes("<h2>eGuard Proxy Server : Non-Reachable..</h2><hr><p>Can't Resolve URL : <b>" & RequestedURL & "</b></p>"))
            Catch ex1 As Exception
            End Try
        End Try
        _cSocket.Close()
    End Sub

    Private Sub ExtractURL(ByVal inp As String)
        Dim i As Long
        Dim rt As String
        Dim cp As String
        Dim inp1 As String
        rt = ""
        cp = ""
        inp1 = Mid(inp, 5, Len(inp) - 5)
        For i = 1 To Len(inp1)
            cp = Mid(inp1, i, 6)
            If cp = " HTTP/" Then
                RequestedURL = rt
                Exit For
            End If
            rt = rt + Mid(inp1, i, 1)
        Next i
        RequestedURL = Trim(rt)
        'Now Extract the Host Name...and the PortNumber...
        rt = RequestedURL
        If rt.StartsWith("http://") Then
            rt = rt.Remove(0, 7)
        End If
        cp = rt.Split("/")(0)
        If (cp.Contains(":")) Then
            Dim tmp() As String = cp.Split(":")
            _RequestedHOSTName = tmp(0)
            If Val(tmp(1)) > 0 Then
                _RequestedPort = Val(tmp(1))
            End If
        Else
            _RequestedHOSTName = cp.Trim
        End If
    End Sub

    Private Function isBlocked(ByVal URLtoCHECK As String) As Boolean
        '===============================
        'Make the Requested URL in th form yahoo.com (from http:// , www. ,etc)
        '===============================
        If URLtoCHECK.StartsWith("http://") Then
            URLtoCHECK = URLtoCHECK.Remove(0, 7)
        End If
        If URLtoCHECK.StartsWith("www") Then
            URLtoCHECK = URLtoCHECK.Remove(0, 4)
        End If
        If URLtoCHECK.EndsWith("/") Then
            URLtoCHECK = URLtoCHECK.Remove(URLtoCHECK.Length - 1)
        End If
        '===============================
        'Check if it is in Database
        '===============================
        Try
            Dim MyCMD As New System.Data.OleDb.OleDbCommand("SELECT * FROM Sites WHERE ipaddress LIKE '%" & URLtoCHECK & "%' OR domainname LIKE '%" & URLtoCHECK & "%'", My.Forms.FrmMain.MyCn)
            MyCMD.Connection.Open()
            Dim MyDataReader As System.Data.OleDb.OleDbDataReader = MyCMD.ExecuteReader()
            If MyDataReader.Read() Then
                Return True
            Else
                Return False
            End If
            MyCMD.Connection.Close()
        Catch ex As Exception
            Return False
        End Try
    End Function

End Class

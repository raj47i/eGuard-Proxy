Public Class FrmSiteBlocker

#Region "Add New Blocks.."

    Private Sub txtDomain_Sele(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtDomain.Enter
        If txtDomain.Text = "Enter Domain / IP Here" Then
            txtDomain.Text = ""
            txtDomain.Select()
        End If
    End Sub
    Private Sub Btn_New_BLOCK(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        txtDomain.Text = txtDomain.Text.Trim()
        Dim AlreadyBLOCKED As Boolean = False
        Dim curRow As DataGridViewRow
        'Check IF already Blocked...!
        For Each curRow In grid.Rows
            If curRow.Cells("domainname").Value.ToString = txtDomain.Text Or curRow.Cells("ipaddress").Value.ToString = txtDomain.Text Then
                AlreadyBLOCKED = True
            End If
        Next
        '
        If txtDomain.Text = "" Or txtDomain.Text = "Enter Domain / IP Here" Then
            MsgBox("You must enter A Domain Name or IP Address..")
        Else
            If IsIPAddress(txtDomain.Text) Then
                If AlreadyBLOCKED Then
                    MsgBox("[" & txtDomain.Text & "] is already Blocked..")
                ElseIf MsgBox("Are Sure that you want to block the IP Address [" & txtDomain.Text & "]..?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
                    Add_To_Database(txtDomain.Text, True)
                End If
            ElseIf txtDomain.Text.Contains(" ") And Not AlreadyBLOCKED Then
                If MsgBox("[" & txtDomain.Text & "] doesn't seems to be a valid Domain, Do you wish to Block any way..?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
                    Add_To_Database(txtDomain.Text, False)
                End If
            ElseIf AlreadyBLOCKED Then
                MsgBox("[" & txtDomain.Text & "] is already Blocked..")
            Else
                Add_To_Database(txtDomain.Text, False)
            End If
        End If
    End Sub
    Private Sub Add_To_Database(ByVal Str As String, ByVal IsIP As Boolean)
        Dim MyCMD As New Data.OleDb.OleDbCommand
        If IsIP Then
            MyCMD = New Data.OleDb.OleDbCommand("INSERT INTO Sites VALUES('" & Str & "','')", FrmMain.MyCn)
        Else
            MyCMD = New Data.OleDb.OleDbCommand("INSERT INTO Sites VALUES('','" & Str & "')", FrmMain.MyCn)
        End If
        Try
            MyCMD.Connection.Open()
            MyCMD.ExecuteNonQuery()
            MyCMD.Connection.Close()
        Catch ex As Exception
        End Try
        PopulateGrid()
    End Sub

#End Region

    Private Sub PopulateGrid()
        Try
            Dim MyDA As New Data.OleDb.OleDbDataAdapter("SELECT * from Sites ORDER BY domainname,ipaddress", FrmMain.MyCn)
            Dim MyTable As New Data.DataTable
            MyDA.Fill(MyTable)
            grid.DataSource = MyTable
            txtDomain.Text = "Enter Domain / IP Here"
            MyDA.Dispose()
            MyTable.Dispose()
            grid.Columns("domainname").HeaderText = "Domain Name"
            grid.Columns("ipaddress").HeaderText = "IP Address"
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub
    Private Function IsIPAddress(ByVal DestnIP As String) As Boolean
        Dim i, p As Byte
        Try
            Dim IPParts() As String = DestnIP.Split(".")
            p = IPParts.GetLength(0)
            If Not p = 4 Then
                Return False
            Else
                For i = 0 To 3
                    If Not IsNumeric(IPParts(i)) Then
                        Return False
                    ElseIf Val(IPParts(i)) > 255 Then
                        Return False
                    ElseIf Val(IPParts(i)) < 0 Then
                        Return False
                    End If
                Next
            End If
            Return True
        Catch
            Return False
        End Try
    End Function
    Private Sub FrmSiteBlocker_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        PopulateGrid()
    End Sub
    Private Sub RemoveFROM_List(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles grid.CellDoubleClick
        Dim MyCMD As Data.OleDb.OleDbCommand
        MyCMD = New Data.OleDb.OleDbCommand("DELETE FROM sites WHERE ipaddress='" & grid.Rows(e.RowIndex).Cells("ipaddress").Value & "'AND domainname='" & grid.Rows(e.RowIndex).Cells("domainname").Value & "'", FrmMain.MyCn)
        If Not FrmMain.MyCn.State = ConnectionState.Open Then
            FrmMain.MyCn.Open()
        End If
        If MsgBox("Do you wish to Unblock this IP / domain Address ?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then MyCMD.ExecuteNonQuery()
        FrmMain.MyCn.Close()
        PopulateGrid()
    End Sub
End Class
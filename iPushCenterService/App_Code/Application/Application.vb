' NOTE: You can use the "Rename" command on the context menu to change the class name "ApplicationAdmin" in code, svc and config file together.
Imports System.Data
Imports System.Data.SqlClient
Imports System.Globalization
Imports System.IO
Imports System.Net
Imports System.Net.Http
Imports System.Threading.Tasks
Imports System.Web.Configuration
Imports System.Web.Script.Serialization
Imports Newtonsoft.Json

Public Class Application
    Implements IApplication
    Dim cultures As New CultureInfo("en-US")
    Dim dateFormat As DateTimeFormatInfo = cultures.DateTimeFormat
    Dim MainClass As New MainClass
    Dim NotiflClass As New Notification
    Dim Connect_DATABASE As String = WebConfigurationManager.ConnectionStrings("ConnectionString").ConnectionString
    Dim appSettings = ConfigurationManager.AppSettings
    Dim NotifyThreading() As Threading.Thread
    Public Shared ErrorNotifyThreading As String = ""
    Public Shared ErrorNotifyThreading_Msg As String = ""

    Sub TestPush() Implements IApplication.TestPush

        PrepareDataPushNotifyMessage("", "", "", "")
        SetVersion("")
        PushNotifyCondition("", "", "", "", "")

    End Sub

    Function CenterPushNotification(ByVal key As String,
                                   ByVal jsonStr As String) As String Implements IApplication.CenterPushNotification

        Dim sendSuccess As Integer = 0
        Dim dataPush As Integer = 0
        Dim ds1 As DataSet
        Try
            If (MainClass.KeyValidate_WCF(key)) Then

                Dim deserial = Function(str As String)
                                   Dim json As New JavaScriptSerializer
                                   json.MaxJsonLength = Int32.MaxValue
                                   Return json.Deserialize(Of CenterNoti)(str)
                               End Function
                Dim obj As CenterNoti = deserial(jsonStr)

                Try

                    Dim parameters() As SqlParameter = New SqlParameter() _
                                       {
                                         New SqlParameter("@P_KEY1", SqlDbType.VarChar, 50) With {.Value = obj.PKEY1}
                                       }
                    ds1 = MainClass.ExecuteDataset(Connect_DATABASE, "dbo.usp_GetFCMToken", parameters)

                Catch ex As Exception

                    Throw ex
                End Try


                Dim dsKey As DataSet = NotiflClass.GetNotificationKey()

                If (ds1.Tables(0).Rows.Count > 0) Then

                    For Each item As DataRow In ds1.Tables(0).Rows

                        Dim parameters() As SqlParameter = New SqlParameter() _
                                       {
                                          New SqlParameter("@P_FCMTOKEN", SqlDbType.VarChar, 50) With {.Value = item("FCM_TOKEN").ToString()},
                                          New SqlParameter("@P_CUSTID", SqlDbType.VarChar, 50) With {.Value = item("CUST_ID").ToString()},
                                          New SqlParameter("@P_EXT_CONTRACT", SqlDbType.VarChar, 50) With {.Value = obj.PKEY1},
                                          New SqlParameter("@P_TITLE_TH", SqlDbType.VarChar, 1000) With {.Value = obj.PKEY2},
                                          New SqlParameter("@P_TITLE_EN", SqlDbType.VarChar, 1000) With {.Value = obj.PKEY3},
                                          New SqlParameter("@P_MESSGE_TH", SqlDbType.VarChar, -1) With {.Value = obj.PKEY4},
                                          New SqlParameter("@P_MESSGE_EN", SqlDbType.VarChar, -1) With {.Value = obj.PKEY5},
                                          New SqlParameter("@P_SELECT", SqlDbType.VarChar, 100) With {.Value = obj.PSELECT.ToString()}
                                       }
                        Dim ds2 As DataSet = MainClass.ExecuteDataset(Connect_DATABASE, "dbo.usp_SetMessageNotifyCenter", parameters)

                        Try
                            If Not String.IsNullOrEmpty(ds2.Tables(0).Rows(0)("JSON_RESULT").ToString()) Then
                                dataPush += 1
                                Dim resData = ds2.Tables(0).Rows(0)("JSON_RESULT").ToString()
                                Dim servPush = ds2.Tables(0).Rows(0)("SERVPUSH").ToString()
                                Dim pushStatus = "N"
                                Dim resPush = NotiflClass.SendNotification(resData, servPush, dsKey, obj.PKEY1.ToString(), item("CUST_ID").ToString(), obj.PSELECT.ToString())

                                sendSuccess += 1

                            End If
                        Catch ex As Exception
                            Throw ex
                        End Try
                    Next
                End If
            Else
                Throw New Exception("Your key is invalid!")
            End If

            Return "Data Push :  " + sendSuccess.ToString
        Catch ex As Exception
            Return ex.Message.ToString
        End Try
        Return "Data Push : " + sendSuccess.ToString
    End Function


#Region "Run "

    Sub SetVersion(ByVal key As String) Implements IApplication.SetVersion
        Try

            Dim ds As DataSet = MainClass.ExecuteDatasetNoParam(Connect_DATABASE, "dbo.usp_Push_Version__I")

        Catch ex As Exception
            Throw ex
        End Try
    End Sub

    Function PrepareDataPushNotifyMessage(ByVal key As String,
                                          Optional ByVal P_SELECT As String = "",
                                          Optional ByVal P_KEY1 As String = "",
                                          Optional ByVal P_KEY2 As String = "") As String Implements IApplication.PrepareDataPushNotifyMessage
        Try

            Dim version As String = 0
            Dim sp As New SqlParameter()
            Dim parameters() As SqlParameter = New SqlParameter() _
                               {
                                 New SqlParameter("@P_SELECT", SqlDbType.VarChar, 50) With {.Value = P_SELECT},
                                 New SqlParameter("@P_KEY1", SqlDbType.VarChar, 50) With {.Value = P_KEY1},
                                 New SqlParameter("@P_KEY2", SqlDbType.VarChar, 50) With {.Value = P_KEY2}
                               }
            Dim ds As DataSet = MainClass.ExecuteDataset_Noti(Connect_DATABASE, "dbo.usp_SetPushNofityMessage_Tracking", parameters)
            If (ds.Tables(0).Rows.Count > 0) Then
                PrepareMessage(ds, version)
            End If
        Catch ex As Exception

            Throw ex
        End Try
    End Function

    Function PushNotifyCondition(ByVal key As String,
                                Optional ByVal P_SELECT As String = "",
                                Optional ByVal P_KEY1 As String = "",
                                Optional ByVal P_KEY2 As String = "",
                                Optional ByVal P_KEY3 As String = "") As Boolean Implements IApplication.PushNotifyCondition
        Try


            Dim typeProcess As String = "AUTO"  ' ------------ Only Pending ----------------- '
            Dim sp As New SqlParameter()
            Dim parameters() As SqlParameter = New SqlParameter() _
                               {
                                 New SqlParameter("@P_SELECT", SqlDbType.VarChar, 50) With {.Value = P_SELECT},
                                 New SqlParameter("@P_KEY1", SqlDbType.VarChar, 50) With {.Value = P_KEY1},
                                 New SqlParameter("@P_KEY2", SqlDbType.VarChar, 50) With {.Value = P_KEY2},
                                 New SqlParameter("@P_KEY3", SqlDbType.VarChar, 50) With {.Value = P_KEY3}
                               }
            Dim ds As DataSet = MainClass.ExecuteDataset(Connect_DATABASE, "dbo.usp_tbPushNotiMessage_S", parameters)

            If (ds.Tables(0).Rows.Count > 0) Then
                Try
                    Dim totalRecord As Integer = ds.Tables(0).Rows.Count
                    ErrorNotifyThreading = "N"
                    ErrorNotifyThreading_Msg = ""
                    If totalRecord > 100 Then 'Split data when over 1000 record. 
                        Dim Thread_ As Integer = Math.Ceiling(totalRecord / 100)
                        Dim split_table As Integer = 0
                        Dim Diff_data As Integer = 0
                        ReDim NotifyThreading(Thread_ - 1)
                        split_table = Math.Floor(totalRecord / Thread_)
                        Diff_data = totalRecord - (split_table * Thread_)
                        For i As Integer = 0 To Thread_ - 1
                            Dim dvData As DataView = ds.Tables(0).DefaultView
                            dvData.RowFilter = "C_ROWNUM >= " & IIf(i = 0, (i * split_table), (i * split_table) + 1) & " and " &
                                               "C_ROWNUM <= " & IIf(i = Thread_ - 1, (((i + 1) * split_table) + Diff_data), ((i + 1) * split_table))
                            Dim objNoti As New NotifyThreading(dvData.ToTable, typeProcess, i)
                            AddHandler objNoti.Push_Complete, AddressOf Push_Complete
                            NotifyThreading(i) = New Threading.Thread(AddressOf objNoti.Notify_Data)
                            NotifyThreading(i).Start()

                        Next


                        For i As Integer = 0 To Thread_ - 1
                            NotifyThreading(i).Join()
                        Next

                        If ErrorNotifyThreading = "N" Then 'Thread Success
                            ' Complete 
                            Return True
                        Else
                            ' Thread.ResetAbort()
                            Try
                                ' Throw New Exception("F")
                                Return False
                            Catch gg As Exception
                                Return False
                            End Try
                            Return True
                        End If
                    Else 'data when under 1000 record.
                        If Push_Data(ds.Tables(0), typeProcess) = "S" Then
                            ' Complete 
                            Return True
                        End If
                    End If

                Catch ex As Exception
                    Return False
                End Try
            End If
            Return True


        Catch ex As Exception

            Throw ex
        End Try
    End Function


    Function PushNotifyConditionReRun(ByVal key As String,
                                      Optional ByVal P_SELECT As String = "",
                                      Optional ByVal P_KEY1 As String = "",
                                      Optional ByVal P_KEY2 As String = "",
                                      Optional ByVal P_KEY3 As String = "") As Boolean Implements IApplication.PushNotifyConditionReRun
        Try



            Dim typeProcess As String = "RE-RUN" ' ------------ Only Pending ----------------- '
            Dim sp As New SqlParameter()
            Dim parameters() As SqlParameter = New SqlParameter() _
                                   {
                                     New SqlParameter("@P_SELECT", SqlDbType.VarChar, 50) With {.Value = P_SELECT},
                                     New SqlParameter("@P_KEY1", SqlDbType.VarChar, 50) With {.Value = P_KEY1},
                                     New SqlParameter("@P_KEY2", SqlDbType.VarChar, 50) With {.Value = P_KEY2},
                                     New SqlParameter("@P_KEY3", SqlDbType.VarChar, 50) With {.Value = P_KEY3}
                                   }
            Dim ds As DataSet = MainClass.ExecuteDataset(Connect_DATABASE, "dbo.usp_tbPushNotiMessageReRun_S", parameters)

            If (ds.Tables(0).Rows.Count > 0) Then
                Try
                    Dim totalRecord As Integer = ds.Tables(0).Rows.Count
                    ErrorNotifyThreading = "N"
                    ErrorNotifyThreading_Msg = ""
                    If totalRecord > 100 Then 'Split data when over 1000 record. 
                        Dim Thread_ As Integer = Math.Ceiling(totalRecord / 100)
                        Dim split_table As Integer = 0
                        Dim Diff_data As Integer = 0
                        ReDim NotifyThreading(Thread_ - 1)
                        split_table = Math.Floor(totalRecord / Thread_)
                        Diff_data = totalRecord - (split_table * Thread_)
                        For i As Integer = 0 To Thread_ - 1
                            Dim dvData As DataView = ds.Tables(0).DefaultView
                            dvData.RowFilter = "C_ROWNUM >= " & IIf(i = 0, (i * split_table), (i * split_table) + 1) & " and " &
                                               "C_ROWNUM <= " & IIf(i = Thread_ - 1, (((i + 1) * split_table) + Diff_data), ((i + 1) * split_table))
                            Dim objNoti As New NotifyThreading(dvData.ToTable, typeProcess, i)
                            AddHandler objNoti.Push_Complete, AddressOf Push_Complete
                            NotifyThreading(i) = New Threading.Thread(AddressOf objNoti.Notify_Data)
                            NotifyThreading(i).Start()

                        Next

                        For i As Integer = 0 To Thread_ - 1
                            NotifyThreading(i).Join()
                        Next

                        If ErrorNotifyThreading = "N" Then 'Thread Success
                            ' Complete 
                            Return True
                        Else
                            'Thread.ResetAbort()
                            Try
                                ' Throw New Exception("F")
                                Return False
                            Catch gg As Exception
                                Return False
                            End Try
                            Return True
                        End If
                    Else 'data when under 1000 record.
                        If Push_Data(ds.Tables(0), typeProcess) = "S" Then
                            ' Complete 
                            Return True
                        End If
                    End If

                Catch ex As Exception
                    Return False
                End Try
            End If

            Return True


        Catch ex As Exception

            Throw ex
        End Try
    End Function

    Function UpdateStatusPush(ByVal cus_id As String, ByVal version As String, ByVal status As String, ByVal type_msg As String) As Boolean
        Try
            Dim ds As New DataSet()
            Dim sp As New SqlParameter()
            Dim parameters() As SqlParameter = New SqlParameter() _
                                   {
                                     New SqlParameter("@P_SELECT", SqlDbType.VarChar, 50) With {.Value = cus_id},
                                     New SqlParameter("@P_KEY1", SqlDbType.VarChar, 50) With {.Value = status},
                                     New SqlParameter("@P_KEY2", SqlDbType.VarChar, 50) With {.Value = version},
                                     New SqlParameter("@P_KEY3", SqlDbType.VarChar, 50) With {.Value = type_msg}
                                   }
            ds = MainClass.ExecuteDataset(Connect_DATABASE, "dbo.usp_tbPushNotiMessageStatus_U", parameters)
            Return True
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Public Async Function SendPush(ByVal jsonParam As String,
                      ByVal typePush As String,
                      ByVal server_key As String,
                      ByVal sender As String,
                      ByVal cust_id As String,
                      ByVal token As String,
                      ByVal ver As String,
                      ByVal PushBy As String,
                      ByVal typeProcess As String,
                      Optional typeDevice As String = "",
                      Optional seq_id As String = ""
                      ) As Task
        Dim strRes As String = ""
        Dim checkVal As String = "invalid"
        Dim msg_error As String = ""
        Dim servPush As String = ""
        Try

            Dim urlPath As String = "https://fcm.googleapis.com/fcm/send"
            Dim tRequest As WebRequest = WebRequest.Create(urlPath)
            tRequest.Method = "post"
            tRequest.ContentType = "application/json"
            Dim json = jsonParam
            Dim byteArray() As Byte = Encoding.UTF8.GetBytes(json)
            tRequest.Headers.Add(String.Format("Authorization: key={0}", server_key.Trim()))
            tRequest.ContentLength = byteArray.Length
            Dim dataStream As Stream = tRequest.GetRequestStream
            dataStream.Write(byteArray, 0, byteArray.Length)
            Dim tResponse As WebResponse = tRequest.GetResponse
            Dim dataStreamResponse As Stream = tResponse.GetResponseStream
            Dim tReader As StreamReader = New StreamReader(dataStreamResponse)
            Dim sResponseFromServer As String = tReader.ReadToEnd
            strRes = sResponseFromServer

            Dim result = JsonConvert.DeserializeObject(strRes.ToString)

            If (result.SelectToken("results")(0)("message_id") IsNot Nothing) Then
                checkVal = "SUCCESS"

            Else
                msg_error = result.SelectToken("results")(0)("error")
                checkVal = "FAIL"
            End If

        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Public Function Push_Data(ByVal dt As DataTable, ByVal typeProcess As String) As String
        Try
            Dim server_key As String = ""
            Dim sender As String = ""
            Dim server_key_hms As String = ""
            Dim sender_hms As String = ""
            Dim dsKey As DataSet = NotiflClass.GetNotificationKey()

            For Each dr As DataRow In dsKey.Tables(0).Rows
                If ("SERVER_KEY" = dr.Item("MASTER_KEY_01") And "FCM" = dr.Item("MASTER_KEY_02")) Then
                    server_key = dr.Item("MASTER_DESC").trim()
                End If
                If ("SERVER_KEY" = dr.Item("MASTER_KEY_01") And "HMS" = dr.Item("MASTER_KEY_02")) Then
                    server_key_hms = dr.Item("MASTER_DESC").trim()
                End If
                If ("SENDER_ID" = dr.Item("MASTER_KEY_01") And "FCM" = dr.Item("MASTER_KEY_02")) Then
                    sender = dr.Item("MASTER_DESC").trim()
                End If
                If ("SENDER_ID" = dr.Item("MASTER_KEY_01") And "HMS" = dr.Item("MASTER_KEY_02")) Then
                    sender_hms = dr.Item("MASTER_DESC").trim()
                End If
            Next

            If (dt.Rows.Count > 0) Then
                For i As Integer = 0 To dt.Rows.Count - 1
                    Try
                        Dim cus_id As String = dt.Rows(i).Item("CUST_ID")
                        Dim type_msg As String = dt.Rows(i).Item("TYPE_MESSAGE")
                        Dim jsonParam As String = dt.Rows(i).Item("MESSAGE")
                        Dim device_token As String = dt.Rows(i).Item("FCM_TOKEN")
                        Dim typeDevice As String = dt.Rows(i).Item("DEVICE")
                        Dim version As String = dt.Rows(i).Item("PUSHED_VER")
                        Dim seq_id As String = dt.Rows(i).Item("SEQ_ID")
                        If (typeDevice = "HUAWEI") Then
                            SendPushHMS(jsonParam, type_msg, server_key_hms, sender_hms, cus_id, device_token, version, "RUNJOB", typeProcess, typeDevice, seq_id)
                        ElseIf (typeDevice = "ANDROID" Or typeDevice = "IOS") Then
                            SendPush(jsonParam, type_msg, server_key, sender, cus_id, device_token, version, "RUNJOB", typeProcess, typeDevice, seq_id)
                        Else
                            If (typeDevice = "HUAWEI") Then
                                SendPushHMS(jsonParam, type_msg, server_key_hms, sender_hms, cus_id, device_token, version, "RUNJOB", typeProcess, typeDevice, seq_id)
                            ElseIf (typeDevice = "ANDROID" Or typeDevice = "IOS") Then
                                SendPush(jsonParam, type_msg, server_key, sender, cus_id, device_token, version, "RUNJOB", typeProcess, typeDevice, seq_id)
                            End If
                        End If
                    Catch ex As Exception

                    End Try
                Next

            End If
            Return "S"
        Catch ex As Exception
            Return "F"
            'Throw New Exception("F")
        End Try
    End Function

    Public Async Function SendPushHMS(ByVal jsonParam As String,
                      ByVal typePush As String,
                      ByVal server_key As String,
                      ByVal sender_id As String,
                      ByVal cust_id As String,
                      ByVal token As String,
                      ByVal ver As String,
                      ByVal PushBy As String,
                      ByVal typeProcess As String,
                      Optional typeDevice As String = "",
                      Optional seq_id As String = ""
                      ) As Task
        Dim strRes As String = ""
        Dim checkVal As String = "invalid"
        Dim msg_error As String = ""
        Dim hToken As String = String.Empty
        Try

            Try
                Using httpRequest = New HttpRequestMessage(HttpMethod.Post, "https://oauth-login.cloud.huawei.com/oauth2/v3/token")
                    Dim keyValues = New List(Of KeyValuePair(Of String, String))()
                    keyValues.Add(New KeyValuePair(Of String, String)("grant_type", "client_credentials"))
                    keyValues.Add(New KeyValuePair(Of String, String)("client_id", sender_id))
                    keyValues.Add(New KeyValuePair(Of String, String)("client_secret", server_key))
                    httpRequest.Content = New FormUrlEncodedContent(keyValues)

                    Using httpClient = New HttpClient()
                        Dim result = Await httpClient.SendAsync(httpRequest)

                        If result.IsSuccessStatusCode Then
                            Dim data = Await result.Content.ReadAsStringAsync()
                            Dim deserial = Function(str As String)
                                               Dim json As New JavaScriptSerializer
                                               json.MaxJsonLength = Int32.MaxValue
                                               Return json.Deserialize(Of HMSOAuthenClass)(str)
                                           End Function
                            Dim objHMS As HMSOAuthenClass = deserial(data)
                            hToken = objHMS.access_token
                        End If
                    End Using
                End Using

                If hToken <> String.Empty Then
                    Dim url = "https://push-api.cloud.huawei.com/v1/{0}/messages:send"
                    url = url.Replace("{0}", sender_id).Trim()

                    Using httpRequest = New HttpRequestMessage(HttpMethod.Post, url)
                        httpRequest.Headers.TryAddWithoutValidation("Authorization", hToken)
                        httpRequest.Content = New StringContent(jsonParam, Encoding.UTF8, "application/json")

                        Using httpClient = New HttpClient()
                            Dim result = Await httpClient.SendAsync(httpRequest)

                            If result.IsSuccessStatusCode Then
                                Dim data = Await result.Content.ReadAsStringAsync()
                                Dim deserial = Function(str As String)
                                                   Dim json As New JavaScriptSerializer
                                                   json.MaxJsonLength = Int32.MaxValue
                                                   Return json.Deserialize(Of HMSPushResponseClass)(str)
                                               End Function
                                Dim objHMS As HMSPushResponseClass = deserial(data)
                                If objHMS.code = "80000000" AndAlso objHMS.msg = "Success" Then
                                    checkVal = "SUCCESS"
                                Else
                                    checkVal = "FAIL"
                                End If
                            Else
                                checkVal = "FAIL"
                            End If
                        End Using
                    End Using
                End If
            Catch e As Exception
                checkVal = "FAIL"
            End Try


        Catch ex As Exception
            Throw ex
        End Try
    End Function


    Sub PrepareMessage(ByVal dataMsg As DataSet, ByVal version As String)
        Dim cus_id As String = ""
        Dim jsonParam = "{}"
        Dim device_token As String = ""
        Dim server_key As String = ""
        Dim legacy As String = ""
        Dim sender As String = ""
        Dim title_msg As String = ""
        Dim type_msg As String = ""
        Dim message As String = ""
        Dim RegNo As String = ""
        Dim InstallAmt As String = ""
        Dim expDate As String = ""
        Dim img_url As String = ""
        Dim media_url As String = ""
        Dim seq_id As String = ""
        Dim option_name As String = ""
        Dim flag_chkbox As String = ""
        Dim ic_color As String = ""
        Dim navi As String = ""
        Dim typeDevice As String = ""

        Try

            For i As Integer = 0 To dataMsg.Tables(0).Rows.Count - 1
                title_msg = ""
                type_msg = ""
                message = ""
                RegNo = ""
                InstallAmt = ""
                expDate = ""
                device_token = ""
                img_url = ""
                media_url = ""
                seq_id = ""
                option_name = ""
                flag_chkbox = ""
                ic_color = ""
                navi = ""

                cus_id = dataMsg.Tables(0).Rows(i).Item("CUST_ID")
                title_msg = dataMsg.Tables(0).Rows(i).Item("TITLE_MSG")
                type_msg = dataMsg.Tables(0).Rows(i).Item("NOTI_TYPE")
                message = dataMsg.Tables(0).Rows(i).Item("Msg")
                RegNo = dataMsg.Tables(0).Rows(i).Item("REG_NO")
                InstallAmt = dataMsg.Tables(0).Rows(i).Item("INSTALL_AMT")
                expDate = dataMsg.Tables(0).Rows(i).Item("NextDate")
                img_url = dataMsg.Tables(0).Rows(i).Item("IMG_URL")
                media_url = dataMsg.Tables(0).Rows(i).Item("MEDIA_URL")
                seq_id = dataMsg.Tables(0).Rows(i).Item("SEQ_ID")
                option_name = dataMsg.Tables(0).Rows(i).Item("OPTION_NAME")
                flag_chkbox = dataMsg.Tables(0).Rows(i).Item("FLAG_CHKBOX")
                ic_color = dataMsg.Tables(0).Rows(i).Item("IC_COLOR")
                navi = dataMsg.Tables(0).Rows(i).Item("NAVIGATION")
                device_token = dataMsg.Tables(0).Rows(i).Item("FCM_TOKEN").Trim()
                typeDevice = dataMsg.Tables(0).Rows(i).Item("DEVICE").Trim()
                jsonParam = FormatForPush(title_msg, type_msg, message,
                                           RegNo, InstallAmt, expDate, device_token,
                                           img_url, media_url, seq_id, option_name,
                                           flag_chkbox, ic_color, navi, typeDevice)
                Try
                    Dim sp As New SqlParameter()
                    Dim parameters() As SqlParameter = New SqlParameter() _
                                       {
                                         New SqlParameter("@P_CUST_ID", SqlDbType.VarChar, 50) With {.Value = cus_id},
                                         New SqlParameter("@P_FCM_TOKEN", SqlDbType.VarChar, -1) With {.Value = device_token},
                                         New SqlParameter("@P_NOTI_TYPE", SqlDbType.VarChar, 50) With {.Value = type_msg},
                                         New SqlParameter("@P_Msg", SqlDbType.VarChar, -1) With {.Value = jsonParam},
                                         New SqlParameter("@P_VERSION", SqlDbType.VarChar, 50) With {.Value = version},
                                         New SqlParameter("@P_DEVICE", SqlDbType.VarChar, 50) With {.Value = typeDevice},
                                           New SqlParameter("@P_SEQ_ID", SqlDbType.VarChar, 50) With {.Value = seq_id}
                                       }
                    Dim ds As DataSet = MainClass.ExecuteDataset(Connect_DATABASE, "dbo.usp_tbPushNotiMessage_I", parameters)
                Catch ex As Exception
                    Throw ex
                End Try
            Next
        Catch ex As Exception
            Throw ex
        End Try

    End Sub

    Function FormatForPush(ByVal title_msg As String, ByVal type_msg As String, ByVal message As String,
                             ByVal RegNo As String, ByVal InstallAmt As String,
                             ByVal expDate As String, ByVal device_token As String,
                             ByVal image_url As String, ByVal media_url As String,
                             ByVal seq_id As String, ByVal option_name As String,
                             ByVal flag_chkbox As String, ByVal ic_color As String,
                             ByVal navi As String, ByVal typeDevice As String) As String
        Dim data As Object
        Dim json As Object

        If (typeDevice = "ANDROID") Then
            data = New With {Key .[to] = device_token,
                                 Key .[priority] = "high",
                                 Key .[content_available] = True,
                                    Key .[mutable_content] = True,
                                 Key .data = New With {Key .title = title_msg,
                                                       Key .reg_no = RegNo,
                                                       Key .msg_type = type_msg,
                                                       Key .msg = message,
                                                       Key .exp_date = expDate,
                                                       Key .install_amt = InstallAmt,
                                                       Key .image = image_url,
                                                       Key .media = media_url,
                                                       Key .seq_id = seq_id,
                                                       Key .option_name = option_name,
                                                       Key .flag_chkbox = flag_chkbox,
                                                       Key .ic_color = ic_color,
                                                       Key .navigation = navi,
                                                       Key .dateTime = Now.ToString("dd/MM/yyyy HH:mm:ss")}
                               }
            Dim serializer = New JavaScriptSerializer
            json = serializer.Serialize(data)
        ElseIf (typeDevice = "IOS") Then
            data = New With {Key .[to] = device_token,
                                     Key .[priority] = "high",
                                     Key .[content_available] = True,
                                        Key .[mutable_content] = True,
                                     Key .notification = New With {Key .title = title_msg,
                                                                   Key .body = message,
                                                                   Key .sound = "default"
                                                                  },
                                          Key .data = New With {Key .reg_no = RegNo,
                                                           Key .msg_type = type_msg,
                                                           Key .msg = message,
                                                           Key .exp_date = expDate,
                                                           Key .install_amt = InstallAmt,
                                                           Key .image = image_url,
                                                           Key .media = media_url,
                                                           Key .seq_id = seq_id,
                                                           Key .option_name = option_name,
                                                           Key .flag_chkbox = flag_chkbox,
                                                           Key .ic_color = ic_color,
                                                           Key .navigation = navi,
                                                           Key .dateTime = Now.ToString("dd/MM/yyyy HH:mm:ss")}
              }
            Dim serializer = New JavaScriptSerializer
            json = serializer.Serialize(data)
        ElseIf (typeDevice = "HUAWEI") Then
            Dim str As String = "{'reg_no':'" & RegNo & "'" &
                      ",'title':'" & title_msg & "'" &
                      ",'msg_type':'" & type_msg & "'" &
                      ",'msg':'" & message & "'" &
                      ",'exp_date':'" & expDate & "'" &
                      ",'install_amt':'" & InstallAmt & "'" &
                      ",'image':'" & image_url & "'" &
                      ",'media':'" & media_url & "'" &
                      ",'seq_id':'" & seq_id & "'" &
                      ",'option_name':'" & option_name & "'" &
                      ",'flag_chkbox':'" & flag_chkbox & "'" &
                      ",'ic_color':'" & ic_color & "'" &
                      ",'navigation':'" & navi & "'" &
                      ",'dateTime':'" & Now.ToString("dd/MM/yyyy HH:mm:ss") & "'}"
            Dim token As New ArrayList
            token.Add(device_token)
            data = New With {Key .[validate_only] = False,
                                    Key .[message] = New With {Key .[data] = str, token}}

            Dim serializer = New JavaScriptSerializer
            json = serializer.Serialize(data)
            json = json.ToString.Replace("\u0027", "'")
        Else
            data = New With {Key .[to] = device_token,
                                     Key .[priority] = "high",
                                     Key .[content_available] = True,
                                     Key .[mutable_content] = True,
                                     Key .notification = New With {Key .title = title_msg,
                                                                   Key .body = message,
                                                                   Key .sound = "default"
                                                                  },
                                          Key .data = New With {Key .reg_no = RegNo,
                                                           Key .msg_type = type_msg,
                                                           Key .msg = message,
                                                           Key .exp_date = expDate,
                                                           Key .install_amt = InstallAmt,
                                                           Key .image = image_url,
                                                           Key .media = media_url,
                                                           Key .seq_id = seq_id,
                                                           Key .option_name = option_name,
                                                           Key .flag_chkbox = flag_chkbox,
                                                           Key .ic_color = ic_color,
                                                           Key .navigation = navi,
                                                           Key .dateTime = Now.ToString("dd/MM/yyyy HH:mm:ss")}
              }
            Dim serializer = New JavaScriptSerializer
            json = serializer.Serialize(data)

        End If

        Return json
    End Function



#End Region

    Private Sub Push_Complete(ByVal idx As Integer)
        If idx < NotifyThreading.Length - 1 Then
            NotifyThreading(idx + 1).Join()
        End If
    End Sub

End Class

Public Class NotifyThreading

    Public Sub New(ByVal source As DataTable, ByVal type_process_ As String, ByVal threadIndex As Integer)
        MyBase.New()
        dt = source
        type_process = type_process_
        intIndex = threadIndex
    End Sub

    Dim obj As New Application
    Dim intIndex As Int16
    Dim dt As DataTable
    Dim type_process As String
    Event Push_Complete(ByVal index As Integer)

    Public Sub Notify_Data()
        Try
            If obj.Push_Data(dt, type_process) = "S" Then

            Else

            End If
        Catch ex As Exception
            Application.ErrorNotifyThreading_Msg = ex.Message.ToString
            Application.ErrorNotifyThreading = "Y"
        End Try
    End Sub

End Class

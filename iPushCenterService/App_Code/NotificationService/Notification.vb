' NOTE: You can use the "Rename" command on the context menu to change the class name "NotificationService" in code, svc and config file together.

Imports System.Data
Imports System.Data.SqlClient
Imports System.Globalization
Imports System.IdentityModel.Tokens.Jwt
Imports System.IO
Imports System.Net
Imports System.Security.Cryptography
Imports System.Web.Configuration
Imports System.Web.Script.Serialization
Imports Newtonsoft.Json
Imports System.Net.Http
Imports System.Threading.Tasks

Public Class Notification
    Implements INotification
    Dim cultures As New CultureInfo("en-US")
    Dim dateFormat As DateTimeFormatInfo = cultures.DateTimeFormat
    Dim MainClass As New MainClass
    Dim Connect_DATABASE As String = WebConfigurationManager.ConnectionStrings("ConnectionString").ConnectionString

    Function GetNotificationKey() As DataSet
        Dim res As DataSet
        Dim sp As New SqlParameter()
        res = MainClass.ExecuteDatasetNoParam(Connect_DATABASE, "dbo.usp_Master_Notifykey")
        Return res
    End Function

    Public Async Function SendNotification(ByVal jsonParam As String, ByVal servPush As String, ByVal dsKey As DataSet, contract As String, custid As String, user As String) As Task(Of Boolean)
        Dim senderID As String = String.Empty
        Dim serverKey As String = String.Empty
        Dim hToken As String = String.Empty
        Dim sent As Boolean = False
        Dim checkVal As String = "invalid"
        Dim msg_error As String = ""
        Dim strRes As String = ""
        Try

            If servPush = "HMS" Then
                For Each dr As DataRow In dsKey.Tables(0).Rows
                    If "SENDER_ID".Equals(dr("MASTER_KEY_01")) AndAlso servPush.Equals(dr("MASTER_KEY_02")) Then senderID = Convert.ToString(dr("MASTER_DESC")).Trim()
                    If "SERVER_KEY".Equals(dr("MASTER_KEY_01")) AndAlso servPush.Equals(dr("MASTER_KEY_02")) Then serverKey = Convert.ToString(dr("MASTER_DESC")).Trim()
                Next


                Try
                    Using httpRequest = New HttpRequestMessage(HttpMethod.Post, "https://oauth-login.cloud.huawei.com/oauth2/v3/token")
                        Dim keyValues = New List(Of KeyValuePair(Of String, String))()
                        keyValues.Add(New KeyValuePair(Of String, String)("grant_type", "client_credentials"))
                        keyValues.Add(New KeyValuePair(Of String, String)("client_id", senderID))
                        keyValues.Add(New KeyValuePair(Of String, String)("client_secret", serverKey))
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
                        url = url.Replace("{0}", senderID).Trim()

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
                                        checkVal = "Y"
                                        sent = True

                                    Else
                                        checkVal = "N"
                                        sent = False
                                    End If
                                Else
                                    checkVal = "N"
                                    sent = False
                                End If
                            End Using
                        End Using
                    End If

                Catch ex As Exception
                    checkVal = "FAIL"

                End Try

            Else
                Try
                    For Each dr As DataRow In dsKey.Tables(0).Rows
                        If "SENDER_ID".Equals(dr("MASTER_KEY_01")) AndAlso servPush.Equals(dr("MASTER_KEY_02")) Then senderID = Convert.ToString(dr("MASTER_DESC")).Trim()
                        If "SERVER_KEY".Equals(dr("MASTER_KEY_01")) AndAlso servPush.Equals(dr("MASTER_KEY_02")) Then serverKey = Convert.ToString(dr("MASTER_DESC")).Trim()
                    Next

                    Dim urlPath As String = "https://fcm.googleapis.com/fcm/send"
                    Dim tRequest As WebRequest = WebRequest.Create(urlPath)
                    tRequest.Method = "post"
                    tRequest.ContentType = "application/json"
                    Dim json = jsonParam
                    Dim byteArray() As Byte = Encoding.UTF8.GetBytes(json)
                    tRequest.Headers.Add(String.Format("Authorization: key={0}", serverKey.Trim()))
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
                        checkVal = "Y"

                    Else
                        msg_error = result.SelectToken("results")(0)("error")
                        checkVal = "N"
                    End If

                Catch ex As Exception
                    Throw ex
                End Try
            End If
        Catch ex As Exception
            sent = False
            Throw ex

        End Try
        Return sent
    End Function


End Class

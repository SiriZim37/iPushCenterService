
Imports System.Data
Imports System.Data.SqlClient
Imports System.IO
Imports System.Web.Script.Serialization
Imports System.Web.Configuration
Imports System.Globalization
Imports System.Security.Cryptography
Imports Newtonsoft.Json
Imports Ionic.Zip

Public Class MainClass

    Dim appSettings = ConfigurationManager.AppSettings
    Dim serveText As String = appSettings("Serv")
    Dim cultures As New CultureInfo("en-US")
    Dim dateFormat As DateTimeFormatInfo = cultures.DateTimeFormat
    Dim Connect_DATABASE As String = WebConfigurationManager.ConnectionStrings("ConnectionString").ConnectionString


    Function EncodeJsString(ByVal s As String) As String
        'http://json.org/
        Dim sb As New StringBuilder()
        sb.Append("""")
        For Each c As Char In s
            Select Case c
                Case """"c
                    sb.Append("\""")
                    Exit Select
                Case "\"c
                    sb.Append("\\")
                    Exit Select
                Case "/"c
                    sb.Append("\/")
                    Exit Select
                Case ControlChars.Back
                    sb.Append("\b")
                    Exit Select
                Case ControlChars.FormFeed
                    sb.Append("\f")
                    Exit Select
                Case ControlChars.Lf
                    sb.Append("\n")
                    Exit Select
                Case ControlChars.Cr
                    sb.Append("\r")
                    Exit Select
                Case ControlChars.Tab
                    sb.Append("\t")
                    Exit Select
                Case Else
                    Dim i As Integer = AscW(c)
                    If i < 32 OrElse i > 127 Then
                        sb.AppendFormat("\u{0:X04}", i)
                    Else
                        sb.Append(c)
                    End If
                    Exit Select
            End Select
        Next
        sb.Append("""")
        Return sb.ToString()
    End Function

    Function GenKey_WCF() As String
        Dim UTCTime As DateTime = DateTime.UtcNow
        Dim Current_Date As String = UTCTime.AddHours(7).ToString("dd/MM/yyyy", dateFormat)
        Dim Find_Date As Integer = CInt(Current_Date.Substring(0, 2))
        Dim Find_Month As Integer = CInt(Current_Date.Substring(3, 2))
        Dim Find_Year As Integer = CInt(Current_Date.Substring(6, 4))
        Dim GetKey As String = ""
        Dim TempKey As String = ""
        Dim TempDate As Double = 0
        Dim TempMonth As Double = 0
        Dim TempYear As Double = 0
        If Find_Date Mod 2 = 0 Then
            TempKey = Format(DateAdd(DateInterval.Day, -15, Now), "dd/MM/yyyy")
            If Find_Month Mod 2 = 0 Then
                TempDate = CDbl(TempKey.Substring(0, 2))
                TempMonth = CDbl(TempKey.Substring(3, 2))
                TempYear = CDbl(TempKey.Substring(6, 4))
                TempKey = ((TempDate + TempMonth + TempYear) / 2.22).ToString.Replace(".", "")
            Else
                TempDate = CDbl(TempKey.Substring(0, 2))
                TempMonth = CDbl(TempKey.Substring(3, 2))
                TempYear = CDbl(TempKey.Substring(6, 4))
                TempKey = ((TempDate + TempMonth + TempYear) / 3.33).ToString.Replace(".", "")
            End If
        Else
            TempKey = Format(DateAdd(DateInterval.Day, -5, Now), "dd/MM/yyyy")
            If Find_Month Mod 2 = 0 Then
                TempDate = CDbl(TempKey.Substring(0, 2))
                TempMonth = CDbl(TempKey.Substring(3, 2))
                TempYear = CDbl(TempKey.Substring(6, 4))
                TempKey = ((TempDate + TempMonth + TempYear) / 2.22).ToString.Replace(".", "")
            Else
                TempDate = CDbl(TempKey.Substring(0, 2))
                TempMonth = CDbl(TempKey.Substring(3, 2))
                TempYear = CDbl(TempKey.Substring(6, 4))
                TempKey = ((TempDate + TempMonth + TempYear) / 3.33).ToString.Replace(".", "")
            End If
        End If
        GetKey = TempKey.Substring(0, 4) & TempKey.Substring(4)


        Return GetKey
    End Function

    Function GenKey_WCF_Back5Min() As String
        Dim UTCTime As DateTime = DateTime.UtcNow
        UTCTime = UTCTime.AddHours(7).ToString("dd/MM/yyyy", dateFormat)
        Dim Current_Date As String = Format(DateAdd(DateInterval.Minute, -5, UTCTime), "dd/MM/yyyy")
        Dim Find_Date As Integer = CInt(Current_Date.Substring(0, 2))
        Dim Find_Month As Integer = CInt(Current_Date.Substring(3, 2))
        Dim Find_Year As Integer = CInt(Current_Date.Substring(6, 4))
        Dim GetKey As String = ""
        Dim TempKey As String = ""
        Dim TempDate As Double = 0
        Dim TempMonth As Double = 0
        Dim TempYear As Double = 0
        If Find_Date Mod 2 = 0 Then
            TempKey = Format(DateAdd(DateInterval.Day, -15, Now), "dd/MM/yyyy")
            If Find_Month Mod 2 = 0 Then
                TempDate = CDbl(TempKey.Substring(0, 2))
                TempMonth = CDbl(TempKey.Substring(3, 2))
                TempYear = CDbl(TempKey.Substring(6, 4))
                TempKey = ((TempDate + TempMonth + TempYear) / 2.22).ToString.Replace(".", "")
            Else
                TempDate = CDbl(TempKey.Substring(0, 2))
                TempMonth = CDbl(TempKey.Substring(3, 2))
                TempYear = CDbl(TempKey.Substring(6, 4))
                TempKey = ((TempDate + TempMonth + TempYear) / 3.33).ToString.Replace(".", "")
            End If
        Else
            TempKey = Format(DateAdd(DateInterval.Day, -5, Now), "dd/MM/yyyy")
            If Find_Month Mod 2 = 0 Then
                TempDate = CDbl(TempKey.Substring(0, 2))
                TempMonth = CDbl(TempKey.Substring(3, 2))
                TempYear = CDbl(TempKey.Substring(6, 4))
                TempKey = ((TempDate + TempMonth + TempYear) / 2.22).ToString.Replace(".", "")
            Else
                TempDate = CDbl(TempKey.Substring(0, 2))
                TempMonth = CDbl(TempKey.Substring(3, 2))
                TempYear = CDbl(TempKey.Substring(6, 4))
                TempKey = ((TempDate + TempMonth + TempYear) / 3.33).ToString.Replace(".", "")
            End If
        End If
        GetKey = TempKey.Substring(0, 4) & TempKey.Substring(4)


        Return GetKey
    End Function

    Function GetJson(ByVal dt As DataTable) As String
        Dim serializer As New System.Web.Script.Serialization.JavaScriptSerializer()
        serializer.MaxJsonLength = Int32.MaxValue
        Dim rows As New List(Of Dictionary(Of String, Object))()
        Dim row As Dictionary(Of String, Object) = Nothing
        For Each dr As DataRow In dt.Rows
            row = New Dictionary(Of String, Object)()
            For Each dc As DataColumn In dt.Columns
                row.Add(dc.ColumnName.Trim(), dr(dc))
            Next
            rows.Add(row)
        Next
        Dim return_val As String = serializer.Serialize(rows)
        Return return_val
    End Function

    Function Dataset_toJson(ByVal ds As DataSet, ByVal index As Integer) As String
        Dim serializer As New System.Web.Script.Serialization.JavaScriptSerializer()
        serializer.MaxJsonLength = Int32.MaxValue
        Dim rows As New List(Of Dictionary(Of String, Object))()
        Dim row As Dictionary(Of String, Object) = Nothing
        Dim sub_arr As Object = ""
        For Each dr As DataRow In ds.Tables(index).Rows
            row = New Dictionary(Of String, Object)()
            For Each dc As DataColumn In ds.Tables(index).Columns
                sub_arr = dr(dc)
                row.Add(dc.ColumnName.Trim(), sub_arr.ToString)
            Next
            rows.Add(row)
        Next
        Dim return_val As String = serializer.Serialize(rows)
        Return return_val
    End Function

    Function DerializeDataTable(ByVal jsonstr As String) As DataTable
        Try
            Return JsonConvert.DeserializeObject(Of DataTable)(jsonstr)
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Function Clear_Junk(ByVal Txt_String As String) As String
        Dim tmpString As String = ""
        tmpString = Txt_String.Replace("\340", "")
        tmpString = tmpString.Replace("\271", "")
        tmpString = tmpString.Replace("\270", "")
        tmpString = tmpString.Replace("*", "")
        Return tmpString
    End Function


    Function KeyValidate_WCF(ByVal keyVal As String) As Boolean
        Try
            Dim bool As Boolean = False
            Dim dPass As String = GenKey_WCF()
            Dim hashStr As String = BCrypt.Net.BCrypt.HashPassword(dPass, keyVal)
            If hashStr = keyVal AndAlso (BCrypt.Net.BCrypt.Verify(dPass, keyVal)) Then
                bool = True
            End If
            If bool = False Then 'Validate Back 5 Min
                Dim dPassBack As String = GenKey_WCF_Back5Min()
                Dim hashStrBack As String = BCrypt.Net.BCrypt.HashPassword(dPassBack, keyVal)
                If hashStrBack = keyVal AndAlso (BCrypt.Net.BCrypt.Verify(dPassBack, keyVal)) Then
                    bool = True
                End If
            End If
            Return bool
        Catch ex As Exception
            Throw New Exception("Your key is invalid!")
        End Try
    End Function

    Function ClearNull(objValue As Object) As String
        Try
            If (IsDBNull(objValue)) Or (objValue Is Nothing) Then
                Return ""
            Else
                Return Convert.ToString(objValue).Trim
            End If
        Catch ex As Exception
            Throw ex
        End Try
    End Function



    Friend Class DESCrypt

        ' define the triple des provider
        Private m_des As New DESCryptoServiceProvider 'TripleDESCryptoServiceProvider

        ' define the string handler
        Private m_utf8 As New UTF8Encoding

        ' define the local property arrays
        Private m_key() As Byte
        Private m_iv() As Byte

        Public Sub New(ByVal key() As Byte, ByVal iv() As Byte)
            Me.m_key = key
            Me.m_iv = iv
        End Sub

        Public Function Encrypt(ByVal input() As Byte) As Byte()
            Return Transform(input, m_des.CreateEncryptor(m_key, m_iv))
        End Function

        Public Function Decrypt(ByVal input() As Byte) As Byte()
            Return Transform(input, m_des.CreateDecryptor(m_key, m_iv))
        End Function

        Public Function Encrypt(ByVal text As String) As String
            Dim input() As Byte = m_utf8.GetBytes(text)
            Dim output() As Byte = Transform(input,
                            m_des.CreateEncryptor(m_key, m_iv))
            Return Convert.ToBase64String(output)
        End Function

        Public Function Decrypt(ByVal text As String) As String
            Dim input() As Byte = Convert.FromBase64String(text)
            Dim output() As Byte = Transform(input,
                             m_des.CreateDecryptor(m_key, m_iv))
            Return m_utf8.GetString(output)
        End Function

        Private Function Transform(ByVal input() As Byte,
            ByVal CryptoTransform As ICryptoTransform) As Byte()
            ' create the necessary streams
            Dim memStream As MemoryStream = New MemoryStream
            Dim cryptStream As CryptoStream = New _
                CryptoStream(memStream, CryptoTransform,
                CryptoStreamMode.Write)
            ' transform the bytes as requested
            cryptStream.Write(input, 0, input.Length)
            cryptStream.FlushFinalBlock()
            ' Read the memory stream and convert it back into byte array
            memStream.Position = 0
            Dim result(CType(memStream.Length - 1, System.Int32)) As Byte
            memStream.Read(result, 0, CType(result.Length, System.Int32))
            ' close and release the streams
            memStream.Close()
            cryptStream.Close()
            ' hand back the encrypted buffer
            Return result
        End Function

    End Class

    Friend Class AESCrypt

        Public Sub New()
            MyBase.New()
        End Sub

        Public Function EncryptString(ByVal plainSourceStringToEncrypt As String, ByVal passPhrase As String) As String
            Using acsp As AesCryptoServiceProvider = GetProvider(Encoding.Default.GetBytes(passPhrase))
                Dim sourceBytes As Byte() = Encoding.ASCII.GetBytes(plainSourceStringToEncrypt)
                Dim ictE As ICryptoTransform = acsp.CreateEncryptor()

                'Set up stream to contain the encryption
                Dim msS As New MemoryStream()

                'Perform the encrpytion, storing output into the stream
                Dim csS As New CryptoStream(msS, ictE, CryptoStreamMode.Write)
                csS.Write(sourceBytes, 0, sourceBytes.Length)
                csS.FlushFinalBlock()

                'sourceBytes are now encrypted as an array of secure bytes
                Dim encryptedBytes As Byte() = msS.ToArray() '.ToArray() is important, don't mess with the buffer

                'return the encrypted bytes as a BASE64 encoded string
                Return Convert.ToBase64String(encryptedBytes)
            End Using
        End Function

        ''' <summary>
        ''' Decrypts a BASE64 encoded string of encrypted data, returns a plain string
        ''' </summary>
        ''' <param name="base64StringToDecrypt">an Aes encrypted AND base64 encoded string</param>
        ''' <param name="passphrase">The passphrase.</param>
        ''' <returns>returns a plain string</returns>
        ''' 
        Public Function DecryptString(ByVal base64StringToDecrypt As String, ByVal passphrase As String) As String
            Using acsp As AesCryptoServiceProvider = GetProvider(Encoding.Default.GetBytes(passphrase))
                Dim RawBytes As Byte() = Convert.FromBase64String(base64StringToDecrypt)
                Dim ictD As ICryptoTransform = acsp.CreateDecryptor()

                'RawBytes now contains original byte array, still in Encrypted state

                'Decrypt into stream
                Dim msD As MemoryStream = New MemoryStream(RawBytes, 0, RawBytes.Length)
                Dim csD As CryptoStream = New CryptoStream(msD, ictD, CryptoStreamMode.Read)
                'csD now contains original byte array, fully decrypted

                'return the content of msD as a regular string
                Return New StreamReader(csD).ReadToEnd()
            End Using
        End Function

        Private Function GetProvider(ByVal key As Byte()) As AesCryptoServiceProvider
            Dim result As New AesCryptoServiceProvider()
            result.BlockSize = 128
            result.KeySize = 128
            result.Mode = CipherMode.CBC
            result.Padding = PaddingMode.PKCS7

            result.GenerateIV()
            result.IV = New Byte() {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}

            Dim RealKey As Byte() = GetKey(key, result)
            result.Key = RealKey
            Return result
        End Function

        Private Function GetKey(ByVal suggestedKey As Byte(), ByVal p As SymmetricAlgorithm) As Byte()
            Dim kRaw As Byte() = suggestedKey
            Dim kList As New List(Of Byte)()

            For i As Integer = 0 To p.LegalKeySizes(0).MinSize - 1 Step 8
                kList.Add(kRaw((i \ 8) Mod kRaw.Length))
            Next
            Dim k As Byte() = kList.ToArray()
            Return k
        End Function
    End Class

    Function GenKey_EX() As String
        Dim UTCTime As DateTime = DateTime.UtcNow
        Dim Current_Date As String = UTCTime.AddHours(7).ToString("dd/MM/yyyy", dateFormat)
        Dim Find_Date As Integer = CInt(Current_Date.Substring(0, 2))
        Dim Find_Month As Integer = CInt(Current_Date.Substring(3, 2))
        Dim Find_Year As Integer = CInt(Current_Date.Substring(6, 4))
        Dim GetKey As String = ""
        Dim TempKey As String = ""
        Dim TempDate As Double = 0
        Dim TempMonth As Double = 0
        Dim TempYear As Double = 0
        If Find_Date Mod 2 = 0 Then
            TempKey = Format(DateAdd(DateInterval.Day, -15, Now), "dd/MM/yyyy")
            If Find_Month Mod 2 = 0 Then
                TempDate = CDbl(TempKey.Substring(0, 2))
                TempMonth = CDbl(TempKey.Substring(3, 2))
                TempYear = CDbl(TempKey.Substring(6, 4))
                TempKey = ((TempDate + TempMonth + TempYear) / 2.22).ToString.Replace(".", "")
            Else
                TempDate = CDbl(TempKey.Substring(0, 2))
                TempMonth = CDbl(TempKey.Substring(3, 2))
                TempYear = CDbl(TempKey.Substring(6, 4))
                TempKey = ((TempDate + TempMonth + TempYear) / 3.33).ToString.Replace(".", "")
            End If
        Else
            TempKey = Format(DateAdd(DateInterval.Day, -5, Now), "dd/MM/yyyy")
            If Find_Month Mod 2 = 0 Then
                TempDate = CDbl(TempKey.Substring(0, 2))
                TempMonth = CDbl(TempKey.Substring(3, 2))
                TempYear = CDbl(TempKey.Substring(6, 4))
                TempKey = ((TempDate + TempMonth + TempYear) / 2.22).ToString.Replace(".", "")
            Else
                TempDate = CDbl(TempKey.Substring(0, 2))
                TempMonth = CDbl(TempKey.Substring(3, 2))
                TempYear = CDbl(TempKey.Substring(6, 4))
                TempKey = ((TempDate + TempMonth + TempYear) / 3.33).ToString.Replace(".", "")
            End If
        End If
        GetKey = TempKey.Substring(0, 4) & TempKey.Substring(4)


        Return GetKey
    End Function

    Public Function ExecuteDataset_Noti(ByVal connectionstring As String, ByVal str_store As String, ByVal para() As SqlParameter) As DataSet
        Dim cnn As SqlConnection = New SqlConnection(connectionstring)
        Dim cmd As SqlCommand = New SqlCommand(str_store, cnn)
        cmd.CommandTimeout = 1000 * 1800
        Dim ds As DataSet = New DataSet
        Dim da As SqlDataAdapter = New SqlDataAdapter
        Try
            cnn.Open()
            cmd.CommandType = CommandType.StoredProcedure
            cmd.CommandText = str_store
            Dim i As Integer = 0
            Do While (i _
                        <= (para.Length - 1))
                cmd.Parameters.Add(para(i))
                i = (i + 1)
            Loop

            da.SelectCommand = cmd
            da.Fill(ds)
            Return ds
        Catch ex As Exception
            Throw ex
        Finally
            cmd.Parameters.Clear()
            cmd.Dispose()
            cnn.Close()
            cnn.Dispose()
        End Try

    End Function

    Public Function ExecuteDataset(ByVal connectionstring As String, ByVal str_store As String, ByVal para() As SqlParameter) As DataSet
        Dim cnn As SqlConnection = New SqlConnection(connectionstring)
        Dim cmd As SqlCommand = New SqlCommand(str_store, cnn)
        Dim ds As DataSet = New DataSet
        Dim da As SqlDataAdapter = New SqlDataAdapter
        Try
            cnn.Open()
            cmd.CommandType = CommandType.StoredProcedure
            cmd.CommandText = str_store
            Dim i As Integer = 0
            Do While (i _
                        <= (para.Length - 1))
                cmd.Parameters.Add(para(i))
                i = (i + 1)
            Loop

            da.SelectCommand = cmd
            da.Fill(ds)
            Return ds
        Catch ex As Exception
            Throw ex
        Finally
            cmd.Parameters.Clear()
            cmd.Dispose()
            cnn.Close()
            cnn.Dispose()
        End Try

    End Function

    Public Function ExecuteNonQuery(ByVal connectionstring As String, ByVal str_store As String, ByVal para() As SqlParameter) As Integer
        Dim cnn As SqlConnection = New SqlConnection(connectionstring)
        Dim cmd As SqlCommand = New SqlCommand(str_store, cnn)
        Try
            cmd.CommandType = CommandType.StoredProcedure
            Dim i As Integer = 0
            Do While (i _
                        <= (para.Length - 1))
                cmd.Parameters.Add(para(i))
                i = (i + 1)
            Loop
            cnn.Open()
            Dim retval As Integer = cmd.ExecuteNonQuery()
            Return retval
        Catch ex As Exception
            Throw ex
        Finally
            cmd.Parameters.Clear()
            cmd.Dispose()
            cnn.Close()
            cnn.Dispose()
        End Try

    End Function

    Public Function ExecuteDatasetNoParam(ByVal connectionstring As String, ByVal str_store As String) As DataSet
        Dim cnn As SqlConnection = New SqlConnection(connectionstring)
        Dim cmd As SqlCommand = New SqlCommand(str_store, cnn)
        Dim ds As DataSet = New DataSet
        Dim da As SqlDataAdapter = New SqlDataAdapter
        Try
            cnn.Open()
            cmd.CommandType = CommandType.StoredProcedure
            cmd.CommandText = str_store
            da.SelectCommand = cmd
            da.Fill(ds)
            Return ds
        Catch ex As Exception
            Throw ex
        Finally
            cmd.Parameters.Clear()
            cmd.Dispose()
            cnn.Close()
            cnn.Dispose()
        End Try

    End Function
#Region "for unzip file"
    Public Function ZipInstallmentPDFFile(ByVal ZipPath As String, ByVal request As String) As Byte()
        Dim branch As String = request.Substring(0, 1)
        Dim contractNo As String = request.Substring(3)
        'Dim fileName As String = GetPathNameByRef(request)
        'If String.IsNullOrEmpty(pahtName) Then Return ""
        ZipPath = ZipPath.Replace("\\\\", "\")
        Dim words As String() = ZipPath.Split("\"c)

        Dim folderDestination As String
        Dim fileName As String
        'Dim assDate As String = words(5)
        'Dim fileName As String = words(6)
        'Dim folderName As String = words(3)
        Dim salt As String = ""
        Dim id As String = ""
        Dim dData As Byte()
        Dim docDate As String = ""
        Dim resEx As String = ""
        Dim result As String = ""
        Dim ext_H As String = ""
        Dim salt_s As String = ""
        Dim pass_s As String = ""
        Dim PDFpath As String = ""
        Dim FMode As String = ""
        If words.Length() >= 7 Then
            folderDestination = String.Join("\\", words(0), words(1), words(2), words(3), words(4), words(5), words(6))
            fileName = words(7).Replace(".zip", "")
            FMode = words(4)
            Try
                Dim ds As DataSet = New DataSet()
                Dim FilePDF As String = fileName & ".pdf"
                Dim FilePDFTmp As String = fileName & ".zip.tmp"
                salt_s = GetDataSaltByRef(request, FMode)
                ext_H = HashZip(salt_s)
                pass_s = Validate_ZIP(ext_H, salt_s)
                Dim savefiledel = FilePDF

                Try

                    If (pass_s.Length > 0) Then
                        result = ExtractZip(folderDestination, ZipPath, pass_s)
                        PDFpath = folderDestination & "\" & savefiledel
                        dData = AnyFileToBase64(PDFpath)
                        Dim dir As FileInfo = New FileInfo(PDFpath)

                        If dir.Exists Then
                            'File.Delete(PDFpath)
                        End If

                        Dim PathTmp = folderDestination & FilePDFTmp
                        Dim dir2 As FileInfo = New FileInfo(PathTmp)

                        If dir2.Exists Then
                            File.Delete(PathTmp)
                        End If
                    End If

                Catch ex As Exception
                    'Dim valueMsg As String = $"branch:{branch}, contractNo:{contractNo}"

                    'KeepLogEX("MainClass", "ZipPDFFile_h", ex.ToString(), valueMsg, "")
                    Throw ex
                End Try

                Return dData
            Catch ex As Exception
                'Dim valueMsg As String = $"request:{request}"
                'KeepLogEX("MainClass", "ZipPDFFile_f", ex.ToString(), valueMsg, "")
                Throw ex
            End Try
        End If
        'Dim FilePDFzip As String = fileName & ".zip"
        'Dim folderDestination As String = SettingEContractFolderSavePDF(fileName, folderName, "", assDate)
        'Dim pahtFilezip As String = folderDestination & "\" & FilePDFzip


    End Function
    Public Function GetPathNameByRef(ByVal refId As String) As String
        Dim fileName As String = ""
        Dim ds As DataSet = New DataSet()

        Try
            Dim parameters = New SqlParameter() {New SqlParameter("@P_REF_ID", SqlDbType.VarChar, 50) With {
                .Value = refId
            }}
            ds = ExecuteDataset(Connect_DATABASE, "usp_GetFileNameInstallment", parameters)

            If ds.Tables(0).Rows.Count > 0 Then
                fileName = ds.Tables(0).Rows(0)("FILENAME").ToString()
            End If

            Return fileName
        Catch ex As Exception
            'Dim valueMsg As String = $"refId:{refId}"
            'KeepLogEX("MainClass", "GetFileNameByRef", ex.ToString(), valueMsg, "")
            Return fileName
        End Try
    End Function
    Public Function SettingEContractFolderSavePDF(ByVal doc_name As String, ByVal folderName As String, ByVal Optional doc_type As String = "", ByVal Optional assDate As String = "") As String
        Try
            If String.IsNullOrEmpty(assDate) Then assDate = DateTime.Now.ToString("yyyyMMdd")
            Dim Directory As String '= $"C:\\inetpub\\wwwroot\\{folderName}\\"
            Dim folderDestination As String = Directory & doc_type & "\" & assDate & "\" & doc_name

            If Not System.IO.Directory.Exists(folderDestination) Then
                System.IO.Directory.CreateDirectory(folderDestination)
            End If

            Return folderDestination
        Catch ex As Exception
            'Dim valueMsg As String = $"doc_name:{doc_name}, doc_type:{doc_type}"
            'KeepLogEX("MainClass", "SettingEContractFolderSavePDF", ex.ToString(), valueMsg, "")
            Throw ex
        End Try
    End Function
    Public Function GetDataSaltByRef(ByVal refId As String, ByVal Fmode As String, ByVal Optional pselect As String = "SALT") As String
        Dim salt As String = ""
        Dim ds As DataSet = New DataSet()

        Try
            Dim parameters = New SqlParameter() {New SqlParameter("@P_SELECT", SqlDbType.VarChar, 50) With {
                .Value = pselect
            }, New SqlParameter("@P_REF_ID", SqlDbType.VarChar, 50) With {
                .Value = refId
            }}
            ds = ExecuteDataset(Connect_DATABASE, "usp_GetSaltEContract", parameters)

            If ds.Tables(0).Rows.Count > 0 Then
                If Fmode.ToUpper() = "DRAFT" Then
                    salt = ds.Tables(0).Rows(0)("SALT_DRAFT").ToString()
                Else
                    salt = ds.Tables(0).Rows(0)("SALT_COMPLETE").ToString()
                End If
            End If
            Return salt
        Catch ex As Exception
            'Dim valueMsg As String = $"refId:{refId}, pselect:{pselect}"
            'KeepLogEX("MainClass", "GetDataSaltByRef", ex.ToString(), valueMsg, "")
            Return salt
        End Try
    End Function
    Public Function HashZip(ByVal ext_con As String) As String
        Dim bool As Boolean = False
        Dim res As String = ""

        Try
            Dim hashStr As String = BCrypt.Net.BCrypt.HashPassword(ext_con, 10)
            res = hashStr
        Catch ex As Exception
            'Dim valueMsg As String = $"ext_con:{ext_con}"
            'KeepLogEX("MainClass", "HashZip", ex.ToString(), valueMsg, "")
            Return ""
        End Try

        Return res
    End Function
    Public Function Validate_ZIP(ByVal PIN_C As String, ByVal PIN_S As String) As String
        Dim bool As Boolean = False
        Dim res As String = ""

        Try
            Dim hashStr As String = BCrypt.Net.BCrypt.HashPassword(PIN_S, PIN_C)
            If hashStr = PIN_C AndAlso (BCrypt.Net.BCrypt.Verify(PIN_S, PIN_C)) Then bool = True

            If (bool) Then
                res = PIN_S
            Else
                res = ""
            End If

        Catch ex As Exception
            'Dim valueMsg As String = $"PIN_C:{PIN_C}, PIN_S:{PIN_S}"
            'KeepLogEX("MainClass", "Validate_ZIP", ex.ToString(), valueMsg, "")
            Return ""
        End Try

        Return res
    End Function
    Public Function ExtractZip(ByVal folderDestination As String, ByVal pahtFilezip As String, ByVal password As String) As String
        Dim res = "success"

        Try

            Try

                Using zip = ZipFile.Read(pahtFilezip)

                    For Each e In zip.Entries
                        e.ExtractWithPassword(folderDestination, ExtractExistingFileAction.OverwriteSilently, password)
                    Next
                End Using

            Catch ex As Exception
                'Dim valueMsg As String = $"folderDestination:{folderDestination}, pahtFilezip:{pahtFilezip}, password:{password}"
                'KeepLogEX("MainClass", "ExtractZip_h", ex.ToString(), valueMsg, "")
                res = ex.ToString()
            End Try

        Catch ex As Exception
            'Dim valueMsg As String = $"folderDestination:{folderDestination}, pahtFilezip:{pahtFilezip}, password:{password}"
            'KeepLogEX("MainClass", "ExtractZip_f", ex.ToString(), valueMsg, "")
            Return ex.ToString()
        End Try

        Return res
    End Function
    Public Function AnyFileToBase64(ByVal File_Path As String) As Byte()
        Dim fInfo As FileInfo = New FileInfo(File_Path)
        Dim numBytes As Long = fInfo.Length
        Dim fStream As FileStream = New FileStream(File_Path, FileMode.Open, FileAccess.Read)
        Dim br As BinaryReader = New BinaryReader(fStream)
        Dim data As Byte() = br.ReadBytes(System.Convert.ToInt32(numBytes))
        'Dim base64String As String = Convert.ToBase64String(data)
        br.Close()
        fStream.Close()
        'Return base64String
        Return data
    End Function


#End Region
End Class

Public Class Message
    Public Sub New(ByVal source As Object)
        MyBase.New()
        objSource = source
    End Sub

    Public Sub New(ByVal status As String, ByVal msg As String)
        MyBase.New()
        Dim objResult As New Dictionary(Of String, String)
        objResult.Add("status", status)
        objResult.Add("alertmessage", msg)
        objSource = objResult
    End Sub

    Dim objSource As Object

    Public Function ToJsonString() As String
        Dim jsonString As New JavaScriptSerializer
        Return jsonString.Serialize(objSource).ToString
    End Function

    Public Function JsonToString(ByVal value As String) As Object
        Dim jsonString As New JavaScriptSerializer
        Return jsonString.DeserializeObject(value)
    End Function

End Class
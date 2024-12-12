Imports Microsoft.VisualBasic


Public Class HMSOAuthenClass
    Public Property access_token As String
    Public Property expires_in As Double
    Public Property token_type As String
End Class

Public Class HMSPushResponseClass
    Public Property code As String
    Public Property msg As String
    Public Property requestId As String
End Class


Public Class CenterNoti
    Dim P_KEY1 As String = ""
    Dim P_KEY2 As String = ""
    Dim P_KEY3 As String = ""
    Dim P_KEY4 As String = ""
    Dim P_KEY5 As String = ""
    Dim P_KEY6 As String = ""
    Dim P_SELECT As String = ""

    Public Property PKEY1() As String
        Get
            Return P_KEY1
        End Get
        Set(ByVal value As String)
            P_KEY1 = value
        End Set
    End Property

    Public Property PKEY2() As String
        Get
            Return P_KEY2
        End Get
        Set(ByVal value As String)
            P_KEY2 = value
        End Set
    End Property

    Public Property PKEY3() As String
        Get
            Return P_KEY3
        End Get
        Set(ByVal value As String)
            P_KEY3 = value
        End Set
    End Property
    Public Property PKEY4() As String
        Get
            Return P_KEY4
        End Get
        Set(ByVal value As String)
            P_KEY4 = value
        End Set
    End Property
    Public Property PKEY5() As String
        Get
            Return P_KEY5
        End Get
        Set(ByVal value As String)
            P_KEY5 = value
        End Set
    End Property

    Public Property PKEY6() As String
        Get
            Return P_KEY6
        End Get
        Set(ByVal value As String)
            P_KEY6 = value
        End Set
    End Property

    Public Property PSELECT() As String
        Get
            Return P_SELECT
        End Get
        Set(ByVal value As String)
            P_SELECT = value
        End Set
    End Property


End Class


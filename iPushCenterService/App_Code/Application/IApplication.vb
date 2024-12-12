Imports System.Data
Imports System.ServiceModel

' NOTE: You can use the "Rename" command on the context menu to change the interface name "IApplicationAdmin" in both code and config file together.
<ServiceContract()>
Public Interface IApplication


    <OperationContract>
    Function CenterPushNotification(ByVal key As String,
                                   ByVal str As String) As String

    <OperationContract()>
    Sub TestPush()

    <OperationContract()>
    Sub SetVersion(ByVal key As String)


    <OperationContract()>
    Function PrepareDataPushNotifyMessage(ByVal key As String,
                                          Optional ByVal P_SELECT As String = "",
                                          Optional ByVal P_KEY1 As String = "",
                                          Optional ByVal P_KEY2 As String = "") As String


    <OperationContract()>
    Function PushNotifyCondition(ByVal key As String,
                                      Optional ByVal P_SELECT As String = "",
                                      Optional ByVal P_KEY1 As String = "",
                                      Optional ByVal P_KEY2 As String = "",
                                      Optional ByVal P_KEY3 As String = "") As Boolean

    <OperationContract()>
    Function PushNotifyConditionReRun(ByVal key As String,
                                      Optional ByVal P_SELECT As String = "",
                                      Optional ByVal P_KEY1 As String = "",
                                      Optional ByVal P_KEY2 As String = "",
                                      Optional ByVal P_KEY3 As String = "") As Boolean
End Interface


Public Class ParseContext
    Private _request As Web.HttpRequest
    Private _response As Web.HttpResponse
    Private _variables As Dictionary(Of String, String)
    Private _debug As Boolean
    Private _fileName As String


    Public Sub New(req As Web.HttpRequest, resp As Web.HttpResponse)

        _request = req
        _response = resp

        _variables = New Dictionary(Of String, String)

        If Request.Form IsNot Nothing Then
            For Each item In Request.Form.Keys
                _variables.Add("@form/" & item, Request.Form(item))
            Next
        End If

        For Each item In Request.QueryString
            _variables.Add("@queryString/" & item, Request.QueryString(item))
        Next


        For Each item In Request.Files.Keys
            Dim file As System.Web.HttpPostedFile = Request.Files.Item(item)
            Dim ms As New IO.MemoryStream
            file.InputStream.CopyTo(ms)
            _variables.Add("@files/" & item, Convert.ToBase64String(ms.ToArray))
            ms.Close()

        Next

        'Add system variables 

        _variables.Add("@system/dateTime", DateTime.Now.ToString)

    End Sub

    Public Property Request As Web.HttpRequest
        Get
            Return _request
        End Get
        Set(value As Web.HttpRequest)
            _request = value
        End Set
    End Property

    Public Property Response As Web.HttpResponse
        Get
            Return _response
        End Get
        Set(value As Web.HttpResponse)
            _response = value
        End Set
    End Property

    Public ReadOnly Property Variables As Dictionary(Of String, String)
        Get
            Return _variables
        End Get
    End Property

    Public Property Debug As Boolean
        Get
            Return _debug
        End Get
        Set(value As Boolean)
            _debug = value
        End Set
    End Property

    Public Property fileName As String
        Get
            Return _fileName
        End Get
        Set(value As String)
            _fileName = value
        End Set
    End Property


End Class

Public Class ParseContext
    Private _request As Web.HttpRequest
    Private _response As Web.HttpResponse
    Private _variables As Dictionary(Of String, String)

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



End Class

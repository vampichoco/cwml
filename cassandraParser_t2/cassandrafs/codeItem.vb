Public Class codeItem
    Inherits cassandrafs.objectItem

    Private _code As String
    Private _codeType As String

    Public Property code As String
        Get
            Return _code
        End Get
        Set(value As String)
            _code = value
        End Set
    End Property

    Public Property codeType As String
        Get
            Return _codeType
        End Get
        Set(value As String)
            _codeType = value
        End Set
    End Property

End Class

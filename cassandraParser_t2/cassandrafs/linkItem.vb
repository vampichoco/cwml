Public Class linkItem
    Inherits objectItem

    Private _linkText As String

    Public Property LinkText As String
        Get
            Return _linkText
        End Get
        Set(value As String)
            _linkText = value
        End Set
    End Property

End Class

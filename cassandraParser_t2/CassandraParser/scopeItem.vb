Public Class scopeItem

    Private _scope As String
    Private _element As XElement

    Public Property Scope As String
        Get
            Return _scope
        End Get
        Set(value As String)
            _scope = value
        End Set
    End Property

    Public Property Element As XElement
        Get
            Return _element
        End Get
        Set(value As XElement)
            _element = value
        End Set
    End Property

End Class

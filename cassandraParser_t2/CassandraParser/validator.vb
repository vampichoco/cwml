Public Class validator
    Private _tagName As String
    Private _validatorHandler As Func(Of XElement, ParseContext, Boolean)
    Private _validationFailed As Func(Of XElement, ParseContext, XElement)

    Public Property tagName As String
        Get
            Return _tagName
        End Get
        Set(value As String)
            _tagName = value
        End Set
    End Property

    Public Property validatorHandler As Func(Of XElement, ParseContext, Boolean)
        Get
            Return _validatorHandler
        End Get
        Set(value As Func(Of XElement, ParseContext, Boolean))
            _validatorHandler = value
        End Set
    End Property

    Public Property validationFailed As Func(Of XElement, ParseContext, XElement)
        Get
            Return _validationFailed
        End Get
        Set(value As Func(Of XElement, ParseContext, XElement))
            _validationFailed = value
        End Set
    End Property

End Class

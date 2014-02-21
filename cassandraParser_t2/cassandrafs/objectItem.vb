Public Class objectItem
    Private _name As String
    Private _directory As String
    Private _hyperlink As String
    Private _dataStream As IO.Stream
    Private _type As ObjectType
    Private _indexKey As String
    Private _id As String


    Public Enum ObjectType
        File
        HyperLink
    End Enum

    Public Property id As String
        Get
            Return _id
        End Get
        Set(value As String)
            _id = value
        End Set
    End Property

    Public Property name As String
        Get
            Return _name
        End Get
        Set(value As String)
            _name = value
        End Set
    End Property

    Public Property directory As String
        Get
            Return _directory
        End Get
        Set(value As String)
            _directory = value
        End Set
    End Property

    Public Property Type As ObjectType
        Get
            Return _type
        End Get
        Set(value As ObjectType)
            _type = value
        End Set
    End Property

    Public Property hyperlink As String
        Get
            If Not Me.Type = ObjectType.HyperLink Then
                Throw New Exception("hyperlink property is only available when object type is 'Link'")
            Else
                Return _hyperlink
            End If

        End Get
        Set(value As String)
            If Not Me.Type = ObjectType.HyperLink Then
                Throw New Exception("hyperlink property is only available when object type is 'Link'")
            Else
                _hyperlink = value
            End If
        End Set
    End Property

    Public Property dataStream
        Get
            If Not Me.Type = ObjectType.File Then
                Throw New Exception("dataStream property is only available when object type is 'File'")
            Else
                Return _dataStream
            End If
        End Get
        Set(value)

            If Not Me.Type = ObjectType.File Then
                Throw New Exception("dataStream property is only available when object type is 'File'")
            Else
                _dataStream = value
            End If
        End Set
    End Property

    Public Property IndexKey
        Get
            Return _indexKey
        End Get
        Set(value)
            _indexKey = value
        End Set
    End Property


End Class

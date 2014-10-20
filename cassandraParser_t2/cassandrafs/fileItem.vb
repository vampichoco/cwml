Public Class fileItem
    Inherits objectItem

    Private _data As MongoDB.Bson.BsonBinaryData
    Private _contentType As String

    Public Property data As MongoDB.Bson.BsonBinaryData
        Get
            Return _data
        End Get
        Set(value As MongoDB.Bson.BsonBinaryData)
            _data = value
        End Set
    End Property

    Public Property ContentType As String
        Get
            Return _contentType
        End Get
        Set(value As String)
            _contentType = value
        End Set
    End Property

End Class

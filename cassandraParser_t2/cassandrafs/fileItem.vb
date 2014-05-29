Public Class fileItem
    Inherits objectItem

    Private _data As MongoDB.Bson.BsonBinaryData

    Public Property data As MongoDB.Bson.BsonBinaryData
        Get
            Return _data
        End Get
        Set(value As MongoDB.Bson.BsonBinaryData)
            _data = value
        End Set
    End Property

End Class

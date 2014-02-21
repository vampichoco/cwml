Imports MongoDB
Imports MongoDB.Bson
Imports MongoDB.Driver
Imports MongoDB.Driver.Linq
Public Class directory

    Private _directory As String
    Private _id As String
    Private _name As String
    Private _indexKey As String

    Public Property id As String
        Get
            Return _id
        End Get
        Set(value As String)
            _id = value
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

    Public Property name As String
        Get
            Return _name
        End Get
        Set(value As String)
            _name = value
        End Set
    End Property

    Public Property indexKey As String
        Get
            Return _indexKey
        End Get
        Set(value As String)
            _indexKey = value
        End Set
    End Property

    Public Function GetFiles(ByVal collection As MongoCollection(Of objectItem)) As IEnumerable(Of objectItem)
        Dim query = From f In collection.AsQueryable(Of objectItem)() Where f.IndexKey = Me.indexKey And f.id = Me.id

        Return query.AsEnumerable

    End Function


End Class

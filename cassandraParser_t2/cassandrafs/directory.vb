Imports MongoDB
Imports MongoDB.Bson
Imports MongoDB.Driver
Imports MongoDB.Driver.Linq
Public Class directory

    Inherits objectItem

    Public Function GetLinks(ByVal collection As MongoCollection(Of BsonDocument)) As IEnumerable(Of linkItem)
        Dim query = From f In collection.AsQueryable(Of linkItem)() Where f.indexKey = Me.indexKey And f.directory = Me.id And f.type = ObjectType.HyperLink

        Return query

    End Function

    Public Function GetFiles(ByVal collection As MongoCollection(Of BsonDocument)) As IEnumerable(Of fileItem)
        Dim query = From f In collection.AsQueryable(Of fileItem)() Where f.indexKey = Me.indexKey And f.directory = Me.id And f.type = ObjectType.File

        Return query
    End Function

    Public Function GetDirectories(ByVal collection As MongoCollection(Of BsonDocument)) As IEnumerable(Of directory)
        Dim query = From f In collection.AsQueryable(Of directory)() Where f.indexKey = Me.indexKey And f.directory = Me.id And f.type = ObjectType.Directory

        Return query
    End Function

    Public Overrides Function ToString() As String
        Return String.Format("File Name: {0}, ID={1}", Me.name, Me.id)
    End Function

    


End Class

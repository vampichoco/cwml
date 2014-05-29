﻿Imports MongoDB
Imports MongoDB.Bson
Imports MongoDB.Driver
Imports MongoDB.Driver.Linq

Public Class fileManager

    Private _connectionString As String
    Private _indexkey As String
    Private _collection As Driver.MongoCollection(Of BsonDocument)

    Public Sub New(ByVal connectionString As String, indexKey As String)
        _connectionString = connectionString
        _indexkey = indexKey
        _collection = GetCollection("filesystemstore", connectionString)
    End Sub

    Public ReadOnly Property Collection As Driver.MongoCollection(Of BsonDocument)
        Get
            Return _collection
        End Get
    End Property

    Public Function GetDirectory(ByVal directoryId As String) As directory
        Dim Collection = GetCollection("filesystemstore", _connectionString)

        Dim dir = Collection.AsQueryable(Of directory).SingleOrDefault(Function(d) d.indexKey = _indexkey And d.id = directoryId)



        Return dir

    End Function

    Public Function GetParentDirectory() As directory
        Dim Collection = GetCollection("filesystemstore", _connectionString)

        Dim dir = Collection.AsQueryable(Of directory).SingleOrDefault(Function(d) d.indexKey = _indexkey And d.id = "{null}}")



        Return dir
    End Function

    Public Function AddDirectory(ByVal directory As directory) As Boolean
        Dim collection = GetCollection("filesystemstore", _connectionString)

        Try
            collection.Insert(Of directory)(directory)
            Return True
        Catch ex As Exception
            Return False
        End Try


    End Function

    Public Function AddFile(ByVal file As objectItem) As Boolean
        Dim collection = GetCollection("filesystemstore", _connectionString)

        Try
            collection.Insert(Of objectItem)(file)

            Return True
        Catch ex As Exception
            Console.WriteLine(ex.Message)
            Return False
        End Try

    End Function

    Public Function GetCollection(ByVal collectionName As String, ByVal connectionString As String) As MongoCollection(Of BsonDocument)
        Dim client = New MongoClient(connectionString)

        Dim server = client.GetServer
        Dim dataBase = server.GetDatabase("sadzee")

        Dim collection = dataBase.GetCollection(collectionName)

        Return collection
    End Function

    Public Function GetStream(ByVal stream As System.IO.Stream) As MongoDB.Bson.BsonBinaryData
        Dim ms As New System.IO.MemoryStream
        stream.CopyTo(ms)

        Return New MongoDB.Bson.BsonBinaryData(ms.ToArray())
    End Function

    Public Function GrantPermission(ByVal [object] As objectItem, userToGrant As String, permission As permissionObject.PermissionType) As Boolean
        Try
            [object].permission.Add(New permissionObject With {.permission = permission, .user = userToGrant})
            _collection.Save([object])

            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function RevokePermission(ByVal [object] As objectItem, userToRevoke As String, permission As permissionObject.PermissionType) As Boolean
        Try
            Dim perm = [object].permission.SingleOrDefault(Function(p) p.permission = permission And p.user = userToRevoke)
            If perm IsNot Nothing Then
                [object].permission.Remove(perm)
                Return True
            Else
                Return False
            End If
        Catch ex As Exception
            Return False
        End Try
    End Function


End Class
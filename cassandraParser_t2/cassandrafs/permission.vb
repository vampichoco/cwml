Imports MongoDB
Imports MongoDB.Bson

Public Class permissionObject
    Public Enum PermissionType
        Read
        Write
        Edit
        Delete
        Full
    End Enum

    Private _user As ObjectId
    Private _permission As PermissionType

    Public Property user As ObjectId
        Get
            Return _user
        End Get
        Set(value As ObjectId)
            _user = value
        End Set
    End Property

    Public Property permission As PermissionType
        Get
            Return _permission
        End Get
        Set(value As PermissionType)
            _permission = value
        End Set
    End Property

    Public Shared Function [Default](ByVal userId As ObjectId) As permissionObject
        Return New permissionObject With {.permission = PermissionType.Full, .user = userId}
    End Function


End Class

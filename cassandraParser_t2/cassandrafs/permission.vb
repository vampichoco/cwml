Public Class permissionObject
    Public Enum PermissionType
        Read
        Write
        Edit
        Delete
        Full
    End Enum

    Private _user As String
    Private _permission As PermissionType

    Public Property user As String
        Get
            Return _user
        End Get
        Set(value As String)
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

    Public Shared Function [Default]() As permissionObject
        Return New permissionObject With {.permission = PermissionType.Full, .user = "$owner"}
    End Function


End Class

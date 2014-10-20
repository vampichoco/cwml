Public Class objectItem
    Private _name As String
    Private _directory As Guid
    Private _type As Integer
    Private _indexKey As Guid
    Private _id As Guid
    Private _permission As List(Of permissionObject)

    Public Enum PermissionType
        Read = 0
        Write = 1
        Edit = 2
        Delete = 3
        Full = 4
    End Enum

    Public Enum ObjectType
        File = 0
        HyperLink = 1
        Directory = 2
        Code = 3
    End Enum

    Public Property id As Guid
        Get
            Return _id
        End Get
        Set(value As Guid)
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

    Public Property directory As Guid
        Get
            Return _directory
        End Get
        Set(value As Guid)
            _directory = value
        End Set
    End Property

    Public Property type As Integer
        Get
            Return _type
        End Get
        Set(value As Integer)
            _type = value
        End Set
    End Property


    Public Property indexKey As Guid
        Get
            Return _indexKey
        End Get
        Set(value As Guid)
            _indexKey = value
        End Set
    End Property

    Public Property permission As List(Of permissionObject)
        Get
            Return _permission
        End Get
        Set(value As List(Of permissionObject))
            _permission = value
        End Set
    End Property

    Shared Iterator Function DefaultPermission(ByVal userId As Guid) As IEnumerable(Of permissionObject)
        Yield New permissionObject With {.permission = permissionObject.PermissionType.Full, .user = userId}
    End Function


    Public Overrides Function ToString() As String
        Return String.Format("File Name: {0}, ID={1}, Type={2}", Me.name, Me.id, Me.Type)
    End Function

    Public Sub New()
        permission = New List(Of permissionObject)
    End Sub


End Class

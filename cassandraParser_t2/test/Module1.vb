Module Module1

    Sub Main()
        Dim manager As New cassandrafs.fileManager("mongodb://localhost", "Eskimo")

        Dim ip = Console.ReadLine
        Select Case ip
            Case "createDir"
                Dim dir As New cassandrafs.directory With
            {.directory = "{null}",
             .id = Guid.NewGuid.ToString(),
             .IndexKey = "Eskimo",
             .name = "Home directory",
             .Type = cassandrafs.objectItem.ObjectType.Directory}

                manager.AddDirectory(dir)

                Console.WriteLine("Created directory")
                Console.WriteLine(dir.ToString)
                Console.ReadLine()
            Case "getDir"

                Dim dirId As String = "99866ba4-48c0-4422-add7-cf80c00f8352"

                Dim dir = manager.GetDirectory(dirId)


                Console.WriteLine(dir.ToString)

                For Each item In dir.GetLinks(manager.Collection)

                    'MongoDB.Bson.Serialization.BsonClassMap.LookupClassMap(GetType(cassandrafs.linkItem))
                    Dim _item = DirectCast(item, cassandrafs.linkItem)
                    manager.GrantPermission(item, "vichu", cassandrafs.objectItem.PermissionType.Read)
                    Console.WriteLine(_item.ToString)
                Next

                For Each item In dir.GetFiles(manager.Collection)
                    Console.WriteLine(item.ToString)
                Next

                Console.ReadLine()

            Case "addLink"
                Dim dirId As String = "99866ba4-48c0-4422-add7-cf80c00f8352"

                Dim dir = manager.GetDirectory(dirId)

                Dim file As New cassandrafs.linkItem With
                    {.LinkText = "http://eskipublic.s3.amazonaws.com/uff.jpg",
                     .directory = dirId,
                     .id = Guid.NewGuid.ToString,
                     .indexKey = "Eskimo",
                     .name = "uff.jpg",
                     .type = cassandrafs.objectItem.ObjectType.HyperLink,
                     .permission = cassandrafs.objectItem.DefaultPermission.ToList
                    }

                manager.AddFile(file)

                Console.WriteLine(file.ToString)
                Console.ReadLine()

            Case "addFile"

                Dim dirId As String = "99866ba4-48c0-4422-add7-cf80c00f8352"

                Dim dir = manager.GetDirectory(dirId)

                Dim fstream As IO.FileStream = IO.File.Open("C:\karem\chisaki.jpg", IO.FileMode.Open)


                Dim file As New cassandrafs.fileItem With
                    {.directory = dirId,
                     .id = Guid.NewGuid.ToString,
                     .indexKey = "Eskimo",
                     .name = "Chisaki",
                     .type = cassandrafs.objectItem.ObjectType.File,
                     .data = manager.GetStream(fstream),
                     .permission = cassandrafs.objectItem.DefaultPermission.ToList
                    }

                fstream.Close()

                manager.AddFile(file)


                Console.WriteLine(file.ToString)
                Console.ReadLine()

            Case "grant"


        End Select

    End Sub

End Module

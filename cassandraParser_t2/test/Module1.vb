Module Module1

    Sub Main()
        Dim manager As New cassandrafs.fileManager("mongodb://localhost", Guid.Empty)

        Dim ip = Console.ReadLine
        Select Case ip
            Case "createDir"
                Dim dir As New cassandrafs.directory With
            {.directory = Guid.Empty,
             .id = Guid.NewGuid(),
             .indexKey = Guid.Empty,
             .name = "Home directory",
             .type = cassandrafs.objectItem.ObjectType.Directory}

                manager.AddDirectory(dir)

                Console.WriteLine("Created directory")
                Console.WriteLine(dir.ToString)
                Console.ReadLine()
            Case "getDir"

                Dim dirId As Guid = Guid.Parse("99866ba4-48c0-4422-add7-cf80c00f8352")

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
                Dim dirId As Guid = Guid.Parse("99866ba4-48c0-4422-add7-cf80c00f8352")

                Dim dir = manager.GetDirectory(dirId)

                Dim file As New cassandrafs.linkItem With
                    {.LinkText = "http://eskipublic.s3.amazonaws.com/uff.jpg",
                     .directory = dirId,
                     .id = Guid.NewGuid,
                     .indexKey = Guid.Empty,
                     .name = "uff.jpg",
                     .type = cassandrafs.objectItem.ObjectType.HyperLink,
                     .permission = cassandrafs.objectItem.DefaultPermission(.indexKey).ToList
                    }

                manager.AddFile(file)

                Console.WriteLine(file.ToString)
                Console.ReadLine()

            Case "addFile"

                Dim dirId As Guid = Guid.Parse("99866ba4-48c0-4422-add7-cf80c00f8352")

                Dim dir = manager.GetDirectory(dirId)

                Dim fstream As IO.FileStream = IO.File.Open("C:\karem\chisaki.jpg", IO.FileMode.Open)


                Dim file As New cassandrafs.fileItem With
                    {.directory = dirId,
                     .id = Guid.NewGuid,
                     .indexKey = Guid.Empty,
                     .name = "Chisaki",
                     .type = cassandrafs.objectItem.ObjectType.File,
                     .data = manager.GetStream(fstream),
                     .permission = cassandrafs.objectItem.DefaultPermission(.indexKey).ToList
                    }

                fstream.Close()

                manager.AddFile(file)


                Console.WriteLine(file.ToString)
                Console.ReadLine()

            Case "grant"


        End Select

    End Sub

End Module

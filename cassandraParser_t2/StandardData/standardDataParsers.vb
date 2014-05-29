Imports System.Text.RegularExpressions
Imports MongoDB.Bson
Imports MongoDB.Driver
Imports MongoDB.Driver.Builders
Public Class StandardDataParser
    Implements CWML.iBlockParserList

    Private _cassandraParser As CWML.CassandraParser

    Public ReadOnly Property CassandraParser As CWML.CassandraParser Implements CWML.iBlockParserList.CassandraParser
        Get
            Return _cassandraParser
        End Get
    End Property

    Public Sub New(ByVal cassandraParser As CWML.CassandraParser)
        _cassandraParser = cassandraParser

        cassandraParser.BlockParsers.Add("query", AddressOf ParseQuery)
        cassandraParser.BlockParsers.Add("dataInsert", AddressOf ParseDataInsert)

    End Sub

    Public Function ParseQueryCondition(ByVal input As XElement, context As CWML.ParseContext) As IMongoQuery
        Select Case input.Name.ToString
            Case "and"
                Dim qNodes = ParseNodes(input, context)

                Return Query.And(qNodes(0), qNodes(1))

            Case "or"

                Dim qNodes = ParseNodes(input, context)

                Return Query.Or(qNodes(0), qNodes(1))

            Case "equals"
                Dim name As String = input.@name
                Dim value As String = input.@value

                If Regex.IsMatch(name, CWML.CassandraParser.variablePattern) Then : name = context.Variables(name)
                End If

                If Regex.IsMatch(value, CWML.CassandraParser.variablePattern) Then : value = context.Variables(value)
                End If

                Return Query.EQ(name, value)


            Case Else
                If input.Name = "all" = False Then
                    Throw New Exception("No valid operation")
                End If
                Return Nothing
        End Select
    End Function

    Public Function ParseNodes(ByVal node As XElement, context As CWML.ParseContext) As IMongoQuery()
        Dim nodes = node.Nodes.ToArray

        If nodes.Length < 2 Then
            Throw New Exception("Or Operator requres two values to be compared")
        End If

        Dim nodeA As IMongoQuery = ParseQueryCondition(nodes(0), context)
        Dim nodeB As IMongoQuery = ParseQueryCondition(nodes(1), context)

        Return {nodeA, nodeB}

    End Function

    Public Function ParseDataType(ByVal strType As String) As BsonType
        Select Case strType
            Case "string"
                Return BsonType.String
            Case "int"
                Return BsonType.Int32
            Case "bigint"
                Return BsonType.Int64
            Case "dateTime"
                Return BsonType.DateTime
            Case "binary"
                Return BsonType.Binary
            Case Else
                Return BsonType.String
        End Select
    End Function

    Public Function parseDataElement(ByVal data As XElement, ByVal context As CWML.ParseContext) As BsonElement
        Dim type = ParseDataType(data.@as)
        Dim name = data.@name
        Dim value = data.@value



        If Regex.IsMatch(value, CWML.CassandraParser.variablePattern) Then

            If context.Variables.ContainsKey(value) = False Then
                Throw New Exception(String.Format("Variable '{0}' not found in variable dictionary", value))
            End If

            value = context.Variables(value)

        End If


        Select Case type
            Case BsonType.String
                Return New BsonElement(name, New BsonString(value))
            Case BsonType.Int32
                Return New BsonElement(name, New BsonInt32(Integer.Parse(value)))
            Case BsonType.Int64
                Return New BsonElement(name, New BsonInt64(Int64.Parse(value)))
            Case BsonType.DateTime
                Return New BsonElement(name, New BsonDateTime(Date.Parse(value)))
            Case BsonType.Binary
                Dim val As New BsonBinaryData(Convert.FromBase64String(value))
                Return New BsonElement(name, val)
            Case Else
                Return New BsonElement(name, New BsonString(value))
        End Select
    End Function

    Public Function ParseQuery(ByVal data As XElement, context As CWML.ParseContext) As XElement

        Dim connectionString = context.Variables("$system/connectionString")
        Dim dbName As String = context.Variables("$system/dbName")


        Dim sortedquery As Boolean = False

       


        Dim collectionName As String = data.@collection

        Dim client = New MongoClient(connectionString)

        Dim server = client.GetServer
        Dim dataBase = server.GetDatabase(dbName)

        Dim collection = dataBase.GetCollection(collectionName)

        Dim condition = data.<condition>.Elements.First

        Dim sort = Nothing
        If data.Elements.SingleOrDefault(Function(e) e.Name = "sort") IsNot Nothing Then
            sortedquery = True
            Dim sortData = data.Elements.SingleOrDefault(Function(e) e.Name = "sort")
            Dim attrName = sortData.@name
            Dim sortType = sortData.@type

            Select Case sortType
                Case "ascending"
                    sort = SortBy.Ascending(attrName.Split(","))
                Case "descending"
                    sort = SortBy.Descending(attrName.Split(","))
                Case Else
                    sort = SortBy.Null
            End Select

        End If

        Dim queryExpression = ParseQueryCondition(condition, context)

        Dim _query As MongoCursor(Of BsonDocument)

        If condition.Name = "all" Then
            If sortedquery Then
                _query = collection.FindAll().SetSortOrder(sort)
            Else
                _query = collection.FindAll()
            End If

        Else
            If sortedquery Then
                _query = collection.Find(queryExpression).SetSortOrder(sort)
            Else
                _query = collection.Find(queryExpression)
            End If


        End If

        Dim queryop = <div></div>

        Dim opPattern = data.Element("output")

        For Each item In _query
            Dim op As New XElement(opPattern)
            Dim ctx As New CWML.ParseContext(context.Request, context.Response)

            For Each attr In item.Names
                Dim element = item(attr)
                Select Case element.BsonType
                    Case BsonType.Binary
                        ctx.Variables.Add("$" & attr, Convert.ToBase64String(element.AsBsonBinaryData.Bytes))
                    Case Else
                        ctx.Variables.Add("$" & attr, item(attr).ToString)
                End Select

            Next


            queryop.Add(CassandraParser.Parse(op, ctx))

        Next

        Return queryop



    End Function

    Public Function ParseDataInsert(ByVal data As XElement, context As CWML.ParseContext) As XElement
        Dim connectionString = context.Variables("$system/connectionString")
        Dim dbName As String = context.Variables("$system/dbName")

        Dim checkPresenceOf As Boolean = False

        If data.@presenceOf IsNot Nothing Then

            For Each item In data.@presenceOf.Split(" ")
                If context.Variables.ContainsKey(item) Then
                    checkPresenceOf = True
                Else
                    checkPresenceOf = False
                    Exit For
                End If
            Next
        End If


        If checkPresenceOf Then

            Try
                Dim collectionName As String = data.@collection

                Dim client = New MongoClient(connectionString)

                Dim server = client.GetServer
                Dim dataBase = server.GetDatabase(dbName)

                Dim collection = dataBase.GetCollection(collectionName)
                Dim document As New BsonDocument

                For Each item In data.<data>...<element>

                    Dim value = parseDataElement(item, context)

                    document.Add(value)
                Next

                collection.Insert(document)

                Return CassandraParser.Parse(data.Element("ready"), context)

            Catch ex As Exception
                context.Variables.Add("$system/error", ex.Message)
                context.Variables.Add("$system/error/stackTrace", ex.StackTrace)

                Return CassandraParser.Parse(data.Element("error"), context)
            End Try
        Else
            context.Variables.Add("$system/error", "No error")
            Return <div/>
        End If


    End Function

End Class

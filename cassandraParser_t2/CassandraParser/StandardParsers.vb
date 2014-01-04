Imports CWML
Imports MongoDB.Bson
Imports MongoDB.Driver
Imports MongoDB.Driver.Builders

''' <summary>
''' Provides basic parsing functions from CWML to HTML
''' </summary>
''' <remarks></remarks>
Public Class StandardParsers
    Implements iBlockParserList


    Private _cassandra As CassandraParser

    ''' <summary>
    ''' Gets a class used to parse CWML blocks
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property CassandraParser As CassandraParser Implements iBlockParserList.CassandraParser
        Get
            Return _cassandra
        End Get
    End Property


    ''' <summary>
    ''' Iniatilize this Standard Parsers Class
    ''' </summary>
    ''' <param name="CassandraParser">The cassandra parser that will make the parsing function</param>
    ''' <remarks></remarks>
    Public Sub New(ByVal CassandraParser As CassandraParser)
        _cassandra = CassandraParser

        'Add all the parsers to the list here 

        With CassandraParser.BlockParsers
            .Add("page", AddressOf ParsePage)
            .Add("head", AddressOf ParseHead)
            .Add("content", AddressOf ParseBody)
            .Add("link", AddressOf ParseLink)
            .Add("image", AddressOf ParseImage)
            .Add("h1", AddressOf ParseH1)
            .Add("br", AddressOf ParseBr)
            .Add("p", AddressOf ParseP)
            .Add("div", AddressOf ParseDiv)
            .Add("text", AddressOf ParseText)
            .Add("css", AddressOf ParseCss)
            .Add("form", AddressOf ParseForm)
            .Add("textBox", AddressOf ParseTextBox)
            .Add("button", AddressOf ParseButton)
            .Add("query", AddressOf ParseQuery)
            .Add("dataInsert", AddressOf ParseDataInsert)
            .Add("ready", AddressOf ParseReady)
            .Add("error", AddressOf ParseError)
        End With

    End Sub


#Region "Parsers Region"

    ''' <summary>
    ''' Parse a page block element 
    ''' </summary>
    ''' <param name="data">Xml Element containing data to be parsed</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function ParsePage(ByVal data As XElement, context As ParseContext) As XElement
        Dim content = <html>
                          <%= From item In data.Elements Select CassandraParser.Parse(item, context) %>
                      </html>

        Return content
    End Function


    ''' <summary>
    ''' Parse a head block element 
    ''' </summary>
    ''' <param name="data">Xml Element containing data to be parsed</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function ParseHead(ByVal data As XElement, context As ParseContext) As XElement
        Dim Content = <head>
                          <%= From item In data.Elements Select CassandraParser.Parse(item, context) %>
                      </head>

        Return Content
    End Function

    ''' <summary>
    ''' Parse a body block element
    ''' </summary>
    ''' <param name="data">Xml Element containing data to be parsed</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function ParseBody(ByVal data As XElement, context As ParseContext) As XElement
        Dim content = <body>
                          <%= From item In data.Elements Select CassandraParser.Parse(item, context) %>
                      </body>

        Return content
    End Function

    ''' <summary>
    ''' Parse an image block element
    ''' </summary>
    ''' <param name="data">Xml Element containing data to be parsed</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function ParseImage(ByVal data As XElement, context As ParseContext) As XElement
        Dim content = <img src=<%= data.Value %>/>
        Return content
    End Function

    ''' <summary>
    ''' Parse a link block element
    ''' </summary>
    ''' <param name="data">Xml Element containing data to be parsed</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function ParseLink(ByVal data As XElement, context As ParseContext) As XElement
        Dim content = <a href=<%= data.Attribute("goesto").Value %>>
                      </a>

        If Not data.HasElements Then
            content.Value = data.Value
        Else
            'Dim parsed = CassandraParser.Parse(data.Elements.First, req)
            content.Add(CassandraParser.Parse(data.Elements.First, context))
        End If

        Return content
    End Function

    ''' <summary>
    ''' Parse a h1 block element
    ''' </summary>
    ''' <param name="data">Xml Element containing data to be parsed</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function ParseH1(ByVal data As XElement, context As ParseContext) As XElement
        Dim content = <h1><%= data.Value %></h1>
        Return content
    End Function

    ''' <summary>
    ''' Parse a br block element
    ''' </summary>
    ''' <param name="data">Xml Element containing data to be parsed</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function ParseBr(ByVal data As XElement, context As ParseContext) As XElement
        Return <br/>
    End Function

    ''' <summary>
    ''' Parse a p block element
    ''' </summary>
    ''' <param name="data">Xml Element containing data to be parsed</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function ParseP(ByVal data As XElement, context As ParseContext) As XElement
        Dim content = <p <%= From item In data.Attributes Select item %>>

                      </p>

        If Not data.HasElements Then
            content.Value = data.Value
        Else
            For Each item In data.Elements
                content.Add(CassandraParser.Parse(item, context))
            Next
        End If


        Return (CassandraParser.SetDefaultCss(data, content))

    End Function

    ''' <summary>
    ''' Takes the inner data and then creates a properly html formed output to keep lines structure.
    ''' </summary>
    ''' <param name="data">Xml Element containing data to be parsed</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function ParseText(ByVal data As XElement, context As ParseContext) As XElement
        Dim lines = data.Value.Split(vbLf)
        Dim result = <p <%= From item In data.Attributes Select item %>>
                         <div>
                             <%= From item In lines Select CassandraParser.SetDefaultCss(data, (New XElement("div", item))) %>
                         </div>
                     </p>

        Return result
    End Function

    Public Function ParseDiv(ByVal data As XElement, context As ParseContext) As XElement
        Dim result = <div <%= From item In data.Attributes Select item %>>

                     </div>

        If Not data.HasElements Then
            result.Value = data.Value
        Else
            For Each item In data.Elements
                result.Add(CassandraParser.Parse(item, context))
            Next
        End If


        Return (CassandraParser.SetDefaultCss(data, result))
    End Function

    Public Function ParseCss(ByVal data As XElement, context As ParseContext) As XElement
        Dim content = <link rel="stylesheet" type="text/css" href=<%= data.Value %>/>
        Return content
    End Function

    Public Function ParseForm(ByVal data As XElement, context As ParseContext) As XElement
        Dim content = <form action=<%= data.Attribute("action").Value %> method=<%= data.Attribute("method").Value %>>
                          <%= From item In data.Elements Select _cassandra.Parse(item, context) %>
                      </form>

        Return content
    End Function

    Public Function ParseTextBox(ByVal data As XElement, context As ParseContext) As XElement
        Dim content = <input type="Text" id=<%= data.Attribute("id").Value %> name=<%= data.Attribute("id").Value %>/>

        Return content

    End Function

    Public Function ParseButton(ByVal data As XElement, context As ParseContext) As XElement
        Dim content = <input type="Submit" id=<%= data.Attribute("id").Value %> value=<%= data.Value %> name=<%= data.Attribute("id").Value %>/>

        Return content

    End Function

    Public Function ParseSubmit(ByVal data As XElement, context As ParseContext) As XElement
        Dim input = <input type=<%= data.Attribute("type").Value %> <%= From attr In data.Attributes Select attr %>>

                    </input>
    End Function

    Public Function ParseQuery(ByVal data As XElement, context As ParseContext) As XElement

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
        Else
            checkPresenceOf = True
        End If


        If checkPresenceOf = True Then
            Dim collectionName As String = data.@collection

            Dim connectionString As String = "mongodb://localhost"
            Dim client = New MongoClient(connectionString)

            Dim server = client.GetServer
            Dim dataBase = server.GetDatabase("test")

            Dim collection = dataBase.GetCollection(collectionName)

            Dim filter = data.<filter>
            Dim type As String = filter.@type

            Select Case type
                Case "equals"
                    Dim name As String = filter.<name>.Value
                    Dim value As String = filter.<value>.Value

                    If name.StartsWith("@") Then
                        name = context.Variables(name)
                    End If

                    If value.StartsWith("@") Then
                        value = context.Variables(value)
                    End If

                    Dim _query = collection.Find(Query.EQ(name, value))

                    Dim queryop = <div></div>


                    For Each item In _query
                        Dim outputPattern = XElement.Parse(data.<output>.Value)
                        Dim ctx As New CWML.ParseContext(context.Request, context.Response)

                        For Each attr In item.Names
                            ctx.Variables.Add("@" & attr, item(attr).ToString)
                        Next

                        queryop.Add(_cassandra.Parse(outputPattern, ctx))

                    Next

                    Return queryop

                Case Else
                    Return <div>Error</div>


            End Select
        Else
            Return <div></div>
        End If



    End Function

    Public Function ParseDataInsert(ByVal data As XElement, context As ParseContext) As XElement
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

        If context.Request.Form("__password") Is Nothing Then
            checkPresenceOf = False
        Else
            If context.Request.Form("__password") = data.@insertPassword = False Then
                checkPresenceOf = False
            End If
        End If

        If checkPresenceOf Then

            Try
                Dim collectionName As String = data.@collection

                Dim connectionString As String = "mongodb://localhost"
                Dim client = New MongoClient(connectionString)

                Dim server = client.GetServer
                Dim dataBase = server.GetDatabase("test")

                Dim collection = dataBase.GetCollection(collectionName)
                Dim document As New BsonDocument

                For Each item In data.<data>...<element>

                    Dim name As String = item.<name>.Value
                    If name.StartsWith("@") Then
                        name = context.Variables(name)
                    End If

                    Dim value As String = item.<value>.Value
                    If value.StartsWith("@") Then
                        value = context.Variables(value)
                    End If

                    document.Add(name, value)
                Next

                collection.Insert(document)

                Return CassandraParser.Parse(data.Element("ready"), context)

            Catch ex As Exception
                context.Variables.Add("@system/error", ex.Message)
                context.Variables.Add("@system/error/stackTrace", ex.StackTrace)

                Return CassandraParser.Parse(data.Element("error"), context)
            End Try
        Else
            context.Variables.Add("@system/error", "No error")
            Return <div/>
        End If


    End Function

    Public Function ParseReady(data As XElement, context As ParseContext) As XElement
        Dim result = <div>
                         <%= From item In data.Elements Select CassandraParser.Parse(item, context) %>
                     </div>

        Return result
    End Function

    Public Function ParseError(data As XElement, context As ParseContext) As XElement
        Dim result = <div>
                         <%= From item In data.Elements Select CassandraParser.Parse(item, context) %>
                     </div>

        Return result
    End Function


#End Region

End Class


Imports CWML
Imports IronPython.Hosting
Imports Microsoft.Scripting
Imports Microsoft.Scripting.Hosting


''' <summary>
''' This is a sample class to extend parsers of CWML
''' </summary>
''' <remarks></remarks> 
Public Class CustomParsers
    Implements CWML.iBlockParserList

    Private _cassandra As CWML.CassandraParser

    'User for bit.ly shorten links 
    Private bitlyLogin = ""
    Private BitlyApiKey = ""
    Public ReadOnly Property CassandraParser As CWML.CassandraParser Implements CWML.iBlockParserList.CassandraParser
        Get
            Return _cassandra
        End Get
    End Property

    Public Sub New(ByVal cassandra As CassandraParser)
        _cassandra = cassandra


        With CassandraParser.BlockParsers
            .Add("header", AddressOf ParseHeader)
            .Add("elle", AddressOf ParseElle)
            .Add("bitly", AddressOf parseBitly)
            .Add("camera", AddressOf parseCamera)
            '.Add("person", AddressOf ParsePerson)
            '.Add("Name", AddressOf ParseName)
            '.Add("Location", AddressOf ParseLocation)
            .Add("entryInput", AddressOf parseEntryInput)
            .Add("rawImage", AddressOf parseRawImage)
            .Add("coffeeLib", AddressOf ParseCoffeeLib)
            .Add("coffee", AddressOf ParseCoffee)
            .Add("imageEntry", AddressOf ParseImageEntry)
            .Add("import", AddressOf Import)
            .Add("javascript", AddressOf ParseJavaScript)

            'Bootstrap Parsers 

            .Add("jquery", AddressOf parseJquery)
            .Add("bootstrap", AddressOf ParseBootstrap)
            .Add("glyphicon", AddressOf ParseGlyphicon)


        End With

    End Sub

#Region "Parsers"
    Public Function ParseHeader(ByVal data As XElement, context As ParseContext) As XElement
        Dim content = <div>
                          <h1>Webxi using CWML</h1>
                          <%= From item In data.Elements Select _cassandra.Parse(item, context) %>
                      </div>

        Return CassandraParser.SetDefaultCss(data, content)
    End Function

    Public Function ParsePlain(ByVal data As XElement, context As ParseContext) As XElement
        Dim content = <div><%= data.Value %></div>
        Return content
    End Function

    Public Function ParseElle(ByVal data As XElement, context As ParseContext) As XElement
        Dim ellebd As New DateTime(1998, 4, 9)
        Dim td As DateTime = DateTime.Now

        Dim hbd As XElement = Nothing


        If td.Month = ellebd.Month And td.Day = ellebd.Day Then
            hbd = <div>Happy birthday Elle!</div>
        Else
            hbd = <div>she born in April 9 1998</div>
        End If


        Dim Content = <div>
                          <div>Mary Elle Fanning</div>
                          <div>Born in Conyers Georgie</div>
                          <%= hbd %>

                      </div>

        Return CassandraParser.SetDefaultCss(data, Content)
    End Function

    Public Function parseBitly(ByVal data As XElement, context As ParseContext) As XElement
        Dim longUrl = HttpUtility.UrlEncode(data.Value)
        Dim apiUrl As String =
            String.Format("https://api-ssl.bitly.com/v3/shorten?login={0}&apiKey={1}&longUrl={2}&format=txt", bitlyLogin, BitlyApiKey, longUrl)

        Dim webClient As New Net.WebClient
        Dim shortUrlStr = webClient.DownloadString(apiUrl)

        Dim shortUrlData = New XCData(shortUrlStr)

        Dim content = <a href=<%= shortUrlStr %>><%= shortUrlStr %></a>

        Return content
    End Function

    Public Function parseCamera(ByVal data As XElement, context As ParseContext) As XElement

        Dim xDoc As XDocument = XDocument.Load(context.Request.MapPath("cam.xml"))

        Dim container = <div>
                            <%= From item In xDoc.<cam>.Elements Select item %>
                        </div>

        Return container

    End Function

    Public Function ParseName(ByVal data As XElement, context As ParseContext) As XElement
        Return <div>Name: <%= data.Value %></div>
    End Function

    Public Function ParseLocation(ByVal data As XElement, context As ParseContext) As XElement
        Return <div>Location: <!-- Location--><%= data.Value %></div>
    End Function

    Public Function ParsePerson(ByVal data As XElement, context As ParseContext) As XElement

        Dim container = New XElement("div")

        container.Add(New XElement("h1"))



        Return container

    End Function

    Public Function parseEntry(ByVal data As XElement, context As ParseContext) As XElement

        Dim entryText As XElement = data.<entryText>

        Dim entry = <div>
                        <h1><%= data.<entryTitle> %></h1>
                        <strong>By <%= data.<author> %></strong>

                        <div><%= _cassandra.Parse(entryText, context) %></div>
                    </div>

        Return entry
    End Function

    Public Function parseEntryInput(ByVal data As XElement, context As ParseContext) As XElement
        Dim textarea = <textarea rows="4" cols="50" name=<%= data.@id %> id=<%= data.@id %>>

                       </textarea>

        Return textarea
    End Function

    Public Function parseRawImage(ByVal data As XElement, context As ParseContext) As XElement

        Dim img = <img src=<%= "data:image/jpeg;base64," & data.Value %> alt="binary image"/>
        Return img
    End Function

    Public Function ParseImageEntry(ByVal data As XElement, context As ParseContext) As XElement
        Dim imgstyle = String.Format("background-image: url({0})", data.Value)
        Dim download = <div><a href=<%= data.Value %>>Download image</a></div>
        Dim img = <div style=<%= imgstyle %>>
                      <%= CassandraParser.SetCss("download-area", download) %>
                  </div>

        Return CassandraParser.SetDefaultCss(data, img)

    End Function

    Public Function ParseVideo(ByVal data As XElement, context As ParseContext) As XElement
        Dim video = <video width=<%= data.@width.ToString %> height=<%= data.@height %>>
                        <source src=<%= data.Value %> type=<%= data.@type %>/>
                    </video>

        Return video
    End Function

    Public Function parseEntryContent(ByVal data As XElement, context As ParseContext) As XElement
        Dim parser As New CassandraParser()
        parser.BlockParsers.Add("elle", AddressOf ParseElle)

        Return parser.Parse(data, context)


    End Function

    Public Function ParseCoffee(ByVal data As XElement, context As ParseContext) As XElement
        Dim result = <script type="text/coffeescript">
                         <%= data.Value %>
                     </script>

        Return result

    End Function

    Public Function ParseJavaScript(ByVal data As XElement, context As ParseContext) As XElement
        Dim result = <script type="text/javascript" src=<%= data.Value %>></script>

        Return result
    End Function

    Public Function ParseCoffeeLib(ByVal data As XElement, context As ParseContext) As XElement
        Dim result = <script type="text/javascript" src="coffee-script.js"></script>
        Return result
    End Function

    Public Function Import(ByVal data As XElement, context As ParseContext) As XElement
        Dim stopWatch As New System.Diagnostics.Stopwatch
        stopWatch.Start()

        Dim Script As String = data.Value
        Dim engine = Python.CreateEngine
        Dim source = engine.CreateScriptSourceFromString(Script, SourceCodeKind.Statements)

        Dim compiled = source.Compile

        Dim scope = engine.CreateScope

        Dim result = compiled.Execute(scope)

        Dim tagnames As String() = data.Attribute("tagName").Value.Split(" ")
        Dim validators As String()

        If data.Attribute("customValidators") IsNot Nothing Then
            validators = data.Attribute("customValidators").Value.Split(" ")

            For Each v In validators
                Dim varName As String = "__validator_" & v
                Dim failedValidName As String = "__validation_failed" & v

                Dim cassandraValidator As New validator With {.tagName = v}

                If scope.ContainsVariable(varName) Then
                    Dim validator As Func(Of XElement, ParseContext, Boolean) =
                        scope.GetVariable(Of Func(Of XElement, ParseChildrenAttribute, Boolean))(varName)

                    cassandraValidator.validatorHandler =
                        validator

                End If

                If scope.ContainsVariable(failedValidName) Then
                    Dim failureHandler As Func(Of XElement, ParseContext, XElement) =
                        scope.GetVariable(Of Func(Of XElement, ParseContext, XElement))(failedValidName)

                    cassandraValidator.validationFailed = failureHandler
                End If

                CassandraParser.Validators.Add(cassandraValidator)

            Next

        End If


        For Each item In tagnames
            Dim plugin = scope.GetVariable(Of Func(Of XElement, ParseContext, XElement))("__plugin_" & item)
            CassandraParser.ImportedTags.Add(item, plugin)
        Next

        stopWatch.Stop()
        context.Response.Write("Elapsed: " & stopWatch.Elapsed.TotalMilliseconds)

        Return Nothing



    End Function

    Public Function parseJquery(ByVal data As XElement, ctx As ParseContext) As XElement
        Dim s = <script src="/Scripts/jquery-1.9.0.js"></script>
        Return s
    End Function

    Public Function ParseBootstrap(ByVal data As XElement, ctx As ParseContext) As XElement
        Dim b = <script src="/Scripts/bootsrap.js"></script>

        Return b
    End Function

    Public Function ParseGlyphicon(ByVal data As XElement, ctx As ParseContext) As XElement
        Dim glStr As String = String.Format("glyphicon glyphicon-{0}", data.Value)
        Return <span class=<%= glStr %>></span>
    End Function




#End Region

End Class

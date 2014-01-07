
Imports CWML


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
            .Add("plain", AddressOf ParsePlain)
            .Add("elle", AddressOf ParseElle)
            .Add("bitly", AddressOf parseBitly)
            .Add("camera", AddressOf parseCamera)
            .Add("person", AddressOf ParsePerson)
            .Add("Name", AddressOf ParseName)
            .Add("Location", AddressOf ParseLocation)
            .Add("entryInput", AddressOf parseEntryInput)
            .Add("rawImage", AddressOf parseRawImage)
        End With

    End Sub

#Region "Parsers"
    Public Function ParseHeader(ByVal data As XElement, context As ParseContext) As XElement
        Dim content = <div>
                          <h1>My site of Elle fanning</h1>
                          <%= From item In data.Elements Select _cassandra.Parse(item, context) %>
                      </div>

        Return CassandraParser.SetDefaultCss(data, content)
    End Function

    Public Function ParsePlain(ByVal data As XElement, context As ParseContext) As XElement
        Dim content = <div><%= data.Value %></div>
        Return content
    End Function

    Public Function ParseElle(ByVal data As XElement, context As ParseContext) As XElement
        Dim ellebd As New DateTime(1988, 4, 9)
        Dim td As DateTime = DateTime.Now

        Dim hbd As XElement = Nothing


        If td.Month = ellebd.Month And td.Day = ellebd.Day Then
            hbd = <div>Happy birthday Elle!</div>
        Else
            hbd = <div>she born in April 9 1988</div>
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

        Dim container = <div>
                            <%= From item In data.Elements Select CassandraParser.Parse(item, context) %>
                        </div>


        Return container

    End Function

    Public Function parseEntry(ByVal data As XElement, context As ParseContext) As XElement
        Dim entry = <div>
                        <h1><%= data.<entryTitle> %></h1>
                        <strong>By <%= data.<author> %></strong>
                        <div><%= data.<entryText> %></div>
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


#End Region

End Class

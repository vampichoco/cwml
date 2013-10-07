
Imports CWML


''' <summary>
''' This is a sample class to extend parsers of CWML
''' </summary>
''' <remarks></remarks> 
Public Class CustomParsers
    Implements CWML.iBlockParserList

    Private _blockParsers As Dictionary(Of String, Func(Of XElement, Web.HttpRequest, XElement))
    Private _cassandra As CWML.CassandraParser

    'User for bit.ly shorten links 
    Private bitlyLogin = ""
    Private BitlyApiKey = ""

    Public ReadOnly Property BlockParsers As Dictionary(Of String, Func(Of XElement, HttpRequest, XElement)) Implements CWML.iBlockParserList.BlockParsers
        Get
            Return _blockParsers
        End Get
    End Property

    Public ReadOnly Property CassandraParser As CWML.CassandraParser Implements CWML.iBlockParserList.CassandraParser
        Get
            Return _cassandra
        End Get
    End Property

    Public Sub New(ByVal cassandra As CassandraParser)
        _cassandra = cassandra
        _blockParsers = New Dictionary(Of String, Func(Of XElement, HttpRequest, XElement))

        With _blockParsers
            .Add("header", AddressOf ParseHeader)
            .Add("plain", AddressOf ParsePlain)
            .Add("elle", AddressOf ParseElle)
            .Add("bitly", AddressOf parseBitly)
            .Add("camera", AddressOf parseCamera)
        End With

    End Sub

#Region "Parsers"
    Public Function ParseHeader(ByVal data As XElement, ByVal req As Web.HttpRequest) As XElement
        Dim content = <div>
                          <h1>My site of Elle fanning</h1>
                          <%= From item In data.Elements Select _cassandra.Parse(item, req) %>
                      </div>

        Return CassandraParser.SetDefaultCss(data, content)
    End Function

    Public Function ParsePlain(ByVal data As XElement, ByVal req As Web.HttpRequest) As XElement
        Dim content = <div><%= data.Value %></div>
        Return content
    End Function

    Public Function ParseElle(ByVal data As XElement, ByVal req As Web.HttpRequest) As XElement
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

    Public Function parseBitly(ByVal data As XElement, req As Web.HttpRequest) As XElement
        Dim longUrl = HttpUtility.UrlEncode(data.Value)
        Dim apiUrl As String =
            String.Format("https://api-ssl.bitly.com/v3/shorten?login={0}&apiKey={1}&longUrl={2}&format=txt", bitlyLogin, BitlyApiKey, longUrl)

        Dim webClient As New Net.WebClient
        Dim shortUrlStr = webClient.DownloadString(apiUrl)

        Dim shortUrlData = New XCData(shortUrlStr)

        Dim content = <a href=<%= shortUrlStr %>><%= shortUrlStr %></a>

        Return content
    End Function

    Public Function parseCamera(ByVal data As XElement, req As Web.HttpRequest) As XElement

        Dim xDoc As XDocument = XDocument.Load(req.MapPath("cam.xml"))

        Dim container = <div>
                            <%= From item In xDoc.<cam>.Elements Select item %>
                        </div>

        Return container

    End Function

   
#End Region

End Class

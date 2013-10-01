Namespace CWML

    ''' <summary>
    ''' Provides basic parsing functions from CWML to HTML
    ''' </summary>
    ''' <remarks></remarks>
    Public Class StandardParsers
        Implements iBlockParserList

        Private _blockParsers As Dictionary(Of String, Func(Of XElement, System.Web.HttpRequest, XElement))
        Private _cassandra As CassandraParser

        ''' <summary>
        ''' Get a list of all the functions used to parse CWML blocks
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property BlockParsers As Dictionary(Of String, Func(Of XElement, Web.HttpRequest, XElement)) Implements iBlockParserList.BlockParsers
            Get
                Return _blockParsers
            End Get
        End Property

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

            _blockParsers = New Dictionary(Of String, Func(Of XElement, Web.HttpRequest, XElement))

            'Add all the parsers to the list here 

            With _blockParsers
                .Add("page", AddressOf ParsePage)
                .Add("head", AddressOf ParseHead)
                .Add("body", AddressOf ParseBody)
                .Add("link", AddressOf ParseLink)
                .Add("image", AddressOf ParseImage)
                .Add("h1", AddressOf ParseH1)
            End With

        End Sub

#Region "Parsers Region"

        ''' <summary>
        ''' Parse a page block element 
        ''' </summary>
        ''' <param name="data">Xml Element containing data to be parsed</param>
        ''' <param name="req">Request of the page that must be parsed</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ParsePage(ByVal data As XElement, req As Web.HttpRequest) As XElement
            Dim content = <html>
                              <%= From item In data.Elements Select CassandraParser.Parse(item, req) %>
                          </html>

            Return content
        End Function


        ''' <summary>
        ''' Parse a head block element 
        ''' </summary>
        ''' <param name="data">Xml Element containing data to be parsed</param>
        ''' <param name="req">Request of the page that must be parsed</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ParseHead(ByVal data As XElement, req As Web.HttpRequest) As XElement
            Dim Content = <head>
                              <%= From item In data.Elements Select CassandraParser.Parse(item, req) %>
                          </head>

            Return Content
        End Function

        ''' <summary>
        ''' Parse a body block element
        ''' </summary>
        ''' <param name="data">Xml Element containing data to be parsed</param>
        ''' <param name="req">Request of the page that must be parsed</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ParseBody(ByVal data As XElement, req As Web.HttpRequest) As XElement
            Dim content = <body>
                              <%= From item In data.Elements Select CassandraParser.Parse(item, req) %>
                          </body>

            Return content
        End Function

        ''' <summary>
        ''' Parse an image block element
        ''' </summary>
        ''' <param name="data">Xml Element containing data to be parsed</param>
        ''' <param name="req">Request of the page that must be parsed</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ParseImage(ByVal data As XElement, req As Web.HttpRequest) As XElement
            Dim content = <img src=<%= data.Value %>/>
            Return content
        End Function

        ''' <summary>
        ''' Parse a link block element
        ''' </summary>
        ''' <param name="data">Xml Element containing data to be parsed</param>
        ''' <param name="req">Request of the page that must be parsed</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ParseLink(ByVal data As XElement, req As Web.HttpRequest) As XElement
            Dim content = <a href=<%= data.Attribute("goesto").Value %>>
                              <%= CassandraParser.Parse(data.Elements.First(), req) %>
                          </a>

            Return content
        End Function

        Public Function ParseH1(ByVal data As XElement, req As Web.HttpRequest) As XElement
            Dim content = <h1><%= data.Value %></h1>
            Return content
        End Function

#End Region

    End Class
End Namespace

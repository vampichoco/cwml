Imports CWML
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
            .Add("text", AddressOf ParseText)
            .Add("css", AddressOf ParseCss)
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
    Public Function ParsePage(ByVal data As XElement, req As Web.HttpRequest, res As Web.HttpResponse) As XElement
        Dim content = <html>
                          <%= From item In data.Elements Select CassandraParser.Parse(item, req, res) %>
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
    Public Function ParseHead(ByVal data As XElement, req As Web.HttpRequest, res As Web.HttpResponse) As XElement
        Dim Content = <head>
                          <%= From item In data.Elements Select CassandraParser.Parse(item, req, res) %>
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
    Public Function ParseBody(ByVal data As XElement, req As Web.HttpRequest, res As Web.HttpResponse) As XElement
        Dim content = <body>
                          <%= From item In data.Elements Select CassandraParser.Parse(item, req, res) %>
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
    Public Function ParseImage(ByVal data As XElement, req As Web.HttpRequest, res As Web.HttpResponse) As XElement
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
    Public Function ParseLink(ByVal data As XElement, req As Web.HttpRequest, res As Web.HttpResponse) As XElement
        Dim content = <a href=<%= data.Attribute("goesto").Value %>>
                      </a>

        If Not data.HasElements Then
            content.Value = data.Value
        Else
            'Dim parsed = CassandraParser.Parse(data.Elements.First, req)
            content.Add(CassandraParser.Parse(data.Elements.First, req, res))
        End If

        Return content
    End Function

    ''' <summary>
    ''' Parse a h1 block element
    ''' </summary>
    ''' <param name="data">Xml Element containing data to be parsed</param>
    ''' <param name="req">Request of the page that must be parsed</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function ParseH1(ByVal data As XElement, req As Web.HttpRequest, res As Web.HttpResponse) As XElement
        Dim content = <h1><%= data.Value %></h1>
        Return content
    End Function

    ''' <summary>
    ''' Parse a br block element
    ''' </summary>
    ''' <param name="data">Xml Element containing data to be parsed</param>
    ''' <param name="req">Request of the page that must be parsed</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function ParseBr(ByVal data As XElement, req As Web.HttpRequest, res As Web.HttpResponse) As XElement
        Return <br/>
    End Function

    ''' <summary>
    ''' Parse a p block element
    ''' </summary>
    ''' <param name="data">Xml Element containing data to be parsed</param>
    ''' <param name="req">Request of the page that must be parsed</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function ParseP(ByVal data As XElement, req As Web.HttpRequest, res As Web.HttpResponse) As XElement
        Dim content = <p <%= From item In data.Attributes Select item %>>

                      </p>

        If Not data.HasElements Then
            content.Value = data.Value
        Else
            For Each item In data.Elements
                content.Add(CassandraParser.Parse(item, req, res))
            Next
        End If


        Return (CassandraParser.SetDefaultCss(data, content))

    End Function

    ''' <summary>
    ''' Takes the inner data and then creates a properly html formed output to keep lines structure.
    ''' </summary>
    ''' <param name="data">Xml Element containing data to be parsed</param>
    ''' <param name="req">Request of the page that must be parsed</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function ParseText(ByVal data As XElement, req As Web.HttpRequest, res As Web.HttpResponse) As XElement
        Dim lines = data.Value.Split(vbLf)
        Dim result = <p <%= From item In data.Attributes Select item %>>
                         <div>
                             <%= From item In lines Select CassandraParser.SetDefaultCss(data, (New XElement("div", item))) %>
                         </div>
                     </p>

        Return result
    End Function

    Public Function ParseCss(ByVal data As XElement, req As Web.HttpRequest, res As Web.HttpResponse) As XElement
        Dim content = <link rel="stylesheet" type="text/css" href=<%= data.Value %>/>
        Return content
    End Function


#End Region

End Class

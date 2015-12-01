Imports CWML
Imports MongoDB.Bson
Imports MongoDB.Driver
Imports MongoDB.Driver.Builders
Imports System.Text.RegularExpressions

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
            .Add("topBar", AddressOf ParseTopBar)
            .Add("head", AddressOf ParseHead)
            .Add("content", AddressOf ParseBody)
            .Add("link", AddressOf ParseLink)
            .Add("image", AddressOf ParseImage)
            '.Add("h1", AddressOf ParseH1)
            .Add("br", AddressOf ParseBr)
            .Add("p", AddressOf ParseP)
            .Add("container", AddressOf ParseDiv)
            .Add("text", AddressOf ParseText)
            .Add("css", AddressOf ParseCss)
            .Add("form", AddressOf ParseForm)
            .Add("textBox", AddressOf ParseTextBox)
            '.Add("button", AddressOf ParseButton)
            .Add("ready", AddressOf ParseReady)
            .Add("error", AddressOf ParseError)
            .Add("upload", AddressOf parseFileUpload)
            .Add("condition", AddressOf parseCondition)
            .Add("checkBox", AddressOf ParseCheckBox)
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

        CassandraParser.OutputScope.Add(New scopeItem With {.Scope = "head", .Element = Content})

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
                          <div class="main-container">
                              <%= From item In data.Elements Select CassandraParser.Parse(item, context) %>
                          </div>
                      </body>

        CassandraParser.OutputScope.Add(New scopeItem With {.Scope = "body", .Element = content})

        Return content
    End Function

    ''' <summary>
    ''' Parse an image block element
    ''' </summary>
    ''' <param name="data">Xml Element containing data to be parsed</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function ParseImage(ByVal data As XElement, context As ParseContext) As XElement
        Dim content = <img src=<%= data.Value %><%= From item In data.Attributes Select item %>/>
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
        Dim content = <h1><%= CassandraParser.Parse(data.Value, context) %></h1>
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
        Dim val As String = data.Value
        Dim pattern As String = "(\[%).*?(%\])"



        Dim htmlopBody As String = Regex.Replace(val, pattern,
                                             Function(m As Match) As String
                                                 Dim ip = m.Value.Replace("[%", "").Replace("%]", "")
                                                 Dim html = CassandraParser.Parse(XElement.Parse(ip), context).ToString


                                                 Return html

                                             End Function)

        Dim htmlop As String = "<div>" & htmlopBody & "</div>"

        Return XElement.Parse(htmlop)


    End Function


    Function _ret(ByVal input As String) As String
        Return input
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
        Dim content = <form action=<%= data.Attribute("action").Value %> method=<%= data.Attribute("method").Value %> enctype="multipart/form-data">
                          <%= From item In data.Elements Select _cassandra.Parse(item, context) %>
                      </form>

        Return content
    End Function

    Public Function ParseTextBox(ByVal data As XElement, context As ParseContext) As XElement

        Dim content = <input type="Text" id=<%= data.Attribute("id").Value %> name=<%= data.Attribute("id").Value %>/>

        Return content

    End Function

    Public Function ParseCheckBox(ByVal data As XElement, context As ParseContext) As XElement
        Dim content = <input type="checkbox" id=<%= data.Attribute("id").Value %> name=<%= data.Attribute("id").Value %>></input>
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

    Public Function parseFileUpload(ByVal data As XElement, context As ParseContext) As XElement
        Dim fileupload = <input type="file" id=<%= data.Attribute("id").Value %> name=<%= data.Attribute("id").Value %>/>

        Return fileupload
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

    Public Function parseRawImage(ByVal data As XElement, context As ParseContext) As XElement

        Dim img = <img src=<%= "data:image/jpeg;base64," & data.Value %> alt="binary image"/>
        Return img
    End Function

    Public Function ParseTopBar(ByVal data As XElement, context As ParseContext) As XElement
        Dim bar = <div class="cwml-top-bar">
                      <%= From item In data.Elements Select CassandraParser.Parse(item, context) %>
                  </div>

        Return bar
    End Function

    Public Function ParseFromString(ByVal data As String, context As ParseContext) As XElement
        Dim value = data.Replace("{{", "<").Replace("}}", ">")
        Dim pattern As String = "(\[%).*?(%\])"



        Dim htmlopBody As String = Regex.Replace(data, pattern,
                                             Function(m As Match) As String
                                                 Dim ip = m.Value.Replace("[%", "").Replace("%]", "")
                                                 Dim html = CassandraParser.Parse(XElement.Parse(ip), context).ToString


                                                 Return _ret(html)

                                             End Function)

        Dim htmlop As String = "<div>" & htmlopBody & "</div>"

        Return XElement.Parse(htmlop)
    End Function

    Public Function ParseRadioButton(ByVal data As XElement, context As ParseContext) As XElement



    End Function

    Public Function parseCondition(ByVal data As XElement, context As ParseContext) As XElement

        Dim condType As CassandraParser.ConditionType = [Enum].Parse(GetType(CassandraParser.ConditionType), data.@operator)
        Dim right As String = data.@right
        Dim left As String = data.@left

        Dim equals = CassandraParser.ParseCondition(condType, right, left, context)

        If equals = True Then
            Dim div = <div>
                          <%= From item In data.Elements Select CassandraParser.Parse(item, context) %>
                          <!-- Conditional block -->
                      </div>

            Return div
        Else

            Return Nothing

        End If

    End Function




#End Region

End Class

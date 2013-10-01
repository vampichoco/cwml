
Imports CassandraParser


''' <summary>
''' This is a sample class to extend parsers of CWML
''' </summary>
''' <remarks></remarks> 
Public Class CustomParsers
    Implements CassandraParser.CWML.iBlockParserList

    Private _blockParsers As Dictionary(Of String, Func(Of XElement, Web.HttpRequest, XElement))
    Private _cassandra As CassandraParser.CWML.CassandraParser

    Public ReadOnly Property BlockParsers As Dictionary(Of String, Func(Of XElement, HttpRequest, XElement)) Implements CassandraParser.CWML.iBlockParserList.BlockParsers
        Get
            Return _blockParsers
        End Get
    End Property

    Public ReadOnly Property CassandraParser As CassandraParser.CWML.CassandraParser Implements CassandraParser.CWML.iBlockParserList.CassandraParser
        Get
            Return _cassandra
        End Get
    End Property

    Public Sub New(ByVal cassandra As CWML.CassandraParser)
        _cassandra = cassandra
        _blockParsers = New Dictionary(Of String, Func(Of XElement, HttpRequest, XElement))

        With _blockParsers
            .Add("header", AddressOf ParseHeader)
            .Add("plain", AddressOf ParsePlain)
            .Add("elle", AddressOf ParseElle)
        End With

    End Sub

#Region "Parsers"
    Public Function ParseHeader(ByVal data As XElement, ByVal req As Web.HttpRequest) As XElement
        Dim content = <div Style="background-color:CCFFFF; color:CC0033">
                          <h1>My site of Elle fanning</h1>
                          <%= From item In data.Elements Select _cassandra.Parse(item, req) %>
                      </div>

        Return content
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


        Dim Content = <div style="padding:5px; background-color:CCFFCC">
                          <div>Mary Elle Fanning</div>
                          <div>Born in Conyers Georgie</div>
                          <%= hbd %>

                      </div>

        Return Content
    End Function
#End Region

End Class

Imports IronPython.Hosting
Imports Microsoft.Scripting
Imports Microsoft.Scripting.Hosting

''' <summary>
''' Provides dynamic execution of blocks containing IronPython code
''' </summary>
''' <remarks></remarks>
Public Class DynamicBlockParser
    Implements CWML.iBlockParserList

    Private _blockParser As Dictionary(Of String, Func(Of XElement, System.Web.HttpRequest, XElement))
    Private _cassandraParser As CWML.CassandraParser

    Public ReadOnly Property BlockParsers As Dictionary(Of String, Func(Of XElement, System.Web.HttpRequest, XElement)) Implements CWML.iBlockParserList.BlockParsers
        Get
            Return _blockParser
        End Get
    End Property

    Public ReadOnly Property CassandraParser As CWML.CassandraParser Implements CWML.iBlockParserList.CassandraParser
        Get
            Return _cassandraParser
        End Get
    End Property

    Public Sub New(ByVal CassandraParser As CWML.CassandraParser)
        _blockParser = New Dictionary(Of String, Func(Of XElement, Web.HttpRequest, XElement))
        _cassandraParser = CassandraParser

        CassandraParser.BlockParsers.Add("dynamic", AddressOf ParseDynamic)

    End Sub

    Public Function ParseDynamic(ByVal data As XElement, req As Web.HttpRequest) As XElement
        Dim script = data.Value

        Dim engine = Python.CreateEngine
        Dim scope = engine.CreateScope
        Dim source = engine.CreateScriptSourceFromString(script, SourceCodeKind.Statements)

        scope.SetVariable("__request", req)
        scope.SetVariable("__data", data)
        scope.SetVariable("__parser", CassandraParser)

        Dim compiled = source.Compile

        Dim result = compiled.Execute(scope)

        Dim XResult = scope.GetVariable("__result")

        Return XResult
    End Function


   
End Class

Imports IronPython.Hosting
Imports Microsoft.Scripting
Imports Microsoft.Scripting.Hosting

''' <summary>
''' Provides dynamic execution of blocks containing IronPython code
''' </summary>
''' <remarks></remarks>
Public Class DynamicBlockParser
    Implements CWML.iBlockParserList


    Private _cassandraParser As CWML.CassandraParser

    

    Public ReadOnly Property CassandraParser As CWML.CassandraParser Implements CWML.iBlockParserList.CassandraParser
        Get
            Return _cassandraParser
        End Get
    End Property

    Public Sub New(ByVal CassandraParser As CWML.CassandraParser)

        _cassandraParser = CassandraParser

        CassandraParser.BlockParsers.Add("dynamic", AddressOf ParseDynamic)


    End Sub

    Public Function ParseDynamic(ByVal data As XElement, req As Web.HttpRequest, res As Web.HttpResponse) As XElement
        Dim script = data.Value

        Dim engine = Python.CreateEngine
        Dim scope = engine.CreateScope
        Dim source = engine.CreateScriptSourceFromString(script, SourceCodeKind.Statements)

        scope.SetVariable("__request", req)
        scope.SetVariable("__data", data)
        scope.SetVariable("__parser", CassandraParser)
        scope.SetVariable("__response", res)

        Dim compiled = source.Compile

        Dim result = compiled.Execute(scope)

        Dim XResult = scope.GetVariable("__result")

        Return XResult
    End Function


   
End Class

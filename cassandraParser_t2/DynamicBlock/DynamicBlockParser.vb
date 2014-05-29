Imports IronPython.Hosting
Imports Microsoft.Scripting
Imports Microsoft.Scripting.Hosting

Imports MongoDB.Bson
Imports MongoDB.Driver
Imports MongoDB.Driver.Builders

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
        CassandraParser.BlockParsers.Add("lambda", AddressOf ParseLambda)


    End Sub

    Public Function ParseDynamic(ByVal data As XElement, context As ParseContext) As XElement
        Dim script = data.Value

        Dim engine = Python.CreateEngine
        Dim scope = engine.CreateScope
        Dim source = engine.CreateScriptSourceFromString(script, SourceCodeKind.Statements)

        scope.SetVariable("__request", context.Request)
        scope.SetVariable("__data", data)
        scope.SetVariable("__parser", CassandraParser)
        scope.SetVariable("__response", context.Response)

        Dim compiled = source.Compile

        Dim result = compiled.Execute(scope)

        Dim XResult = scope.GetVariable("__result")

        Return XResult
    End Function

    Public Function ParseLambda(ByVal data As XElement, context As ParseContext) As XElement
        Dim script = data.Value

        Dim engine = Python.CreateEngine
        Dim scope = engine.CreateScope
        Dim source = engine.CreateScriptSourceFromString(script, SourceCodeKind.Statements)

        scope.SetVariable("__request", context.Request)
        scope.SetVariable("__data", data)
        scope.SetVariable("__parser", CassandraParser)
        scope.SetVariable("__response", context.Response)

        Dim compiled = source.Compile

        Dim result = compiled.Execute(scope)

        Dim hello As Func(Of Integer, Integer) = scope.GetVariable(Of Func(Of Integer, Integer))("hello")


        Return <div><%= hello.Invoke(2) %></div>
    End Function
   
End Class

Namespace CWML
    Public Interface iBlockParserList

        ReadOnly Property BlockParsers As Dictionary(Of String, Func(Of XElement, System.Web.HttpRequest, XElement))
        ReadOnly Property CassandraParser As CassandraParser

    End Interface
End Namespace

Namespace CWML
    Public Class CassandraParser
        Private _blockParsers As Dictionary(Of String, Func(Of XElement, System.Web.HttpRequest, XElement))

        Public Sub New()
            _blockParsers = New Dictionary(Of String, Func(Of XElement, Web.HttpRequest, XElement))
        End Sub

        ''' <summary>
        ''' Gets a list of the parsers that will be used on the current parse proccess
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property BlockParsers As Dictionary(Of String, Func(Of XElement, System.Web.HttpRequest, XElement))
            Get
                Return _blockParsers
            End Get
        End Property

        ''' <summary>
        ''' Parse one CWML block into an HTML block
        ''' </summary>
        ''' <param name="data">Xml Element containing CWML data</param>
        ''' <param name="req">Request information</param>
        ''' <returns>One Xml Element containing parsed data into HTML</returns>
        ''' <remarks></remarks>
        Public Function Parse(ByVal data As XElement, req As System.Web.HttpRequest) As XElement
            Dim act = BlockParsers(data.Name.ToString)
            Return act.Invoke(data, req)
        End Function

    End Class
End Namespace

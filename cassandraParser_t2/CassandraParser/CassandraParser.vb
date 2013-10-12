
Public Class CassandraParser
    Private _blockParsers As Dictionary(Of String, Func(Of XElement, System.Web.HttpRequest, System.Web.HttpResponse, XElement))

    Private _useDefaultCss As Boolean

    Public Sub New()
        _blockParsers = New Dictionary(Of String, Func(Of XElement, Web.HttpRequest, Web.HttpResponse, XElement))
    End Sub

    ''' <summary>
    ''' Gets a list of the parsers that will be used on the current parse proccess
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property BlockParsers As Dictionary(Of String, Func(Of XElement, System.Web.HttpRequest, System.Web.HttpResponse, XElement))
        Get
            Return _blockParsers
        End Get
    End Property

    Public Property UseDefaultCss As Boolean
        Get
            Return _useDefaultCss
        End Get
        Set(value As Boolean)
            _useDefaultCss = value
        End Set
    End Property

    ''' <summary>
    ''' Parse one CWML block into an HTML block
    ''' </summary>
    ''' <param name="data">Xml Element containing CWML data</param>
    ''' <param name="req">Request information</param>
    ''' <returns>One Xml Element containing parsed data into HTML</returns>
    ''' <remarks></remarks>
    Public Function Parse(ByVal data As XElement, req As System.Web.HttpRequest, res As Web.HttpResponse) As XElement
        Dim act = BlockParsers(data.Name.ToString)
        ''If data.NodeType = Xml.XmlNodeType.Text Then
        ''    Return data
        ''Else
        Return act.Invoke(data, req, res)
        'End If

    End Function

    Public Function SetDefaultCss(ByVal original As XElement, target As XElement) As XElement
        If UseDefaultCss Then
            Dim ElName = String.Format("cwml-default-{0}", original.Name)
            target.SetAttributeValue("class", ElName)
            Return target
        Else
            Return target
        End If
    End Function

End Class


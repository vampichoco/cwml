
Public Class CassandraParser
    Private _blockParsers As Dictionary(Of String, Func(Of XElement, ParseContext, XElement))

    Private _useDefaultCss As Boolean

    Public Sub New()
        _blockParsers = New Dictionary(Of String, Func(Of XElement, ParseContext, XElement))
    End Sub

    ''' <summary>
    ''' Gets a list of the parsers that will be used on the current parse proccess
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property BlockParsers As Dictionary(Of String, Func(Of XElement, ParseContext, XElement))
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
    ''' <returns>One Xml Element containing parsed data into HTML</returns>
    ''' <remarks></remarks>
    Public Function Parse(ByVal data As XElement, context As ParseContext) As XElement
        Dim blockName As String = data.Name.ToString
        If data.Attributes.SingleOrDefault(Function(a) a.Name = "inherits") IsNot Nothing Then
            blockName = data.Attributes.SingleOrDefault(Function(a) a.Name = "inherits")
        End If
        Dim act = BlockParsers(blockName)
        ''If data.NodeType = Xml.XmlNodeType.Text Then
        ''    Return data
        ''Else

        If data.HasElements = False Then
            If data.Value.StartsWith("@") Then
                Dim varValue As String = context.Variables(data.Value)
                data.SetValue(varValue)
            Else
                data.SetValue(data.Value)
            End If
        End If

        Return act.Invoke(data, context)
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


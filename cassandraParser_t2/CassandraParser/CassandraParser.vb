Imports MongoDB.Bson
Imports MongoDB.Driver
Imports MongoDB.Driver.Builders
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

        If BlockParsers.ContainsKey(blockName) = False Then
            Throw New Exception(String.Format("Block parsers has no delegate to proccess {0} tag ", blockName))
        End If

        Dim act = BlockParsers(blockName)
        ''If data.NodeType = Xml.XmlNodeType.Text Then
        ''    Return data
        ''Else

        If data.HasElements = False Then
            If data.Value.StartsWith("@") Then

                If context.Variables.ContainsKey(data.Value) = False Then
                    Throw New Exception(String.Format("Variables dictionary does not contains '{0}' variable", data.Value))
                End If

                Dim varValue As String = context.Variables(data.Value)
                data.SetValue(varValue)

            End If
        End If

        'If BlockParsers.ContainsKey(data.Name.ToString) Then
        Try
            Return act.Invoke(data, context)
        Catch ex As Exception
            Return <div class="cwml-system-error">
                       <strong>Error trying to invoke delegate to parse <%= data.Name %></strong>
                       <div><%= ex.Message %></div>
                       <div><%= ex.StackTrace %></div>
                   </div>
        End Try

        'Else
        'Return data
        'End If
        'End If

    End Function

    Public Sub Save(context As ParseContext)


        Dim physicalPath As String = context.Request.MapPath(context.Request.Path)
        Dim file As New System.IO.FileInfo(physicalPath)


    End Sub

    Public Function ParseQueryCondition(ByVal input As XElement, context As ParseContext) As IMongoQuery
        Select Case input.Name.ToString
            Case "and"
                Dim qNodes = ParseNodes(input, context)

                Return Query.And(qNodes(0), qNodes(1))

            Case "or"

                Dim qNodes = ParseNodes(input, context)

                Return Query.Or(qNodes(0), qNodes(1))

            Case "equals"
                Dim name As String = input.@name
                Dim value As String = input.@value

                If name.StartsWith("@") Then : name = context.Variables(name)
                End If

                If value.StartsWith("@") Then : value = context.Variables(value)
                End If

                Return Query.EQ(name, value)


            Case Else
                If input.Name = "all" = False Then
                    Throw New Exception("No valid operation")
                End If
                Return Nothing
        End Select
    End Function

    Public Function ParseNodes(ByVal node As XElement, context As ParseContext) As IMongoQuery()
        Dim nodes = node.Nodes.ToArray

        If nodes.Length < 2 Then
            Throw New Exception("Or Operator requres two values to be compared")
        End If

        Dim nodeA As IMongoQuery = ParseQueryCondition(nodes(0), context)
        Dim nodeB As IMongoQuery = ParseQueryCondition(nodes(1), context)

        Return {nodeA, nodeB}

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

    Public Function SetCss(ByVal element As String, target As XElement) As XElement
        target.SetAttributeValue("class", element)
        Return target
    End Function

End Class
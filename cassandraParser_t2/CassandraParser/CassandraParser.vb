Imports MongoDB.Bson
Imports MongoDB.Driver
Imports MongoDB.Driver.Builders
Imports System.Text.RegularExpressions
Imports IronPython.Hosting
Imports Microsoft.Scripting
Imports Microsoft.Scripting.Hosting


Public Class CassandraParser

    Public Const tagPattern = "(<%).*?(%>)"
    Public Const variablePattern = "\$\w+(\/\w+|\w+)"

    Public Const secureOpeningTag = "\[\w+\]"
    Public Const secureClosingTag = "\[\/\w+\]"


    Private _importedTags As Dictionary(Of String, Func(Of XElement, ParseContext, XElement))
    Private _blockParsers As Dictionary(Of String, Func(Of XElement, ParseContext, XElement))
    Private _validators As List(Of validator)

    Private _useDefaultCss As Boolean

    Public Sub New()
        _blockParsers = New Dictionary(Of String, Func(Of XElement, ParseContext, XElement))
        _importedTags = New Dictionary(Of String, Func(Of XElement, ParseContext, XElement))
        _validators = New List(Of validator)
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


    Public ReadOnly Property ImportedTags As Dictionary(Of String, Func(Of XElement, ParseContext, XElement))
        Get
            Return _importedTags
        End Get
    End Property

    Public ReadOnly Property Validators As List(Of validator)
        Get
            Return _validators
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

        If BlockParsers.ContainsKey(blockName) = False And ImportedTags.ContainsKey(blockName) = False Then
            Throw New Exception(String.Format("Block parsers has no delegate to proccess {0} tag ", blockName))
        End If

        Dim tagValidators =
            From v In Validators Where v.tagName = blockName

        Dim isValidated As Boolean = True

        For Each tagValidator In tagValidators
            Dim isTagValid As Boolean =
                tagValidator.validatorHandler.Invoke(data, context)

            isValidated = isValidated And isTagValid

            If isTagValid = False Then
                Return tagValidator.validationFailed.Invoke(data, context)
            End If

        Next

        If isValidated Then
            If context.Extend.ContainsKey(blockName) Then
                Dim ext = context.Extend(blockName)
                ext.Invoke(data)

            End If

            Dim act As Func(Of XElement, ParseContext, XElement)
            ''If data.NodeType = Xml.XmlNodeType.Text Then
            ''    Return data
            ''Else

            If data.HasElements = False Then

                If Regex.IsMatch(data.Value, variablePattern) Then

                    If context.Variables.ContainsKey(data.Value) = False Then
                        Throw New Exception(String.Format("Variables dictionary does not contains '{0}' variable", data.Value))
                    End If

                    Dim varValue As String = context.Variables(data.Value)
                    data.SetValue(varValue)

                End If
            End If

            'If BlockParsers.ContainsKey(data.Name.ToString) Then
            Try
                If ImportedTags.ContainsKey(blockName) Then



                    Dim plugInMembers As New Dictionary(Of String, Func(Of XElement, ParseContext, XElement))


                    Dim plugin As Func(Of XElement, ParseContext, XElement) =
                        ImportedTags(blockName)

                    act = plugin
                    Return act.Invoke(data, context)

                Else
                    act = BlockParsers(blockName)
                    Return act.Invoke(data, context)
                End If
            Catch ex As Exception
                Return <div class="cwml-system-error">
                           <strong>Error trying to invoke delegate to parse <%= data.Name %></strong>
                           <div><%= ex.Message %></div>
                           <div><%= ex.StackTrace %></div>
                       </div>
            End Try
        End If

        'Else
        'Return data
        'End If
        'End If

    End Function

    Public Iterator Function getTags(ByVal input As String) As IEnumerable(Of String)

        Dim regex As New Regex(tagPattern)

        For Each item As Match In regex.Matches(input)
            Yield item.Value.Remove(0, 1).Remove(item.Value.Count, item.Value.Count - 1)
        Next

    End Function

    Public Sub Save(context As ParseContext)


        Dim physicalPath As String = context.Request.MapPath(context.Request.Path)
        Dim file As New System.IO.FileInfo(physicalPath)


    End Sub


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

    Public Structure ImportTag
        Private _name As String
        Private _code As String

        Public Property Name As String
            Get
                Return _name
            End Get
            Set(value As String)
                _name = value
            End Set
        End Property

        Public Property Code As String
            Get
                Return _code
            End Get
            Set(value As String)
                _code = value
            End Set
        End Property

    End Structure

End Class
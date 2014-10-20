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
    Public Const ParamPattern = "\@(\w+|[0-9])"

    Public Const SubClassPattern = "[\w+\.\w+]+"


    Private _importedTags As Dictionary(Of String, Func(Of XElement, ParseContext, XElement))
    Private _blockParsers As Dictionary(Of String, Func(Of XElement, ParseContext, XElement))

    Private _StyleCss As Text.StringBuilder
    Private _styleMixins As Dictionary(Of String, XElement)

    Private _output_scope As List(Of scopeItem)

    Private _validators As List(Of validator)

    Private _useDefaultCss As Boolean

    Private _localId As Integer = 0

    Public Enum ConditionType
        equals = 0
        notEquals = 1
    End Enum

    Public Sub New()
        _blockParsers = New Dictionary(Of String, Func(Of XElement, ParseContext, XElement))
        _importedTags = New Dictionary(Of String, Func(Of XElement, ParseContext, XElement))
        _validators = New List(Of validator)
        _output_scope = New List(Of scopeItem)
        _StyleCss = New Text.StringBuilder()
        _styleMixins = New Dictionary(Of String, XElement)
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

    Public ReadOnly Property OutputScope As List(Of scopeItem)
        Get
            Return _output_scope
        End Get
        
    End Property

    Public ReadOnly Property Style As String
        Get
            Return _StyleCss.ToString
        End Get
    End Property

    Public ReadOnly Property StyleMixins As Dictionary(Of String, XElement)
        Get
            Return _styleMixins
        End Get
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

            ParseStyle(data, context)

            'If BlockParsers.ContainsKey(data.Name.ToString) Then
            Try
                If ImportedTags.ContainsKey(blockName) Then



                    Dim plugInMembers As New Dictionary(Of String, Func(Of XElement, ParseContext, XElement))


                    Dim plugin As Func(Of XElement, ParseContext, XElement) =
                        ImportedTags(blockName)

                    act = plugin
                    Dim result = act.Invoke(data, context)
                    Return result

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

    Public Sub ParseStyle(ByVal data As XElement, ctx As ParseContext)
       
        Dim id As String = ""
        Dim setClass As Boolean = False

        For Each _style In data.Elements().Where(Function(s) s.Name = "style" Or s.Name = "mixin")

            If id = "" Then
                id = GenerateName(data, ctx)
            End If

            If setClass = False Then
                setClass = True
            End If

            Select Case _style.Name
                Case "style"

                    _StyleCss.Append(GenerateCss(id, _style, ctx, _style.Attributes))

                Case "mixin"
                    Dim mixinData = _style
                    Dim mixinName As String = mixinData.@name
                    Dim mixin As XElement = _styleMixins(mixinName)

                    _StyleCss.Append(GenerateCss(id, mixin, ctx, _style.Attributes))

            End Select

            _style.Remove()

        Next

        If setClass Then
            SetClassAttribute(data, id)
        End If


    End Sub

    Private Function GenerateName(ByVal data As XElement, ctx As ParseContext) As String
        If data.@id IsNot Nothing Then
            Return GetAttributeValue(data, "id", ctx)
        Else
            Return GenerateUniqueName()
        End If
    End Function

    Private Sub SetClassAttribute(ByVal data As XElement, id As String)
        If data.@class Is Nothing Then
            data.Add(New XAttribute("class", id))
        Else
            Dim [class] = data.@class
            data.Attribute("class").Remove()
            [class] = String.Format("{0} {1}", [class], id)
            data.Add(New XAttribute("class", [class]))
        End If
    End Sub

    Public Function GenerateCss(ByVal id As String, style As XElement, ctx As ParseContext, params As IEnumerable(Of XAttribute)) As String


        Dim cssBuilder As New Text.StringBuilder
        Dim selectors As New List(Of XElement)

        If style.@media IsNot Nothing Then

        End If

        cssBuilder.Append(String.Format(".{0}{{", id))
        For Each item In style.Elements
            With cssBuilder

                Select Case item.Name
                    Case "selector"
                        selectors.Add(item)
                    Case "import"
                        Dim import As String =
                            String.Format("@import url({0});", item.Value)
                        _StyleCss.AppendLine(import)

                    Case Else

                        Dim propertyValue As String = item.Value

                        If Regex.IsMatch(propertyValue, variablePattern) Then
                            propertyValue = ctx.Variables(propertyValue)
                        ElseIf Regex.IsMatch(propertyValue, ParamPattern) Then
                            Dim pv = propertyValue.Replace("@", "")
                            propertyValue =
                                params.Single(Function(p) p.Name.ToString = pv)
                        End If

                        .AppendLine(String.Format("{0}:{1};", item.Name, propertyValue))
                End Select

            End With
        Next

        cssBuilder.AppendLine("}")

        For Each selector In selectors

            Dim selectorName = SubstituteVariableByValue(selector.Attribute("name").Value, ctx)
            cssBuilder.AppendLine(String.Format(".{0}{{", selectorName))

            For Each item In selector.Elements

                Dim propertyValue As String = item.Value
                If Regex.IsMatch(propertyValue, variablePattern) Then
                    propertyValue = ctx.Variables(propertyValue)
                ElseIf Regex.IsMatch(propertyValue, ParamPattern) Then
                    Dim pv = propertyValue.Replace("@", "")
                    propertyValue =
                        params.Single(Function(p) p.Name.ToString = pv)
                End If
                cssBuilder.AppendLine(String.Format("{0}:{1};", item.Name, propertyValue))

            Next

            cssBuilder.AppendLine("}")
        Next

        Return cssBuilder.ToString

    End Function

    Public Function SubstituteVariableByValue(ByVal str As String, ctx As ParseContext) As String
        Dim regex As New Regex(variablePattern)
        Dim result As String =
            regex.Replace(str, variablePattern,
                          Function(m As Match) As String
                              If ctx.Variables.ContainsKey(m.Value) Then
                                  Return ctx.Variables(m.Value)
                              Else
                                  Return m.Value
                              End If
                          End Function)

        Return result
    End Function

    Public Iterator Function getTags(ByVal input As String) As IEnumerable(Of String)

        Dim regex As New Regex(tagPattern)

        For Each item As Match In regex.Matches(input)
            Yield item.Value.Remove(0, 1).Remove(item.Value.Count, item.Value.Count - 1)
        Next

    End Function

    Public Function GetAttributeValue(ByVal data As XElement, AttributeName As String, ctx As ParseContext) As String
        Dim attrValue As String = data.Attribute(AttributeName).Value
        If Regex.IsMatch(attrValue, variablePattern) Then

            If ctx.Variables.ContainsKey(attrValue) Then
                Return ctx.Variables(attrValue)
            Else
                Return attrValue
            End If
        Else
            Return attrValue

        End If
    End Function

    Public Function ParseCondition(ByVal conditionType As ConditionType, ByVal right As String, Left As String, ByVal ctx As ParseContext) As Boolean
        Dim valR As String = right
        Dim valL As String = Left

        If isVariable(right) Then
            If ctx.Variables.ContainsKey(right) Then
                valR = ctx.Variables(right)
            Else
                valR = ""
            End If
        End If

        If isVariable(Left) Then
            If ctx.Variables.ContainsKey(Left) Then
                valL = ctx.Variables(Left)
            Else
                valL = ""
            End If
        End If

        Select Case conditionType
            Case CassandraParser.ConditionType.equals
                Return String.Equals(valR, valL)
            Case CassandraParser.ConditionType.notEquals
                Return Not (String.Equals(valR, valL))
            Case Else
                Throw New Exception("how do you got here?")
        End Select

    End Function

    Public Function isVariable(ByVal value As String) As Boolean
        If Not value = "" Then
            Return Regex.IsMatch(value, variablePattern)
        Else
            Return False
        End If
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

    Public Function GenerateUniqueName() As String
        _localId += 1
        Return String.Format("element-{0}", _localId)
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
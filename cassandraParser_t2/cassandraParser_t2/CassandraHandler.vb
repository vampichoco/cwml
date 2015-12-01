Imports System.Web
Imports CWML
Public Class CassandraHandler
    Implements IHttpHandler

    ''' <summary>
    '''  You will need to configure this handler in the Web.config file of your 
    '''  web and register it with IIS before being able to use it. For more information
    '''  see the following link: http://go.microsoft.com/?linkid=8101007
    ''' </summary>
#Region "IHttpHandler Members"

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            ' Return false in case your Managed Handler cannot be reused for another request.
            ' Usually this would be false in case you have some state information preserved per request.
            Return True
        End Get
    End Property

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest

        Dim physicalPath As String = context.Server.MapPath(context.Request.Path)
        Dim file As New System.IO.FileInfo(physicalPath)

        If file.Exists = False Then
            context.Response.Write("(404) file not found")
        Else
            Dim xDoc As XDocument = XDocument.Load(physicalPath)

            Dim parser As New CWML.CassandraParser
            parser.UseDefaultCss = False


            Dim stdLib As New CWML.StandardParsers(parser) 'Load standard library 

            'Disable some std elements to avoid bootstrap collides 

            parser.BlockParsers.Remove("button")
            parser.BlockParsers.Remove("textBox")
            parser.BlockParsers.Remove("upload")
            parser.BlockParsers.Remove("form")

            Dim DynamicParser As New DynamicBlockParser(parser) 'Load dynamic block library
            Dim stdData As New StandardData.StandardDataParser(parser)

            Dim queryvalidator As New CWML.validator With
                {.tagName = "query",
                 .validatorHandler = Function(data, ctx)
                                         Dim checkPresenceOf As Boolean = False
                                         If data.@presenceOf IsNot Nothing Then

                                             For Each item In data.@presenceOf.Split(" ")
                                                 If ctx.Variables.ContainsKey(item) Then
                                                     checkPresenceOf = True
                                                 Else
                                                     checkPresenceOf = False
                                                     Exit For
                                                 End If
                                             Next
                                         Else
                                             checkPresenceOf = True
                                         End If

                                         Return checkPresenceOf
                                     End Function,
                 .validationFailed = Function(data, ctx) As XElement
                                         Return <div>Not validated request</div>
                                     End Function}

            parser.Validators.Add(queryvalidator)

            Dim dataInsertPresenceValidator As New CWML.validator With {
                .tagName = "dataInsert",
                .validatorHandler =
                Function(data, ctx) As Boolean
                    Dim checkPresenceOf As Boolean = False
                    If data.@presenceOf IsNot Nothing Then

                        For Each item In data.@presenceOf.Split(" ")
                            If ctx.Variables.ContainsKey(item) Then
                                checkPresenceOf = True
                            Else
                                checkPresenceOf = False
                                Exit For
                            End If
                        Next
                    Else
                        checkPresenceOf = True
                    End If

                    Return checkPresenceOf
                End Function,
                .validationFailed = Function(data, ctx) As XElement
                                        Return <div>Insufficent data to proccess this tag</div>
                                    End Function}

            Dim passwordValidator As New CWML.validator With {
                .tagName = "dataInsert",
                .validatorHandler =
                Function(data, ctx) As Boolean
                    Dim passwordInForm = ctx.Variables("$form/__password")
                    Dim requiredPassword As String = data.@insertPassword.ToString

                    Return passwordInForm = requiredPassword

                End Function,
                .validationFailed = Function(data, ctx) As XElement
                                        Return <div>you have no permission for this! =(</div>
                                    End Function}

            parser.Validators.Add(dataInsertPresenceValidator)
            parser.Validators.Add(passwordValidator)

            Dim custom As New CustomParsers(parser) 'Load custom blocks parser


            Dim parseContext As New CWML.ParseContext(context.Request, context.Response)

#If DEBUG Then
            parseContext.Variables.Add("$system/connectionString", "mongodb://localhost")
#Else
            parseContext.Variables.Add("$system/connectionString", "mongodb://localhost")
#End If

            parseContext.Variables.Add("$system/dbName", "test")
            

            Dim parsed = parser.Parse(xDoc.Elements.First, parseContext)

            Dim head = parser.OutputScope.SingleOrDefault(Function(si) si.Scope = "head")


            Dim body = parser.OutputScope.SingleOrDefault(Function(si) si.Scope = "body")

            context.Response.Write(parsed.ToString)

            'parser.Save(parseContext)

        End If





    End Sub

#End Region

End Class

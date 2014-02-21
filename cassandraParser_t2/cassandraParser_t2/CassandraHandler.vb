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
            parser.UseDefaultCss = True


            Dim stdLib As New CWML.StandardParsers(parser) 'Load standard library
            Dim DynamicParser As New DynamicBlockParser(parser) 'Load dynamic block library

            Dim custom As New CustomParsers(parser) 'Load custom blocks parser


            Dim parseContext As New CWML.ParseContext(context.Request, context.Response)

#If DEBUG Then
            parseContext.Variables.Add("@system/connectionString", "mongodb://localhost")
#Else
            parseContext.Variables.Add("@system/connectionString", "mongodb://localhost")
#End If

            Dim parsed = parser.Parse(xDoc.Elements.First, parseContext)
            context.Response.Write(parsed.ToString)

            parser.Save(parseContext)

        End If





    End Sub

#End Region

End Class

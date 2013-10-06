Imports System.Web
Imports CassandraParser
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

        ' Write your handler implementation here.

        Dim physicalPath As String = context.Server.MapPath(context.Request.Path)

        Dim xDoc As XDocument = XDocument.Load(physicalPath)

        Dim parser As New CWML.CassandraParser

        Dim stdLib As New CWML.StandardParsers(parser)

        For Each blockParser In stdLib.BlockParsers
            parser.BlockParsers.Add(blockParser.Key, blockParser.Value)
        Next


        parser.UseDefaultCss = True



        'This is a sample of how to add custom parsers 

        Dim custom As New CustomParsers(parser)

        For Each bParser In custom.BlockParsers
            parser.BlockParsers.Add(bParser.Key, bParser.Value)
        Next


        Dim parsed = parser.Parse(xDoc.Elements.First, context.Request)
        context.Response.Write(parsed.ToString)



    End Sub

#End Region

End Class

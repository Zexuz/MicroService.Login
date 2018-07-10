using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MicroService.Common.Core.Web;
using MicroService.Login.WebApi.Models;
using Serilog;
using Serilog.Events;

namespace MicroService.Login.WebApi.Middlewares
{
    internal class SerilogMiddleware
    {
        private const string MessageTemplate =
            "Client:{client} Path:{path} Referer:{referer} UserAgent:{userAgent} Duration:{duration} StausCode:{statusCode} UserId:{userId}";

        private static readonly ILogger Log = Serilog.Log.ForContext<SerilogMiddleware>();

        private readonly RequestDelegate _next;

        public SerilogMiddleware(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));

            var sw = Stopwatch.StartNew();
            var path = $"{httpContext.Request.Method}: {httpContext.Request.Path}";
            var ip = httpContext.Connection.RemoteIpAddress;
            var referer = httpContext.Request.Headers["Referer"];
            var userAgent = httpContext.Request.Headers["User-Agent"];
            var userId = httpContext.User.Identity.Name;
            try
            {
                await _next(httpContext);
                sw.Stop();

                var statusCode = httpContext.Response?.StatusCode;
                var level = statusCode > 499 ? LogEventLevel.Error : LogEventLevel.Information;

                Log.Write(
                    level,
                    MessageTemplate,
                    ip.ToString(), path, referer, userAgent, sw.ElapsedMilliseconds, statusCode, userId
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                var id = Guid.NewGuid().ToString();

                LogForErrorContext(httpContext).ForContext("CorrelationId", id)
                    .Error(ex, MessageTemplate, ip.ToString(), path, referer, userAgent, sw.ElapsedMilliseconds, 500, userId);

//                httpContext.Request.HttpContext.Response.Headers.Add("Access-Control-Allow-Origin", "*");
//                httpContext.Request.HttpContext.Response.Headers.Add("Access-Control-Allow-Methods", "*");
//                httpContext.Request.HttpContext.Response.Headers.Add("Access-Control-Allow-Headers", "*");
                if (httpContext.Response == null) return;
                httpContext.Response.Headers.Add("Cache-Control", "no-cache");
                httpContext.Response.Headers.Add("Content-Type", "application/json; charset=utf-8");

                httpContext.Response.StatusCode = 500;

                var json = JsonHelper.GetJsonStringFromObjcet(new ErrorResult<string>()
                {
                    Error = $"An error occurred, if you continue to see this error, contact support with this id '{id}' to get help"
                });

                await httpContext.Response.WriteAsync(json);
            }
        }

        private static ILogger LogForErrorContext(HttpContext httpContext)
        {
            var request = httpContext.Request;

            var result = Log
                .ForContext("RequestHeaders", request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()), destructureObjects: true)
                .ForContext("RequestHost", request.Host)
                .ForContext("RequestProtocol", request.Protocol);

            if (request.HasFormContentType)
                result = result.ForContext("RequestForm", request.Form.ToDictionary(v => v.Key, v => v.Value.ToString()));

            return result;
        }
    }
}
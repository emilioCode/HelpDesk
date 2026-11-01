using HelpDesk.Core.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Net;

namespace HelpDesk.Infrastructure.Filters
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            var exception = context.Exception;
            var type = exception.GetType();

            HttpStatusCode status;
            string title;
            if (type == typeof(Exception))
            {
                status = HttpStatusCode.BadRequest;
                title = "Bad Request";
            }
            else if (type == typeof(AccessDeniedException))
            {
                status = HttpStatusCode.Forbidden;
                title = "Forbidden";
            }
            else if (type == typeof(TooManyAttemptsException))
            {
                status = (HttpStatusCode)429;
                title = "Too Many Requests";
            }
            else
            {
                return;
            }

            var validation = new
            {
                Status = (int)status,
                Title = title,
                Detail = exception.Message
            };

            var json = new
            {
                errors = new[] { validation }
            };

            context.Result = new ObjectResult(json) { StatusCode = (int)status };
            context.HttpContext.Response.StatusCode = (int)status;
            context.ExceptionHandled = true;
        }
    }
}

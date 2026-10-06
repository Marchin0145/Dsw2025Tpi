using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Dsw2025Tpi.Application.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Dsw2025Tpi.Application.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            var statusCode = HttpStatusCode.InternalServerError; // Default a 500
            var message = "Ocurrió un error inesperado al procesar la solicitud.";

            switch (exception)
            {
                case ArgumentException:
                    statusCode = HttpStatusCode.BadRequest; // 400 Bad Request
                    message = exception.Message;
                    break;

                case NotFoundEntityException:
                    statusCode = HttpStatusCode.NotFound; // 404 Not Found
                    message = exception.Message;
                    break;

                case DuplicatedEntityException:
                    statusCode = HttpStatusCode.BadRequest; // 400 Bad Request
                    message = exception.Message;
                    break;

                case IncorrectPriceException:
                    statusCode = HttpStatusCode.BadRequest; // 400 Bad Request
                    message = exception.Message;
                    break;

                case NoContentException:
                    statusCode = HttpStatusCode.NoContent; // 204 Not Found
                    message = exception.Message;
                    break;

                case InternalServerErrorException:
                    statusCode = HttpStatusCode.InternalServerError; //500  Internal Server Error
                    message = exception.Message;
                    break;
                case BadRequestException:
                    statusCode = HttpStatusCode.BadRequest;
                    message = exception.Message;
                    break;
                default:
                    break;
            }

            context.Response.StatusCode = (int)statusCode;
            var result = JsonSerializer.Serialize(new { error = message });
            return context.Response.WriteAsync(result);
        }


    }
    }




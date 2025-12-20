using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Warehouse.Entities.Shared.ResponseHandling
{
    public class ResponseHandler
    {
        public Response<T> Deleted<T>(string message)
        {
            return new Response<T>()
            {
                StatusCode = HttpStatusCode.OK,
                Succeeded = true,
                Message = message
            };
        }

        public Response<T> Success<T>(T entity, string message)
        {
            return new Response<T>()
            {
                Data = entity,
                StatusCode = HttpStatusCode.OK,
                Succeeded = true,
                Message = message
            };
        }

        public Response<T> Unauthorized<T>(string message)
        {
            return new Response<T>()
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Succeeded = false,
                Message = message
            };
        }

        public Response<T> Forbidden<T>(string message)
        {
            return new Response<T>
            {
                StatusCode = HttpStatusCode.Forbidden,
                Succeeded = false,
                Message = message
            };
        }

        public Response<T> BadRequest<T>(string message)
        {
            return new Response<T>()
            {
                StatusCode = HttpStatusCode.BadRequest,
                Succeeded = false,
                Message = message
            };
        }

        public Response<T> UnprocessableEntity<T>(string message)
        {
            return new Response<T>()
            {
                StatusCode = HttpStatusCode.UnprocessableEntity,
                Succeeded = false,
                Message = message
            };
        }

        public Response<T> NotFound<T>(string message)
        {
            return new Response<T>()
            {
                StatusCode = HttpStatusCode.NotFound,
                Succeeded = false,
                Message = message
            };
        }

        public Response<T> Created<T>(T entity, string message)
        {
            return new Response<T>()
            {
                Data = entity,
                StatusCode = HttpStatusCode.Created,
                Succeeded = true,
                Message = message
            };
        }

        public Response<T> ServerError<T>(string message = "An unexpected error occurred.")
        {
            return new Response<T>
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Succeeded = false,
                Message = message,
            };
        }

        public Response<T> InternalServerError<T>(string message)
        {
            return new Response<T>()
            {
                StatusCode = System.Net.HttpStatusCode.InternalServerError,
                Succeeded = false,
                Message = message
            };
        }

        public IActionResult HandleModelStateErrors(ModelStateDictionary modelState)
        {
            var errors = modelState.Values.SelectMany(v => v.Errors)
                                         .Select(e => e.ErrorMessage)
                                         .ToList();
            return new BadRequestObjectResult(new Response<object>
            {
                StatusCode = HttpStatusCode.BadRequest,
                Succeeded = false,
                Errors = errors,
                Message = "There is an error while operation please try again"
            });
        }

        public Response<T> Found<T>(T entity, string message)
        {
            return new Response<T>()
            {
                Data = entity,
                StatusCode = HttpStatusCode.Found,
                Succeeded = true,
                Message = message
            };
        }
    }
}

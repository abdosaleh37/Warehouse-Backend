using System.Net;

namespace Warehouse.Entities.Shared.ResponseHandling
{
    public class Response<T> : IResponse
    {
        public Response()
        {
            Message = default;
            Errors = new List<string>();
            Data = default;
        }

        public Response(T data, string? message = null)
        {
            Succeeded = true;
            Message = message ?? string.Empty;
            Data = data;
            Errors = new List<string>();
        }

        public Response(string message)
        {
            Succeeded = false;
            Message = message;
            Errors = new List<string>();
            Data = default!;
        }

        public Response(string message, bool succeeded)
        {
            Succeeded = succeeded;
            Message = message;
            Errors = new List<string>();
            Data = default!;
        }

        public HttpStatusCode StatusCode { get; set; }
        public bool Succeeded { get; set; }
        public string? Message { get; set; }
        public List<string> Errors { get; set; }
        public T? Data { get; set; }
    }
}

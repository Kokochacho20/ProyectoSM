namespace PA_API.DTOs
{
    public class ResultDto<T>
    {
        public bool Success { get; init; }
        public T? Data { get; init; }
        public string? Message { get; init; }
        public int StatusCode { get; init; }

        public static ResultDto<T> Ok(
           T data,
           int statusCode = StatusCodes.Status200OK,
           string? message = null)
        {
            return new()
            {
                Success = true,
                Data = data,
                StatusCode = statusCode,
                Message = message
            };
        }

        public static ResultDto<T> Fail(
            int statusCode,
            string message)
        {
            return new()
            {
                Success = false,
                StatusCode = statusCode,
                Message = message
            };
        }
    }

    public class ResultDto
    {
        public bool Success { get; init; }
        public string? Message { get; init; }
        public int StatusCode { get; init; }

        public static ResultDto Ok(
           int statusCode = StatusCodes.Status200OK,
           string? message = null)
        {
            return new()
            {
                Success = true,
                StatusCode = statusCode,
                Message = message
            };
        }

        public static ResultDto Fail(
            int statusCode,
            string message)
        {
            return new()
            {
                Success = false,
                StatusCode = statusCode,
                Message = message
            };
        }
    }
}

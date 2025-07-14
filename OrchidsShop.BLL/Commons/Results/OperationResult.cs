using OrchidsShop.BLL.Commons.Errors;
using OrchidsShop.BLL.Commons.Paginations;

namespace OrchidsShop.BLL.Commons.Results;

/// <summary>
/// Common result class for operations in the application.
/// </summary>
/// <typeparam name="T"></typeparam>
public class OperationResult<T>
{
    public StatusCode StatusCode { get; set; } = StatusCode.Ok;
    public string? Message { get; set; } 
    public bool IsError { get; set; }
    public T? Payload { get; set; }
    public object? MetaData { get; set; }
    //public Pagination? Pagination { get; set; }
    public List<Error> Errors { get; set; } = new List<Error>();
    public void AddError(StatusCode code, string message)
        {
            HandleError(code, message);
        }
    public void AddResponseStatusCode(
        StatusCode code,
        string message,
        T? payload,
        object? metaData = null
        )
    { 
        HandleResponse(code, message, payload, metaData);
    }
    public void AddResponseErrorStatusCode(
        StatusCode code,
        string message,
        T? payload,
        object? metaData = null
        )
    { 
        HandleResponseError(code, message, payload, metaData);
    }

    public static OperationResult<T> Failure(StatusCode statusCode, List<string> messages) 
    { 
        var errorList = messages.Select(message => new Error
        { 
            Code = statusCode, 
            Message = message
            
        }).ToList();

            
        return new OperationResult<T>
        {
            StatusCode = statusCode,
            Message = string.Join(", ", messages),
            IsError = true,
            Errors = errorList
        };
    }
    
       
    public static OperationResult<T> Success(
        T payload, 
        StatusCode statusCode = StatusCode.Ok,
        string? message = null
        )
    {
        return new OperationResult<T>
        {
            StatusCode = statusCode,
            Message = message ?? "Success",
            IsError = false,
            Payload = payload
        };
    }
    
    private void HandleResponse(
        StatusCode code, 
        string message,
        T? payload, object? metaData
        )
    {
        StatusCode = code;
        IsError = false;
        Message = message;
        Payload = payload;
        MetaData ??= metaData;
    }
    
    private void HandleResponseError(
        StatusCode code, 
        string message,
        T? payload, 
        object? metaData
        )
    {
        StatusCode = code;
        Errors.Add(new Error { Code = code, Message = message });
        IsError = true;
        Message = message;
        Payload = payload;
        MetaData ??= metaData;
    }

    public void AddUnknownError(string message)
    {
        HandleError(StatusCode.UnknownError, message);
    }

        public void ResetIsErrorFlag()
        {
            IsError = false;
        }

    private void HandleResponse(
        StatusCode code, 
        string message, 
        T? payload, 
        Pagination? pagination = null,
        object? metaData = null
        )
    {
        StatusCode = code; 
        IsError = false;
        Message = message;
        Payload = payload; 
        // Pagination = pagination ?? new Pagination(0,1,1);
        MetaData ??= metaData;
    }

    private void HandleError(StatusCode code, string message)
    {
        Errors.Add(new Error { Code = code, Message = message });
        IsError = true;
    }

    public void AddValidationError(string foodIdAndSupplierIdCannotBeTheSame)
    {
        HandleError(StatusCode.UnknownError, foodIdAndSupplierIdCannotBeTheSame);
    }
}
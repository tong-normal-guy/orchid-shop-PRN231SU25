namespace OrchidsShop.BLL.Commons.Results;

public enum StatusCode
{
    Ok = 200,
    Created = 201,
    NoContent = 204,
    BadRequest = 400,
    NotFound = 404,
    ServerError = 500,
    UnAuthorize = 401,
    Forbidden = 403,
    InvalidInput = 422,
    UnknownError = 520,
    FirebaseAuthError = 600, // Specific Firebase authentication error
    ValidationError = 422 // Data validation error
}
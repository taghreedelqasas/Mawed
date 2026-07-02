namespace Maw3ed.BLL.Common
{
    public enum ServiceError
    {
        None,
        NotFound,
        Forbidden,
        Conflict,
        BadRequest
    }

    public record ServiceResult(bool Success, string Message, ServiceError Error = ServiceError.None);

    public record ServiceResult<T>(bool Success, string Message, T? Data, ServiceError Error = ServiceError.None);
}

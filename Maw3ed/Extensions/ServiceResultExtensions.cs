using Maw3ed.BLL.Common;
using Microsoft.AspNetCore.Mvc;

namespace Maw3ed.Extensions
{
    public static class ServiceResultExtensions
    {
        public static IActionResult ToActionResult(this ServiceResult result, ControllerBase controller)
        {
            if (result.Success)
                return controller.Ok(new { result.Message });

            return result.Error switch
            {
                ServiceError.NotFound  => controller.NotFound(new { result.Message }),
                ServiceError.Forbidden => controller.StatusCode(403, new { result.Message }),
                ServiceError.Conflict  => controller.Conflict(new { result.Message }),
                _                      => controller.BadRequest(new { result.Message })
            };
        }

        public static IActionResult ToActionResult<T>(this ServiceResult<T> result, ControllerBase controller)
        {
            if (result.Success)
                return controller.Ok(new { result.Message, result.Data });

            return result.Error switch
            {
                ServiceError.NotFound  => controller.NotFound(new { result.Message }),
                ServiceError.Forbidden => controller.StatusCode(403, new { result.Message }),
                ServiceError.Conflict  => controller.Conflict(new { result.Message }),
                _                      => controller.BadRequest(new { result.Message })
            };
        }

        public static IActionResult ToCreatedResult<T>(
            this ServiceResult<T> result,
            ControllerBase controller,
            string actionName,
            object routeValues,
            object value)
        {
            if (result.Success)
                return controller.CreatedAtAction(actionName, routeValues, value);

            return result.Error switch
            {
                ServiceError.NotFound  => controller.NotFound(new { result.Message }),
                ServiceError.Forbidden => controller.StatusCode(403, new { result.Message }),
                ServiceError.Conflict  => controller.Conflict(new { result.Message }),
                _                      => controller.BadRequest(new { result.Message })
            };
        }
    }
}

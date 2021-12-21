using ColorMyTree.Models;
using Microsoft.AspNetCore.Mvc;

namespace ColorMyTree.Controllers
{
    public abstract class ApiController
    {
        public OkObjectResult Ok(object? value)
        {
            return new OkObjectResult(new ResponseModel<object?>(value));
        }

        public BadRequestObjectResult BadRequest(string message)
        {
            return new BadRequestObjectResult(new ResponseModel
            {
                Error = true,
                Message = message
            });
        }

        public ConflictObjectResult Conflict(string message)
        {
            return new ConflictObjectResult(new ResponseModel
            {
                Error = true,
                Message = message
            });
        }

        public ObjectResult Unauthorized(string message)
        {
            return new ObjectResult(new ResponseModel
            {
                Error = true,
                Message = message
            })
            {
                StatusCode = 401
            };
        }
    }
}

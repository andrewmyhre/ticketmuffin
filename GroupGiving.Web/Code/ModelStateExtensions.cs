using System.Collections.Generic;
using System.Web.Mvc;
using GroupGiving.Web.Models;

namespace GroupGiving.Web.Code
{
    public static class ModelStateExtensions
    {
        public static ErrorResponse ToErrorResponse(this ModelStateDictionary modelState)
        {
            ErrorResponse response = new ErrorResponse();
            List<Error> errorList = new List<Error>();

            foreach(var key in modelState.Keys)
            {
                var ms = modelState[key];
                foreach(var error in ms.Errors)
                {
                    errorList.Add(new Error{Key=key, ErrorMessage=error.ErrorMessage});
                }
            }

            response.Errors = errorList;

            return response;
        }
    }
}
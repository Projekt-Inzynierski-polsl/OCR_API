
using OCR_API.Entities;
using OCR_API.Exceptions;
using OCR_API.Repositories;

namespace OCR_API.Middleware
{
    public class ErrorHandlingMiddleware : IMiddleware
    {
        private readonly ILogger<ErrorHandlingMiddleware> logger;
        private readonly IUnitOfWork unitOfWork;
        private readonly JwtTokenHelper jwtTokenHelper;

        public ErrorHandlingMiddleware(ILogger<ErrorHandlingMiddleware> logger, IUnitOfWork unitOfWork, JwtTokenHelper jwtTokenHelper)
        {
            this.logger = logger;
            this.unitOfWork = unitOfWork;
            this.jwtTokenHelper = jwtTokenHelper;
        }
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {       
                var jwtToken = context.Request.Headers["Authorization"].ToString()?.Split(" ").LastOrDefault();
                if (!string.IsNullOrEmpty(jwtToken))
                {
                    if (unitOfWork.BlackListedTokens.Entity.Any(token => token.Token.Equals(jwtToken)))
                    {
                        throw new UnauthorizedAccessException("Unauthorized: Token is blacklisted.");
                    }
                    var userId = jwtTokenHelper.GetUserIdFromToken(jwtToken);
                    unitOfWork.UserId = userId;
                }

                await next.Invoke(context);
            }
            catch(NotFoundException e)
            {
                logger.LogError(e, e.Message);
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync(e.Message);
            }
            catch (BadRequestException e)
            {
                logger.LogError(e, e.Message);
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync(e.Message);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Something went wrong.");
            }
        }
    }
}

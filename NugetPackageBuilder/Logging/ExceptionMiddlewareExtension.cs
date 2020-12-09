using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using System;
using System.Net;

namespace ShivOhm.Infrastructure
{
    public static class ExceptionMiddlewareExtension
    {
        public static void ConfigureApplication(this IApplicationBuilder app, ILog logger, bool Autorun, InfraEnums.MigrationType? MigrationType = null, long? VersionNo = null)
        {
            app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "application/json";

                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (contextFeature != null)
                    {
                        logger.Error($"{contextFeature.Error}");

                        await context.Response.WriteAsync(new ErrorDetails()
                        {
                            StatusCode = context.Response.StatusCode,
                            Message = $"Internal Server Error. Reason: {contextFeature.Error?.Message}"
                        }.ToString());
                    }
                });
            });


            app.UseSwagger();

            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", string.Empty);
                options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
                options.DefaultModelsExpandDepth(-1);
            });

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            if (Autorun)
            {
                if (MigrationType==InfraEnums.MigrationType.Up)
                {
                    app.Migrate();
                }
                else
                {
                    app.MigrateDown(Convert.ToInt64(VersionNo));
                }
            }
        }
    }
}

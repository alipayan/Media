using MassTransit;
using Media.Infrastructure;
using Media.Infrastructure.IntegrationEvents;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Minio;
using Minio.DataModel.Args;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<MediaDbContext>(config =>
{
    config.UseInMemoryDatabase("MediaDb");
});

builder.Services.AddMinio(configure =>
{
    var endpoint = builder.Configuration.GetConnectionString("MininoEndpoint");
    configure.WithEndpoint(endpoint);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/{bucket_name}/{catalog_id}", async (
    [FromRoute(Name = "bucket-name")] string bucketName,
    [FromRoute(Name = "catalog-id")] string catalogId,
    MediaDbContext dbContext,
    IFormFile file,
    IPublishEndpoint publisher,
    IConfiguration configuration) =>
{
    var endpoint = configuration[""];
    var accessKey = configuration[""];
    var secretKey = configuration[""];
    var minio = new MinioClient()
    .WithEndpoint(endpoint)
    .WithCredentials(accessKey, secretKey)
    .Build();

    var args = new PutObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject("name.jpg")
                    .WithContentType(file.ContentType)
                    .WithStreamData(file.OpenReadStream())
                    .WithObjectSize(file.Length);

    var response = await minio.PutObjectAsync(args);

    //var state = new StatObjectArgs()
    //.WithBucket()
    //.WithObject();

    //minio.StatObjectAsync(state);
    var token = new UrlToken() { BacketName = bucketName, FileName = file.FileName, Id = Guid.NewGuid(), ContentType = file.ContentType };


    await dbContext.UrlTokens.AddAsync(token);
    await dbContext.SaveChangesAsync();

    //var url = $"{endpoint}/{bucketName}/{file.FileName}";
    var url = $"https://localhost:/{token.Id}";
    await publisher.Publish(new MediaUploadedEvent(file.FileName, url, catalogId, DateTime.UtcNow));

})
.WithOpenApi();

app.Run();

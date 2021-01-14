using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Interfaces;
using AmberwoodCore.Responses;
using AmbRcnTradeServer.Constants;
using AmbRcnTradeServer.Models.AttachmentModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

namespace AmbRcnTradeServer.Services
{
    public interface IAttachmentService
    {
        Task<ServerResponse> SaveDocuments(HttpRequest httpRequest);
        Task<ServerResponse> SaveImages(HttpRequest httpRequest);
        Task<ServerResponse> DeleteAttachments(IEnumerable<DeleteAttachmentRequest> requests);
        Task<FileContentResult> GetAttachment(string contractId, string fileName);
    }

    public class AttachmentServiceBase : IAttachmentService
    {
        private readonly IAsyncDocumentSession _session;

        protected AttachmentServiceBase(IAsyncDocumentSession session)
        {
            _session = session;
        }


        public async Task<ServerResponse> SaveDocuments(HttpRequest httpRequest)
        {
            var requests = await GetAttachmentPostRequests(httpRequest);

            foreach (var request in requests)
                _session.Advanced.Attachments.Store(request.ContractId, request.FileName, request.File.OpenReadStream(), request.File.ContentType);

            return await Task.FromResult(new ServerResponse("Uploaded documents"));
        }

        public async Task<ServerResponse> SaveImages(HttpRequest httpRequest)
        {
            var requests = await GetAttachmentPostRequests(httpRequest);

            foreach (var request in requests)
            {
                var imageFormat = GetImageFormat(request.FileName);

                await using var ms = new MemoryStream();
                await request.File.OpenReadStream().CopyToAsync(ms);
                ms.Position = 0;

                using var image = await Image.LoadAsync(ms);
                image.Mutate(x => x.Resize(300, 0));

                var outStream = new MemoryStream();
                await image.SaveAsync(outStream, imageFormat);
                outStream.Position = 0;

                var fs = new FormFile(outStream, 0, outStream.Length, "", "");
                _session.Advanced.Attachments.Store(request.ContractId, request.FileName, fs.OpenReadStream(), request.File.ContentType);
            }

            return new ServerResponse("Uploaded images");
        }

        public async Task<ServerResponse> DeleteAttachments(IEnumerable<DeleteAttachmentRequest> deleteRequests)
        {
            foreach (var request in deleteRequests)
            {
                _session.Advanced.Attachments.Delete(request.ContractId, request.FileName);
            }

            return await Task.FromResult(new ServerResponse("Deleted documents"));
        }

        public async Task<FileContentResult> GetAttachment(string contractId, string fileName)
        {
            using var attachment = await _session.Advanced.Attachments.GetAsync(contractId, fileName);

            await using var ms = new MemoryStream();

            if (attachment == null)
                return null;

            await attachment.Stream.CopyToAsync(ms);
            var contentType = attachment.Details.ContentType;
            ms.Position = 0;
            return new FileContentResult(ms.ToArray(), contentType);
        }


        public async Task<List<AttachmentInfo>> GetAttachmentRoutes<T>(HttpRequest httpRequest, string controllerName, string contractId) where T : IIdentity
        {
            var entity = await _session.LoadAsync<T>(contractId);
            var attachmentNames = _session.Advanced.Attachments.GetNames(entity);

            var routes = (from name in attachmentNames
                let route = $"{httpRequest.Scheme}://{httpRequest.Host}/api/{controllerName}/getAttachment?contractId={contractId}&fileName={name.Name}"
                select new AttachmentInfo
                {
                    Name = name.Name,
                    Route = route,
                    Size = name.Size,
                    ImageType = GetImageType(name.Name)
                }).ToList();

            return routes;
        }

        private static async Task<IEnumerable<PostAttachmentRequest>> GetAttachmentPostRequests(HttpRequest httpRequest)
        {
            var form = await httpRequest.ReadFormAsync();
            var files = form.Files;

            var id = httpRequest.Headers["id"];

            var requests = files.Select(file => new PostAttachmentRequest
            {
                ContractId = id,
                File = file
            });
            return requests;
        }

        private static IImageFormat GetImageFormat(string fileName)
        {
            var fileFormat = fileName.Split(".")[1];
            return fileFormat switch
            {
                "jpeg" => JpegFormat.Instance,
                "jpg" => JpegFormat.Instance,
                "gif" => GifFormat.Instance,
                "png" => PngFormat.Instance,
                _ => throw new InvalidOperationException($"Unable to handle {fileFormat} files")
            };
        }

        private static ImageType GetImageType(string name)
        {
            var fileType = name.Split(".")[1];

            return fileType switch
            {
                "jpeg" => ImageType.Image,
                "jpg" => ImageType.Image,
                "gif" => ImageType.Image,
                "png" => ImageType.Image,
                "pdf" => ImageType.Pdf,
                _ => ImageType.Other
            };
        }
    }
}
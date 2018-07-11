using System;
using Umbraco.Core.Models;

namespace Umbraco.Deploy.Contrib.Tests.TestHelpers
{
    public class MockedMedia
    {
        public static Media CreateImageMedia(IMediaType mediaType, string name, int parentId = -1, string path = null)
        {
            var media = new Media(name, parentId, mediaType);

            if (string.IsNullOrWhiteSpace(path) == false)
            {
                media.SetValue("umbracoFile", path);
            }
            return media;
        }

        public static Media CreateCuteKittenMedia(IMediaType mediaType)
        {
            var media = CreateImageMedia(mediaType, "Cute kitten", -1, "/media/1001/cute-little-kitten-cute-kittens-16288222-1024-768.jpg");
            media.Id = 1061;
            media.Key = Guid.Parse("eb0ad3395cad417a90051bd871eccc9c");
            return media;
        }

        public static Media CreateAngryKittenMedia(IMediaType mediaType)
        {
            var media = CreateImageMedia(mediaType, "Angry kitten", -1, "/media/1002/angry-little-kitten-1024-768.jpg");
            media.Id = 1062;
            media.Key = Guid.Parse("51e50c3a1a494507b4364068c4b429cd");
            return media;
        }
    }
}

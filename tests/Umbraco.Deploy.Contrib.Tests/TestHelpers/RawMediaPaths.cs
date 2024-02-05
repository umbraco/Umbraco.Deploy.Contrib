using Umbraco.Core.Services;
using Umbraco.Deploy.Environments;

namespace Umbraco.Deploy.Contrib.Tests.TestHelpers
{
    public class RawMediaPaths : IMediaPaths
    {
        private readonly IMediaService _mediaService;

        public RawMediaPaths(IMediaService mediaService)
        {
            _mediaService = mediaService;
        }

        public MediaIdent Get(string mediaPath)
        {
            var media = _mediaService.GetMediaByPath(mediaPath);
            return media == null ? null : new MediaIdent(media);
        }
    }
}

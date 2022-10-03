//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Threading;
//using System.Threading.Tasks;
//using Umbraco.Core;
//using Umbraco.Core.Deploy;

//namespace Umbraco.Deploy.Contrib.Tests.TestHelpers
//{
//    public class TestFileTypeCollection : IFileTypeCollection
//    {
//        private readonly IFileType _fileType;

//        public TestFileTypeCollection()
//        {
//            Files = new Dictionary<StringUdi, string>();
//            _fileType = new AnyFileType(Files);
//        }

//        public IFileType this[string entityType] => _fileType;

//        public Dictionary<StringUdi, string> Files { get; }

//        public bool Contains(string entityType)
//        {
//            return true;
//        }

//        private class AnyFileType : IFileType
//        {
//            private readonly Dictionary<StringUdi, string> _files;

//            public AnyFileType(Dictionary<StringUdi, string> files)
//            {
//                _files = files;
//            }

//            public Stream GetStream(StringUdi udi)
//            {
//                string path;
//                if (!_files.TryGetValue(udi, out path)) return null;
//                return File.OpenRead(path);
//            }

//            public Task<Stream> GetStreamAsync(StringUdi udi, CancellationToken token)
//            {
//                return Task.FromResult(GetStream(udi));
//            }

//            public Stream GetChecksumStream(StringUdi udi)
//            {
//                return GetStream(udi);
//            }

//            public long GetLength(StringUdi udi)
//            {
//                var stream = GetStream(udi);
//                if (stream == null) return 0;
//                try
//                {
//                    return stream.Length;
//                }
//                finally
//                {
//                    stream.Dispose();
//                }
//            }

//            public void SetStream(StringUdi udi, Stream stream)
//            {
//                throw new NotImplementedException();
//            }

//            public Task SetStreamAsync(StringUdi udi, Stream stream, CancellationToken token)
//            {
//                SetStream(udi, stream);
//                return Async.CompletedTask;
//            }

//            public bool CanSetPhysical => false;

//            public void Set(StringUdi udi, string path, bool copy = false)
//            {
//                throw new NotSupportedException();
//            }

//            public string GetPhysicalPath(StringUdi udi)
//            {
//                throw new NotImplementedException();
//            }

//            public string GetVirtualPath(StringUdi udi)
//            {
//                throw new NotImplementedException();
//            }
//        }
//    }
//}

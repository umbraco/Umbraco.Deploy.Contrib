//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using Umbraco.Core;
//using Umbraco.Core.Deploy;

//namespace Umbraco.Deploy.Contrib.Tests.TestHelpers
//{
//    internal class MemoryFileTypeCollection : IFileTypeCollection
//    {
//        private readonly IFileType _fileType;

//        public MemoryFileTypeCollection()
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
//                string text;
//                if (!_files.TryGetValue(udi, out text)) return null;
//                return new MemoryStream(Encoding.UTF8.GetBytes(text));
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

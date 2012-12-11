using System;
using System.IO;
using System.Text;
using System.Web;
using System.Xml.XPath;
using VersionOne.Web.Plugins.Composition;

namespace VersionOne.Web.Plugins.Api
{
    public class ApiTranslatorFiltersModule : IHttpModule
    {
        public void Dispose()
        {
        }

        public void Init(HttpApplication context)
        {
            context.BeginRequest += context_BeginRequest;
        }

        void context_BeginRequest(object sender, EventArgs e)
        {
            HttpContext.Current.Request.Filter =
                new ApiInputTranslatorFilter(HttpContext.Current.Request.Filter);
        }
    }

    public class ApiInputTranslatorFilter : Stream
    {
        private Stream _sink;

        public ApiInputTranslatorFilter(Stream sink)
        {
            _sink = sink;
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override long Length
        {
            get { return _newBuffer.Length; }
        }

        public override long Position
        {
            get { return _sink.Position; }
            set { throw new NotSupportedException(); }
        }

        private byte[] _newBuffer = new byte[] { };

        /*
         * In the case where the input stream total is less than 8092,
         * then I think this will work fine.
         * 
         * If it's greater, we might be able to read the entire InputStream
         * into a MemoryStream, process it in one swoop, but then
         * chunk out the translated result in multiple reads???
         * 
         * 
         */

        private ITranslateApiInputToAssetXml GetTranslatorForContentType(string contentType)
        {
            var path = HttpContext.Current.Server.MapPath("bin\\Plugins");
            var inputStreamTranslators = new PartsList<ITranslateApiInputToAssetXml>(path);

            foreach (var translator in inputStreamTranslators.Items)
            {
                if (translator.CanTranslate(contentType))
                {
                    return translator;
                }
            }
            return null;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var bytesRead = _sink.Read(buffer, offset, count);
            if (bytesRead == 0)
            {
                return 0;
            }

            var context = HttpContext.Current;
            var contentType = context.Request.Headers["Content-Type"];

            var translator = GetTranslatorForContentType(contentType);
            if (translator != null)
            {
                var content = Encoding.UTF8.GetString(buffer, offset, bytesRead);

                var xml = translator.Execute(content);

                var translatedContent = xml.CreateNavigator().OuterXml;

                _newBuffer = Encoding.UTF8.GetBytes(translatedContent);
                var newBufferByteCountLength = Encoding.UTF8.GetByteCount(translatedContent);

                Encoding.UTF8.GetBytes(translatedContent,
                                       0, newBufferByteCountLength, buffer, 0);

                //if (newBufferByteCountLength > bytesRead)
                //{
                //    _sink.Position = newBufferByteCountLength;
                //}

                return newBufferByteCountLength;
            }
            else
            {
                return bytesRead;
            }
        }

        public override long Seek(long offset, System.IO.SeekOrigin direction)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long length)
        {
            throw new NotSupportedException();
        }

        public override void Close()
        {
            _sink.Close();
        }

        public override void Flush()
        {
            _sink.Flush();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }
}

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
        private readonly Stream _sink;

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
            get { return _sink.Length; }
        }

        public override long Position
        {
            get { return _sink.Position; }
            set { throw new NotSupportedException(); }
        }

        private bool _hasAttemptedReadAndTranslation;
        private string _translatedContent;
        private int _nextOffsetInTranslatedContentToReadFrom = 0;

        public override int Read(byte[] buffer, int offset, int byteCountToRead)
        {
            if (!_hasAttemptedReadAndTranslation)
            {
                var translator = GetInputTranslatorByContentType();
                if (translator == null)
                {
                    return _sink.Read(buffer, offset, byteCountToRead);
                }
                var originalContent = string.Empty;
                using (var streamReader = new StreamReader(_sink))
                {
                    originalContent = streamReader.ReadToEnd();
                }

                // TODO: should just return string instead of XML doc.
                // Makes it lighter weight on the plugin side, since 
                // it will likely just use a StringBuilder to create the XML
                // anyway.
                var xml = translator.Execute(originalContent);
                _translatedContent = xml.CreateNavigator().OuterXml;

                var translatedContentLength = _translatedContent.Length;
                if (translatedContentLength < byteCountToRead)
                {
                    byteCountToRead = translatedContentLength;
                }
                Encoding.UTF8.GetBytes(_translatedContent, 0, byteCountToRead, buffer, 0);

                _nextOffsetInTranslatedContentToReadFrom = byteCountToRead;

                return byteCountToRead;
            }
            else
            { // Coming back for more data...
                if (string.IsNullOrWhiteSpace(_translatedContent))
                {
                    return _sink.Read(buffer, offset, byteCountToRead);
                }
                else
                {
                    var lenDiff = (_translatedContent.Length - byteCountToRead -
                                   _nextOffsetInTranslatedContentToReadFrom);
                    if (lenDiff < 0)
                    {
                        byteCountToRead = _translatedContent.Length - _nextOffsetInTranslatedContentToReadFrom;
                    }

                    if (byteCountToRead == 0)
                    {
                        return 0;
                    }

                    var segment = _translatedContent.Substring(_nextOffsetInTranslatedContentToReadFrom,
                                                               byteCountToRead);

                    Encoding.UTF8.GetBytes(segment, 0, byteCountToRead, buffer, 0);

                    _nextOffsetInTranslatedContentToReadFrom += byteCountToRead;

                    return byteCountToRead;
                }
            }
        }

        private ITranslateApiInputToAssetXml GetInputTranslatorByContentType()
        {
            _hasAttemptedReadAndTranslation = true;

            var context = HttpContext.Current;
            var contentType = context.Request.Headers["Content-Type"];

            return ApiTranslatorPluginsFactory.GetPluginForContentType
                <ITranslateApiInputToAssetXml>(contentType);
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

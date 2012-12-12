using System;
using System.IO;
using System.Text;
using System.Web;

namespace VersionOne.Web.Plugins.Api
{
    public class ApiInputTranslatorFilter : Stream
    {
        private readonly Stream _sink;

        public ApiInputTranslatorFilter(Stream sink)
        {
            _sink = sink;
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
                    return ReadFromWrappedStream(buffer, offset, byteCountToRead);
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
                _translatedContent = translator.Execute(originalContent);

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
                    return ReadFromWrappedStream(buffer, offset, byteCountToRead);
                }
                else
                {
                    byteCountToRead = CalculateByteCountToReadFromTranslatedContent(byteCountToRead);

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

        private int ReadFromWrappedStream(byte[] buffer, int offset, int byteCountToRead)
        {
            return _sink.Read(buffer, offset, byteCountToRead);
        }

        private ITranslateApiInputToAssetXml GetInputTranslatorByContentType()
        {
            _hasAttemptedReadAndTranslation = true;

            var context = HttpContext.Current;
            var contentType = context.Request.Headers["Content-Type"];

            return ApiTranslatorPluginsFactory.GetPluginForContentType
                <ITranslateApiInputToAssetXml>(contentType);
        }

        private int CalculateByteCountToReadFromTranslatedContent(int byteCountToRead)
        {
            var lenDiff = (_translatedContent.Length - byteCountToRead -
                           _nextOffsetInTranslatedContentToReadFrom);
            if (lenDiff < 0)
            {
                byteCountToRead = _translatedContent.Length - _nextOffsetInTranslatedContentToReadFrom;
            }
            return byteCountToRead;
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

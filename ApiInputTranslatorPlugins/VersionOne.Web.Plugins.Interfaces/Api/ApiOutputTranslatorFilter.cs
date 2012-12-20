using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;

namespace VersionOne.Web.Plugins.Api
{
    /// <summary>
    /// TODO: tighten up the Write method on this...
    /// </summary>
    public class ApiOutputTranslatorFilter : Stream
    {
        private readonly Stream _sink;
        private long _position;
        private readonly StringBuilder _originalOutputBuffer = new StringBuilder();

        public ApiOutputTranslatorFilter(Stream sink)
        {
            _sink = sink;
        }

        public override void Write(byte[] buffer, int offset, int byteCountToWrite)
        {
            var translator = GetOutputTranslatorByContentType();
            if (translator == null)
            {
                WriteToWrappedStream(buffer, offset, byteCountToWrite);
                return;
            }

            var originaOutputChunk = UTF8Encoding.UTF8.
                GetString(buffer, offset, byteCountToWrite);

            _originalOutputBuffer.Append(originaOutputChunk);

            //for (var i = 0; i < byteCountToWrite; i++)
            //{
            //    HttpResponse.Out[i] = (byte)'\0';

            //    var bytes = UTF8Encoding.UTF8.GetBytes(translatedContent);
            //    _sink.Write(bytes, 0, byteCountToWrite);
            //}
        }

        private bool _hasAttemptedToGetTranslator;

        private ITranslateAssetXmlOutputToContentType _translator;

        private ITranslateAssetXmlOutputToContentType GetOutputTranslatorByContentType()
        {
            if (_translator != null)
            {
                return _translator;
            }

            if (!_hasAttemptedToGetTranslator)
            {
                var acceptTypes = new List<string>(HttpContext.Current.Request.AcceptTypes);

                var queryAcceptType = HttpContext.Current.Request.QueryString["AcceptFormat"];
                if (!string.IsNullOrWhiteSpace(queryAcceptType))
                {
                    acceptTypes.Add(queryAcceptType);
                    HttpContext.Current.Response.ContentType = queryAcceptType;
                }

                _hasAttemptedToGetTranslator = true;
                _translator = ApiTranslatorPluginsFactory.GetPluginForAcceptTypes
                    <ITranslateAssetXmlOutputToContentType>(acceptTypes);
                return _translator;
            }

            return null;
        }

        private void WriteToWrappedStream(byte[] buffer, int offset, int byteCountToRead)
        {
            _sink.Write(buffer, offset, byteCountToRead);
        }

        #region Secondary Overrides

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override void Close()
        {
            if (_translator != null)
            {
                var originalOutputXml = _originalOutputBuffer.ToString();

                var translatedContent = _translator.Execute(originalOutputXml);
                var byteCountToWrite = translatedContent.Length;

                var bytes = UTF8Encoding.UTF8.GetBytes(translatedContent);
                _sink.Write(bytes, 0, byteCountToWrite);
                _sink.Close();
            }
            else
            {
                _sink.Close();
            }
        }

        public override void Flush()
        {
            _sink.Flush();
        }

        public override long Length
        {
            get { return 0; }
        }

        public override long Position
        {
            get { return _position; }
            set { _position = value; }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _sink.Seek(offset, origin);
        }

        public override void SetLength(long length)
        {
            _sink.SetLength(length);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _sink.Read(buffer, offset, count);
        }

        #endregion
    }
}
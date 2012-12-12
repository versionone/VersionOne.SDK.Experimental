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

            var originalContent = System.Text.UTF8Encoding.UTF8.
                GetString(buffer, offset, byteCountToWrite);

            var translatedContent = translator.Execute(originalContent);

            for (var i = 0; i < byteCountToWrite; i++)
            {
                buffer[i] = (byte)'\0';
            }

            byteCountToWrite = translatedContent.Length;

            var bytesToWrite = UTF8Encoding.UTF8.GetBytes(translatedContent);
            _sink.Write(bytesToWrite, 0, byteCountToWrite);
        }

        private ITranslateAssetXmlOutputToContentType GetOutputTranslatorByContentType()
        {
            var acceptTypes = new List<string>(HttpContext.Current.Request.AcceptTypes);

            var queryAcceptType = HttpContext.Current.Request.QueryString["AcceptFormat"];
            if (!string.IsNullOrWhiteSpace(queryAcceptType))
            {
                acceptTypes.Add(queryAcceptType);
                HttpContext.Current.Response.ContentType = queryAcceptType;
            }

            return ApiTranslatorPluginsFactory.GetPluginForAcceptTypes
                <ITranslateAssetXmlOutputToContentType>(acceptTypes);
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
            _sink.Close();
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
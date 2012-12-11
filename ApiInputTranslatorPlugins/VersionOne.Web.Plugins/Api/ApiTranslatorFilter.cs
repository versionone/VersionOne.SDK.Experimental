﻿using System;
using System.IO;
using System.Text;
using System.Web;

namespace VersionOne.Web.Plugins.Api
{
    public class ApiTranslatorFiltersModule : IHttpModule
    {
        public void Dispose()
        {
        }

        public void Init(HttpApplication context)
        {
            context.BeginRequest += new EventHandler(context_BeginRequest);
        }

        void context_BeginRequest(object sender, EventArgs e)
        {
            HttpContext.Current.Request.Filter = new ApiInputTranslatorFilter(HttpContext.Current.Request.Filter);
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

        public override int Read(byte[] buffer, int offset, int count)
        {
            var context = HttpContext.Current;
            var accept = context.Request.Headers["Accept"];

            if (accept.Equals("upper"))
            {
                var bytesRead = _sink.Read(buffer, offset, count);

                if (bytesRead == 0)
                {
                    return 0;
                }

                var newContent =
@"<Asset>
	<Attribute name=""Phone"" act=""set"">777-666-888-99</Attribute>
</Asset>";

                _newBuffer = Encoding.UTF8.GetBytes(newContent);
                var newBufferByteCountLength = Encoding.UTF8.GetByteCount(newContent);

                Encoding.UTF8.GetBytes(newContent,
                                       0, Encoding.UTF8.GetByteCount(newContent), buffer, 0);

                return newBufferByteCountLength;
            }
            else
            {
                return _sink.Read(buffer, offset, count);
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

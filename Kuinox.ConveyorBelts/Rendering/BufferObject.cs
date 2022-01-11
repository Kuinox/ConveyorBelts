using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuinox.ConveyorBelts.Rendering
{
    public class BufferObject<TDataType> : IDisposable
        where TDataType : unmanaged
    {
        readonly uint _handle;
        readonly BufferTargetARB _bufferType;
        readonly GL _gl;

        public unsafe BufferObject( GL gl, Span<TDataType> data, BufferTargetARB bufferType )
        {
            _gl = gl;
            _bufferType = bufferType;

            _handle = _gl.GenBuffer();
            Bind();
            fixed( void* d = data )
            {
                _gl.BufferData( bufferType, (nuint)(data.Length * sizeof( TDataType )), d, BufferUsageARB.StaticDraw );
            }
        }

        public void Bind() => _gl.BindBuffer( _bufferType, _handle );

        public void Dispose() => _gl.DeleteBuffer( _handle );
    }
}

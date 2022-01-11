using Kuinox.ConveyorBelts.Rendering;
using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuinox.ConveyorBelts.Map
{
    public class Chunk : IDisposable
    {
        readonly Cell[] _cells = new Cell[2048];

        readonly BufferObject<float> _bo;
        readonly BufferObject<uint> _ebo;
        public Chunk(GL gl)
        {
            _bo = new( gl, new float[]
            {
                 0.0f,  0.0f,  0.0f,  0.0f, 1.0f,
                 0.5f,  0.5f,  0.0f,  1.0f, 1.0f,
                 1.0f,  0.25f, 0.0f,  1.0f, 0.0f
            }, BufferTargetARB.ArrayBuffer );
            _ebo = new( gl, new uint[]
            {

            }, BufferTargetARB.ElementArrayBuffer );
            OnCellUpdate( gl );
        }
        public VertexArrayObject<float, uint> VAO { get; private set; }

        public void Dispose()
        {
            VAO.Dispose();
            _ebo.Dispose();
            _bo.Dispose();
        }

        void OnCellUpdate(GL gl)
        {

            VAO = new VertexArrayObject<float, uint>(gl,_bo, _ebo  );
        }

    }
}

using Kuinox.ConveyorBelts.Map;
using Kuinox.ConveyorBelts.Rendering;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Kuinox.ConveyorBelts.Rendering;

public class Program
{
    static IWindow window;
    static GL Gl;
    static IKeyboard primaryKeyboard;

    const int Width = 800;
    const int Height = 700;

    static Texture Texture;
    static Shader Shader;
    static Chunk _chunk;
    //Setup the camera's location, directions, and movement speed
    static Vector3 CameraPosition = new Vector3( 0.0f, 0.0f, 3.0f );
    static Vector3 CameraFront = new Vector3( 0.0f, 0.0f, -1.0f );
    static Vector3 CameraUp = new Vector3( 0.0f, 1.0f, 0.0f );
    static Vector3 CameraDirection = Vector3.Zero;
    static float CameraYaw = -90f;
    static float CameraPitch = 0f;
    static float CameraZoom = 45f;

    static readonly float[] Vertices =
    {
            //X    Y      Z     U   V
            -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
             0.5f, -0.5f, -0.5f,  1.0f, 1.0f,
             0.5f,  0.5f, -0.5f,  1.0f, 0.0f,
             0.5f,  0.5f, -0.5f,  1.0f, 0.0f,
            -0.5f,  0.5f, -0.5f,  0.0f, 0.0f,
            -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,

            -0.5f, -0.5f,  0.5f,  0.0f, 1.0f,
             0.5f, -0.5f,  0.5f,  1.0f, 1.0f,
             0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
             0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
            -0.5f,  0.5f,  0.5f,  0.0f, 0.0f,
            -0.5f, -0.5f,  0.5f,  0.0f, 1.0f,

            -0.5f,  0.5f,  0.5f,  1.0f, 1.0f,
            -0.5f,  0.5f, -0.5f,  1.0f, 0.0f,
            -0.5f, -0.5f, -0.5f,  0.0f, 0.0f,
            -0.5f, -0.5f, -0.5f,  0.0f, 0.0f,
            -0.5f, -0.5f,  0.5f,  0.0f, 1.0f,
            -0.5f,  0.5f,  0.5f,  1.0f, 1.0f,

             0.5f,  0.5f,  0.5f,  1.0f, 1.0f,
             0.5f,  0.5f, -0.5f,  1.0f, 0.0f,
             0.5f, -0.5f, -0.5f,  0.0f, 0.0f,
             0.5f, -0.5f, -0.5f,  0.0f, 0.0f,
             0.5f, -0.5f,  0.5f,  0.0f, 1.0f,
             0.5f,  0.5f,  0.5f,  1.0f, 1.0f,

            -0.5f, -0.5f, -0.5f,  0.0f, 0.0f,
             0.5f, -0.5f, -0.5f,  1.0f, 0.0f,
             0.5f, -0.5f,  0.5f,  1.0f, 1.0f,
             0.5f, -0.5f,  0.5f,  1.0f, 1.0f,
            -0.5f, -0.5f,  0.5f,  0.0f, 1.0f,
            -0.5f, -0.5f, -0.5f,  0.0f, 0.0f,

            -0.5f,  0.5f, -0.5f,  0.0f, 0.0f,
             0.5f,  0.5f, -0.5f,  1.0f, 0.0f,
             0.5f,  0.5f,  0.5f,  1.0f, 1.0f,
             0.5f,  0.5f,  0.5f,  1.0f, 1.0f,
            -0.5f,  0.5f,  0.5f,  0.0f, 1.0f,
            -0.5f,  0.5f, -0.5f,  0.0f, 0.0f
        };

    static readonly uint[] Indices =
    {};

    static void Main( string[] args )
    {
        var options = WindowOptions.Default;
        options.Size = new Vector2D<int>( 800, 600 );
        options.Title = "LearnOpenGL with Silk.NET";
        window = Window.Create( options );

        window.Load += OnLoad;
        window.Update += OnUpdate;
        window.Render += OnRender;
        window.Closing += OnClose;

        window.Run();
    }

    static void OnLoad()
    {
        IInputContext input = window.CreateInput();
        primaryKeyboard = input.Keyboards.FirstOrDefault();
        if( primaryKeyboard != null )
        {
            primaryKeyboard.KeyDown += KeyDown;
        }
        for( int i = 0; i < input.Mice.Count; i++ )
        {
            input.Mice[i].Cursor.CursorMode = CursorMode.Raw;
            input.Mice[i].MouseMove += OnMouseMove;
            input.Mice[i].Scroll += OnMouseWheel;
        }

        Gl = GL.GetApi( window );
        _chunk = new( Gl );
        //Ebo = new BufferObject<uint>( Gl, Indices, BufferTargetARB.ElementArrayBuffer );
        //Vbo = new BufferObject<float>( Gl, Vertices, BufferTargetARB.ArrayBuffer );
        //Vao = new VertexArrayObject<float, uint>( Gl, Vbo, Ebo );

        Shader = new Shader( Gl, "Rendering/shader.vert", "Rendering/shader.frag" );

        Texture = new Texture( Gl, "Rendering/silk.png" );
    }

    static unsafe void OnUpdate( double deltaTime )
    {
        float moveSpeed = 2.5f * (float)deltaTime;

        if( primaryKeyboard.IsKeyPressed( Key.W ) )
        {
            //Move up
            CameraPosition += moveSpeed * CameraUp;
        }
        if( primaryKeyboard.IsKeyPressed( Key.S ) )
        {
            //Move down'
            CameraPosition += moveSpeed * -CameraUp;
        }
        if( primaryKeyboard.IsKeyPressed( Key.A ) )
        {
            //Move left
            CameraPosition -= Vector3.Normalize( Vector3.Cross( CameraFront, CameraUp ) ) * moveSpeed;
        }
        if( primaryKeyboard.IsKeyPressed( Key.D ) )
        {
            //Move right
            CameraPosition += Vector3.Normalize( Vector3.Cross( CameraFront, CameraUp ) ) * moveSpeed;
        }
    }

    static unsafe void OnRender( double deltaTime )
    {
        Gl.Enable( EnableCap.DepthTest );
        Gl.Clear( (uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit) );

        _chunk.VAO.Bind();
        Texture.Bind();
        Shader.Use();
        Shader.SetUniform( "uTexture0", 0 );

        //Use elapsed time to convert to radians to allow our cube to rotate over time
        float difference = (float)(window.Time * 100);

        var model = Matrix4x4.CreateRotationY( MathHelper.DegreesToRadians( difference ) ) * Matrix4x4.CreateRotationX( MathHelper.DegreesToRadians( difference ) );
        var view = Matrix4x4.CreateLookAt( CameraPosition, CameraPosition + CameraFront, CameraUp );
        var projection = Matrix4x4.CreatePerspectiveFieldOfView( MathHelper.DegreesToRadians( CameraZoom ), Width / Height, 0.1f, 100.0f );

        Shader.SetUniform( "uModel", model );
        Shader.SetUniform( "uView", view );
        Shader.SetUniform( "uProjection", projection );

        //We're drawing with just vertices and no indices, and it takes 36 vertices to have a six-sided textured cube
        Gl.DrawArrays( PrimitiveType.Triangles, 0, 36 );
    }

    static unsafe void OnMouseMove( IMouse mouse, Vector2 position )
    {

    }

    static unsafe void OnMouseWheel( IMouse mouse, ScrollWheel scrollWheel )
    {
        //We don't want to be able to zoom in too close or too far away so clamp to these values
        CameraZoom = Math.Clamp( CameraZoom - scrollWheel.Y, 1.0f, 45f );
    }

    static void OnClose()
    {
        
        Shader.Dispose();
        Texture.Dispose();
    }

    static void KeyDown( IKeyboard keyboard, Key key, int arg3 )
    {
        if( key == Key.Escape )
        {
            window.Close();
        }
    }
}

using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Ziewaar.RAD.Doodads.Stage;
public class VboModel<TData>(TData[] worldData, Continuity worldAllocations) where TData : struct
{
    private readonly List<Matrix4> Transformations = new();
}
public class QuiltSegment
{
    public Vector2 P, Q, R, S;
    public SortedSet<int> UserIndices = new();
}
public interface IQuiltSegmentAdaptor
{
    Vector2 P { set; }
    Vector2 Q { set; }
    Vector2 R { set; }
    Vector2 S { set; }
}
public class InterruptedVBO<TData> : IDisposable where TData : struct, IVboAligned, IQuiltSegmentAdaptor
{
    private readonly TData[] Data;
    private readonly Continuity AllocationContinuity;
    private readonly Continuity ChangedContinuity;
    
    private readonly VboModel<TData> Model;
    private readonly SortedList<string, QuiltSegment> ActiveSegments = new();
    private readonly int VboId;
    private readonly Image<Rgba32> WorkingQuilt;
    private readonly byte[] QuiltBuffer;
    private readonly int TextureId;
    public InterruptedVBO(int allocation, int textureSize)
    {
        this.Data = new TData[allocation];
        AllocationContinuity = new Continuity(allocation);
        ChangedContinuity = new Continuity(allocation);
        this.VboId = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, this.VboId);
        GL.BufferData(
            BufferTarget.ArrayBuffer,
            this.Data.Length * Marshal.SizeOf<TLQuad3>(),
            this.Data, BufferUsageHint.StaticDraw);

        this.WorkingQuilt = new(textureSize, textureSize,
            new Rgba32(byte.MaxValue, byte.MinValue, byte.MaxValue, byte.MaxValue));
        this.WorkingQuilt.CopyPixelDataTo(QuiltBuffer);
        this.TextureId = GL.GenTexture();

        GL.BindTexture(TextureTarget.Texture2D, this.TextureId);
        GL.TexImage2D(
            TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, textureSize, textureSize,
            0, PixelFormat.Rgba, PixelType.UnsignedByte, this.QuiltBuffer);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

        this.Model = new VboModel<TData>(this.Data, this.AllocationContinuity);
    }
    public void ApplyImage(int index, string path)
    {
        if (ActiveSegments.TryGetValue(path, out var segment))
        {
            segment.UserIndices.Add(index);
            (Data[index].P, Data[index].Q, Data[index].R, Data[index].S) = (segment.P, segment.Q, segment.R, segment.S);
            ChangedContinuity.Include(index);
        }
        else
        {
            var newSeg = new QuiltSegment();
            newSeg.UserIndices.Add(index);
            ActiveSegments.Add(path, newSeg);

            var gridSize = Math.Sqrt(ActiveSegments.Count);
            var rows = Math.Floor(gridSize);
            var columns = Math.Ceiling(gridSize);
            if (rows * columns < ActiveSegments.Count)
                rows += 1;
            if (rows * columns < ActiveSegments.Count)
                columns += 1;

            float uvWidth = (float)(1 / columns);
            float uvHeight = (float)(1 / rows);
            int bmpWidth = Math.Max(1, (int)(WorkingQuilt.Width / columns));
            int bmpHeight = Math.Max(1, (int)(WorkingQuilt.Height / rows));

            var segQueue = new Queue<KeyValuePair<string, QuiltSegment>>(this.ActiveSegments);

            for (int uPos = 0; uPos < columns && segQueue.Count > 0; uPos++)
            {
                for (int vPos = 0; vPos < rows && segQueue.Count > 0; vPos++)
                {
                    var currentSegment = segQueue.Dequeue();

                    using var image = Image<Rgba32>.Load<Rgba32>(currentSegment.Key);
                    image.Mutate(x => x.Resize(bmpWidth, bmpHeight, KnownResamplers.Bicubic));
                    this.WorkingQuilt.Mutate(x => x.DrawImage(image, new Point(
                        uPos * bmpWidth, vPos * bmpHeight), PixelColorBlendingMode.Normal, 1));

                    var tl = new Vector2(uPos * uvWidth, vPos * uvHeight);
                    var br = new Vector2(uPos * uvWidth + uvWidth, vPos * uvHeight + uvHeight);

                    (currentSegment.Value.P, currentSegment.Value.Q,
                        currentSegment.Value.R, currentSegment.Value.S) = (
                        new Vector2(tl.X, br.Y), br, new(br.X, tl.Y), tl);

                    foreach (var valueUserIndex in currentSegment.Value.UserIndices)
                    {
                        ChangedContinuity.Include(valueUserIndex);
                        (Data[index].P, Data[index].Q, Data[index].R, Data[index].S) =
                            (currentSegment.Value.P, currentSegment.Value.Q,
                                currentSegment.Value.R, currentSegment.Value.S);
                    }
                }
            }
        }
    }
    public InterruptedVBO<TData> Scope()
    {
        GL.BindTexture(TextureTarget.Texture2D, TextureId);
        GL.BindBuffer(BufferTarget.ArrayBuffer, VboId);
        TData.Accomodate();
        return this;
    }
    public void Draw()
    {
        for (var i = this.AllocationContinuity; i != null; i = i.Next)
            GL.DrawArrays(PrimitiveType.Quads, (int)i.Start * 4, (int)(i.End - i.Start) * 4);
    }
    public void Dispose()
    {
        this.WorkingQuilt.Dispose();
        GL.DeleteBuffer(VboId);
        GL.DeleteTexture(TextureId);
    }
}
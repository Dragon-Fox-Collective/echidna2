using Echidna2.Core;
using Echidna2.Rendering;
using OpenTK.Graphics.OpenGL4;

namespace Echidna2.Rendering3D;

public class SkyboxRenderer : INotificationListener<IDraw.Notification>
{
	public Mesh Mesh { get; set; } = Mesh.Cube;
	public Shader Shader { get; set; } = Shader.Skybox;
	public CubeMap CubeMap { get; set; } = CubeMap.Skybox;
	
	public void OnNotify(IDraw.Notification notification)
	{
		GL.Disable(EnableCap.CullFace);
		GL.DepthFunc(DepthFunction.Lequal);
		
		Shader.Bind(notification.Camera.ViewMatrix, notification.Camera.ProjectionMatrix);
		CubeMap.Bind();
		Mesh.Draw();
		
		GL.DepthFunc(DepthFunction.Less);
	}
}
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.XR;

public class VFXCameraBinder_XR : MonoBehaviour
{
	[Header("Camera used for XR rendering")]
	public Camera xrCamera;

	[Header("Target Visual Effect Graph")]
	public VisualEffect vfx;

	[Header("Name of the exposed Matrix4x4 property in the VFX Graph")]
	public string viewProjProperty = "ViewProj";

	void LateUpdate()
	{
		if (xrCamera == null || vfx == null)
			return;

		// Retrieve the correct projection matrix depending on XR state
		Matrix4x4 projection = XRSettings.enabled
			? xrCamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left)
			: xrCamera.projectionMatrix;

		// Retrieve the view matrix (world-to-camera)
		Matrix4x4 view = XRSettings.enabled
			? xrCamera.GetStereoViewMatrix(Camera.StereoscopicEye.Left)
			: xrCamera.worldToCameraMatrix;

		// Compute the ViewProjection matrix
		Matrix4x4 viewProj = projection * view;

		// Push the matrix to the VFX Graph
		vfx.SetMatrix4x4(viewProjProperty, viewProj);
	}
}

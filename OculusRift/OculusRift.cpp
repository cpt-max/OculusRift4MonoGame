
#define OCULUSRIFT_API __declspec(dllexport)

#include "LibOVR/Include/OVR_CAPI_D3D.h"
#include "LibOVR/Include/Extras/OVR_Math.h"
#include "d3d11.h"
#include "vector"

#pragma comment(lib, "LibOVR/Lib/LibOVR.lib")

using namespace OVR;


struct HmdInfo
{
	int Type;
	int VendorId;
	int ProductId;
	int FirmwareMajor;
	int FirmwareMinor;
	ovrSizei DisplayResolution;
	float DisplayRefreshRate;
	ovrFovPort DefaultFovLeft;
	ovrFovPort DefaultFovRight;
	ovrFovPort MaxFovLeft;
	ovrFovPort MaxFovRight;
	unsigned int AvailableHmdCaps;
	unsigned int DefaultHmdCaps;
	unsigned int AvailableTrackingCap;
	unsigned int DefaultTrackingCaps;
};
 
struct HeadTracking
{
	unsigned int StatusFlags;
	ovrMatrix4f HeadPose;
	ovrMatrix4f EyePoseLeft;
	ovrMatrix4f EyePoseRight;
};


ID3D11Device* dxDevice;
ID3D11DeviceContext* dxContext;

ovrSession session;
ovrGraphicsLuid luid;
ovrHmdDesc hmdDesc;

ovrTextureSwapChain texSwapChain[2];
std::vector<ID3D11RenderTargetView*> renderTargetViews[2];

ovrEyeRenderDesc eyeRenderDesc[2];
ovrVector3f hmdToEyeOffet[2];

ovrLayerEyeFov layer;

ovrResult result;


extern "C"
{
	ID3D11RenderTargetView* GetActiveRenderTargetView(int eye)
	{
		int index = 0;
		ovr_GetTextureSwapChainCurrentIndex(session, texSwapChain[eye], &index);
		return renderTargetViews[eye][index];
	}

	OCULUSRIFT_API ovrResult Init(ID3D11Device* device, ID3D11DeviceContext* context)
	{
		dxDevice = device;
		dxContext = context;

		result = ovr_Initialize(nullptr);
		if (OVR_FAILURE(result))
			return result;
	
		
		result = ovr_Create(&session, &luid);
		if (OVR_FAILURE(result))
		{
			ovr_Shutdown();
			return result;
		}

		hmdDesc = ovr_GetHmdDesc(session);

		return result;
	}

	OCULUSRIFT_API HmdInfo GetHmdInfo()
	{
		HmdInfo inf;

		inf.Type = hmdDesc.Type;
		inf.VendorId = hmdDesc.VendorId;
		inf.ProductId = hmdDesc.ProductId;
		inf.FirmwareMajor = hmdDesc.FirmwareMajor;
		inf.FirmwareMinor = hmdDesc.FirmwareMinor;

		inf.DisplayRefreshRate = hmdDesc.DisplayRefreshRate;
		inf.DisplayResolution = hmdDesc.Resolution;

		inf.DefaultFovLeft = hmdDesc.DefaultEyeFov[0];
		inf.DefaultFovRight = hmdDesc.DefaultEyeFov[1];
		inf.MaxFovLeft = hmdDesc.MaxEyeFov[0];
		inf.MaxFovRight = hmdDesc.MaxEyeFov[1];

		inf.AvailableHmdCaps = hmdDesc.AvailableHmdCaps;
		inf.DefaultHmdCaps = hmdDesc.DefaultHmdCaps;
		inf.AvailableTrackingCap = hmdDesc.AvailableTrackingCaps;
		inf.DefaultTrackingCaps = hmdDesc.DefaultTrackingCaps;

		return inf;
	}

	OCULUSRIFT_API void GetRecommendedRenderTargetRes(
		ovrFovPort fovLeft, ovrFovPort fovRight, 
		float pixelsPerDisplayPixel,
		ovrSizei& texResLeft, ovrSizei& texResRight)
	{
		texResLeft = ovr_GetFovTextureSize(session, ovrEye_Left, fovLeft, pixelsPerDisplayPixel);
		texResRight = ovr_GetFovTextureSize(session, ovrEye_Right, fovRight, pixelsPerDisplayPixel);
	}

	OCULUSRIFT_API ovrResult CreateDXSwapChains(
		ovrSizei texResLeft, ovrSizei texResRight,
		ovrFovPort fovLeft, ovrFovPort fovRight)
	{
		ovrTextureSwapChainDesc desc = {};
		desc.Type = ovrTexture_2D;
		desc.Format = OVR_FORMAT_R8G8B8A8_UNORM_SRGB; 
		//desc.Format = OVR_FORMAT_R8G8B8A8_UNORM;
		desc.ArraySize = 1;
		desc.MipLevels = 1;
		desc.SampleCount = 1;
		desc.StaticImage = ovrFalse;
		desc.MiscFlags = ovrTextureMisc_None;
		desc.BindFlags = ovrTextureBind_DX_RenderTarget;

		for (int eye = 0; eye < 2; eye++)
		{
			desc.Width = eye == 0 ? texResLeft.w : texResRight.w;
			desc.Height = eye == 0 ? texResLeft.h : texResRight.h;

			auto& fov = eye == 0 ? fovLeft : fovRight;
			auto& swapChain = texSwapChain[eye];	
			auto& rtv = renderTargetViews[eye];

			result = ovr_CreateTextureSwapChainDX(session, dxDevice, &desc, &swapChain);
			if (OVR_FAILURE(result))
				return result;

			int count = 0;
			result = ovr_GetTextureSwapChainLength(session, swapChain, &count);
			if (OVR_FAILURE(result))
				return result;
	
			rtv.resize(count);

			for (int i = 0; i < count; ++i)
			{
				ID3D11Texture2D* texture = nullptr;
				result = ovr_GetTextureSwapChainBufferDX(session, swapChain, i, IID_PPV_ARGS(&texture));
				if (OVR_FAILURE(result))
					return result;

				dxDevice->CreateRenderTargetView(texture, nullptr, &rtv[i]);
				texture->Release();
			}
	
			eyeRenderDesc[eye] = ovr_GetRenderDesc(session, (ovrEyeType)eye, fov);

			hmdToEyeOffet[eye] = eyeRenderDesc[eye].HmdToEyeOffset;

			layer.Header.Type = ovrLayerType_EyeFov;
			layer.Header.Flags = 0;
			layer.ColorTexture[eye] = swapChain;
			layer.Fov[eye] = eyeRenderDesc[eye].Fov;
			layer.Viewport[eye] = OVR::Recti(0, 0, desc.Width, desc.Height);
		}

		return ovrSuccess;
	}

	OCULUSRIFT_API ovrMatrix4f GetProjectionMatrix(int eye, float nearClip, float farClip, unsigned int projectionModeFlags)
	{
		return ovrMatrix4f_Projection(eyeRenderDesc[eye].Fov, nearClip, farClip, projectionModeFlags);
	}

	OCULUSRIFT_API HeadTracking TrackHead(int frame)
	{ 
		HeadTracking tracking;
		memset(&tracking, 0, sizeof(HeadTracking));

		ovrTrackingState ts = ovr_GetTrackingState(session, ovr_GetPredictedDisplayTime(session, frame), ovrTrue);

		tracking.StatusFlags = ts.StatusFlags;
		 
		if (ts.StatusFlags & (ovrStatus_OrientationTracked | ovrStatus_PositionTracked))
		{
			tracking.HeadPose = Matrix4f(ts.HeadPose.ThePose);

			for (int eye = 0; eye < 2; eye++)
			{
				double sensorSampleTime;
				ovrPosef eyePoses[2];
				ovr_GetEyePoses(session, frame, ovrTrue, hmdToEyeOffet, eyePoses, &sensorSampleTime);
				//ovr_CalcEyePoses(ts.HeadPose.ThePose, hmdToEyeOffet, eyePoses);

				if (eye == 0)
					tracking.EyePoseLeft = Matrix4f(eyePoses[0]);
				else
					tracking.EyePoseRight = Matrix4f(eyePoses[1]);

				//layer.SensorSampleTime = sensorSampleTime;
				layer.RenderPose[0] = eyePoses[0];
				layer.RenderPose[1] = eyePoses[1];
				
			}
		}

		return tracking;
	}

	OCULUSRIFT_API ovrResult SubmitRenderTargets(ID3D11Texture2D* texLeft, ID3D11Texture2D* texRight, int frame)
	{
		for (int eye = 0; eye < 2; eye++)
		{
			ID3D11Resource* res;
			GetActiveRenderTargetView(eye)->GetResource(&res);
			dxContext->CopyResource(res, eye == 0 ? texLeft : texRight);
			ovr_CommitTextureSwapChain(session, texSwapChain[eye]);
		}

		// Submit frame with one layer we have.
		ovrLayerHeader* layers = &layer.Header;
		result = ovr_SubmitFrame(session, frame, nullptr, &layers, 1);
		return result;
	}

	OCULUSRIFT_API void Shutdown()
	{
		ovr_Destroy(session);
		ovr_Shutdown();
	}
}








using System;

using Foundation;
using AVFoundation;
using CoreMedia;
using Photos;

namespace AVCamSample
{
	public class PhotoCaptureDelegate : AVCapturePhotoCaptureDelegate
	{
		public AVCapturePhotoSettings RequestedPhotoSettings { get; private set; }

		Action willCapturePhotoAnimation;
		Action<bool> capturingLivePhoto;
		Action<PhotoCaptureDelegate> completed;

		NSData photoData;
		NSUrl livePhotoCompanionMovieUrl;


		public PhotoCaptureDelegate(AVCapturePhotoSettings requestedPhotoSettings,
									 Action willCapturePhotoAnimation,
									 Action<bool> capturingLivePhoto,
									 Action<PhotoCaptureDelegate> completed)
		{
			RequestedPhotoSettings = requestedPhotoSettings;
			this.willCapturePhotoAnimation = willCapturePhotoAnimation;
			this.capturingLivePhoto = capturingLivePhoto;
			this.completed = completed;
		}

		void DidFinish()
		{
			var livePhotoCompanionMoviePath = livePhotoCompanionMovieUrl?.Path;
			if (livePhotoCompanionMoviePath != null)
			{
				if (NSFileManager.DefaultManager.FileExists(livePhotoCompanionMoviePath))
				{
					NSError error;
					if (!NSFileManager.DefaultManager.Remove(livePhotoCompanionMoviePath, out error))
						Console.WriteLine($"Could not remove file at url: {livePhotoCompanionMoviePath}");
				}
			}

			completed(this);
		}

		public override void WillBeginCapture(AVCapturePhotoOutput captureOutput, AVCaptureResolvedPhotoSettings resolvedSettings)
		{
			if (resolvedSettings.LivePhotoMovieDimensions.Width > 0 && resolvedSettings.LivePhotoMovieDimensions.Height > 0)
				capturingLivePhoto(true);
		}

		public override void WillCapturePhoto(AVCapturePhotoOutput captureOutput, AVCaptureResolvedPhotoSettings resolvedSettings)
		{
			willCapturePhotoAnimation();
		}

		public override void DidFinishProcessingPhoto(AVCapturePhotoOutput captureOutput, CMSampleBuffer photoSampleBuffer, CMSampleBuffer previewPhotoSampleBuffer, AVCaptureResolvedPhotoSettings resolvedSettings, AVCaptureBracketedStillImageSettings bracketSettings, NSError error)
		{
			if (photoSampleBuffer != null)
				photoData = AVCapturePhotoOutput.GetJpegPhotoDataRepresentation(photoSampleBuffer, previewPhotoSampleBuffer);
			else
				Console.WriteLine($"Error capturing photo: {error.LocalizedDescription}");
		}

		public override void DidFinishRecordingLivePhotoMovie(AVCapturePhotoOutput captureOutput, NSUrl outputFileUrl, AVCaptureResolvedPhotoSettings resolvedSettings)
		{
			capturingLivePhoto(false);
		}

		public override void DidFinishProcessingLivePhotoMovie(AVCapturePhotoOutput captureOutput, NSUrl outputFileUrl, CMTime duration, CMTime photoDisplayTime, AVCaptureResolvedPhotoSettings resolvedSettings, NSError error)
		{
			if (error != null)
			{
				Console.WriteLine($"Error processing live photo companion movie: {error.LocalizedDescription})");
				return;
			}
			livePhotoCompanionMovieUrl = outputFileUrl;
		}

		public override void DidFinishCapture(AVCapturePhotoOutput captureOutput, AVCaptureResolvedPhotoSettings resolvedSettings, NSError error)
		{
			if (error != null)
			{
				Console.WriteLine($"Error capturing photo: {error.LocalizedDescription})");
				DidFinish();
				return;
			}

			if (photoData == null)
			{
				Console.WriteLine("No photo data resource");
				DidFinish();
				return;
			}

			PHPhotoLibrary.RequestAuthorization(status => {
				if (status == PHAuthorizationStatus.Authorized)
				{
					PHPhotoLibrary.SharedPhotoLibrary.PerformChanges(() => {
						var creationRequest = PHAssetCreationRequest.CreationRequestForAsset();
						creationRequest.AddResource(PHAssetResourceType.Photo, photoData, null);

						var url = livePhotoCompanionMovieUrl;
						if (url != null)
						{
							var livePhotoCompanionMovieFileResourceOptions = new PHAssetResourceCreationOptions
							{
								ShouldMoveFile = true
							};
							creationRequest.AddResource(PHAssetResourceType.PairedVideo, url, livePhotoCompanionMovieFileResourceOptions);
						}
					}, (success, err) => {
						if (err != null)
							Console.WriteLine($"Error occurered while saving photo to photo library: {error.LocalizedDescription}");
						DidFinish();
					});
				}
				else
				{
					DidFinish();
				}
			});
		}

	}
}

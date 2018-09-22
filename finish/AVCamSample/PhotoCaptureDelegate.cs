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

        public override void DidFinishProcessingPhoto(AVCapturePhotoOutput output, AVCapturePhoto photo, NSError error)
        {
            if (error != null)
                Console.WriteLine($"Error capturing photo: {error.LocalizedDescription}");
            else
                photoData = photo.FileDataRepresentation;
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




        // func photoOutput(_ output: AVCapturePhotoOutput, didFinishCaptureFor resolvedSettings: AVCaptureResolvedPhotoSettings, error: Error?) {
        //     if let error = error {
        //         print("Error capturing photo: \(error)")
        //         didFinish()
        //         return
        //     }

        //     guard let photoData = photoData else {
        //         print("No photo data resource")
        //         didFinish()
        //         return
        //     }

        //     PHPhotoLibrary.requestAuthorization { [unowned self] status in
        //         if status == .authorized {
        //             PHPhotoLibrary.shared().performChanges({ [unowned self] in
        //                 let options = PHAssetResourceCreationOptions()
        //                 let creationRequest = PHAssetCreationRequest.forAsset()
        //                 options.uniformTypeIdentifier = self.requestedPhotoSettings.processedFileType.map { $0.rawValue }
        //                 creationRequest.addResource(with: .photo, data: photoData, options: options)

        //                 if let livePhotoCompanionMovieURL = self.livePhotoCompanionMovieURL {
        //                     let livePhotoCompanionMovieFileResourceOptions = PHAssetResourceCreationOptions()
        //                     livePhotoCompanionMovieFileResourceOptions.shouldMoveFile = true
        //                     creationRequest.addResource(with: .pairedVideo, fileURL: livePhotoCompanionMovieURL, options: livePhotoCompanionMovieFileResourceOptions)
        //                 }

        //                 }, completionHandler: { [unowned self] _, error in
        //                     if let error = error {
        //                         print("Error occurered while saving photo to photo library: \(error)")
        //                     }

        //                     self.didFinish()
        //                 }
        //             )
        //         } else {
        //             self.didFinish()
        //         }
        //     }
        // }




        public override void DidFinishCapture(AVCapturePhotoOutput captureOutput, AVCaptureResolvedPhotoSettings resolvedSettings, NSError error)
		{
            //     if let error = error {
            //         print("Error capturing photo: \(error)")
            //         didFinish()
            //         return
            //     }
            if (error != null)
			{
				Console.WriteLine($"Error capturing photo: {error.LocalizedDescription})");
				DidFinish();
				return;
			}

            //     guard let photoData = photoData else {
            //         print("No photo data resource")
            //         didFinish()
            //         return
            //     }
            if (photoData == null)
			{
				Console.WriteLine("No photo data resource");
				DidFinish();
				return;
			}


            //     PHPhotoLibrary.requestAuthorization { [unowned self] status in
            //         if status == .authorized {
            //             PHPhotoLibrary.shared().performChanges({ [unowned self] in
            //                 let options = PHAssetResourceCreationOptions()
            //                 let creationRequest = PHAssetCreationRequest.forAsset()
            //                 options.uniformTypeIdentifier = self.requestedPhotoSettings.processedFileType.map { $0.rawValue }
            //                 creationRequest.addResource(with: .photo, data: photoData, options: options)

            //                 if let livePhotoCompanionMovieURL = self.livePhotoCompanionMovieURL {
            //                     let livePhotoCompanionMovieFileResourceOptions = PHAssetResourceCreationOptions()
            //                     livePhotoCompanionMovieFileResourceOptions.shouldMoveFile = true
            //                     creationRequest.addResource(with: .pairedVideo, fileURL: livePhotoCompanionMovieURL, options: livePhotoCompanionMovieFileResourceOptions)
            //                 }

            //                 }, completionHandler: { [unowned self] _, error in
            //                     if let error = error {
            //                         print("Error occurered while saving photo to photo library: \(error)")
            //                     }

            //                     self.didFinish()
            //                 }
            //             )
            //         } else {
            //             self.didFinish()
            //         }
            //     }

            // PHPhotoLibrary.requestAuthorization { [unowned self] status in
            PHPhotoLibrary.RequestAuthorization(status => {
                // if status == .authorized {
                if (status == PHAuthorizationStatus.Authorized)
				{
                    // PHPhotoLibrary.shared().performChanges({ [unowned self] in
                    PHPhotoLibrary.SharedPhotoLibrary.PerformChanges(() => {
                        // let options = PHAssetResourceCreationOptions()
                        var options = new PHAssetResourceCreationOptions();
                        // let creationRequest = PHAssetCreationRequest.forAsset()
                        var creationRequest = PHAssetCreationRequest.CreationRequestForAsset();
                        // options.uniformTypeIdentifier = self.requestedPhotoSettings.processedFileType.map { $0.rawValue }
                        options.UniformTypeIdentifier = RequestedPhotoSettings.ProcessedFileType;
                        // creationRequest.addResource(with: .photo, data: photoData, options: options)
                        creationRequest.AddResource(PHAssetResourceType.Photo, photoData, options);

                        // if let livePhotoCompanionMovieURL = self.livePhotoCompanionMovieURL {
                        // let livePhotoCompanionMovieFileResourceOptions = PHAssetResourceCreationOptions()
                        // livePhotoCompanionMovieFileResourceOptions.shouldMoveFile = true
                        // creationRequest.addResource(with: .pairedVideo, fileURL: livePhotoCompanionMovieURL, options: livePhotoCompanionMovieFileResourceOptions)
                        // }

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

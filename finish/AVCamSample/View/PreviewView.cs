using System;

using UIKit;
using Foundation;
using AVFoundation;
using ObjCRuntime;

namespace AVCamSample
{
    [Register("PreviewView")]
    public class PreviewView : UIView
    {
        static Class layerClass;

        public static Class LayerClass
        {
            [Export("layerClass")]
            get => layerClass = layerClass ?? new Class(typeof(AVCaptureVideoPreviewLayer));
        }

        public AVCaptureSession Session
        {
            get => VideoPreviewLayer.Session;
            set => VideoPreviewLayer.Session = value;
        }

        public AVCaptureVideoPreviewLayer VideoPreviewLayer
        {
            get => (AVCaptureVideoPreviewLayer)Layer;
        }

    }
}
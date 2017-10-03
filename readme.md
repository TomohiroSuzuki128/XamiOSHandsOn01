# 準備 #

今回はApple公式の写真撮影のサンプルアプリを題材にします。以下よりサンプルコードをダウンロードして下さい。

[https://developer.apple.com/library/content/samplecode/AVCam/Introduction/Intro.html](https://developer.apple.com/library/content/samplecode/AVCam/Introduction/Intro.html "AVCam-iOS")

## ソリューション作成 ##

[ファイル]->[新しいソリューション]でソリューションを作成する

[iOS]->[アプリ]->[単一ビューアプリ]->[次へ]

![](https://github.com/TomohiroSuzuki128/XamiOSHandsOn01/blob/master/images/001.png?raw=true)



下記を設定し、[次へ]を押す。

組織の識別子は、com.<ユニークな自分だけの名前>になるようにしてください。

![](https://github.com/TomohiroSuzuki128/XamiOSHandsOn01/blob/master/images/002.png?raw=true)



プロジェクト名などを入力し、[作成]を押す。

![](https://github.com/TomohiroSuzuki128/XamiOSHandsOn01/blob/master/images/003.png?raw=true)

以上で、ソリューションの作成は完了です。


# PhotoCaptureDelegate.swift の Xamarin.iOS への移植 #


## クラス本体の移植 ##

<code>PhotoCaptureDelegate.cs</code>ファイルを作成します。

![](https://github.com/TomohiroSuzuki128/XamiOSHandsOn01/blob/master/images/004.png?raw=true)


それでは<code>PhotoCaptureDelegate.swift</code>ファイルのコードを見てみましょう。

### using ###

importを確認すると

**Swift**
```swift
import AVFoundation
import Photos
```

とありますので、追加します。

C#
```csharp
using System;
using AVFoundation;
using Photos;
```

### クラス定義 ###

次にクラスの定義部分に注目すると、以下のようにクラスの定義とエクステンションがあります。

**Swift**
```swift
class PhotoCaptureProcessor: NSObject {
```


**Swift**
```swift
extension PhotoCaptureProcessor: AVCapturePhotoCaptureDelegate {
```


Swift で<code>AVCapturePhotoCaptureDelegate</code>のように定義済みのプロトコルが利用されている場合、基本的には Xamarin.iOS 側には対応する interface および class が準備されています。
よって、<code>AVCapturePhotoCaptureDelegate</code>のメタ情報を確認すると

**C#**
```csharp
public class AVCapturePhotoCaptureDelegate : NSObject, IAVCapturePhotoCaptureDelegate, INativeObject, IDisposable
```

とありますので、<code>AVCapturePhotoCaptureDelegate</code>を継承すれば、<code>NSObject</code>を継承し、<code>AVCapturePhotoCaptureDelegate</code>を実装するクラスになります。
よって、以下のようにクラスを定義します。

**C#**
```csharp
using System;
using AVFoundation;
using Photos;
using Foundation;

namespace AVCamSample
{
	public class PhotoCaptureDelegate : AVCapturePhotoCaptureDelegate
	{
	}
}
```

### フィールド ###

次に、インスタンス変数（C#ではフィールド）を移植します。

**Swift**
```swift
private(set) var requestedPhotoSettings: AVCapturePhotoSettings

private let willCapturePhotoAnimation: () -> Void

private let livePhotoCaptureHandler: (Bool) -> Void

private let completionHandler: (PhotoCaptureProcessor) -> Void

private var photoData: Data?

private var livePhotoCompanionMovieURL: URL?
```

Swift では[アクセス修飾子] [var or let] [変数名] : [型名] の順で記述されています。

よって、1行目で言えば、<code>requestedPhotoSettings</code>が変数名、<code>AVCapturePhotoSettings</code>が型名です。
これは、C#に簡単に書き換えられます。
<code>AVCapturePhotoSettings</code>の型も Xamarin.iOS に定義済みです。
もし「型が見つからない」とエラーが出る場合は、using を確認してみてください。

<code>private(set)</code> は、setter のみ<code>private</code>という意味です。

3行目の場合、<code>willCapturePhotoAnimation</code>が変数名、<code>() -> Void</code>が型名です。
これは、型名が<code>() -> Void</code>ですから、C#で言うデリゲートですね（Swiftではクロージャ）。<code>-></code>の左辺が引数の型、右辺が戻り値の型です。

9,11行目の<code>Data?</code>,<code>URL?</code>は、Xamarin.iOS ではそれぞれ<code>NSData</code>,<code>NSUrl</code>になります。
<code>NS</code>がつくのは、Xamarin.iOS は、Objective-C に基づいており、Objective-C で、<code>NS</code>がつく型名になっているからです。
また、<code>NSData</code>,<code>NSUrl</code>を使う場合、<code>using Foundation;</code>が必要になります。

これらを考慮するとフィールド定義は以下のようになります。

**C#**
```csharp
using System;
using AVFoundation;
using Photos;
using Foundation;

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
	}
}
```

### コンストラクタ ###

次に、イニシャライザ（C#ではコンストラクタ）を移植します。

**Swift**
```swift
init(with requestedPhotoSettings: AVCapturePhotoSettings,
	 willCapturePhotoAnimation: @escaping () -> Void,
	 livePhotoCaptureHandler: @escaping (Bool) -> Void,
	 completionHandler: @escaping (PhotoCaptureProcessor) -> Void) {
	self.requestedPhotoSettings = requestedPhotoSettings
	self.willCapturePhotoAnimation = willCapturePhotoAnimation
	self.livePhotoCaptureHandler = livePhotoCaptureHandler
	self.completionHandler = completionHandler
}
```

引数がクロージャであるものに全て<code>@escaping</code>がついていますが、移植する際にはあまり気にしなくても良いです。
※クロージャがスコープから抜けても存在し続けるときに<code>@escaping</code>が必要になります。

<code>self</code>はC#では<code>this</code>です。
あとはベタで移植すれば大丈夫です。

これらを考慮しコンストラクタを追加すると以下のようになります。

**C#**
```csharp
using System;
using AVFoundation;
using Photos;
using Foundation;

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

	}
}
```

### メソッド ###

次に、メソッドを移植します。

**Swift**
```swift
private func didFinish() {
	if let livePhotoCompanionMoviePath = livePhotoCompanionMovieURL?.path {
		if FileManager.default.fileExists(atPath: livePhotoCompanionMoviePath) {
			do {
				try FileManager.default.removeItem(atPath: livePhotoCompanionMoviePath)
			} catch {
				print("Could not remove file at url: \(livePhotoCompanionMoviePath)")
			}
		}
	}
	
	completionHandler(self)
}
```

ここは多少厄介です。なぜなら、Xamarin.iOS は、Objective-C に基づいており、
Swift をそのまま移植できず、若干表現を変えなければならないためです。

ここは、仕方ないので Swift のコードの処理を理解し、同等の処理を Xamarin.iOS で書かなくてはいけません。

まず<code>if let</code>ですがこれは、<code>nil</code> チェックです。
<code>livePhotoCompanionMoviePath</code>が<code>nil</code>でなければ、<code>{}</code>内部を実行します

<code>FileManager</code>は、Xamarin.iOS では<code>NSFileManager</code>です。
このNSがつくかどうかの件については、慣れるとだんだん迷わなくなります。

最初のうちは「型が見つからない」とエラーが出る場合は、まずは<code>using</code>を確認、
次に、<code>NS</code>をつけてみるという手順を取るとつまづきにくいです。

後は、インテリセンスを利用して<code>NSFileManager</code>のAPIにあわせて、書き換えていきます。

これらを考慮し<code>DidFinish()</code>を追加すると以下のようになります。

**C#**
```csharp
using System;
using AVFoundation;
using Photos;
using Foundation;

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

	}
}
```

これでクラス本体部分の移植が完了しました！


## エクステンションの移植 ##

それでは<code>PhotoCaptureDelegate.swift</code>のエクステンション部分のコードを見てみましょう。

### コールバックメソッドの定義 ###

プロトコルの実装部分を確認してみると以下のようにコールバックのメソッドが定義されています。
※実装の中身は省略しています。

Swift
```swift
extension PhotoCaptureProcessor: AVCapturePhotoCaptureDelegate {
    /*
     This extension includes all the delegate callbacks for AVCapturePhotoCaptureDelegate protocol
    */
    
    func photoOutput(_ output: AVCapturePhotoOutput, willBeginCaptureFor resolvedSettings: AVCaptureResolvedPhotoSettings) {
    }
    
    func photoOutput(_ output: AVCapturePhotoOutput, willCapturePhotoFor resolvedSettings: AVCaptureResolvedPhotoSettings) {
    }
    
    func photoOutput(_ output: AVCapturePhotoOutput, didFinishProcessingPhoto photo: AVCapturePhoto, error: Error?) {
    }

    func photoOutput(_ output: AVCapturePhotoOutput, didFinishRecordingLivePhotoMovieForEventualFileAt outputFileURL: URL, resolvedSettings: AVCaptureResolvedPhotoSettings) {
    }
    
    func photoOutput(_ output: AVCapturePhotoOutput, didFinishProcessingLivePhotoToMovieFileAt outputFileURL: URL, duration: CMTime, photoDisplayTime: CMTime, resolvedSettings: AVCaptureResolvedPhotoSettings, error: Error?) {
    }
    
    func photoOutput(_ output: AVCapturePhotoOutput, didFinishCaptureFor resolvedSettings: AVCaptureResolvedPhotoSettings, error: Error?) {
    }
```

コールバックメソッドの名前が全て<code>photoOutput</code>と同じになっています。
ここで、C#の対応するクラス<code>AVCapturePhotoCaptureDelegate</code>のコールバックメソッドのメタ情報を確認してみましょう。

**C#**
```csharp
[CompilerGenerated]
[Export("captureOutput:didCapturePhotoForResolvedSettings:")]
public virtual void DidCapturePhoto(AVCapturePhotoOutput captureOutput, AVCaptureResolvedPhotoSettings resolvedSettings);

[CompilerGenerated]
[Export("captureOutput:didFinishCaptureForResolvedSettings:error:")]
public virtual void DidFinishCapture(AVCapturePhotoOutput captureOutput, AVCaptureResolvedPhotoSettings resolvedSettings, NSError error);

[CompilerGenerated]
[Export("captureOutput:didFinishProcessingLivePhotoToMovieFileAtURL:duration:photoDisplayTime:resolvedSettings:error:")]
public virtual void DidFinishProcessingLivePhotoMovie(AVCapturePhotoOutput captureOutput, NSUrl outputFileUrl, CMTime duration, CMTime photoDisplayTime, AVCaptureResolvedPhotoSettings resolvedSettings, NSError error);

[CompilerGenerated]
[Export("captureOutput:didFinishProcessingPhotoSampleBuffer:previewPhotoSampleBuffer:resolvedSettings:bracketSettings:error:")]
public virtual void DidFinishProcessingPhoto(AVCapturePhotoOutput captureOutput, CMSampleBuffer photoSampleBuffer, CMSampleBuffer previewPhotoSampleBuffer, AVCaptureResolvedPhotoSettings resolvedSettings, AVCaptureBracketedStillImageSettings bracketSettings, NSError error);

[CompilerGenerated]
[Export("captureOutput:didFinishProcessingRawPhotoSampleBuffer:previewPhotoSampleBuffer:resolvedSettings:bracketSettings:error:")]
public virtual void DidFinishProcessingRawPhoto(AVCapturePhotoOutput captureOutput, CMSampleBuffer rawSampleBuffer, CMSampleBuffer previewPhotoSampleBuffer, AVCaptureResolvedPhotoSettings resolvedSettings, AVCaptureBracketedStillImageSettings bracketSettings, NSError error);

[CompilerGenerated]
[Export("captureOutput:didFinishRecordingLivePhotoMovieForEventualFileAtURL:resolvedSettings:")]
public virtual void DidFinishRecordingLivePhotoMovie(AVCapturePhotoOutput captureOutput, NSUrl outputFileUrl, AVCaptureResolvedPhotoSettings resolvedSettings);

[CompilerGenerated]
[Export("captureOutput:willBeginCaptureForResolvedSettings:")]
public virtual void WillBeginCapture(AVCapturePhotoOutput captureOutput, AVCaptureResolvedPhotoSettings resolvedSettings);

[CompilerGenerated]
[Export("captureOutput:willCapturePhotoForResolvedSettings:")]
public virtual void WillCapturePhoto(AVCapturePhotoOutput captureOutput, AVCaptureResolvedPhotoSettings resolvedSettings);
```

メソッドは全て違う名前になっています。

### コールバックメソッドの Swift, Xamarin.iOS の対応の判別 ###

ここで Swift と C# のメソッドの定義を良く見比べて下さい。Swiftの第2引数のラベル名に<code>willBeginCaptureFor</code>とあります。C# の<code>WillBeginCapture</code>メソッドの ExportAttribute を見ると、<code>[Export("captureOutput:willBeginCaptureForResolvedSettings:")]</code>とあります。どちらにも <code>willBeginCaptureFor</code>という文字列が含まれているので、このメソッドが対応しているメソッドになります。

**Swift**
```swift
func photoOutput(_ output: AVCapturePhotoOutput, willBeginCaptureFor resolvedSettings: AVCaptureResolvedPhotoSettings) {
}
```

**C#**
```csharp
[CompilerGenerated]
[Export("captureOutput:willBeginCaptureForResolvedSettings:")]
public virtual void WillBeginCapture(AVCapturePhotoOutput captureOutput, AVCaptureResolvedPhotoSettings resolvedSettings);
```

微妙に名前が違うのは、繰り返しになりますが、Xamarin.iOS が、Objective-C に基づいているからです。

試しに Objective-C のメソッド定義を確認してみましょう。

**Objective-C**
```objc
- (void)captureOutput:(AVCapturePhotoOutput *)captureOutput willBeginCaptureForResolvedSettings:(AVCaptureResolvedPhotoSettings *)resolvedSettings
{
}
```

C# の ExportAttribute <code>[Export("captureOutput:willBeginCaptureForResolvedSettings:")]</code>と、Objective-C のメソッド名<code>captureOutput</code>、第2引数ラベル名<code>willBeginCaptureForResolvedSettings</code>となっており見事に名称が一致しています。Swift では残念ながら名称が若干変更されているのでわかりにくくなってしまっています。

これで、エクステンションのコールバックメソッドの部分の対応がわかりました。同じ要領で、全てのコールバックメソッドの定義を追加すると以下のようになります。

**C#**
```csharp
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

		public override void WillBeginCapture (AVCapturePhotoOutput captureOutput, AVCaptureResolvedPhotoSettings resolvedSettings)
		{
		}

		public override void WillCapturePhoto (AVCapturePhotoOutput captureOutput, AVCaptureResolvedPhotoSettings resolvedSettings)
		{
		}

		public override void DidFinishProcessingPhoto (AVCapturePhotoOutput captureOutput, CMSampleBuffer photoSampleBuffer, CMSampleBuffer previewPhotoSampleBuffer, AVCaptureResolvedPhotoSettings resolvedSettings, AVCaptureBracketedStillImageSettings bracketSettings, NSError error)
		{
		}

		public override void DidFinishRecordingLivePhotoMovie (AVCapturePhotoOutput captureOutput, NSUrl outputFileUrl, AVCaptureResolvedPhotoSettings resolvedSettings)
		{
		}

		public override void DidFinishProcessingLivePhotoMovie (AVCapturePhotoOutput captureOutput, NSUrl outputFileUrl, CMTime duration, CMTime photoDisplayTime, AVCaptureResolvedPhotoSettings resolvedSettings, NSError error)
		{
		}

		public override void DidFinishCapture (AVCapturePhotoOutput captureOutput, AVCaptureResolvedPhotoSettings resolvedSettings, NSError error)
		{
		}
	}
}
```

### コールバックメソッドの実装 ###

#### WillBeginCapture ####

では、早速、1つ目のコールバックの Swift のコードを見ていきましょう。

**Swift**
```swift
func photoOutput(_ output: AVCapturePhotoOutput, willBeginCaptureFor resolvedSettings: AVCaptureResolvedPhotoSettings) {
	if resolvedSettings.livePhotoMovieDimensions.width > 0 && resolvedSettings.livePhotoMovieDimensions.height > 0 {
		livePhotoCaptureHandler(true)
	}
}
```

その1でご説明したように、<code>livePhotoCaptureHandler</code>に対応するC#のデリゲートは<code>capturingLivePhoto</code>ですので、そこだけ気をつけて書き換えれば、後はベタに移植するだけです。

**C#**
```csharp
public override void WillBeginCapture (AVCapturePhotoOutput captureOutput, AVCaptureResolvedPhotoSettings resolvedSettings)
{
	if (resolvedSettings.LivePhotoMovieDimensions.Width > 0 && resolvedSettings.LivePhotoMovieDimensions.Height > 0)
		capturingLivePhoto (true);
}
```

#### WillCapturePhoto  ####

2つ目です。1つ目同様にC#のデリゲートの対応だけ確認すれば後は簡単です。

**Swift**
```swift
func photoOutput(_ output: AVCapturePhotoOutput, willCapturePhotoFor resolvedSettings: AVCaptureResolvedPhotoSettings) {
	willCapturePhotoAnimation()
}
```


**C#**
```csharp
public override void WillCapturePhoto (AVCapturePhotoOutput captureOutput, AVCaptureResolvedPhotoSettings resolvedSettings)
{
	willCapturePhotoAnimation ();
}
```


#### DidFinishProcessingPhoto  ####

3つ目ですが、これは Swift と Xamarin.iOS で既にAPIが違っています。Appleのリファレンスを確認したところ、Swift のサンプルは、iOS11 対応で、 Xamarin.iOS は iOS10 対応のためです。
調べたところ<code>jpegPhotoDataRepresentation</code>は iOS11で Deprecated になっていました。

**Swift**
```swift
func photoOutput(_ output: AVCapturePhotoOutput, didFinishProcessingPhoto photo: AVCapturePhoto, error: Error?) {
	if let error = error {
		print("Error capturing photo: \(error)")
	} else {
		photoData = photo.fileDataRepresentation()
	}
}
```

このまま移植も可能ですが、わかりにくいので iOS10 対応のサンプルを探したところ以下のようになっていました。これで見比べるとAPIがそろっていますね。

**Swift iOS10対応**
```swift
func capture(_ captureOutput: AVCapturePhotoOutput, didFinishProcessingPhotoSampleBuffer photoSampleBuffer: CMSampleBuffer?, previewPhotoSampleBuffer: CMSampleBuffer?, resolvedSettings: AVCaptureResolvedPhotoSettings, bracketSettings: AVCaptureBracketedStillImageSettings?, error: Error?) {
	if let photoSampleBuffer = photoSampleBuffer {
		photoData = AVCapturePhotoOutput.jpegPhotoDataRepresentation(forJPEGSampleBuffer: photoSampleBuffer, previewPhotoSampleBuffer: previewPhotoSampleBuffer)
	}
	else {
		print("Error capturing photo: \(error)")
		return
	}
}
```

わかりにくいのは、前にも出てきた<code>if let</code>ですが、これは<code>nil</code>チェックです。<code>photoSampleBuffer</code>が<code>nil</code>でなければ、<code>{}</code>内部を実行します。
あとはベタに移植すれば大丈夫です。

**C#**
```csharp
public override void DidFinishProcessingPhoto (AVCapturePhotoOutput captureOutput, CMSampleBuffer photoSampleBuffer, CMSampleBuffer previewPhotoSampleBuffer, AVCaptureResolvedPhotoSettings resolvedSettings, AVCaptureBracketedStillImageSettings bracketSettings, NSError error)
{
	if (photoSampleBuffer != null)
		photoData = AVCapturePhotoOutput.GetJpegPhotoDataRepresentation (photoSampleBuffer, previewPhotoSampleBuffer);
	else
		Console.WriteLine ($"Error capturing photo: {error.LocalizedDescription}");
}
```


#### DidFinishRecordingLivePhotoMovie  ####

4つ目です。<code>livePhotoCaptureHandler</code>に対応するC#のデリゲートは<code>capturingLivePhoto</code>です。

**Swift**
```swift
func photoOutput(_ output: AVCapturePhotoOutput, didFinishRecordingLivePhotoMovieForEventualFileAt outputFileURL: URL, resolvedSettings: AVCaptureResolvedPhotoSettings) {
	livePhotoCaptureHandler(false)
}
```

**C#**
```csharp
public override void DidFinishRecordingLivePhotoMovie (AVCapturePhotoOutput captureOutput, NSUrl outputFileUrl, AVCaptureResolvedPhotoSettings resolvedSettings)
{
	capturingLivePhoto (false);
}
```

#### DidFinishProcessingLivePhotoMovie  ####

5つ目です。これもベタに移植するだけです。

**Swift**
```swift
func photoOutput(_ output: AVCapturePhotoOutput, didFinishProcessingLivePhotoToMovieFileAt outputFileURL: URL, duration: CMTime, photoDisplayTime: CMTime, resolvedSettings: AVCaptureResolvedPhotoSettings, error: Error?) {
	if error != nil {
		print("Error processing live photo companion movie: \(String(describing: error))")
		return
	}
	livePhotoCompanionMovieURL = outputFileURL
}
```

**C#**
```csharp
public override void DidFinishProcessingLivePhotoMovie (AVCapturePhotoOutput captureOutput, NSUrl outputFileUrl, CMTime duration, CMTime photoDisplayTime, AVCaptureResolvedPhotoSettings resolvedSettings, NSError error)
{
	if (error != null)
	{
		Console.WriteLine ($"Error processing live photo companion movie: {error.LocalizedDescription})");
		return;
	}
	livePhotoCompanionMovieUrl = outputFileUrl;
}
```

#### DidFinishCapture ####

6つ目です。こちらは<code>options.uniformTypeIdentifier = self.requestedPhotoSettings.processedFileType.map { $0.rawValue }</code> で iOS11 からのAPI、processedFileTypeが使われていました。

**Swift**
```swift
func photoOutput(_ output: AVCapturePhotoOutput, didFinishCaptureFor resolvedSettings: AVCaptureResolvedPhotoSettings, error: Error?) {
    if let error = error {
        print("Error capturing photo: \(error)")
        didFinish()
        return
    }
    
    guard let photoData = photoData else {
        print("No photo data resource")
        didFinish()
        return
    }
    
    PHPhotoLibrary.requestAuthorization { [unowned self] status in
        if status == .authorized {
            PHPhotoLibrary.shared().performChanges({ [unowned self] in
                let options = PHAssetResourceCreationOptions()
                let creationRequest = PHAssetCreationRequest.forAsset()
                options.uniformTypeIdentifier = self.requestedPhotoSettings.processedFileType.map { $0.rawValue }
                creationRequest.addResource(with: .photo, data: photoData, options: options)
                
                if let livePhotoCompanionMovieURL = self.livePhotoCompanionMovieURL {
                    let livePhotoCompanionMovieFileResourceOptions = PHAssetResourceCreationOptions()
                    livePhotoCompanionMovieFileResourceOptions.shouldMoveFile = true
                    creationRequest.addResource(with: .pairedVideo, fileURL: livePhotoCompanionMovieURL, options: livePhotoCompanionMovieFileResourceOptions)
                }
                
                }, completionHandler: { [unowned self] _, error in
                    if let error = error {
                        print("Error occurered while saving photo to photo library: \(error)")
                    }
                    
                    self.didFinish()
                }
            )
        } else {
            self.didFinish()
        }
    }
}
```

iOS10 対応のサンプルを探したところ以下のようになっていましたので、こちらを移植していきます。

**Swift iOS10対応**
```swift
func capture(_ captureOutput: AVCapturePhotoOutput, didFinishCaptureForResolvedSettings resolvedSettings: AVCaptureResolvedPhotoSettings, error: Error?) {
	if let error = error {
		print("Error capturing photo: \(error)")
		didFinish()
		return
	}
	
	guard let photoData = photoData else {
		print("No photo data resource")
		didFinish()
		return
	}
	
	PHPhotoLibrary.requestAuthorization { [unowned self] status in
		if status == .authorized {
			PHPhotoLibrary.shared().performChanges({ [unowned self] in
					let creationRequest = PHAssetCreationRequest.forAsset()
					creationRequest.addResource(with: .photo, data: photoData, options: nil)
				
					if let livePhotoCompanionMovieURL = self.livePhotoCompanionMovieURL {
						let livePhotoCompanionMovieFileResourceOptions = PHAssetResourceCreationOptions()
						livePhotoCompanionMovieFileResourceOptions.shouldMoveFile = true
						creationRequest.addResource(with: .pairedVideo, fileURL: livePhotoCompanionMovieURL, options: livePhotoCompanionMovieFileResourceOptions)
					}
				
				}, completionHandler: { [unowned self] success, error in
					if let error = error {
						print("Error occurered while saving photo to photo library: \(error)")
					}
					
					self.didFinish()
				}
			)
		}
		else {
			self.didFinish()
		}
	}
}
```

いくつか、C#erにはなじみのない表現が使われています。

まず、<code>guard</code>ですが、<code>guard let photoData = photoData else {}</code>は、アンラップとnilチェックを同時に行い、アンラップした<code>photoData</code>を<code>guard～else</code>ブロック外で使用できます。
※アンラップとは、<code>nil</code>を代入できるオプショナル型から値を取り出すことです。

もう一つ、<code>[unowned self]</code>は、非所有参照で<code>self</code>をキャプチャします。これを使うと、クロージャー内ではクロージャ外の<code>self</code>とは別の非所有参照のselfを使うのため循環参照が起こりません。

これは、移植時にはメモリリーク防止のおまじないとでも認識しておけば十分です。

<code>PerformChanges</code>メソッドを確認すると以下のようになっていますので、APIにあわせて実装していきます。


**C#**
```csharp
public virtual void PerformChanges(Action changeHandler, Action<bool, NSError> completionHandler);
```


**C#**
```csharp
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
```

これで、コールバックメソッドの実装が完了しました。以上で<code>PhotoCaptureDelegate</code>の移植が完了です！
お疲れさまでした。


最後に完成した<code>PhotoCaptureDelegate.cs</code>のコードは以下のようになります。

**C#**
```csharp
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

		public override void WillBeginCapture (AVCapturePhotoOutput captureOutput, AVCaptureResolvedPhotoSettings resolvedSettings)
		{
			if (resolvedSettings.LivePhotoMovieDimensions.Width > 0 && resolvedSettings.LivePhotoMovieDimensions.Height > 0)
				capturingLivePhoto (true);
		}

		public override void WillCapturePhoto (AVCapturePhotoOutput captureOutput, AVCaptureResolvedPhotoSettings resolvedSettings)
		{
			willCapturePhotoAnimation ();
		}

		public override void DidFinishProcessingPhoto (AVCapturePhotoOutput captureOutput, CMSampleBuffer photoSampleBuffer, CMSampleBuffer previewPhotoSampleBuffer, AVCaptureResolvedPhotoSettings resolvedSettings, AVCaptureBracketedStillImageSettings bracketSettings, NSError error)
		{
			if (photoSampleBuffer != null)
				photoData = AVCapturePhotoOutput.GetJpegPhotoDataRepresentation (photoSampleBuffer, previewPhotoSampleBuffer);
			else
				Console.WriteLine ($"Error capturing photo: {error.LocalizedDescription}");
		}

		public override void DidFinishRecordingLivePhotoMovie (AVCapturePhotoOutput captureOutput, NSUrl outputFileUrl, AVCaptureResolvedPhotoSettings resolvedSettings)
		{
			capturingLivePhoto (false);
		}

		public override void DidFinishProcessingLivePhotoMovie (AVCapturePhotoOutput captureOutput, NSUrl outputFileUrl, CMTime duration, CMTime photoDisplayTime, AVCaptureResolvedPhotoSettings resolvedSettings, NSError error)
		{
			if (error != null) {
				Console.WriteLine ($"Error processing live photo companion movie: {error.LocalizedDescription})");
				return;
			}

			livePhotoCompanionMovieUrl = outputFileUrl;
		}

		public override void DidFinishCapture (AVCapturePhotoOutput captureOutput, AVCaptureResolvedPhotoSettings resolvedSettings, NSError error)
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
```

# PreviewView.swift の Xamarin.iOS への移植 #

これはカメラのプレビューを表示するUIViewです。

<code>View</code>フォルダを作成します。

![](https://github.com/TomohiroSuzuki128/XamiOSHandsOn01/blob/master/images/005.png?raw=true)

<code>PreviewView.cs</code>ファイルを作成します。

![](https://github.com/TomohiroSuzuki128/XamiOSHandsOn01/blob/master/images/006.png?raw=true)


それでは<code>PreviewView.swift</code>ファイルのコードを見てみましょう。

**Swift**
```swift
import UIKit
import AVFoundation

class PreviewView: UIView {
    var videoPreviewLayer: AVCaptureVideoPreviewLayer {
        guard let layer = layer as? AVCaptureVideoPreviewLayer else {
            fatalError("Expected `AVCaptureVideoPreviewLayer` type for layer. Check PreviewView.layerClass implementation.")
        }
        
        return layer
    }
	
	var session: AVCaptureSession? {
		get {
			return videoPreviewLayer.session
		}
		set {
			videoPreviewLayer.session = newValue
		}
	}
	
	// MARK: UIView
	
    override class var layerClass: AnyClass {
		return AVCaptureVideoPreviewLayer.self
	}
}
```

ここで、新しく出てきた内容は、<code>override class var layerClass: AnyClass {	 return AVCaptureVideoPreviewLayer.self }</code>です。

通常、<code>UIView</code>の<code>Layer</code>は自動的に作成され割り当てられます。
デフォルトでは<code>Layer</code>は<code>CALayer</code>のインスタンスになりますが、overrideすることで<code>CALayer</code>派生の任意の型を使用できます。

Xamarin.iOSで同じことを実現するには、<code>using ObjCRuntime;</code>を追加し、<code>CALayer</code>派生の任意の型を返す静的メソッド、または静的プロパティを作成し、<code>[Export("layerClass")]</code>を設定します。

具体的には下記のようになります。

**C#**
```csharp
public static Class LayerClass
{
	[Export("layerClass")]
	get
	{
		return layerClass = layerClass ?? new Class(typeof(AVCaptureVideoPreviewLayer));
	}
}
```

または

```csharp
[Export ("layerClass")]
public static Class GetLayerClass ()
{
    return new Class(typeof(AVCaptureVideoPreviewLayer));
}
```

あとは、そのまま移植するだけです。

このあたりを詳しくご理解したい方は[こちら](https://developer.xamarin.com/api/type/UIKit.UIView/#Changing_the_CALayer "Changing_the_CALayer")を参照してください。



あとは、そのまま移植するだけです。

**C#**
```csharp
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
			get
			{
				return layerClass = layerClass ?? new Class(typeof(AVCaptureVideoPreviewLayer));
			}
		}

		public AVCaptureSession Session
		{
			get
			{
				return VideoPreviewLayer.Session;
			}
			set
			{
				VideoPreviewLayer.Session = value;
			}
		}

		public AVCaptureVideoPreviewLayer VideoPreviewLayer
		{
			get
			{
				return (AVCaptureVideoPreviewLayer)Layer;
			}
		}

	}
}
```


# CameraViewController.swift の Xamarin.iOS への移植 #

<code>CameraViewController.swift</code>では、撮影処理や録画処理を行います。この処理はとても量が多いので、今回は一部機能以外翻訳済みのcsファイルを準備しましたのでそちらをお使い下さい。

（ここにCameraViewController.csファイルの情報が入る：後日準備）
※メソッド1つくらいは移植するようにする

# UI の Xamarin.iOS への移植 #

UIについては、<code>storyboard</code>をそのまま利用できます。ですが、実際にアプリを開発すると、<code>storyboard</code>だけで完結するのは難しく、バインドするデータに応じて動的にUIをコントロールする場合など、どうしてもコードでUIを記述する場面が出てきます。

ところが、Xamarin.iOSでUIをコードで作成する方法の情報は非常に少ないです。そこで、今回はせっかくの機会なのでUIをコードで作成してみましょう。具体的には、<code>storyboard</code>をXamarin.iOSに翻訳する作業になります。

## UI部品を割り当てるフィールドを追加 ##

<code>CameraViewController.cs</code>にUI部品を割り当てるフィールドを追加します。

**C#**
```csharp
public class CameraViewController : UIViewController, IAVCaptureFileOutputRecordingDelegate
{

	PreviewView PreviewView { get; set; }
	UILabel CameraUnavailableLabel { get; set; }
	UIButton ResumeButton { get; set; }
	UIButton RecordButton { get; set; }
	UIButton CameraButton { get; set; }
	UIButton PhotoButton { get; set; }
	UIButton LivePhotoModeButton { get; set; }
	UISegmentedControl CaptureModeControl { get; set; }
	UILabel CapturingLivePhotoLabel { get; set; }

```

## UI部品を構築するメソッドを追加 ##

UIを構築するメソッドを作成します。

**C#**
```csharp
private void InitUI()
{
}
```

### View ###

<code>InitUI()</code>に<code>storyboard</code>　15行目～18行目の<code>View</code>に関する設定を移植します。

**storyboard**
```xml
<viewController id="BYZ-38-t0r" customClass="CameraViewController" customModule="AVCam" customModuleProvider="target" sceneMemberID="viewController">
    <view key="view" contentMode="scaleToFill" id="8bC-Xf-vdC">
        <rect key="frame" x="0.0" y="0.0" width="375" height="667"/>
        <autoresizingMask key="autoresizingMask" widthSizable="YES" heightSizable="YES"/>
```

**C#**
```csharp
private void InitUI()
{
	View.ContentMode = UIViewContentMode.ScaleToFill;
	View.Frame = new CGRect(0, 0, 375, 667);
	View.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
}
```

### CameraUnavailableLabel ###

<code>storyboard</code>　15行目～18行目の<code>CameraUnavailableLabel</code>に関する設定を移植します。

**storyboard**
```xml
<label hidden="YES" userInteractionEnabled="NO" contentMode="left" horizontalHuggingPriority="251" verticalHuggingPriority="251" text="Camera Unavailable" 
    textAlignment ="center" lineBreakMode="tailTruncation" numberOfLines="0" baselineAdjustment="alignBaselines" adjustsFontSizeToFit="NO" translatesAutoresizingMaskIntoConstraints="NO" id="zf0-db-esM" userLabel="Camera Unavailable">
    <color key="backgroundColor" red="0.0" green="0.0" blue="0.0" alpha="0.0" colorSpace="custom" customColorSpace="sRGB"/>
    <fontDescription key="fontDescription" type="system" pointSize="24"/>
    <color key="textColor" red="1" green="1" blue="0.0" alpha="1" colorSpace="custom" customColorSpace="sRGB"/>
    <nil key="highlightedColor"/>
</label>
```

xmlの各attributeに対応したプロパティを見つけ出し、設定していきます。IntelliSenseをうまく使えば簡単に見つけることができます。

例えば<code>hidden="YES"</code>なら、<code>Hidden = true</code>、<code>userInteractionEnabled="NO"</code>なら<code>UserInteractionEnabled = false</code>といった要領です。

わかりにくいプロパティはcolor関係とfontですが、それぞれ、colorは<code>public static UIColor FromRGBA(nfloat red, nfloat green, nfloat blue, nfloat alpha);</code>、fontは<code>public static UIFont SystemFontOfSize(nfloat size);</code>となります。

全部移植すると以下のようになりますので、<code>InitUI()</code>の先ほど追加したコードの下に以下を追加します。

**C#**
```csharp
CameraUnavailableLabel = new UILabel
{
	Hidden = true,
	UserInteractionEnabled = false,
	ContentMode = UIViewContentMode.Left,
	Text = "Camera Unavailable",
	TextAlignment = UITextAlignment.Center,
	LineBreakMode = UILineBreakMode.TailTruncation,
	Lines = 0,
	BaselineAdjustment = UIBaselineAdjustment.AlignBaselines,
	AdjustsFontSizeToFitWidth = false,
	TranslatesAutoresizingMaskIntoConstraints = false,
	BackgroundColor = UIColor.FromRGBA(0.0f, 0.0f, 0.0f, 1.0f),
	Font = UIFont.SystemFontOfSize(24f),
	TextColor = UIColor.FromRGBA(1.0f, 1.0f, 0.0f, 1.0f),
};
View.AddSubview(CameraUnavailableLabel);
```

では、同じ要領で他のUI部品も追加していきましょう。

### PreviewView ###

**storyboard**
```xml
<view contentMode="scaleToFill" translatesAutoresizingMaskIntoConstraints="NO" id="3eR-Rn-XpZ" userLabel="Preview" customClass="PreviewView">
    <color key="backgroundColor" red="0.0" green="0.0" blue="0.0" alpha="1" colorSpace="custom" customColorSpace="sRGB"/>
    <gestureRecognizers/>
    <connections>
        <outletCollection property="gestureRecognizers" destination="fY6-qX-ntV" appends="YES" id="G6D-dx-xU8"/>
    </connections>
</view>
```

<code>InitUI()</code>の先ほど追加したコードの下に以下を追加します。

**C#**
```csharp
PreviewView = new PreviewView()
{
	ContentMode = UIViewContentMode.ScaleToFill,
	TranslatesAutoresizingMaskIntoConstraints = false,
	BackgroundColor = UIColor.FromRGBA(0.0f, 0.0f, 0.0f, 1f),
};
View.AddSubview(PreviewView);
```

### PhotoButton ###

**storyboard**
```xml
<button opaque="NO" contentMode="scaleToFill" contentHorizontalAlignment="center" contentVerticalAlignment="center" buttonType="roundedRect" lineBreakMode="middleTruncation" translatesAutoresizingMaskIntoConstraints="NO" id="uCj-6P-mHF" userLabel="Still">
    <color key="backgroundColor" red="0.0" green="0.0" blue="0.0" alpha="0.29999999999999999" colorSpace="custom" customColorSpace="sRGB"/>
    <constraints>
        <constraint firstAttribute="height" constant="30" id="NtC-UN-gTs"/>
        <constraint firstAttribute="width" constant="80" id="dxU-UP-4Ae"/>
    </constraints>
    <fontDescription key="fontDescription" type="system" pointSize="20"/>
    <state key="normal" title="Photo">
        <color key="titleShadowColor" red="0.5" green="0.5" blue="0.5" alpha="1" colorSpace="custom" customColorSpace="sRGB"/>
    </state>
    <userDefinedRuntimeAttributes>
        <userDefinedRuntimeAttribute type="number" keyPath="layer.cornerRadius">
            <integer key="value" value="4"/>
        </userDefinedRuntimeAttribute>
    </userDefinedRuntimeAttributes>
    <connections>
        <action selector="capturePhoto:" destination="BYZ-38-t0r" eventType="touchUpInside" id="o5K-SC-fYn"/>
    </connections>
</button>
```

わかりにくい箇所をご説明しますと、
<code>buttonType="roundedRect"</code>はXamarin.iOSではコンストラクタでの設定となり、<code>PhotoButton = new UIButton(UIButtonType.RoundedRect)</code>となります。
<code><constraint firstAttribute="height" constant="30" id="NtC-UN-gTs"/></code>は<code>View</code>に設定した仮想解像度、幅：375, 高さ：667に対し、<code>PhotoButton</code>の高さを30に設定しなさいという制約です。
これは、Xamarin.iOSでは<code>NSLayoutConstraint.Create(PhotoButton, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1.0f, 30)</code>となります。
<code>width</code>も同じ要領で設定できます。

<code><action selector="capturePhoto:" destination="BYZ-38-t0r" eventType="touchUpInside" id="o5K-SC-fYn"/></code>はイベントハンドラの設定です。

<code>CameraViewController.swift</code>の521行目を確認すると

**Swift**
```swift
@IBAction private func capturePhoto(_ photoButton: UIButton) {
```

とありますので、これがイベントハンドラです。Xamarin.iOSではイベントが準備されていますので、C#側のハンドラメソッドの<code>CapturePhoto()</code>を設定すれば大丈夫です。

全部移植すると以下のようになりますので、<code>InitUI()</code>の先ほど追加したコードの下に以下を追加します。

**C#**
```csharp
PhotoButton = new UIButton(UIButtonType.RoundedRect)
{
	Opaque = false,
	ContentMode = UIViewContentMode.ScaleToFill,
	HorizontalAlignment = UIControlContentHorizontalAlignment.Center,
	VerticalAlignment = UIControlContentVerticalAlignment.Center,
	LineBreakMode = UILineBreakMode.MiddleTruncation,
	TranslatesAutoresizingMaskIntoConstraints = false,
	BackgroundColor = UIColor.FromRGBA(0.0f, 0.0f, 0.0f, 0.3f),
	Font = UIFont.SystemFontOfSize(20f),
};

PhotoButton.SetTitle("Photo", UIControlState.Normal);
PhotoButton.SetTitleShadowColor(UIColor.FromRGBA(0.5f, 0.5f, 0.5f, 1.0f), UIControlState.Normal);
PhotoButton.Layer.CornerRadius = 4f;
PhotoButton.TouchUpInside += (s, e) => CapturePhoto();
PhotoButton.AddConstraint(NSLayoutConstraint.Create(PhotoButton, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1.0f, 30));
PhotoButton.AddConstraint(NSLayoutConstraint.Create(PhotoButton, NSLayoutAttribute.Width, NSLayoutRelation.Equal, 1.0f, 80));
View.AddSubview(PhotoButton);
```

### CameraButton ###

これも同じ要領で移植できます。

**storyboard**
```xml
<button opaque="NO" contentMode="scaleToFill" contentHorizontalAlignment="center" contentVerticalAlignment="center" buttonType="roundedRect" lineBreakMode="middleTruncation" translatesAutoresizingMaskIntoConstraints="NO" id="rUJ-G6-RPv" userLabel="Camera">
    <color key="backgroundColor" red="0.0" green="0.0" blue="0.0" alpha="0.29999999999999999" colorSpace="custom" customColorSpace="sRGB"/>
    <fontDescription key="fontDescription" type="system" pointSize="20"/>
    <state key="normal" title="Camera">
        <color key="titleShadowColor" red="0.5" green="0.5" blue="0.5" alpha="1" colorSpace="custom" customColorSpace="sRGB"/>
    </state>
    <userDefinedRuntimeAttributes>
        <userDefinedRuntimeAttribute type="number" keyPath="layer.cornerRadius">
            <integer key="value" value="4"/>
        </userDefinedRuntimeAttribute>
    </userDefinedRuntimeAttributes>
    <connections>
        <action selector="changeCamera:" destination="BYZ-38-t0r" eventType="touchUpInside" id="3W0-h3-6fc"/>
    </connections>
</button>
```

全部移植すると以下のようになりますので、<code>InitUI()</code>の先ほど追加したコードの下に以下を追加します。

**C#**
```csharp
CameraButton = new UIButton(UIButtonType.RoundedRect)
{
	Opaque = false,
	ContentMode = UIViewContentMode.ScaleToFill,
	HorizontalAlignment = UIControlContentHorizontalAlignment.Center,
	VerticalAlignment = UIControlContentVerticalAlignment.Center,
	LineBreakMode = UILineBreakMode.MiddleTruncation,
	TranslatesAutoresizingMaskIntoConstraints = false,
	BackgroundColor = UIColor.FromRGBA(0.0f, 0.0f, 0.0f, 0.3f),
	Font = UIFont.SystemFontOfSize(20f),
};

CameraButton.SetTitle("Camera", UIControlState.Normal);
CameraButton.SetTitleShadowColor(UIColor.FromRGBA(0.5f, 0.5f, 0.5f, 1.0f), UIControlState.Normal);
CameraButton.Layer.CornerRadius = 4f;
CameraButton.TouchUpInside += (s, e) => ChangeCamera();
View.AddSubview(CameraButton);
```

### RecordButton ###

これも同じ要領で移植できます。

**storyboard**
```xml
<button opaque = "NO" contentMode="scaleToFill" contentHorizontalAlignment="center" contentVerticalAlignment="center" buttonType="roundedRect" lineBreakMode="middleTruncation" translatesAutoresizingMaskIntoConstraints="NO" id="eRT-dK-6dM" userLabel="Record">
    <color key = "backgroundColor" red="0.0" green="0.0" blue="0.0" alpha="0.29999999999999999" colorSpace="custom" customColorSpace="sRGB"/>
    <fontDescription key = "fontDescription" type="system" pointSize="20"/>
    <state key = "normal" title="Record">
        <color key = "titleShadowColor" red="0.5" green="0.5" blue="0.5" alpha="1" colorSpace="custom" customColorSpace="sRGB"/>
    </state>
    <userDefinedRuntimeAttributes>
        <userDefinedRuntimeAttribute type = "number" keyPath="layer.cornerRadius">
            <integer key = "value" value="4"/>
        </userDefinedRuntimeAttribute>
    </userDefinedRuntimeAttributes>
    <connections>
        <action selector = "toggleMovieRecording:" destination="BYZ-38-t0r" eventType="touchUpInside" id="9R7-Ok-FpB"/>
    </connections>
</button>
```

全部移植すると以下のようになりますので、<code>InitUI()</code>の先ほど追加したコードの下に以下を追加します。

**C#**
```csharp
RecordButton = new UIButton(UIButtonType.RoundedRect)
{
	Opaque = false,
	ContentMode = UIViewContentMode.ScaleToFill,
	HorizontalAlignment = UIControlContentHorizontalAlignment.Center,
	VerticalAlignment = UIControlContentVerticalAlignment.Center,
	LineBreakMode = UILineBreakMode.MiddleTruncation,
	TranslatesAutoresizingMaskIntoConstraints = false,
	BackgroundColor = UIColor.FromRGBA(0.0f, 0.0f, 0.0f, 0.3f),
};
RecordButton.SetTitle("Record", UIControlState.Normal);
RecordButton.SetTitleShadowColor(UIColor.FromRGBA(0.5f, 0.5f, 0.5f, 1f), UIControlState.Normal);
RecordButton.Layer.CornerRadius = 4f;
RecordButton.TouchUpInside += (s, e) => ToggleMovieRecording();
View.AddSubview(RecordButton);
```

以下執筆中




# 準備 #

今回はApple公式の写真撮影のサンプルアプリを題材にします。以下よりサンプルコードをダウンロードして下さい。

[https://developer.apple.com/library/content/samplecode/AVCam/Introduction/Intro.html](ttps://developer.apple.com/library/content/samplecode/AVCam/Introduction/Intro.html "AVCam-iOS")

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

##　クラス本体 ##

<code>PhotoCaptureDelegate.cs</code>ファイルを作成します。

![](https://github.com/TomohiroSuzuki128/XamiOSHandsOn01/blob/master/images/004.png?raw=true)


それでは<code>PhotoCaptureDelegate.swift</code>ファイルのコードを見てみましょう。

### using ###

importを確認すると

####　Swift ####
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

####　C# ####
```csharp
public class AVCapturePhotoCaptureDelegate : NSObject, IAVCapturePhotoCaptureDelegate, INativeObject, IDisposable
```

とありますので、<code>AVCapturePhotoCaptureDelegate</code>を継承すれば、<code>NSObject</code>を継承し、<code>AVCapturePhotoCaptureDelegate</code>を実装するクラスになります。
よって、以下のようにクラスを定義します。

####　C# ####
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

####　Swift ####
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

####　C# ####
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

####　Swift ####
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

####　C# ####
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

####　Swift ####
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

####　C# ####
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

以下執筆中




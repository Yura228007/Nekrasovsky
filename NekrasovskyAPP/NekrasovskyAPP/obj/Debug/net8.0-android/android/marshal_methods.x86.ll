; ModuleID = 'marshal_methods.x86.ll'
source_filename = "marshal_methods.x86.ll"
target datalayout = "e-m:e-p:32:32-p270:32:32-p271:32:32-p272:64:64-f64:32:64-f80:32-n8:16:32-S128"
target triple = "i686-unknown-linux-android21"

%struct.MarshalMethodName = type {
	i64, ; uint64_t id
	ptr ; char* name
}

%struct.MarshalMethodsManagedClass = type {
	i32, ; uint32_t token
	ptr ; MonoClass klass
}

@assembly_image_cache = dso_local local_unnamed_addr global [367 x ptr] zeroinitializer, align 4

; Each entry maps hash of an assembly name to an index into the `assembly_image_cache` array
@assembly_image_cache_hashes = dso_local local_unnamed_addr constant [728 x i32] [
	i32 2616222, ; 0: System.Net.NetworkInformation.dll => 0x27eb9e => 68
	i32 10166715, ; 1: System.Net.NameResolution.dll => 0x9b21bb => 67
	i32 15721112, ; 2: System.Runtime.Intrinsics.dll => 0xefe298 => 108
	i32 20206211, ; 3: Xamarin.Google.MLKit.Vision.Interfaces.dll => 0x1345283 => 313
	i32 28873261, ; 4: Npgsql.dll => 0x1b8922d => 206
	i32 30793855, ; 5: Xamarin.Google.Android.ODML.Image.dll => 0x1d5e07f => 303
	i32 32687329, ; 6: Xamarin.AndroidX.Lifecycle.Runtime => 0x1f2c4e1 => 260
	i32 34715100, ; 7: Xamarin.Google.Guava.ListenableFuture.dll => 0x211b5dc => 308
	i32 34839235, ; 8: System.IO.FileSystem.DriveInfo => 0x2139ac3 => 48
	i32 39485524, ; 9: System.Net.WebSockets.dll => 0x25a8054 => 80
	i32 40744412, ; 10: Xamarin.AndroidX.Camera.Lifecycle.dll => 0x26db5dc => 227
	i32 42639949, ; 11: System.Threading.Thread => 0x28aa24d => 145
	i32 45981941, ; 12: Xamarin.KotlinX.AtomicFU.Jvm => 0x2bda0f5 => 324
	i32 52581868, ; 13: Xamarin.AndroidX.Concurrent.Futures.Ktx => 0x32255ec => 235
	i32 66541672, ; 14: System.Diagnostics.StackTrace => 0x3f75868 => 30
	i32 67008169, ; 15: zh-Hant\Microsoft.Maui.Controls.resources => 0x3fe76a9 => 362
	i32 68219467, ; 16: System.Security.Cryptography.Primitives => 0x410f24b => 124
	i32 72070932, ; 17: Microsoft.Maui.Graphics.dll => 0x44bb714 => 205
	i32 82292897, ; 18: System.Runtime.CompilerServices.VisualC.dll => 0x4e7b0a1 => 102
	i32 101534019, ; 19: Xamarin.AndroidX.SlidingPaneLayout => 0x60d4943 => 281
	i32 103834273, ; 20: Xamarin.Firebase.Annotations.dll => 0x63062a1 => 294
	i32 117431740, ; 21: System.Runtime.InteropServices => 0x6ffddbc => 107
	i32 120558881, ; 22: Xamarin.AndroidX.SlidingPaneLayout.dll => 0x72f9521 => 281
	i32 122350210, ; 23: System.Threading.Channels.dll => 0x74aea82 => 139
	i32 134690465, ; 24: Xamarin.Kotlin.StdLib.Jdk7.dll => 0x80736a1 => 321
	i32 142721839, ; 25: System.Net.WebHeaderCollection => 0x881c32f => 77
	i32 149972175, ; 26: System.Security.Cryptography.Primitives.dll => 0x8f064cf => 124
	i32 159306688, ; 27: System.ComponentModel.Annotations => 0x97ed3c0 => 13
	i32 165246403, ; 28: Xamarin.AndroidX.Collection.dll => 0x9d975c3 => 231
	i32 166070894, ; 29: Xamarin.KotlinX.AtomicFU.dll => 0x9e60a6e => 323
	i32 176265551, ; 30: System.ServiceProcess => 0xa81994f => 132
	i32 182336117, ; 31: Xamarin.AndroidX.SwipeRefreshLayout.dll => 0xade3a75 => 283
	i32 184328833, ; 32: System.ValueTuple.dll => 0xafca281 => 151
	i32 195452805, ; 33: vi/Microsoft.Maui.Controls.resources.dll => 0xba65f85 => 359
	i32 199333315, ; 34: zh-HK/Microsoft.Maui.Controls.resources.dll => 0xbe195c3 => 360
	i32 205061960, ; 35: System.ComponentModel => 0xc38ff48 => 18
	i32 209399409, ; 36: Xamarin.AndroidX.Browser.dll => 0xc7b2e71 => 224
	i32 218154787, ; 37: Xamarin.AndroidX.Tracing.Tracing.Ktx => 0xd00c723 => 285
	i32 220171995, ; 38: System.Diagnostics.Debug => 0xd1f8edb => 26
	i32 221063263, ; 39: Microsoft.AspNetCore.Http.Connections.Client => 0xd2d285f => 179
	i32 230216969, ; 40: Xamarin.AndroidX.Legacy.Support.Core.Utils.dll => 0xdb8d509 => 253
	i32 230752869, ; 41: Microsoft.CSharp.dll => 0xdc10265 => 1
	i32 231409092, ; 42: System.Linq.Parallel => 0xdcb05c4 => 59
	i32 231814094, ; 43: System.Globalization => 0xdd133ce => 42
	i32 246610117, ; 44: System.Reflection.Emit.Lightweight => 0xeb2f8c5 => 91
	i32 261689757, ; 45: Xamarin.AndroidX.ConstraintLayout.dll => 0xf99119d => 236
	i32 276479776, ; 46: System.Threading.Timer.dll => 0x107abf20 => 147
	i32 278686392, ; 47: Xamarin.AndroidX.Lifecycle.LiveData.dll => 0x109c6ab8 => 256
	i32 280482487, ; 48: Xamarin.AndroidX.Interpolator => 0x10b7d2b7 => 252
	i32 280992041, ; 49: cs/Microsoft.Maui.Controls.resources.dll => 0x10bf9929 => 331
	i32 291076382, ; 50: System.IO.Pipes.AccessControl.dll => 0x1159791e => 54
	i32 298918909, ; 51: System.Net.Ping.dll => 0x11d123fd => 69
	i32 317674968, ; 52: vi\Microsoft.Maui.Controls.resources => 0x12ef55d8 => 359
	i32 318968648, ; 53: Xamarin.AndroidX.Activity.dll => 0x13031348 => 215
	i32 321597661, ; 54: System.Numerics => 0x132b30dd => 83
	i32 336156722, ; 55: ja/Microsoft.Maui.Controls.resources.dll => 0x14095832 => 344
	i32 342366114, ; 56: Xamarin.AndroidX.Lifecycle.Common => 0x146817a2 => 254
	i32 348048101, ; 57: Microsoft.AspNetCore.Http.Connections.Common.dll => 0x14becae5 => 180
	i32 356389973, ; 58: it/Microsoft.Maui.Controls.resources.dll => 0x153e1455 => 343
	i32 360082299, ; 59: System.ServiceModel.Web => 0x15766b7b => 131
	i32 367780167, ; 60: System.IO.Pipes => 0x15ebe147 => 55
	i32 374914964, ; 61: System.Transactions.Local => 0x1658bf94 => 149
	i32 375677976, ; 62: System.Net.ServicePoint.dll => 0x16646418 => 74
	i32 379916513, ; 63: System.Threading.Thread.dll => 0x16a510e1 => 145
	i32 385762202, ; 64: System.Memory.dll => 0x16fe439a => 62
	i32 392610295, ; 65: System.Threading.ThreadPool.dll => 0x1766c1f7 => 146
	i32 395744057, ; 66: _Microsoft.Android.Resource.Designer => 0x17969339 => 363
	i32 403441872, ; 67: WindowsBase => 0x180c08d0 => 165
	i32 425531652, ; 68: Xamarin.AndroidX.Lifecycle.Runtime.Android => 0x195d1904 => 261
	i32 435591531, ; 69: sv/Microsoft.Maui.Controls.resources.dll => 0x19f6996b => 355
	i32 441335492, ; 70: Xamarin.AndroidX.ConstraintLayout.Core => 0x1a4e3ec4 => 237
	i32 442565967, ; 71: System.Collections => 0x1a61054f => 12
	i32 450948140, ; 72: Xamarin.AndroidX.Fragment.dll => 0x1ae0ec2c => 250
	i32 451504562, ; 73: System.Security.Cryptography.X509Certificates => 0x1ae969b2 => 125
	i32 456227837, ; 74: System.Web.HttpUtility.dll => 0x1b317bfd => 152
	i32 458494020, ; 75: Microsoft.AspNetCore.SignalR.Common.dll => 0x1b541044 => 183
	i32 459347974, ; 76: System.Runtime.Serialization.Primitives.dll => 0x1b611806 => 113
	i32 465846621, ; 77: mscorlib => 0x1bc4415d => 166
	i32 469710990, ; 78: System.dll => 0x1bff388e => 164
	i32 476646585, ; 79: Xamarin.AndroidX.Interpolator.dll => 0x1c690cb9 => 252
	i32 485140951, ; 80: Xamarin.Google.Android.DataTransport.TransportRuntime => 0x1ceaa9d7 => 301
	i32 486930444, ; 81: Xamarin.AndroidX.LocalBroadcastManager.dll => 0x1d05f80c => 269
	i32 495452658, ; 82: Xamarin.Google.Android.DataTransport.TransportRuntime.dll => 0x1d8801f2 => 301
	i32 498788369, ; 83: System.ObjectModel => 0x1dbae811 => 84
	i32 500358224, ; 84: id/Microsoft.Maui.Controls.resources.dll => 0x1dd2dc50 => 342
	i32 503918385, ; 85: fi/Microsoft.Maui.Controls.resources.dll => 0x1e092f31 => 336
	i32 507148113, ; 86: Xamarin.Google.Android.DataTransport.TransportApi.dll => 0x1e3a7751 => 299
	i32 513247710, ; 87: Microsoft.Extensions.Primitives.dll => 0x1e9789de => 199
	i32 513617146, ; 88: Xamarin.Google.MLKit.Vision.Common => 0x1e9d2cfa => 312
	i32 525008092, ; 89: SkiaSharp.dll => 0x1f4afcdc => 208
	i32 526420162, ; 90: System.Transactions.dll => 0x1f6088c2 => 150
	i32 527452488, ; 91: Xamarin.Kotlin.StdLib.Jdk7 => 0x1f704948 => 321
	i32 530272170, ; 92: System.Linq.Queryable => 0x1f9b4faa => 60
	i32 539058512, ; 93: Microsoft.Extensions.Logging => 0x20216150 => 195
	i32 540030774, ; 94: System.IO.FileSystem.dll => 0x20303736 => 51
	i32 545304856, ; 95: System.Runtime.Extensions => 0x2080b118 => 103
	i32 546455878, ; 96: System.Runtime.Serialization.Xml => 0x20924146 => 114
	i32 549171840, ; 97: System.Globalization.Calendars => 0x20bbb280 => 40
	i32 557405415, ; 98: Jsr305Binding => 0x213954e7 => 305
	i32 569601784, ; 99: Xamarin.AndroidX.Window.Extensions.Core.Core => 0x21f36ef8 => 293
	i32 577335427, ; 100: System.Security.Cryptography.Cng => 0x22697083 => 120
	i32 592146354, ; 101: pt-BR/Microsoft.Maui.Controls.resources.dll => 0x234b6fb2 => 350
	i32 597488923, ; 102: CommunityToolkit.Maui => 0x239cf51b => 175
	i32 601371474, ; 103: System.IO.IsolatedStorage.dll => 0x23d83352 => 52
	i32 605376203, ; 104: System.IO.Compression.FileSystem => 0x24154ecb => 44
	i32 613668793, ; 105: System.Security.Cryptography.Algorithms => 0x2493d7b9 => 119
	i32 621990341, ; 106: Xamarin.AndroidX.Lifecycle.Runtime.Android.dll => 0x2512d1c5 => 261
	i32 627609679, ; 107: Xamarin.AndroidX.CustomView => 0x2568904f => 242
	i32 627931235, ; 108: nl\Microsoft.Maui.Controls.resources => 0x256d7863 => 348
	i32 639843206, ; 109: Xamarin.AndroidX.Emoji2.ViewsHelper.dll => 0x26233b86 => 248
	i32 643868501, ; 110: System.Net => 0x2660a755 => 81
	i32 662205335, ; 111: System.Text.Encodings.Web.dll => 0x27787397 => 136
	i32 663517072, ; 112: Xamarin.AndroidX.VersionedParcelable => 0x278c7790 => 289
	i32 666292255, ; 113: Xamarin.AndroidX.Arch.Core.Common.dll => 0x27b6d01f => 222
	i32 672442732, ; 114: System.Collections.Concurrent => 0x2814a96c => 8
	i32 679221896, ; 115: Xamarin.KotlinX.AtomicFU => 0x287c1a88 => 323
	i32 683518922, ; 116: System.Net.Security => 0x28bdabca => 73
	i32 688181140, ; 117: ca/Microsoft.Maui.Controls.resources.dll => 0x2904cf94 => 330
	i32 690569205, ; 118: System.Xml.Linq.dll => 0x29293ff5 => 155
	i32 691348768, ; 119: Xamarin.KotlinX.Coroutines.Android.dll => 0x29352520 => 325
	i32 693804605, ; 120: System.Windows => 0x295a9e3d => 154
	i32 699345723, ; 121: System.Reflection.Emit => 0x29af2b3b => 92
	i32 700284507, ; 122: Xamarin.Jetbrains.Annotations => 0x29bd7e5b => 319
	i32 700358131, ; 123: System.IO.Compression.ZipFile => 0x29be9df3 => 45
	i32 706645707, ; 124: ko/Microsoft.Maui.Controls.resources.dll => 0x2a1e8ecb => 345
	i32 709557578, ; 125: de/Microsoft.Maui.Controls.resources.dll => 0x2a4afd4a => 333
	i32 720511267, ; 126: Xamarin.Kotlin.StdLib.Jdk8 => 0x2af22123 => 322
	i32 722857257, ; 127: System.Runtime.Loader.dll => 0x2b15ed29 => 109
	i32 735137430, ; 128: System.Security.SecureString.dll => 0x2bd14e96 => 129
	i32 752232764, ; 129: System.Diagnostics.Contracts.dll => 0x2cd6293c => 25
	i32 755313932, ; 130: Xamarin.Android.Glide.Annotations.dll => 0x2d052d0c => 212
	i32 759454413, ; 131: System.Net.Requests => 0x2d445acd => 72
	i32 762598435, ; 132: System.IO.Pipes.dll => 0x2d745423 => 55
	i32 775507847, ; 133: System.IO.Compression => 0x2e394f87 => 46
	i32 777317022, ; 134: sk\Microsoft.Maui.Controls.resources => 0x2e54ea9e => 354
	i32 782533833, ; 135: Xamarin.Google.AutoValue.Annotations.dll => 0x2ea484c9 => 304
	i32 789151979, ; 136: Microsoft.Extensions.Options => 0x2f0980eb => 198
	i32 790371945, ; 137: Xamarin.AndroidX.CustomView.PoolingContainer.dll => 0x2f1c1e69 => 243
	i32 804715423, ; 138: System.Data.Common => 0x2ff6fb9f => 22
	i32 807930345, ; 139: Xamarin.AndroidX.Lifecycle.LiveData.Core.Ktx.dll => 0x302809e9 => 258
	i32 823281589, ; 140: System.Private.Uri.dll => 0x311247b5 => 86
	i32 830298997, ; 141: System.IO.Compression.Brotli => 0x317d5b75 => 43
	i32 832635846, ; 142: System.Xml.XPath.dll => 0x31a103c6 => 160
	i32 832711436, ; 143: Microsoft.AspNetCore.SignalR.Protocols.Json.dll => 0x31a22b0c => 184
	i32 834051424, ; 144: System.Net.Quic => 0x31b69d60 => 71
	i32 843511501, ; 145: Xamarin.AndroidX.Print => 0x3246f6cd => 274
	i32 865465478, ; 146: zxing.dll => 0x3395f486 => 328
	i32 873119928, ; 147: Microsoft.VisualBasic => 0x340ac0b8 => 3
	i32 877678880, ; 148: System.Globalization.dll => 0x34505120 => 42
	i32 878954865, ; 149: System.Net.Http.Json => 0x3463c971 => 63
	i32 904024072, ; 150: System.ComponentModel.Primitives.dll => 0x35e25008 => 16
	i32 911108515, ; 151: System.IO.MemoryMappedFiles.dll => 0x364e69a3 => 53
	i32 926902833, ; 152: tr/Microsoft.Maui.Controls.resources.dll => 0x373f6a31 => 357
	i32 928116545, ; 153: Xamarin.Google.Guava.ListenableFuture => 0x3751ef41 => 308
	i32 952186615, ; 154: System.Runtime.InteropServices.JavaScript.dll => 0x38c136f7 => 105
	i32 956575887, ; 155: Xamarin.Kotlin.StdLib.Jdk8.dll => 0x3904308f => 322
	i32 966729478, ; 156: Xamarin.Google.Crypto.Tink.Android => 0x399f1f06 => 306
	i32 967690846, ; 157: Xamarin.AndroidX.Lifecycle.Common.dll => 0x39adca5e => 254
	i32 975236339, ; 158: System.Diagnostics.Tracing => 0x3a20ecf3 => 34
	i32 975874589, ; 159: System.Xml.XDocument => 0x3a2aaa1d => 158
	i32 986514023, ; 160: System.Private.DataContractSerialization.dll => 0x3acd0267 => 85
	i32 987214855, ; 161: System.Diagnostics.Tools => 0x3ad7b407 => 32
	i32 992768348, ; 162: System.Collections.dll => 0x3b2c715c => 12
	i32 994442037, ; 163: System.IO.FileSystem => 0x3b45fb35 => 51
	i32 996733531, ; 164: Xamarin.Google.Android.DataTransport.TransportBackendCct => 0x3b68f25b => 300
	i32 1001831731, ; 165: System.IO.UnmanagedMemoryStream.dll => 0x3bb6bd33 => 56
	i32 1012816738, ; 166: Xamarin.AndroidX.SavedState.dll => 0x3c5e5b62 => 278
	i32 1019214401, ; 167: System.Drawing => 0x3cbffa41 => 36
	i32 1028951442, ; 168: Microsoft.Extensions.DependencyInjection.Abstractions => 0x3d548d92 => 193
	i32 1029334545, ; 169: da/Microsoft.Maui.Controls.resources.dll => 0x3d5a6611 => 332
	i32 1031528504, ; 170: Xamarin.Google.ErrorProne.Annotations.dll => 0x3d7be038 => 307
	i32 1035644815, ; 171: Xamarin.AndroidX.AppCompat => 0x3dbaaf8f => 220
	i32 1036536393, ; 172: System.Drawing.Primitives.dll => 0x3dc84a49 => 35
	i32 1044663988, ; 173: System.Linq.Expressions.dll => 0x3e444eb4 => 58
	i32 1052210849, ; 174: Xamarin.AndroidX.Lifecycle.ViewModel.dll => 0x3eb776a1 => 264
	i32 1058641855, ; 175: Microsoft.AspNetCore.Http.Connections.Common => 0x3f1997bf => 180
	i32 1061503568, ; 176: Xamarin.Google.AutoValue.Annotations => 0x3f454250 => 304
	i32 1067306892, ; 177: GoogleGson => 0x3f9dcf8c => 177
	i32 1082857460, ; 178: System.ComponentModel.TypeConverter => 0x408b17f4 => 17
	i32 1084122840, ; 179: Xamarin.Kotlin.StdLib => 0x409e66d8 => 320
	i32 1098259244, ; 180: System => 0x41761b2c => 164
	i32 1118262833, ; 181: ko\Microsoft.Maui.Controls.resources => 0x42a75631 => 345
	i32 1121599056, ; 182: Xamarin.AndroidX.Lifecycle.Runtime.Ktx.dll => 0x42da3e50 => 262
	i32 1122050967, ; 183: Xamarin.Google.Android.ODML.Image => 0x42e12397 => 303
	i32 1127624469, ; 184: Microsoft.Extensions.Logging.Debug => 0x43362f15 => 197
	i32 1145483052, ; 185: System.Windows.Extensions.dll => 0x4446af2c => 210
	i32 1149092582, ; 186: Xamarin.AndroidX.Window => 0x447dc2e6 => 292
	i32 1157931901, ; 187: Microsoft.EntityFrameworkCore.Abstractions => 0x4504a37d => 186
	i32 1168523401, ; 188: pt\Microsoft.Maui.Controls.resources => 0x45a64089 => 351
	i32 1170634674, ; 189: System.Web.dll => 0x45c677b2 => 153
	i32 1175144683, ; 190: Xamarin.AndroidX.VectorDrawable.Animated => 0x460b48eb => 288
	i32 1178241025, ; 191: Xamarin.AndroidX.Navigation.Runtime.dll => 0x463a8801 => 272
	i32 1202000627, ; 192: Microsoft.EntityFrameworkCore.Abstractions.dll => 0x47a512f3 => 186
	i32 1203215381, ; 193: pl/Microsoft.Maui.Controls.resources.dll => 0x47b79c15 => 349
	i32 1203469131, ; 194: Xamarin.Google.MLKit.Common.dll => 0x47bb7b4b => 311
	i32 1204270330, ; 195: Xamarin.AndroidX.Arch.Core.Common => 0x47c7b4fa => 222
	i32 1204575371, ; 196: Microsoft.Extensions.Caching.Memory.dll => 0x47cc5c8b => 189
	i32 1208641965, ; 197: System.Diagnostics.Process => 0x480a69ad => 29
	i32 1219128291, ; 198: System.IO.IsolatedStorage => 0x48aa6be3 => 52
	i32 1233093933, ; 199: Microsoft.AspNetCore.SignalR.Client.Core.dll => 0x497f852d => 182
	i32 1234928153, ; 200: nb/Microsoft.Maui.Controls.resources.dll => 0x499b8219 => 347
	i32 1243150071, ; 201: Xamarin.AndroidX.Window.Extensions.Core.Core.dll => 0x4a18f6f7 => 293
	i32 1246548578, ; 202: Xamarin.AndroidX.Collection.Jvm.dll => 0x4a4cd262 => 232
	i32 1253011324, ; 203: Microsoft.Win32.Registry => 0x4aaf6f7c => 5
	i32 1260983243, ; 204: cs\Microsoft.Maui.Controls.resources => 0x4b2913cb => 331
	i32 1264511973, ; 205: Xamarin.AndroidX.Startup.StartupRuntime.dll => 0x4b5eebe5 => 282
	i32 1264890200, ; 206: Xamarin.KotlinX.Coroutines.Core.dll => 0x4b64b158 => 326
	i32 1267360935, ; 207: Xamarin.AndroidX.VectorDrawable => 0x4b8a64a7 => 287
	i32 1273260888, ; 208: Xamarin.AndroidX.Collection.Ktx => 0x4be46b58 => 233
	i32 1275534314, ; 209: Xamarin.KotlinX.Coroutines.Android => 0x4c071bea => 325
	i32 1278448581, ; 210: Xamarin.AndroidX.Annotation.Jvm => 0x4c3393c5 => 219
	i32 1293217323, ; 211: Xamarin.AndroidX.DrawerLayout.dll => 0x4d14ee2b => 245
	i32 1309188875, ; 212: System.Private.DataContractSerialization => 0x4e08a30b => 85
	i32 1322716291, ; 213: Xamarin.AndroidX.Window.dll => 0x4ed70c83 => 292
	i32 1324164729, ; 214: System.Linq => 0x4eed2679 => 61
	i32 1335329327, ; 215: System.Runtime.Serialization.Json.dll => 0x4f97822f => 112
	i32 1351347447, ; 216: Xamarin.GooglePlayServices.MLKit.BarcodeScanning => 0x508becf7 => 316
	i32 1355368438, ; 217: Xamarin.Google.MLKit.BarcodeScanning.Common.dll => 0x50c947f6 => 310
	i32 1358509622, ; 218: Xamarin.Google.MLKit.BarcodeScanning => 0x50f93636 => 309
	i32 1364015309, ; 219: System.IO => 0x514d38cd => 57
	i32 1373134921, ; 220: zh-Hans\Microsoft.Maui.Controls.resources => 0x51d86049 => 361
	i32 1376866003, ; 221: Xamarin.AndroidX.SavedState => 0x52114ed3 => 278
	i32 1379779777, ; 222: System.Resources.ResourceManager => 0x523dc4c1 => 99
	i32 1379897097, ; 223: Xamarin.JavaX.Inject => 0x523f8f09 => 318
	i32 1402170036, ; 224: System.Configuration.dll => 0x53936ab4 => 19
	i32 1406073936, ; 225: Xamarin.AndroidX.CoordinatorLayout => 0x53cefc50 => 238
	i32 1408764838, ; 226: System.Runtime.Serialization.Formatters.dll => 0x53f80ba6 => 111
	i32 1411638395, ; 227: System.Runtime.CompilerServices.Unsafe => 0x5423e47b => 101
	i32 1414043276, ; 228: Microsoft.AspNetCore.Connections.Abstractions.dll => 0x5448968c => 178
	i32 1422545099, ; 229: System.Runtime.CompilerServices.VisualC => 0x54ca50cb => 102
	i32 1430672901, ; 230: ar\Microsoft.Maui.Controls.resources => 0x55465605 => 329
	i32 1434145427, ; 231: System.Runtime.Handles => 0x557b5293 => 104
	i32 1435222561, ; 232: Xamarin.Google.Crypto.Tink.Android.dll => 0x558bc221 => 306
	i32 1437299793, ; 233: Xamarin.AndroidX.Lifecycle.Common.Jvm => 0x55ab7451 => 255
	i32 1439761251, ; 234: System.Net.Quic.dll => 0x55d10363 => 71
	i32 1441095154, ; 235: Xamarin.AndroidX.Lifecycle.ViewModel.Android => 0x55e55df2 => 265
	i32 1452070440, ; 236: System.Formats.Asn1.dll => 0x568cd628 => 38
	i32 1453312822, ; 237: System.Diagnostics.Tools.dll => 0x569fcb36 => 32
	i32 1457743152, ; 238: System.Runtime.Extensions.dll => 0x56e36530 => 103
	i32 1458022317, ; 239: System.Net.Security.dll => 0x56e7a7ad => 73
	i32 1461004990, ; 240: es\Microsoft.Maui.Controls.resources => 0x57152abe => 335
	i32 1461234159, ; 241: System.Collections.Immutable.dll => 0x5718a9ef => 9
	i32 1461719063, ; 242: System.Security.Cryptography.OpenSsl => 0x57201017 => 123
	i32 1462112819, ; 243: System.IO.Compression.dll => 0x57261233 => 46
	i32 1469204771, ; 244: Xamarin.AndroidX.AppCompat.AppCompatResources => 0x57924923 => 221
	i32 1470490898, ; 245: Microsoft.Extensions.Primitives => 0x57a5e912 => 199
	i32 1479771757, ; 246: System.Collections.Immutable => 0x5833866d => 9
	i32 1480492111, ; 247: System.IO.Compression.Brotli.dll => 0x583e844f => 43
	i32 1487239319, ; 248: Microsoft.Win32.Primitives => 0x58a57897 => 4
	i32 1490025113, ; 249: Xamarin.AndroidX.SavedState.SavedState.Ktx.dll => 0x58cffa99 => 279
	i32 1493001747, ; 250: hi/Microsoft.Maui.Controls.resources.dll => 0x58fd6613 => 339
	i32 1501494919, ; 251: AppDynamics.Agent => 0x597efe87 => 173
	i32 1514721132, ; 252: el/Microsoft.Maui.Controls.resources.dll => 0x5a48cf6c => 334
	i32 1536373174, ; 253: System.Diagnostics.TextWriterTraceListener => 0x5b9331b6 => 31
	i32 1543031311, ; 254: System.Text.RegularExpressions.dll => 0x5bf8ca0f => 138
	i32 1543355203, ; 255: System.Reflection.Emit.dll => 0x5bfdbb43 => 92
	i32 1550322496, ; 256: System.Reflection.Extensions.dll => 0x5c680b40 => 93
	i32 1551623176, ; 257: sk/Microsoft.Maui.Controls.resources.dll => 0x5c7be408 => 354
	i32 1565862583, ; 258: System.IO.FileSystem.Primitives => 0x5d552ab7 => 49
	i32 1566207040, ; 259: System.Threading.Tasks.Dataflow.dll => 0x5d5a6c40 => 141
	i32 1573704789, ; 260: System.Runtime.Serialization.Json => 0x5dccd455 => 112
	i32 1580037396, ; 261: System.Threading.Overlapped => 0x5e2d7514 => 140
	i32 1582372066, ; 262: Xamarin.AndroidX.DocumentFile.dll => 0x5e5114e2 => 244
	i32 1592978981, ; 263: System.Runtime.Serialization.dll => 0x5ef2ee25 => 115
	i32 1597949149, ; 264: Xamarin.Google.ErrorProne.Annotations => 0x5f3ec4dd => 307
	i32 1601112923, ; 265: System.Xml.Serialization => 0x5f6f0b5b => 157
	i32 1604827217, ; 266: System.Net.WebClient => 0x5fa7b851 => 76
	i32 1618516317, ; 267: System.Net.WebSockets.Client.dll => 0x6078995d => 79
	i32 1622152042, ; 268: Xamarin.AndroidX.Loader.dll => 0x60b0136a => 268
	i32 1622358360, ; 269: System.Dynamic.Runtime => 0x60b33958 => 37
	i32 1624863272, ; 270: Xamarin.AndroidX.ViewPager2 => 0x60d97228 => 291
	i32 1634654947, ; 271: CommunityToolkit.Maui.Core.dll => 0x616edae3 => 176
	i32 1635184631, ; 272: Xamarin.AndroidX.Emoji2.ViewsHelper => 0x6176eff7 => 248
	i32 1636350590, ; 273: Xamarin.AndroidX.CursorAdapter => 0x6188ba7e => 241
	i32 1639515021, ; 274: System.Net.Http.dll => 0x61b9038d => 64
	i32 1639986890, ; 275: System.Text.RegularExpressions => 0x61c036ca => 138
	i32 1641389582, ; 276: System.ComponentModel.EventBasedAsync.dll => 0x61d59e0e => 15
	i32 1657153582, ; 277: System.Runtime => 0x62c6282e => 116
	i32 1658241508, ; 278: Xamarin.AndroidX.Tracing.Tracing.dll => 0x62d6c1e4 => 284
	i32 1658251792, ; 279: Xamarin.Google.Android.Material.dll => 0x62d6ea10 => 302
	i32 1670060433, ; 280: Xamarin.AndroidX.ConstraintLayout => 0x638b1991 => 236
	i32 1675553242, ; 281: System.IO.FileSystem.DriveInfo.dll => 0x63dee9da => 48
	i32 1677501392, ; 282: System.Net.Primitives.dll => 0x63fca3d0 => 70
	i32 1678508291, ; 283: System.Net.WebSockets => 0x640c0103 => 80
	i32 1679769178, ; 284: System.Security.Cryptography => 0x641f3e5a => 126
	i32 1689493916, ; 285: Microsoft.EntityFrameworkCore.dll => 0x64b3a19c => 185
	i32 1691477237, ; 286: System.Reflection.Metadata => 0x64d1e4f5 => 94
	i32 1696967625, ; 287: System.Security.Cryptography.Csp => 0x6525abc9 => 121
	i32 1701541528, ; 288: System.Diagnostics.Debug.dll => 0x656b7698 => 26
	i32 1718006957, ; 289: Xamarin.GooglePlayServices.MLKit.BarcodeScanning.dll => 0x6666b4ad => 316
	i32 1720223769, ; 290: Xamarin.AndroidX.Lifecycle.LiveData.Core.Ktx => 0x66888819 => 258
	i32 1726116996, ; 291: System.Reflection.dll => 0x66e27484 => 97
	i32 1728033016, ; 292: System.Diagnostics.FileVersionInfo.dll => 0x66ffb0f8 => 28
	i32 1729485958, ; 293: Xamarin.AndroidX.CardView.dll => 0x6715dc86 => 230
	i32 1736233607, ; 294: ro/Microsoft.Maui.Controls.resources.dll => 0x677cd287 => 352
	i32 1743415430, ; 295: ca\Microsoft.Maui.Controls.resources => 0x67ea6886 => 330
	i32 1744735666, ; 296: System.Transactions.Local.dll => 0x67fe8db2 => 149
	i32 1746115085, ; 297: System.IO.Pipelines.dll => 0x68139a0d => 209
	i32 1746316138, ; 298: Mono.Android.Export => 0x6816ab6a => 169
	i32 1750313021, ; 299: Microsoft.Win32.Primitives.dll => 0x6853a83d => 4
	i32 1758240030, ; 300: System.Resources.Reader.dll => 0x68cc9d1e => 98
	i32 1763938596, ; 301: System.Diagnostics.TraceSource.dll => 0x69239124 => 33
	i32 1765942094, ; 302: System.Reflection.Extensions => 0x6942234e => 93
	i32 1766324549, ; 303: Xamarin.AndroidX.SwipeRefreshLayout => 0x6947f945 => 283
	i32 1770582343, ; 304: Microsoft.Extensions.Logging.dll => 0x6988f147 => 195
	i32 1772434258, ; 305: NekrasovskyAPP => 0x69a53352 => 0
	i32 1776026572, ; 306: System.Core.dll => 0x69dc03cc => 21
	i32 1777075843, ; 307: System.Globalization.Extensions.dll => 0x69ec0683 => 41
	i32 1780572499, ; 308: Mono.Android.Runtime.dll => 0x6a216153 => 170
	i32 1782862114, ; 309: ms\Microsoft.Maui.Controls.resources => 0x6a445122 => 346
	i32 1788241197, ; 310: Xamarin.AndroidX.Fragment => 0x6a96652d => 250
	i32 1793755602, ; 311: he\Microsoft.Maui.Controls.resources => 0x6aea89d2 => 338
	i32 1808609942, ; 312: Xamarin.AndroidX.Loader => 0x6bcd3296 => 268
	i32 1813058853, ; 313: Xamarin.Kotlin.StdLib.dll => 0x6c111525 => 320
	i32 1813201214, ; 314: Xamarin.Google.Android.Material => 0x6c13413e => 302
	i32 1818569960, ; 315: Xamarin.AndroidX.Navigation.UI.dll => 0x6c652ce8 => 273
	i32 1818787751, ; 316: Microsoft.VisualBasic.Core => 0x6c687fa7 => 2
	i32 1824175904, ; 317: System.Text.Encoding.Extensions => 0x6cbab720 => 134
	i32 1824722060, ; 318: System.Runtime.Serialization.Formatters => 0x6cc30c8c => 111
	i32 1828688058, ; 319: Microsoft.Extensions.Logging.Abstractions.dll => 0x6cff90ba => 196
	i32 1829150748, ; 320: System.Windows.Extensions => 0x6d06a01c => 210
	i32 1842015223, ; 321: uk/Microsoft.Maui.Controls.resources.dll => 0x6dcaebf7 => 358
	i32 1847515442, ; 322: Xamarin.Android.Glide.Annotations => 0x6e1ed932 => 212
	i32 1853025655, ; 323: sv\Microsoft.Maui.Controls.resources => 0x6e72ed77 => 355
	i32 1858542181, ; 324: System.Linq.Expressions => 0x6ec71a65 => 58
	i32 1866818530, ; 325: Xamarin.AndroidX.Camera.Video => 0x6f4563e2 => 228
	i32 1870277092, ; 326: System.Reflection.Primitives => 0x6f7a29e4 => 95
	i32 1875935024, ; 327: fr\Microsoft.Maui.Controls.resources => 0x6fd07f30 => 337
	i32 1876173635, ; 328: Xamarin.Firebase.Encoders.Proto => 0x6fd42343 => 298
	i32 1879696579, ; 329: System.Formats.Tar.dll => 0x7009e4c3 => 39
	i32 1885316902, ; 330: Xamarin.AndroidX.Arch.Core.Runtime.dll => 0x705fa726 => 223
	i32 1888955245, ; 331: System.Diagnostics.Contracts => 0x70972b6d => 25
	i32 1889954781, ; 332: System.Reflection.Metadata.dll => 0x70a66bdd => 94
	i32 1898237753, ; 333: System.Reflection.DispatchProxy => 0x7124cf39 => 89
	i32 1900610850, ; 334: System.Resources.ResourceManager.dll => 0x71490522 => 99
	i32 1908813208, ; 335: Xamarin.GooglePlayServices.Basement => 0x71c62d98 => 315
	i32 1910275211, ; 336: System.Collections.NonGeneric.dll => 0x71dc7c8b => 10
	i32 1939592360, ; 337: System.Private.Xml.Linq => 0x739bd4a8 => 87
	i32 1945717188, ; 338: Microsoft.AspNetCore.SignalR.Client.Core => 0x73f949c4 => 182
	i32 1956758971, ; 339: System.Resources.Writer => 0x74a1c5bb => 100
	i32 1961813231, ; 340: Xamarin.AndroidX.Security.SecurityCrypto.dll => 0x74eee4ef => 280
	i32 1967334205, ; 341: Microsoft.AspNetCore.SignalR.Common => 0x7543233d => 183
	i32 1968388702, ; 342: Microsoft.Extensions.Configuration.dll => 0x75533a5e => 190
	i32 1985761444, ; 343: Xamarin.Android.Glide.GifDecoder => 0x765c50a4 => 214
	i32 2003115576, ; 344: el\Microsoft.Maui.Controls.resources => 0x77651e38 => 334
	i32 2011961780, ; 345: System.Buffers.dll => 0x77ec19b4 => 7
	i32 2019465201, ; 346: Xamarin.AndroidX.Lifecycle.ViewModel => 0x785e97f1 => 264
	i32 2025202353, ; 347: ar/Microsoft.Maui.Controls.resources.dll => 0x78b622b1 => 329
	i32 2031763787, ; 348: Xamarin.Android.Glide => 0x791a414b => 211
	i32 2045470958, ; 349: System.Private.Xml => 0x79eb68ee => 88
	i32 2055257422, ; 350: Xamarin.AndroidX.Lifecycle.LiveData.Core.dll => 0x7a80bd4e => 257
	i32 2060060697, ; 351: System.Windows.dll => 0x7aca0819 => 154
	i32 2066184531, ; 352: de\Microsoft.Maui.Controls.resources => 0x7b277953 => 333
	i32 2070888862, ; 353: System.Diagnostics.TraceSource => 0x7b6f419e => 33
	i32 2079903147, ; 354: System.Runtime.dll => 0x7bf8cdab => 116
	i32 2090596640, ; 355: System.Numerics.Vectors => 0x7c9bf920 => 82
	i32 2100944304, ; 356: BarcodeScanning.Native.Maui.dll => 0x7d39ddb0 => 174
	i32 2124230737, ; 357: Xamarin.Google.Android.DataTransport.TransportBackendCct.dll => 0x7e9d3051 => 300
	i32 2127167465, ; 358: System.Console => 0x7ec9ffe9 => 20
	i32 2129483829, ; 359: Xamarin.GooglePlayServices.Base.dll => 0x7eed5835 => 314
	i32 2142473426, ; 360: System.Collections.Specialized => 0x7fb38cd2 => 11
	i32 2143790110, ; 361: System.Xml.XmlSerializer.dll => 0x7fc7a41e => 162
	i32 2146852085, ; 362: Microsoft.VisualBasic.dll => 0x7ff65cf5 => 3
	i32 2159891885, ; 363: Microsoft.Maui => 0x80bd55ad => 203
	i32 2169148018, ; 364: hu\Microsoft.Maui.Controls.resources => 0x814a9272 => 341
	i32 2174878672, ; 365: Xamarin.Firebase.Annotations => 0x81a203d0 => 294
	i32 2181898931, ; 366: Microsoft.Extensions.Options.dll => 0x820d22b3 => 198
	i32 2188559649, ; 367: Xamarin.Google.MLKit.BarcodeScanning.dll => 0x8272c521 => 309
	i32 2192057212, ; 368: Microsoft.Extensions.Logging.Abstractions => 0x82a8237c => 196
	i32 2193016926, ; 369: System.ObjectModel.dll => 0x82b6c85e => 84
	i32 2201107256, ; 370: Xamarin.KotlinX.Coroutines.Core.Jvm.dll => 0x83323b38 => 327
	i32 2201231467, ; 371: System.Net.Http => 0x8334206b => 64
	i32 2207618523, ; 372: it\Microsoft.Maui.Controls.resources => 0x839595db => 343
	i32 2217644978, ; 373: Xamarin.AndroidX.VectorDrawable.Animated.dll => 0x842e93b2 => 288
	i32 2222056684, ; 374: System.Threading.Tasks.Parallel => 0x8471e4ec => 143
	i32 2229158877, ; 375: Microsoft.Extensions.Features.dll => 0x84de43dd => 194
	i32 2244775296, ; 376: Xamarin.AndroidX.LocalBroadcastManager => 0x85cc8d80 => 269
	i32 2252106437, ; 377: System.Xml.Serialization.dll => 0x863c6ac5 => 157
	i32 2252897993, ; 378: Microsoft.EntityFrameworkCore => 0x86487ec9 => 185
	i32 2256313426, ; 379: System.Globalization.Extensions => 0x867c9c52 => 41
	i32 2265110946, ; 380: System.Security.AccessControl.dll => 0x8702d9a2 => 117
	i32 2266799131, ; 381: Microsoft.Extensions.Configuration.Abstractions => 0x871c9c1b => 191
	i32 2267999099, ; 382: Xamarin.Android.Glide.DiskLruCache.dll => 0x872eeb7b => 213
	i32 2270573516, ; 383: fr/Microsoft.Maui.Controls.resources.dll => 0x875633cc => 337
	i32 2279755925, ; 384: Xamarin.AndroidX.RecyclerView.dll => 0x87e25095 => 276
	i32 2293034957, ; 385: System.ServiceModel.Web.dll => 0x88acefcd => 131
	i32 2294913272, ; 386: Npgsql => 0x88c998f8 => 206
	i32 2295906218, ; 387: System.Net.Sockets => 0x88d8bfaa => 75
	i32 2298471582, ; 388: System.Net.Mail => 0x88ffe49e => 66
	i32 2303942373, ; 389: nb\Microsoft.Maui.Controls.resources => 0x89535ee5 => 347
	i32 2305521784, ; 390: System.Private.CoreLib.dll => 0x896b7878 => 172
	i32 2315684594, ; 391: Xamarin.AndroidX.Annotation.dll => 0x8a068af2 => 217
	i32 2319144366, ; 392: Microsoft.AspNetCore.SignalR.Client => 0x8a3b55ae => 181
	i32 2320631194, ; 393: System.Threading.Tasks.Parallel.dll => 0x8a52059a => 143
	i32 2334995809, ; 394: Npgsql.EntityFrameworkCore.PostgreSQL.dll => 0x8b2d3561 => 207
	i32 2340441535, ; 395: System.Runtime.InteropServices.RuntimeInformation.dll => 0x8b804dbf => 106
	i32 2344264397, ; 396: System.ValueTuple => 0x8bbaa2cd => 151
	i32 2353062107, ; 397: System.Net.Primitives => 0x8c40e0db => 70
	i32 2368005991, ; 398: System.Xml.ReaderWriter.dll => 0x8d24e767 => 156
	i32 2371007202, ; 399: Microsoft.Extensions.Configuration => 0x8d52b2e2 => 190
	i32 2378619854, ; 400: System.Security.Cryptography.Csp.dll => 0x8dc6dbce => 121
	i32 2383496789, ; 401: System.Security.Principal.Windows.dll => 0x8e114655 => 127
	i32 2395872292, ; 402: id\Microsoft.Maui.Controls.resources => 0x8ece1c24 => 342
	i32 2401565422, ; 403: System.Web.HttpUtility => 0x8f24faee => 152
	i32 2403452196, ; 404: Xamarin.AndroidX.Emoji2.dll => 0x8f41c524 => 247
	i32 2418341376, ; 405: Xamarin.AndroidX.Camera.Video.dll => 0x9024f600 => 228
	i32 2421380589, ; 406: System.Threading.Tasks.Dataflow => 0x905355ed => 141
	i32 2423080555, ; 407: Xamarin.AndroidX.Collection.Ktx.dll => 0x906d466b => 233
	i32 2425270691, ; 408: Xamarin.Google.MLKit.BarcodeScanning.Common => 0x908eb1a3 => 310
	i32 2427813419, ; 409: hi\Microsoft.Maui.Controls.resources => 0x90b57e2b => 339
	i32 2435356389, ; 410: System.Console.dll => 0x912896e5 => 20
	i32 2435904999, ; 411: System.ComponentModel.DataAnnotations.dll => 0x9130f5e7 => 14
	i32 2454642406, ; 412: System.Text.Encoding.dll => 0x924edee6 => 135
	i32 2458678730, ; 413: System.Net.Sockets.dll => 0x928c75ca => 75
	i32 2459001652, ; 414: System.Linq.Parallel.dll => 0x92916334 => 59
	i32 2465532216, ; 415: Xamarin.AndroidX.ConstraintLayout.Core.dll => 0x92f50938 => 237
	i32 2471841756, ; 416: netstandard.dll => 0x93554fdc => 167
	i32 2475788418, ; 417: Java.Interop.dll => 0x93918882 => 168
	i32 2480646305, ; 418: Microsoft.Maui.Controls => 0x93dba8a1 => 201
	i32 2483903535, ; 419: System.ComponentModel.EventBasedAsync => 0x940d5c2f => 15
	i32 2484371297, ; 420: System.Net.ServicePoint => 0x94147f61 => 74
	i32 2490993605, ; 421: System.AppContext.dll => 0x94798bc5 => 6
	i32 2501346920, ; 422: System.Data.DataSetExtensions => 0x95178668 => 23
	i32 2505896520, ; 423: Xamarin.AndroidX.Lifecycle.Runtime.dll => 0x955cf248 => 260
	i32 2522472828, ; 424: Xamarin.Android.Glide.dll => 0x9659e17c => 211
	i32 2538310050, ; 425: System.Reflection.Emit.Lightweight.dll => 0x974b89a2 => 91
	i32 2550873716, ; 426: hr\Microsoft.Maui.Controls.resources => 0x980b3e74 => 340
	i32 2562349572, ; 427: Microsoft.CSharp => 0x98ba5a04 => 1
	i32 2570120770, ; 428: System.Text.Encodings.Web => 0x9930ee42 => 136
	i32 2577256205, ; 429: Xamarin.AndroidX.Lifecycle.Runtime.Ktx.Android => 0x999dcf0d => 263
	i32 2581783588, ; 430: Xamarin.AndroidX.Lifecycle.Runtime.Ktx => 0x99e2e424 => 262
	i32 2581819634, ; 431: Xamarin.AndroidX.VectorDrawable.dll => 0x99e370f2 => 287
	i32 2585220780, ; 432: System.Text.Encoding.Extensions.dll => 0x9a1756ac => 134
	i32 2585805581, ; 433: System.Net.Ping => 0x9a20430d => 69
	i32 2589602615, ; 434: System.Threading.ThreadPool => 0x9a5a3337 => 146
	i32 2593496499, ; 435: pl\Microsoft.Maui.Controls.resources => 0x9a959db3 => 349
	i32 2605712449, ; 436: Xamarin.KotlinX.Coroutines.Core.Jvm => 0x9b500441 => 327
	i32 2615233544, ; 437: Xamarin.AndroidX.Fragment.Ktx => 0x9be14c08 => 251
	i32 2616218305, ; 438: Microsoft.Extensions.Logging.Debug.dll => 0x9bf052c1 => 197
	i32 2617129537, ; 439: System.Private.Xml.dll => 0x9bfe3a41 => 88
	i32 2618712057, ; 440: System.Reflection.TypeExtensions.dll => 0x9c165ff9 => 96
	i32 2620111890, ; 441: Xamarin.Firebase.Encoders.dll => 0x9c2bbc12 => 296
	i32 2620871830, ; 442: Xamarin.AndroidX.CursorAdapter.dll => 0x9c375496 => 241
	i32 2624644809, ; 443: Xamarin.AndroidX.DynamicAnimation => 0x9c70e6c9 => 246
	i32 2626831493, ; 444: ja\Microsoft.Maui.Controls.resources => 0x9c924485 => 344
	i32 2627185994, ; 445: System.Diagnostics.TextWriterTraceListener.dll => 0x9c97ad4a => 31
	i32 2629843544, ; 446: System.IO.Compression.ZipFile.dll => 0x9cc03a58 => 45
	i32 2633051222, ; 447: Xamarin.AndroidX.Lifecycle.LiveData => 0x9cf12c56 => 256
	i32 2634653062, ; 448: Microsoft.EntityFrameworkCore.Relational.dll => 0x9d099d86 => 187
	i32 2637500010, ; 449: Microsoft.Extensions.Features => 0x9d350e6a => 194
	i32 2639764100, ; 450: Xamarin.Firebase.Encoders => 0x9d579a84 => 296
	i32 2663391936, ; 451: Xamarin.Android.Glide.DiskLruCache => 0x9ec022c0 => 213
	i32 2663698177, ; 452: System.Runtime.Loader => 0x9ec4cf01 => 109
	i32 2664396074, ; 453: System.Xml.XDocument.dll => 0x9ecf752a => 158
	i32 2665622720, ; 454: System.Drawing.Primitives => 0x9ee22cc0 => 35
	i32 2671474046, ; 455: Xamarin.KotlinX.Coroutines.Core => 0x9f3b757e => 326
	i32 2676780864, ; 456: System.Data.Common.dll => 0x9f8c6f40 => 22
	i32 2686887180, ; 457: System.Runtime.Serialization.Xml.dll => 0xa026a50c => 114
	i32 2693849962, ; 458: System.IO.dll => 0xa090e36a => 57
	i32 2701096212, ; 459: Xamarin.AndroidX.Tracing.Tracing => 0xa0ff7514 => 284
	i32 2715334215, ; 460: System.Threading.Tasks.dll => 0xa1d8b647 => 144
	i32 2717744543, ; 461: System.Security.Claims => 0xa1fd7d9f => 118
	i32 2719963679, ; 462: System.Security.Cryptography.Cng.dll => 0xa21f5a1f => 120
	i32 2724373263, ; 463: System.Runtime.Numerics.dll => 0xa262a30f => 110
	i32 2732626843, ; 464: Xamarin.AndroidX.Activity => 0xa2e0939b => 215
	i32 2735172069, ; 465: System.Threading.Channels => 0xa30769e5 => 139
	i32 2737747696, ; 466: Xamarin.AndroidX.AppCompat.AppCompatResources.dll => 0xa32eb6f0 => 221
	i32 2740948882, ; 467: System.IO.Pipes.AccessControl => 0xa35f8f92 => 54
	i32 2748088231, ; 468: System.Runtime.InteropServices.JavaScript => 0xa3cc7fa7 => 105
	i32 2752995522, ; 469: pt-BR\Microsoft.Maui.Controls.resources => 0xa41760c2 => 350
	i32 2758225723, ; 470: Microsoft.Maui.Controls.Xaml => 0xa4672f3b => 202
	i32 2764765095, ; 471: Microsoft.Maui.dll => 0xa4caf7a7 => 203
	i32 2765824710, ; 472: System.Text.Encoding.CodePages.dll => 0xa4db22c6 => 133
	i32 2766642685, ; 473: Xamarin.AndroidX.Lifecycle.ViewModel.Android.dll => 0xa4e79dfd => 265
	i32 2770495804, ; 474: Xamarin.Jetbrains.Annotations.dll => 0xa522693c => 319
	i32 2778768386, ; 475: Xamarin.AndroidX.ViewPager.dll => 0xa5a0a402 => 290
	i32 2779977773, ; 476: Xamarin.AndroidX.ResourceInspection.Annotation.dll => 0xa5b3182d => 277
	i32 2780199943, ; 477: Xamarin.AndroidX.Lifecycle.Common.Jvm.dll => 0xa5b67c07 => 255
	i32 2785988530, ; 478: th\Microsoft.Maui.Controls.resources => 0xa60ecfb2 => 356
	i32 2788224221, ; 479: Xamarin.AndroidX.Fragment.Ktx.dll => 0xa630ecdd => 251
	i32 2801831435, ; 480: Microsoft.Maui.Graphics => 0xa7008e0b => 205
	i32 2803228030, ; 481: System.Xml.XPath.XDocument.dll => 0xa715dd7e => 159
	i32 2804607052, ; 482: Xamarin.Firebase.Components.dll => 0xa72ae84c => 295
	i32 2806116107, ; 483: es/Microsoft.Maui.Controls.resources.dll => 0xa741ef0b => 335
	i32 2810250172, ; 484: Xamarin.AndroidX.CoordinatorLayout.dll => 0xa78103bc => 238
	i32 2819470561, ; 485: System.Xml.dll => 0xa80db4e1 => 163
	i32 2821205001, ; 486: System.ServiceProcess.dll => 0xa8282c09 => 132
	i32 2821294376, ; 487: Xamarin.AndroidX.ResourceInspection.Annotation => 0xa8298928 => 277
	i32 2824502124, ; 488: System.Xml.XmlDocument => 0xa85a7b6c => 161
	i32 2828186339, ; 489: Xamarin.AndroidX.Concurrent.Futures.Ktx.dll => 0xa892b2e3 => 235
	i32 2831556043, ; 490: nl/Microsoft.Maui.Controls.resources.dll => 0xa8c61dcb => 348
	i32 2838993487, ; 491: Xamarin.AndroidX.Lifecycle.ViewModel.Ktx.dll => 0xa9379a4f => 266
	i32 2847418871, ; 492: Xamarin.GooglePlayServices.Base => 0xa9b829f7 => 314
	i32 2847789619, ; 493: Microsoft.EntityFrameworkCore.Relational => 0xa9bdd233 => 187
	i32 2849599387, ; 494: System.Threading.Overlapped.dll => 0xa9d96f9b => 140
	i32 2853208004, ; 495: Xamarin.AndroidX.ViewPager => 0xaa107fc4 => 290
	i32 2855708567, ; 496: Xamarin.AndroidX.Transition => 0xaa36a797 => 286
	i32 2861098320, ; 497: Mono.Android.Export.dll => 0xaa88e550 => 169
	i32 2861189240, ; 498: Microsoft.Maui.Essentials => 0xaa8a4878 => 204
	i32 2868099152, ; 499: Xamarin.Google.MLKit.Vision.Common.dll => 0xaaf3b850 => 312
	i32 2868488919, ; 500: CommunityToolkit.Maui.Core => 0xaaf9aad7 => 176
	i32 2870099610, ; 501: Xamarin.AndroidX.Activity.Ktx.dll => 0xab123e9a => 216
	i32 2875164099, ; 502: Jsr305Binding.dll => 0xab5f85c3 => 305
	i32 2875220617, ; 503: System.Globalization.Calendars.dll => 0xab606289 => 40
	i32 2875347124, ; 504: Microsoft.AspNetCore.Http.Connections.Client.dll => 0xab6250b4 => 179
	i32 2884993177, ; 505: Xamarin.AndroidX.ExifInterface => 0xabf58099 => 249
	i32 2887636118, ; 506: System.Net.dll => 0xac1dd496 => 81
	i32 2899753641, ; 507: System.IO.UnmanagedMemoryStream => 0xacd6baa9 => 56
	i32 2900621748, ; 508: System.Dynamic.Runtime.dll => 0xace3f9b4 => 37
	i32 2901442782, ; 509: System.Reflection => 0xacf080de => 97
	i32 2905242038, ; 510: mscorlib.dll => 0xad2a79b6 => 166
	i32 2909740682, ; 511: System.Private.CoreLib => 0xad6f1e8a => 172
	i32 2916838712, ; 512: Xamarin.AndroidX.ViewPager2.dll => 0xaddb6d38 => 291
	i32 2919462931, ; 513: System.Numerics.Vectors.dll => 0xae037813 => 82
	i32 2921128767, ; 514: Xamarin.AndroidX.Annotation.Experimental.dll => 0xae1ce33f => 218
	i32 2936416060, ; 515: System.Resources.Reader => 0xaf06273c => 98
	i32 2940926066, ; 516: System.Diagnostics.StackTrace.dll => 0xaf4af872 => 30
	i32 2942453041, ; 517: System.Xml.XPath.XDocument => 0xaf624531 => 159
	i32 2959614098, ; 518: System.ComponentModel.dll => 0xb0682092 => 18
	i32 2965157864, ; 519: Xamarin.AndroidX.Camera.View => 0xb0bcb7e8 => 229
	i32 2968338931, ; 520: System.Security.Principal.Windows => 0xb0ed41f3 => 127
	i32 2972252294, ; 521: System.Security.Cryptography.Algorithms.dll => 0xb128f886 => 119
	i32 2978675010, ; 522: Xamarin.AndroidX.DrawerLayout => 0xb18af942 => 245
	i32 2987532451, ; 523: Xamarin.AndroidX.Security.SecurityCrypto => 0xb21220a3 => 280
	i32 2991449226, ; 524: Xamarin.AndroidX.Camera.Core => 0xb24de48a => 226
	i32 2996846495, ; 525: Xamarin.AndroidX.Lifecycle.Process.dll => 0xb2a03f9f => 259
	i32 3000842441, ; 526: Xamarin.AndroidX.Camera.View.dll => 0xb2dd38c9 => 229
	i32 3016983068, ; 527: Xamarin.AndroidX.Startup.StartupRuntime => 0xb3d3821c => 282
	i32 3023353419, ; 528: WindowsBase.dll => 0xb434b64b => 165
	i32 3024354802, ; 529: Xamarin.AndroidX.Legacy.Support.Core.Utils => 0xb443fdf2 => 253
	i32 3038032645, ; 530: _Microsoft.Android.Resource.Designer.dll => 0xb514b305 => 363
	i32 3047751430, ; 531: Xamarin.AndroidX.Camera.Core.dll => 0xb5a8ff06 => 226
	i32 3056245963, ; 532: Xamarin.AndroidX.SavedState.SavedState.Ktx => 0xb62a9ccb => 279
	i32 3057625584, ; 533: Xamarin.AndroidX.Navigation.Common => 0xb63fa9f0 => 270
	i32 3058099980, ; 534: Xamarin.GooglePlayServices.Tasks => 0xb646e70c => 317
	i32 3059408633, ; 535: Mono.Android.Runtime => 0xb65adef9 => 170
	i32 3059793426, ; 536: System.ComponentModel.Primitives => 0xb660be12 => 16
	i32 3069363400, ; 537: Microsoft.Extensions.Caching.Abstractions.dll => 0xb6f2c4c8 => 188
	i32 3075834255, ; 538: System.Threading.Tasks => 0xb755818f => 144
	i32 3077302341, ; 539: hu/Microsoft.Maui.Controls.resources.dll => 0xb76be845 => 341
	i32 3090735792, ; 540: System.Security.Cryptography.X509Certificates.dll => 0xb838e2b0 => 125
	i32 3099732863, ; 541: System.Security.Claims.dll => 0xb8c22b7f => 118
	i32 3103600923, ; 542: System.Formats.Asn1 => 0xb8fd311b => 38
	i32 3111772706, ; 543: System.Runtime.Serialization => 0xb979e222 => 115
	i32 3121463068, ; 544: System.IO.FileSystem.AccessControl.dll => 0xba0dbf1c => 47
	i32 3124832203, ; 545: System.Threading.Tasks.Extensions => 0xba4127cb => 142
	i32 3132293585, ; 546: System.Security.AccessControl => 0xbab301d1 => 117
	i32 3147165239, ; 547: System.Diagnostics.Tracing.dll => 0xbb95ee37 => 34
	i32 3148237826, ; 548: GoogleGson.dll => 0xbba64c02 => 177
	i32 3155362983, ; 549: Xamarin.Google.Android.DataTransport.TransportApi => 0xbc1304a7 => 299
	i32 3159123045, ; 550: System.Reflection.Primitives.dll => 0xbc4c6465 => 95
	i32 3160747431, ; 551: System.IO.MemoryMappedFiles => 0xbc652da7 => 53
	i32 3178803400, ; 552: Xamarin.AndroidX.Navigation.Fragment.dll => 0xbd78b0c8 => 271
	i32 3192346100, ; 553: System.Security.SecureString => 0xbe4755f4 => 129
	i32 3193515020, ; 554: System.Web => 0xbe592c0c => 153
	i32 3195844289, ; 555: Microsoft.Extensions.Caching.Abstractions => 0xbe7cb6c1 => 188
	i32 3204380047, ; 556: System.Data.dll => 0xbefef58f => 24
	i32 3209718065, ; 557: System.Xml.XmlDocument.dll => 0xbf506931 => 161
	i32 3211777861, ; 558: Xamarin.AndroidX.DocumentFile => 0xbf6fd745 => 244
	i32 3215347189, ; 559: zxing => 0xbfa64df5 => 328
	i32 3220365878, ; 560: System.Threading => 0xbff2e236 => 148
	i32 3226221578, ; 561: System.Runtime.Handles.dll => 0xc04c3c0a => 104
	i32 3230466174, ; 562: Xamarin.GooglePlayServices.Basement.dll => 0xc08d007e => 315
	i32 3251039220, ; 563: System.Reflection.DispatchProxy.dll => 0xc1c6ebf4 => 89
	i32 3258312781, ; 564: Xamarin.AndroidX.CardView => 0xc235e84d => 230
	i32 3265493905, ; 565: System.Linq.Queryable.dll => 0xc2a37b91 => 60
	i32 3265893370, ; 566: System.Threading.Tasks.Extensions.dll => 0xc2a993fa => 142
	i32 3277815716, ; 567: System.Resources.Writer.dll => 0xc35f7fa4 => 100
	i32 3279906254, ; 568: Microsoft.Win32.Registry.dll => 0xc37f65ce => 5
	i32 3280506390, ; 569: System.ComponentModel.Annotations.dll => 0xc3888e16 => 13
	i32 3290767353, ; 570: System.Security.Cryptography.Encoding => 0xc4251ff9 => 122
	i32 3299363146, ; 571: System.Text.Encoding => 0xc4a8494a => 135
	i32 3303498502, ; 572: System.Diagnostics.FileVersionInfo => 0xc4e76306 => 28
	i32 3305363605, ; 573: fi\Microsoft.Maui.Controls.resources => 0xc503d895 => 336
	i32 3316684772, ; 574: System.Net.Requests.dll => 0xc5b097e4 => 72
	i32 3317135071, ; 575: Xamarin.AndroidX.CustomView.dll => 0xc5b776df => 242
	i32 3317144872, ; 576: System.Data => 0xc5b79d28 => 24
	i32 3340387945, ; 577: SkiaSharp => 0xc71a4669 => 208
	i32 3340431453, ; 578: Xamarin.AndroidX.Arch.Core.Runtime => 0xc71af05d => 223
	i32 3345895724, ; 579: Xamarin.AndroidX.ProfileInstaller.ProfileInstaller.dll => 0xc76e512c => 275
	i32 3346324047, ; 580: Xamarin.AndroidX.Navigation.Runtime => 0xc774da4f => 272
	i32 3354801150, ; 581: AppDynamics.Agent.dll => 0xc7f633fe => 173
	i32 3357674450, ; 582: ru\Microsoft.Maui.Controls.resources => 0xc8220bd2 => 353
	i32 3358260929, ; 583: System.Text.Json => 0xc82afec1 => 137
	i32 3359991071, ; 584: Xamarin.AndroidX.Tracing.Tracing.Ktx.dll => 0xc845651f => 285
	i32 3362336904, ; 585: Xamarin.AndroidX.Activity.Ktx => 0xc8693088 => 216
	i32 3362522851, ; 586: Xamarin.AndroidX.Core => 0xc86c06e3 => 239
	i32 3366347497, ; 587: Java.Interop => 0xc8a662e9 => 168
	i32 3371992681, ; 588: Xamarin.Firebase.Encoders.Proto.dll => 0xc8fc8669 => 298
	i32 3374999561, ; 589: Xamarin.AndroidX.RecyclerView => 0xc92a6809 => 276
	i32 3381016424, ; 590: da\Microsoft.Maui.Controls.resources => 0xc9863768 => 332
	i32 3383578424, ; 591: Xamarin.Firebase.Encoders.JSON => 0xc9ad4f38 => 297
	i32 3395150330, ; 592: System.Runtime.CompilerServices.Unsafe.dll => 0xca5de1fa => 101
	i32 3403906625, ; 593: System.Security.Cryptography.OpenSsl.dll => 0xcae37e41 => 123
	i32 3405233483, ; 594: Xamarin.AndroidX.CustomView.PoolingContainer => 0xcaf7bd4b => 243
	i32 3411362516, ; 595: Xamarin.Google.MLKit.Vision.Interfaces => 0xcb5542d4 => 313
	i32 3413944578, ; 596: Xamarin.AndroidX.Camera.Camera2.dll => 0xcb7ca902 => 225
	i32 3421910702, ; 597: Xamarin.AndroidX.Camera.Camera2 => 0xcbf636ae => 225
	i32 3428513518, ; 598: Microsoft.Extensions.DependencyInjection.dll => 0xcc5af6ee => 192
	i32 3429136800, ; 599: System.Xml => 0xcc6479a0 => 163
	i32 3430777524, ; 600: netstandard => 0xcc7d82b4 => 167
	i32 3441283291, ; 601: Xamarin.AndroidX.DynamicAnimation.dll => 0xcd1dd0db => 246
	i32 3445260447, ; 602: System.Formats.Tar => 0xcd5a809f => 39
	i32 3452344032, ; 603: Microsoft.Maui.Controls.Compatibility.dll => 0xcdc696e0 => 200
	i32 3463511458, ; 604: hr/Microsoft.Maui.Controls.resources.dll => 0xce70fda2 => 340
	i32 3466904072, ; 605: Microsoft.AspNetCore.SignalR.Client.dll => 0xcea4c208 => 181
	i32 3471940407, ; 606: System.ComponentModel.TypeConverter.dll => 0xcef19b37 => 17
	i32 3476120550, ; 607: Mono.Android => 0xcf3163e6 => 171
	i32 3479583265, ; 608: ru/Microsoft.Maui.Controls.resources.dll => 0xcf663a21 => 353
	i32 3484440000, ; 609: ro\Microsoft.Maui.Controls.resources => 0xcfb055c0 => 352
	i32 3485117614, ; 610: System.Text.Json.dll => 0xcfbaacae => 137
	i32 3486566296, ; 611: System.Transactions => 0xcfd0c798 => 150
	i32 3493954962, ; 612: Xamarin.AndroidX.Concurrent.Futures.dll => 0xd0418592 => 234
	i32 3509114376, ; 613: System.Xml.Linq => 0xd128d608 => 155
	i32 3515174580, ; 614: System.Security.dll => 0xd1854eb4 => 130
	i32 3530912306, ; 615: System.Configuration => 0xd2757232 => 19
	i32 3539954161, ; 616: System.Net.HttpListener => 0xd2ff69f1 => 65
	i32 3560100363, ; 617: System.Threading.Timer => 0xd432d20b => 147
	i32 3570554715, ; 618: System.IO.FileSystem.AccessControl => 0xd4d2575b => 47
	i32 3580758918, ; 619: zh-HK\Microsoft.Maui.Controls.resources => 0xd56e0b86 => 360
	i32 3597029428, ; 620: Xamarin.Android.Glide.GifDecoder.dll => 0xd6665034 => 214
	i32 3598340787, ; 621: System.Net.WebSockets.Client => 0xd67a52b3 => 79
	i32 3608519521, ; 622: System.Linq.dll => 0xd715a361 => 61
	i32 3624195450, ; 623: System.Runtime.InteropServices.RuntimeInformation => 0xd804d57a => 106
	i32 3626429363, ; 624: Xamarin.Google.MLKit.Common => 0xd826ebb3 => 311
	i32 3627220390, ; 625: Xamarin.AndroidX.Print.dll => 0xd832fda6 => 274
	i32 3633644679, ; 626: Xamarin.AndroidX.Annotation.Experimental => 0xd8950487 => 218
	i32 3638274909, ; 627: System.IO.FileSystem.Primitives.dll => 0xd8dbab5d => 49
	i32 3641597786, ; 628: Xamarin.AndroidX.Lifecycle.LiveData.Core => 0xd90e5f5a => 257
	i32 3643446276, ; 629: tr\Microsoft.Maui.Controls.resources => 0xd92a9404 => 357
	i32 3643854240, ; 630: Xamarin.AndroidX.Navigation.Fragment => 0xd930cda0 => 271
	i32 3645089577, ; 631: System.ComponentModel.DataAnnotations => 0xd943a729 => 14
	i32 3657292374, ; 632: Microsoft.Extensions.Configuration.Abstractions.dll => 0xd9fdda56 => 191
	i32 3660523487, ; 633: System.Net.NetworkInformation => 0xda2f27df => 68
	i32 3672681054, ; 634: Mono.Android.dll => 0xdae8aa5e => 171
	i32 3676461095, ; 635: Xamarin.AndroidX.Camera.Lifecycle => 0xdb225827 => 227
	i32 3682565725, ; 636: Xamarin.AndroidX.Browser => 0xdb7f7e5d => 224
	i32 3684561358, ; 637: Xamarin.AndroidX.Concurrent.Futures => 0xdb9df1ce => 234
	i32 3691870036, ; 638: Microsoft.AspNetCore.SignalR.Protocols.Json => 0xdc0d7754 => 184
	i32 3697841164, ; 639: zh-Hant/Microsoft.Maui.Controls.resources.dll => 0xdc68940c => 362
	i32 3700866549, ; 640: System.Net.WebProxy.dll => 0xdc96bdf5 => 78
	i32 3706696989, ; 641: Xamarin.AndroidX.Core.Core.Ktx.dll => 0xdcefb51d => 240
	i32 3716563718, ; 642: System.Runtime.Intrinsics => 0xdd864306 => 108
	i32 3718780102, ; 643: Xamarin.AndroidX.Annotation => 0xdda814c6 => 217
	i32 3724971120, ; 644: Xamarin.AndroidX.Navigation.Common.dll => 0xde068c70 => 270
	i32 3732100267, ; 645: System.Net.NameResolution => 0xde7354ab => 67
	i32 3737834244, ; 646: System.Net.Http.Json.dll => 0xdecad304 => 63
	i32 3748608112, ; 647: System.Diagnostics.DiagnosticSource => 0xdf6f3870 => 27
	i32 3751444290, ; 648: System.Xml.XPath => 0xdf9a7f42 => 160
	i32 3764085317, ; 649: Xamarin.AndroidX.Lifecycle.Runtime.Ktx.Android.dll => 0xe05b6245 => 263
	i32 3786282454, ; 650: Xamarin.AndroidX.Collection => 0xe1ae15d6 => 231
	i32 3787005001, ; 651: Microsoft.AspNetCore.Connections.Abstractions => 0xe1b91c49 => 178
	i32 3792276235, ; 652: System.Collections.NonGeneric => 0xe2098b0b => 10
	i32 3800979733, ; 653: Microsoft.Maui.Controls.Compatibility => 0xe28e5915 => 200
	i32 3802395368, ; 654: System.Collections.Specialized.dll => 0xe2a3f2e8 => 11
	i32 3817368567, ; 655: CommunityToolkit.Maui.dll => 0xe3886bf7 => 175
	i32 3819260425, ; 656: System.Net.WebProxy => 0xe3a54a09 => 78
	i32 3823082795, ; 657: System.Security.Cryptography.dll => 0xe3df9d2b => 126
	i32 3829621856, ; 658: System.Numerics.dll => 0xe4436460 => 83
	i32 3841636137, ; 659: Microsoft.Extensions.DependencyInjection.Abstractions.dll => 0xe4fab729 => 193
	i32 3844307129, ; 660: System.Net.Mail.dll => 0xe52378b9 => 66
	i32 3849253459, ; 661: System.Runtime.InteropServices.dll => 0xe56ef253 => 107
	i32 3870376305, ; 662: System.Net.HttpListener.dll => 0xe6b14171 => 65
	i32 3873536506, ; 663: System.Security.Principal => 0xe6e179fa => 128
	i32 3875112723, ; 664: System.Security.Cryptography.Encoding.dll => 0xe6f98713 => 122
	i32 3885497537, ; 665: System.Net.WebHeaderCollection.dll => 0xe797fcc1 => 77
	i32 3885922214, ; 666: Xamarin.AndroidX.Transition.dll => 0xe79e77a6 => 286
	i32 3888767677, ; 667: Xamarin.AndroidX.ProfileInstaller.ProfileInstaller => 0xe7c9e2bd => 275
	i32 3889960447, ; 668: zh-Hans/Microsoft.Maui.Controls.resources.dll => 0xe7dc15ff => 361
	i32 3896106733, ; 669: System.Collections.Concurrent.dll => 0xe839deed => 8
	i32 3896760992, ; 670: Xamarin.AndroidX.Core.dll => 0xe843daa0 => 239
	i32 3901907137, ; 671: Microsoft.VisualBasic.Core.dll => 0xe89260c1 => 2
	i32 3910130544, ; 672: Xamarin.AndroidX.Collection.Jvm => 0xe90fdb70 => 232
	i32 3920810846, ; 673: System.IO.Compression.FileSystem.dll => 0xe9b2d35e => 44
	i32 3921031405, ; 674: Xamarin.AndroidX.VersionedParcelable.dll => 0xe9b630ed => 289
	i32 3928044579, ; 675: System.Xml.ReaderWriter => 0xea213423 => 156
	i32 3930554604, ; 676: System.Security.Principal.dll => 0xea4780ec => 128
	i32 3931092270, ; 677: Xamarin.AndroidX.Navigation.UI => 0xea4fb52e => 273
	i32 3934056515, ; 678: Xamarin.JavaX.Inject.dll => 0xea7cf043 => 318
	i32 3945713374, ; 679: System.Data.DataSetExtensions.dll => 0xeb2ecede => 23
	i32 3953953790, ; 680: System.Text.Encoding.CodePages => 0xebac8bfe => 133
	i32 3955647286, ; 681: Xamarin.AndroidX.AppCompat.dll => 0xebc66336 => 220
	i32 3956287295, ; 682: BarcodeScanning.Native.Maui => 0xebd0273f => 174
	i32 3959773229, ; 683: Xamarin.AndroidX.Lifecycle.Process => 0xec05582d => 259
	i32 3970018735, ; 684: Xamarin.GooglePlayServices.Tasks.dll => 0xeca1adaf => 317
	i32 3980434154, ; 685: th/Microsoft.Maui.Controls.resources.dll => 0xed409aea => 356
	i32 3987592930, ; 686: he/Microsoft.Maui.Controls.resources.dll => 0xedadd6e2 => 338
	i32 4003436829, ; 687: System.Diagnostics.Process.dll => 0xee9f991d => 29
	i32 4015948917, ; 688: Xamarin.AndroidX.Annotation.Jvm.dll => 0xef5e8475 => 219
	i32 4023392905, ; 689: System.IO.Pipelines => 0xefd01a89 => 209
	i32 4025784931, ; 690: System.Memory => 0xeff49a63 => 62
	i32 4026433800, ; 691: NekrasovskyAPP.dll => 0xeffe8108 => 0
	i32 4046471985, ; 692: Microsoft.Maui.Controls.Xaml.dll => 0xf1304331 => 202
	i32 4054681211, ; 693: System.Reflection.Emit.ILGeneration => 0xf1ad867b => 90
	i32 4068434129, ; 694: System.Private.Xml.Linq.dll => 0xf27f60d1 => 87
	i32 4073602200, ; 695: System.Threading.dll => 0xf2ce3c98 => 148
	i32 4094352644, ; 696: Microsoft.Maui.Essentials.dll => 0xf40add04 => 204
	i32 4099507663, ; 697: System.Drawing.dll => 0xf45985cf => 36
	i32 4100113165, ; 698: System.Private.Uri => 0xf462c30d => 86
	i32 4101236366, ; 699: Npgsql.EntityFrameworkCore.PostgreSQL => 0xf473e68e => 207
	i32 4101593132, ; 700: Xamarin.AndroidX.Emoji2 => 0xf479582c => 247
	i32 4101842092, ; 701: Microsoft.Extensions.Caching.Memory => 0xf47d24ac => 189
	i32 4102112229, ; 702: pt/Microsoft.Maui.Controls.resources.dll => 0xf48143e5 => 351
	i32 4125707920, ; 703: ms/Microsoft.Maui.Controls.resources.dll => 0xf5e94e90 => 346
	i32 4126470640, ; 704: Microsoft.Extensions.DependencyInjection => 0xf5f4f1f0 => 192
	i32 4127667938, ; 705: System.IO.FileSystem.Watcher => 0xf60736e2 => 50
	i32 4130442656, ; 706: System.AppContext => 0xf6318da0 => 6
	i32 4147896353, ; 707: System.Reflection.Emit.ILGeneration.dll => 0xf73be021 => 90
	i32 4150914736, ; 708: uk\Microsoft.Maui.Controls.resources => 0xf769eeb0 => 358
	i32 4151237749, ; 709: System.Core => 0xf76edc75 => 21
	i32 4159265925, ; 710: System.Xml.XmlSerializer => 0xf7e95c85 => 162
	i32 4161255271, ; 711: System.Reflection.TypeExtensions => 0xf807b767 => 96
	i32 4164802419, ; 712: System.IO.FileSystem.Watcher.dll => 0xf83dd773 => 50
	i32 4181436372, ; 713: System.Runtime.Serialization.Primitives => 0xf93ba7d4 => 113
	i32 4182413190, ; 714: Xamarin.AndroidX.Lifecycle.ViewModelSavedState.dll => 0xf94a8f86 => 267
	i32 4185676441, ; 715: System.Security => 0xf97c5a99 => 130
	i32 4192648326, ; 716: Xamarin.Firebase.Encoders.JSON.dll => 0xf9e6bc86 => 297
	i32 4196529839, ; 717: System.Net.WebClient.dll => 0xfa21f6af => 76
	i32 4213026141, ; 718: System.Diagnostics.DiagnosticSource.dll => 0xfb1dad5d => 27
	i32 4228543782, ; 719: Xamarin.KotlinX.AtomicFU.Jvm.dll => 0xfc0a7526 => 324
	i32 4256097574, ; 720: Xamarin.AndroidX.Core.Core.Ktx => 0xfdaee526 => 240
	i32 4258378803, ; 721: Xamarin.AndroidX.Lifecycle.ViewModel.Ktx => 0xfdd1b433 => 266
	i32 4260525087, ; 722: System.Buffers => 0xfdf2741f => 7
	i32 4271975918, ; 723: Microsoft.Maui.Controls.dll => 0xfea12dee => 201
	i32 4274976490, ; 724: System.Runtime.Numerics => 0xfecef6ea => 110
	i32 4284549794, ; 725: Xamarin.Firebase.Components => 0xff610aa2 => 295
	i32 4292120959, ; 726: Xamarin.AndroidX.Lifecycle.ViewModelSavedState => 0xffd4917f => 267
	i32 4294763496 ; 727: Xamarin.AndroidX.ExifInterface.dll => 0xfffce3e8 => 249
], align 4

@assembly_image_cache_indices = dso_local local_unnamed_addr constant [728 x i32] [
	i32 68, ; 0
	i32 67, ; 1
	i32 108, ; 2
	i32 313, ; 3
	i32 206, ; 4
	i32 303, ; 5
	i32 260, ; 6
	i32 308, ; 7
	i32 48, ; 8
	i32 80, ; 9
	i32 227, ; 10
	i32 145, ; 11
	i32 324, ; 12
	i32 235, ; 13
	i32 30, ; 14
	i32 362, ; 15
	i32 124, ; 16
	i32 205, ; 17
	i32 102, ; 18
	i32 281, ; 19
	i32 294, ; 20
	i32 107, ; 21
	i32 281, ; 22
	i32 139, ; 23
	i32 321, ; 24
	i32 77, ; 25
	i32 124, ; 26
	i32 13, ; 27
	i32 231, ; 28
	i32 323, ; 29
	i32 132, ; 30
	i32 283, ; 31
	i32 151, ; 32
	i32 359, ; 33
	i32 360, ; 34
	i32 18, ; 35
	i32 224, ; 36
	i32 285, ; 37
	i32 26, ; 38
	i32 179, ; 39
	i32 253, ; 40
	i32 1, ; 41
	i32 59, ; 42
	i32 42, ; 43
	i32 91, ; 44
	i32 236, ; 45
	i32 147, ; 46
	i32 256, ; 47
	i32 252, ; 48
	i32 331, ; 49
	i32 54, ; 50
	i32 69, ; 51
	i32 359, ; 52
	i32 215, ; 53
	i32 83, ; 54
	i32 344, ; 55
	i32 254, ; 56
	i32 180, ; 57
	i32 343, ; 58
	i32 131, ; 59
	i32 55, ; 60
	i32 149, ; 61
	i32 74, ; 62
	i32 145, ; 63
	i32 62, ; 64
	i32 146, ; 65
	i32 363, ; 66
	i32 165, ; 67
	i32 261, ; 68
	i32 355, ; 69
	i32 237, ; 70
	i32 12, ; 71
	i32 250, ; 72
	i32 125, ; 73
	i32 152, ; 74
	i32 183, ; 75
	i32 113, ; 76
	i32 166, ; 77
	i32 164, ; 78
	i32 252, ; 79
	i32 301, ; 80
	i32 269, ; 81
	i32 301, ; 82
	i32 84, ; 83
	i32 342, ; 84
	i32 336, ; 85
	i32 299, ; 86
	i32 199, ; 87
	i32 312, ; 88
	i32 208, ; 89
	i32 150, ; 90
	i32 321, ; 91
	i32 60, ; 92
	i32 195, ; 93
	i32 51, ; 94
	i32 103, ; 95
	i32 114, ; 96
	i32 40, ; 97
	i32 305, ; 98
	i32 293, ; 99
	i32 120, ; 100
	i32 350, ; 101
	i32 175, ; 102
	i32 52, ; 103
	i32 44, ; 104
	i32 119, ; 105
	i32 261, ; 106
	i32 242, ; 107
	i32 348, ; 108
	i32 248, ; 109
	i32 81, ; 110
	i32 136, ; 111
	i32 289, ; 112
	i32 222, ; 113
	i32 8, ; 114
	i32 323, ; 115
	i32 73, ; 116
	i32 330, ; 117
	i32 155, ; 118
	i32 325, ; 119
	i32 154, ; 120
	i32 92, ; 121
	i32 319, ; 122
	i32 45, ; 123
	i32 345, ; 124
	i32 333, ; 125
	i32 322, ; 126
	i32 109, ; 127
	i32 129, ; 128
	i32 25, ; 129
	i32 212, ; 130
	i32 72, ; 131
	i32 55, ; 132
	i32 46, ; 133
	i32 354, ; 134
	i32 304, ; 135
	i32 198, ; 136
	i32 243, ; 137
	i32 22, ; 138
	i32 258, ; 139
	i32 86, ; 140
	i32 43, ; 141
	i32 160, ; 142
	i32 184, ; 143
	i32 71, ; 144
	i32 274, ; 145
	i32 328, ; 146
	i32 3, ; 147
	i32 42, ; 148
	i32 63, ; 149
	i32 16, ; 150
	i32 53, ; 151
	i32 357, ; 152
	i32 308, ; 153
	i32 105, ; 154
	i32 322, ; 155
	i32 306, ; 156
	i32 254, ; 157
	i32 34, ; 158
	i32 158, ; 159
	i32 85, ; 160
	i32 32, ; 161
	i32 12, ; 162
	i32 51, ; 163
	i32 300, ; 164
	i32 56, ; 165
	i32 278, ; 166
	i32 36, ; 167
	i32 193, ; 168
	i32 332, ; 169
	i32 307, ; 170
	i32 220, ; 171
	i32 35, ; 172
	i32 58, ; 173
	i32 264, ; 174
	i32 180, ; 175
	i32 304, ; 176
	i32 177, ; 177
	i32 17, ; 178
	i32 320, ; 179
	i32 164, ; 180
	i32 345, ; 181
	i32 262, ; 182
	i32 303, ; 183
	i32 197, ; 184
	i32 210, ; 185
	i32 292, ; 186
	i32 186, ; 187
	i32 351, ; 188
	i32 153, ; 189
	i32 288, ; 190
	i32 272, ; 191
	i32 186, ; 192
	i32 349, ; 193
	i32 311, ; 194
	i32 222, ; 195
	i32 189, ; 196
	i32 29, ; 197
	i32 52, ; 198
	i32 182, ; 199
	i32 347, ; 200
	i32 293, ; 201
	i32 232, ; 202
	i32 5, ; 203
	i32 331, ; 204
	i32 282, ; 205
	i32 326, ; 206
	i32 287, ; 207
	i32 233, ; 208
	i32 325, ; 209
	i32 219, ; 210
	i32 245, ; 211
	i32 85, ; 212
	i32 292, ; 213
	i32 61, ; 214
	i32 112, ; 215
	i32 316, ; 216
	i32 310, ; 217
	i32 309, ; 218
	i32 57, ; 219
	i32 361, ; 220
	i32 278, ; 221
	i32 99, ; 222
	i32 318, ; 223
	i32 19, ; 224
	i32 238, ; 225
	i32 111, ; 226
	i32 101, ; 227
	i32 178, ; 228
	i32 102, ; 229
	i32 329, ; 230
	i32 104, ; 231
	i32 306, ; 232
	i32 255, ; 233
	i32 71, ; 234
	i32 265, ; 235
	i32 38, ; 236
	i32 32, ; 237
	i32 103, ; 238
	i32 73, ; 239
	i32 335, ; 240
	i32 9, ; 241
	i32 123, ; 242
	i32 46, ; 243
	i32 221, ; 244
	i32 199, ; 245
	i32 9, ; 246
	i32 43, ; 247
	i32 4, ; 248
	i32 279, ; 249
	i32 339, ; 250
	i32 173, ; 251
	i32 334, ; 252
	i32 31, ; 253
	i32 138, ; 254
	i32 92, ; 255
	i32 93, ; 256
	i32 354, ; 257
	i32 49, ; 258
	i32 141, ; 259
	i32 112, ; 260
	i32 140, ; 261
	i32 244, ; 262
	i32 115, ; 263
	i32 307, ; 264
	i32 157, ; 265
	i32 76, ; 266
	i32 79, ; 267
	i32 268, ; 268
	i32 37, ; 269
	i32 291, ; 270
	i32 176, ; 271
	i32 248, ; 272
	i32 241, ; 273
	i32 64, ; 274
	i32 138, ; 275
	i32 15, ; 276
	i32 116, ; 277
	i32 284, ; 278
	i32 302, ; 279
	i32 236, ; 280
	i32 48, ; 281
	i32 70, ; 282
	i32 80, ; 283
	i32 126, ; 284
	i32 185, ; 285
	i32 94, ; 286
	i32 121, ; 287
	i32 26, ; 288
	i32 316, ; 289
	i32 258, ; 290
	i32 97, ; 291
	i32 28, ; 292
	i32 230, ; 293
	i32 352, ; 294
	i32 330, ; 295
	i32 149, ; 296
	i32 209, ; 297
	i32 169, ; 298
	i32 4, ; 299
	i32 98, ; 300
	i32 33, ; 301
	i32 93, ; 302
	i32 283, ; 303
	i32 195, ; 304
	i32 0, ; 305
	i32 21, ; 306
	i32 41, ; 307
	i32 170, ; 308
	i32 346, ; 309
	i32 250, ; 310
	i32 338, ; 311
	i32 268, ; 312
	i32 320, ; 313
	i32 302, ; 314
	i32 273, ; 315
	i32 2, ; 316
	i32 134, ; 317
	i32 111, ; 318
	i32 196, ; 319
	i32 210, ; 320
	i32 358, ; 321
	i32 212, ; 322
	i32 355, ; 323
	i32 58, ; 324
	i32 228, ; 325
	i32 95, ; 326
	i32 337, ; 327
	i32 298, ; 328
	i32 39, ; 329
	i32 223, ; 330
	i32 25, ; 331
	i32 94, ; 332
	i32 89, ; 333
	i32 99, ; 334
	i32 315, ; 335
	i32 10, ; 336
	i32 87, ; 337
	i32 182, ; 338
	i32 100, ; 339
	i32 280, ; 340
	i32 183, ; 341
	i32 190, ; 342
	i32 214, ; 343
	i32 334, ; 344
	i32 7, ; 345
	i32 264, ; 346
	i32 329, ; 347
	i32 211, ; 348
	i32 88, ; 349
	i32 257, ; 350
	i32 154, ; 351
	i32 333, ; 352
	i32 33, ; 353
	i32 116, ; 354
	i32 82, ; 355
	i32 174, ; 356
	i32 300, ; 357
	i32 20, ; 358
	i32 314, ; 359
	i32 11, ; 360
	i32 162, ; 361
	i32 3, ; 362
	i32 203, ; 363
	i32 341, ; 364
	i32 294, ; 365
	i32 198, ; 366
	i32 309, ; 367
	i32 196, ; 368
	i32 84, ; 369
	i32 327, ; 370
	i32 64, ; 371
	i32 343, ; 372
	i32 288, ; 373
	i32 143, ; 374
	i32 194, ; 375
	i32 269, ; 376
	i32 157, ; 377
	i32 185, ; 378
	i32 41, ; 379
	i32 117, ; 380
	i32 191, ; 381
	i32 213, ; 382
	i32 337, ; 383
	i32 276, ; 384
	i32 131, ; 385
	i32 206, ; 386
	i32 75, ; 387
	i32 66, ; 388
	i32 347, ; 389
	i32 172, ; 390
	i32 217, ; 391
	i32 181, ; 392
	i32 143, ; 393
	i32 207, ; 394
	i32 106, ; 395
	i32 151, ; 396
	i32 70, ; 397
	i32 156, ; 398
	i32 190, ; 399
	i32 121, ; 400
	i32 127, ; 401
	i32 342, ; 402
	i32 152, ; 403
	i32 247, ; 404
	i32 228, ; 405
	i32 141, ; 406
	i32 233, ; 407
	i32 310, ; 408
	i32 339, ; 409
	i32 20, ; 410
	i32 14, ; 411
	i32 135, ; 412
	i32 75, ; 413
	i32 59, ; 414
	i32 237, ; 415
	i32 167, ; 416
	i32 168, ; 417
	i32 201, ; 418
	i32 15, ; 419
	i32 74, ; 420
	i32 6, ; 421
	i32 23, ; 422
	i32 260, ; 423
	i32 211, ; 424
	i32 91, ; 425
	i32 340, ; 426
	i32 1, ; 427
	i32 136, ; 428
	i32 263, ; 429
	i32 262, ; 430
	i32 287, ; 431
	i32 134, ; 432
	i32 69, ; 433
	i32 146, ; 434
	i32 349, ; 435
	i32 327, ; 436
	i32 251, ; 437
	i32 197, ; 438
	i32 88, ; 439
	i32 96, ; 440
	i32 296, ; 441
	i32 241, ; 442
	i32 246, ; 443
	i32 344, ; 444
	i32 31, ; 445
	i32 45, ; 446
	i32 256, ; 447
	i32 187, ; 448
	i32 194, ; 449
	i32 296, ; 450
	i32 213, ; 451
	i32 109, ; 452
	i32 158, ; 453
	i32 35, ; 454
	i32 326, ; 455
	i32 22, ; 456
	i32 114, ; 457
	i32 57, ; 458
	i32 284, ; 459
	i32 144, ; 460
	i32 118, ; 461
	i32 120, ; 462
	i32 110, ; 463
	i32 215, ; 464
	i32 139, ; 465
	i32 221, ; 466
	i32 54, ; 467
	i32 105, ; 468
	i32 350, ; 469
	i32 202, ; 470
	i32 203, ; 471
	i32 133, ; 472
	i32 265, ; 473
	i32 319, ; 474
	i32 290, ; 475
	i32 277, ; 476
	i32 255, ; 477
	i32 356, ; 478
	i32 251, ; 479
	i32 205, ; 480
	i32 159, ; 481
	i32 295, ; 482
	i32 335, ; 483
	i32 238, ; 484
	i32 163, ; 485
	i32 132, ; 486
	i32 277, ; 487
	i32 161, ; 488
	i32 235, ; 489
	i32 348, ; 490
	i32 266, ; 491
	i32 314, ; 492
	i32 187, ; 493
	i32 140, ; 494
	i32 290, ; 495
	i32 286, ; 496
	i32 169, ; 497
	i32 204, ; 498
	i32 312, ; 499
	i32 176, ; 500
	i32 216, ; 501
	i32 305, ; 502
	i32 40, ; 503
	i32 179, ; 504
	i32 249, ; 505
	i32 81, ; 506
	i32 56, ; 507
	i32 37, ; 508
	i32 97, ; 509
	i32 166, ; 510
	i32 172, ; 511
	i32 291, ; 512
	i32 82, ; 513
	i32 218, ; 514
	i32 98, ; 515
	i32 30, ; 516
	i32 159, ; 517
	i32 18, ; 518
	i32 229, ; 519
	i32 127, ; 520
	i32 119, ; 521
	i32 245, ; 522
	i32 280, ; 523
	i32 226, ; 524
	i32 259, ; 525
	i32 229, ; 526
	i32 282, ; 527
	i32 165, ; 528
	i32 253, ; 529
	i32 363, ; 530
	i32 226, ; 531
	i32 279, ; 532
	i32 270, ; 533
	i32 317, ; 534
	i32 170, ; 535
	i32 16, ; 536
	i32 188, ; 537
	i32 144, ; 538
	i32 341, ; 539
	i32 125, ; 540
	i32 118, ; 541
	i32 38, ; 542
	i32 115, ; 543
	i32 47, ; 544
	i32 142, ; 545
	i32 117, ; 546
	i32 34, ; 547
	i32 177, ; 548
	i32 299, ; 549
	i32 95, ; 550
	i32 53, ; 551
	i32 271, ; 552
	i32 129, ; 553
	i32 153, ; 554
	i32 188, ; 555
	i32 24, ; 556
	i32 161, ; 557
	i32 244, ; 558
	i32 328, ; 559
	i32 148, ; 560
	i32 104, ; 561
	i32 315, ; 562
	i32 89, ; 563
	i32 230, ; 564
	i32 60, ; 565
	i32 142, ; 566
	i32 100, ; 567
	i32 5, ; 568
	i32 13, ; 569
	i32 122, ; 570
	i32 135, ; 571
	i32 28, ; 572
	i32 336, ; 573
	i32 72, ; 574
	i32 242, ; 575
	i32 24, ; 576
	i32 208, ; 577
	i32 223, ; 578
	i32 275, ; 579
	i32 272, ; 580
	i32 173, ; 581
	i32 353, ; 582
	i32 137, ; 583
	i32 285, ; 584
	i32 216, ; 585
	i32 239, ; 586
	i32 168, ; 587
	i32 298, ; 588
	i32 276, ; 589
	i32 332, ; 590
	i32 297, ; 591
	i32 101, ; 592
	i32 123, ; 593
	i32 243, ; 594
	i32 313, ; 595
	i32 225, ; 596
	i32 225, ; 597
	i32 192, ; 598
	i32 163, ; 599
	i32 167, ; 600
	i32 246, ; 601
	i32 39, ; 602
	i32 200, ; 603
	i32 340, ; 604
	i32 181, ; 605
	i32 17, ; 606
	i32 171, ; 607
	i32 353, ; 608
	i32 352, ; 609
	i32 137, ; 610
	i32 150, ; 611
	i32 234, ; 612
	i32 155, ; 613
	i32 130, ; 614
	i32 19, ; 615
	i32 65, ; 616
	i32 147, ; 617
	i32 47, ; 618
	i32 360, ; 619
	i32 214, ; 620
	i32 79, ; 621
	i32 61, ; 622
	i32 106, ; 623
	i32 311, ; 624
	i32 274, ; 625
	i32 218, ; 626
	i32 49, ; 627
	i32 257, ; 628
	i32 357, ; 629
	i32 271, ; 630
	i32 14, ; 631
	i32 191, ; 632
	i32 68, ; 633
	i32 171, ; 634
	i32 227, ; 635
	i32 224, ; 636
	i32 234, ; 637
	i32 184, ; 638
	i32 362, ; 639
	i32 78, ; 640
	i32 240, ; 641
	i32 108, ; 642
	i32 217, ; 643
	i32 270, ; 644
	i32 67, ; 645
	i32 63, ; 646
	i32 27, ; 647
	i32 160, ; 648
	i32 263, ; 649
	i32 231, ; 650
	i32 178, ; 651
	i32 10, ; 652
	i32 200, ; 653
	i32 11, ; 654
	i32 175, ; 655
	i32 78, ; 656
	i32 126, ; 657
	i32 83, ; 658
	i32 193, ; 659
	i32 66, ; 660
	i32 107, ; 661
	i32 65, ; 662
	i32 128, ; 663
	i32 122, ; 664
	i32 77, ; 665
	i32 286, ; 666
	i32 275, ; 667
	i32 361, ; 668
	i32 8, ; 669
	i32 239, ; 670
	i32 2, ; 671
	i32 232, ; 672
	i32 44, ; 673
	i32 289, ; 674
	i32 156, ; 675
	i32 128, ; 676
	i32 273, ; 677
	i32 318, ; 678
	i32 23, ; 679
	i32 133, ; 680
	i32 220, ; 681
	i32 174, ; 682
	i32 259, ; 683
	i32 317, ; 684
	i32 356, ; 685
	i32 338, ; 686
	i32 29, ; 687
	i32 219, ; 688
	i32 209, ; 689
	i32 62, ; 690
	i32 0, ; 691
	i32 202, ; 692
	i32 90, ; 693
	i32 87, ; 694
	i32 148, ; 695
	i32 204, ; 696
	i32 36, ; 697
	i32 86, ; 698
	i32 207, ; 699
	i32 247, ; 700
	i32 189, ; 701
	i32 351, ; 702
	i32 346, ; 703
	i32 192, ; 704
	i32 50, ; 705
	i32 6, ; 706
	i32 90, ; 707
	i32 358, ; 708
	i32 21, ; 709
	i32 162, ; 710
	i32 96, ; 711
	i32 50, ; 712
	i32 113, ; 713
	i32 267, ; 714
	i32 130, ; 715
	i32 297, ; 716
	i32 76, ; 717
	i32 27, ; 718
	i32 324, ; 719
	i32 240, ; 720
	i32 266, ; 721
	i32 7, ; 722
	i32 201, ; 723
	i32 110, ; 724
	i32 295, ; 725
	i32 267, ; 726
	i32 249 ; 727
], align 4

@marshal_methods_number_of_classes = dso_local local_unnamed_addr constant i32 0, align 4

@marshal_methods_class_cache = dso_local local_unnamed_addr global [0 x %struct.MarshalMethodsManagedClass] zeroinitializer, align 4

; Names of classes in which marshal methods reside
@mm_class_names = dso_local local_unnamed_addr constant [0 x ptr] zeroinitializer, align 4

@mm_method_names = dso_local local_unnamed_addr constant [1 x %struct.MarshalMethodName] [
	%struct.MarshalMethodName {
		i64 0, ; id 0x0; name: 
		ptr @.MarshalMethodName.0_name; char* name
	} ; 0
], align 8

; get_function_pointer (uint32_t mono_image_index, uint32_t class_index, uint32_t method_token, void*& target_ptr)
@get_function_pointer = internal dso_local unnamed_addr global ptr null, align 4

; Functions

; Function attributes: "min-legal-vector-width"="0" mustprogress "no-trapping-math"="true" nofree norecurse nosync nounwind "stack-protector-buffer-size"="8" uwtable willreturn
define void @xamarin_app_init(ptr nocapture noundef readnone %env, ptr noundef %fn) local_unnamed_addr #0
{
	%fnIsNull = icmp eq ptr %fn, null
	br i1 %fnIsNull, label %1, label %2

1: ; preds = %0
	%putsResult = call noundef i32 @puts(ptr @.str.0)
	call void @abort()
	unreachable 

2: ; preds = %1, %0
	store ptr %fn, ptr @get_function_pointer, align 4, !tbaa !3
	ret void
}

; Strings
@.str.0 = private unnamed_addr constant [40 x i8] c"get_function_pointer MUST be specified\0A\00", align 1

;MarshalMethodName
@.MarshalMethodName.0_name = private unnamed_addr constant [1 x i8] c"\00", align 1

; External functions

; Function attributes: "no-trapping-math"="true" noreturn nounwind "stack-protector-buffer-size"="8"
declare void @abort() local_unnamed_addr #2

; Function attributes: nofree nounwind
declare noundef i32 @puts(ptr noundef) local_unnamed_addr #1
attributes #0 = { "min-legal-vector-width"="0" mustprogress "no-trapping-math"="true" nofree norecurse nosync nounwind "stack-protector-buffer-size"="8" "stackrealign" "target-cpu"="i686" "target-features"="+cx8,+mmx,+sse,+sse2,+sse3,+ssse3,+x87" "tune-cpu"="generic" uwtable willreturn }
attributes #1 = { nofree nounwind }
attributes #2 = { "no-trapping-math"="true" noreturn nounwind "stack-protector-buffer-size"="8" "stackrealign" "target-cpu"="i686" "target-features"="+cx8,+mmx,+sse,+sse2,+sse3,+ssse3,+x87" "tune-cpu"="generic" }

; Metadata
!llvm.module.flags = !{!0, !1, !7}
!0 = !{i32 1, !"wchar_size", i32 4}
!1 = !{i32 7, !"PIC Level", i32 2}
!llvm.ident = !{!2}
!2 = !{!"Xamarin.Android remotes/origin/release/8.0.4xx @ 82d8938cf80f6d5fa6c28529ddfbdb753d805ab4"}
!3 = !{!4, !4, i64 0}
!4 = !{!"any pointer", !5, i64 0}
!5 = !{!"omnipotent char", !6, i64 0}
!6 = !{!"Simple C++ TBAA"}
!7 = !{i32 1, !"NumRegisterParameters", i32 0}

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

@assembly_image_cache = dso_local local_unnamed_addr global [349 x ptr] zeroinitializer, align 4

; Each entry maps hash of an assembly name to an index into the `assembly_image_cache` array
@assembly_image_cache_hashes = dso_local local_unnamed_addr constant [698 x i32] [
	i32 2616222, ; 0: System.Net.NetworkInformation.dll => 0x27eb9e => 68
	i32 10166715, ; 1: System.Net.NameResolution.dll => 0x9b21bb => 67
	i32 15721112, ; 2: System.Runtime.Intrinsics.dll => 0xefe298 => 108
	i32 20206211, ; 3: Xamarin.Google.MLKit.Vision.Interfaces.dll => 0x1345283 => 299
	i32 28873261, ; 4: Npgsql.dll => 0x1b8922d => 195
	i32 30793855, ; 5: Xamarin.Google.Android.ODML.Image.dll => 0x1d5e07f => 289
	i32 32687329, ; 6: Xamarin.AndroidX.Lifecycle.Runtime => 0x1f2c4e1 => 246
	i32 34715100, ; 7: Xamarin.Google.Guava.ListenableFuture.dll => 0x211b5dc => 294
	i32 34839235, ; 8: System.IO.FileSystem.DriveInfo => 0x2139ac3 => 48
	i32 39485524, ; 9: System.Net.WebSockets.dll => 0x25a8054 => 80
	i32 40744412, ; 10: Xamarin.AndroidX.Camera.Lifecycle.dll => 0x26db5dc => 213
	i32 42639949, ; 11: System.Threading.Thread => 0x28aa24d => 145
	i32 45981941, ; 12: Xamarin.KotlinX.AtomicFU.Jvm => 0x2bda0f5 => 310
	i32 52581868, ; 13: Xamarin.AndroidX.Concurrent.Futures.Ktx => 0x32255ec => 221
	i32 66541672, ; 14: System.Diagnostics.StackTrace => 0x3f75868 => 30
	i32 67008169, ; 15: zh-Hant\Microsoft.Maui.Controls.resources => 0x3fe76a9 => 347
	i32 68219467, ; 16: System.Security.Cryptography.Primitives => 0x410f24b => 124
	i32 72070932, ; 17: Microsoft.Maui.Graphics.dll => 0x44bb714 => 194
	i32 82292897, ; 18: System.Runtime.CompilerServices.VisualC.dll => 0x4e7b0a1 => 102
	i32 101534019, ; 19: Xamarin.AndroidX.SlidingPaneLayout => 0x60d4943 => 267
	i32 103834273, ; 20: Xamarin.Firebase.Annotations.dll => 0x63062a1 => 280
	i32 117431740, ; 21: System.Runtime.InteropServices => 0x6ffddbc => 107
	i32 120558881, ; 22: Xamarin.AndroidX.SlidingPaneLayout.dll => 0x72f9521 => 267
	i32 122350210, ; 23: System.Threading.Channels.dll => 0x74aea82 => 139
	i32 134690465, ; 24: Xamarin.Kotlin.StdLib.Jdk7.dll => 0x80736a1 => 307
	i32 142721839, ; 25: System.Net.WebHeaderCollection => 0x881c32f => 77
	i32 149972175, ; 26: System.Security.Cryptography.Primitives.dll => 0x8f064cf => 124
	i32 159306688, ; 27: System.ComponentModel.Annotations => 0x97ed3c0 => 13
	i32 165246403, ; 28: Xamarin.AndroidX.Collection.dll => 0x9d975c3 => 217
	i32 166070894, ; 29: Xamarin.KotlinX.AtomicFU.dll => 0x9e60a6e => 309
	i32 176265551, ; 30: System.ServiceProcess => 0xa81994f => 132
	i32 182336117, ; 31: Xamarin.AndroidX.SwipeRefreshLayout.dll => 0xade3a75 => 269
	i32 184328833, ; 32: System.ValueTuple.dll => 0xafca281 => 151
	i32 195452805, ; 33: vi/Microsoft.Maui.Controls.resources.dll => 0xba65f85 => 344
	i32 199333315, ; 34: zh-HK/Microsoft.Maui.Controls.resources.dll => 0xbe195c3 => 345
	i32 205061960, ; 35: System.ComponentModel => 0xc38ff48 => 18
	i32 209399409, ; 36: Xamarin.AndroidX.Browser.dll => 0xc7b2e71 => 210
	i32 218154787, ; 37: Xamarin.AndroidX.Tracing.Tracing.Ktx => 0xd00c723 => 271
	i32 220171995, ; 38: System.Diagnostics.Debug => 0xd1f8edb => 26
	i32 230216969, ; 39: Xamarin.AndroidX.Legacy.Support.Core.Utils.dll => 0xdb8d509 => 239
	i32 230752869, ; 40: Microsoft.CSharp.dll => 0xdc10265 => 1
	i32 231409092, ; 41: System.Linq.Parallel => 0xdcb05c4 => 59
	i32 231814094, ; 42: System.Globalization => 0xdd133ce => 42
	i32 246610117, ; 43: System.Reflection.Emit.Lightweight => 0xeb2f8c5 => 91
	i32 261689757, ; 44: Xamarin.AndroidX.ConstraintLayout.dll => 0xf99119d => 222
	i32 276479776, ; 45: System.Threading.Timer.dll => 0x107abf20 => 147
	i32 278686392, ; 46: Xamarin.AndroidX.Lifecycle.LiveData.dll => 0x109c6ab8 => 242
	i32 280482487, ; 47: Xamarin.AndroidX.Interpolator => 0x10b7d2b7 => 238
	i32 280992041, ; 48: cs/Microsoft.Maui.Controls.resources.dll => 0x10bf9929 => 316
	i32 291076382, ; 49: System.IO.Pipes.AccessControl.dll => 0x1159791e => 54
	i32 298918909, ; 50: System.Net.Ping.dll => 0x11d123fd => 69
	i32 317674968, ; 51: vi\Microsoft.Maui.Controls.resources => 0x12ef55d8 => 344
	i32 318968648, ; 52: Xamarin.AndroidX.Activity.dll => 0x13031348 => 201
	i32 321597661, ; 53: System.Numerics => 0x132b30dd => 83
	i32 336156722, ; 54: ja/Microsoft.Maui.Controls.resources.dll => 0x14095832 => 329
	i32 342366114, ; 55: Xamarin.AndroidX.Lifecycle.Common => 0x146817a2 => 240
	i32 356389973, ; 56: it/Microsoft.Maui.Controls.resources.dll => 0x153e1455 => 328
	i32 360082299, ; 57: System.ServiceModel.Web => 0x15766b7b => 131
	i32 367780167, ; 58: System.IO.Pipes => 0x15ebe147 => 55
	i32 374914964, ; 59: System.Transactions.Local => 0x1658bf94 => 149
	i32 375677976, ; 60: System.Net.ServicePoint.dll => 0x16646418 => 74
	i32 379916513, ; 61: System.Threading.Thread.dll => 0x16a510e1 => 145
	i32 385762202, ; 62: System.Memory.dll => 0x16fe439a => 62
	i32 392610295, ; 63: System.Threading.ThreadPool.dll => 0x1766c1f7 => 146
	i32 395744057, ; 64: _Microsoft.Android.Resource.Designer => 0x17969339 => 348
	i32 403441872, ; 65: WindowsBase => 0x180c08d0 => 165
	i32 425531652, ; 66: Xamarin.AndroidX.Lifecycle.Runtime.Android => 0x195d1904 => 247
	i32 435591531, ; 67: sv/Microsoft.Maui.Controls.resources.dll => 0x19f6996b => 340
	i32 441335492, ; 68: Xamarin.AndroidX.ConstraintLayout.Core => 0x1a4e3ec4 => 223
	i32 442565967, ; 69: System.Collections => 0x1a61054f => 12
	i32 450948140, ; 70: Xamarin.AndroidX.Fragment.dll => 0x1ae0ec2c => 236
	i32 451504562, ; 71: System.Security.Cryptography.X509Certificates => 0x1ae969b2 => 125
	i32 456227837, ; 72: System.Web.HttpUtility.dll => 0x1b317bfd => 152
	i32 459347974, ; 73: System.Runtime.Serialization.Primitives.dll => 0x1b611806 => 113
	i32 465846621, ; 74: mscorlib => 0x1bc4415d => 166
	i32 469710990, ; 75: System.dll => 0x1bff388e => 164
	i32 476646585, ; 76: Xamarin.AndroidX.Interpolator.dll => 0x1c690cb9 => 238
	i32 485140951, ; 77: Xamarin.Google.Android.DataTransport.TransportRuntime => 0x1ceaa9d7 => 287
	i32 486930444, ; 78: Xamarin.AndroidX.LocalBroadcastManager.dll => 0x1d05f80c => 255
	i32 495452658, ; 79: Xamarin.Google.Android.DataTransport.TransportRuntime.dll => 0x1d8801f2 => 287
	i32 498788369, ; 80: System.ObjectModel => 0x1dbae811 => 84
	i32 500358224, ; 81: id/Microsoft.Maui.Controls.resources.dll => 0x1dd2dc50 => 327
	i32 503918385, ; 82: fi/Microsoft.Maui.Controls.resources.dll => 0x1e092f31 => 321
	i32 507148113, ; 83: Xamarin.Google.Android.DataTransport.TransportApi.dll => 0x1e3a7751 => 285
	i32 513247710, ; 84: Microsoft.Extensions.Primitives.dll => 0x1e9789de => 188
	i32 513617146, ; 85: Xamarin.Google.MLKit.Vision.Common => 0x1e9d2cfa => 298
	i32 526420162, ; 86: System.Transactions.dll => 0x1f6088c2 => 150
	i32 527452488, ; 87: Xamarin.Kotlin.StdLib.Jdk7 => 0x1f704948 => 307
	i32 530272170, ; 88: System.Linq.Queryable => 0x1f9b4faa => 60
	i32 539058512, ; 89: Microsoft.Extensions.Logging => 0x20216150 => 184
	i32 540030774, ; 90: System.IO.FileSystem.dll => 0x20303736 => 51
	i32 545304856, ; 91: System.Runtime.Extensions => 0x2080b118 => 103
	i32 546455878, ; 92: System.Runtime.Serialization.Xml => 0x20924146 => 114
	i32 549171840, ; 93: System.Globalization.Calendars => 0x20bbb280 => 40
	i32 557405415, ; 94: Jsr305Binding => 0x213954e7 => 291
	i32 569601784, ; 95: Xamarin.AndroidX.Window.Extensions.Core.Core => 0x21f36ef8 => 279
	i32 577335427, ; 96: System.Security.Cryptography.Cng => 0x22697083 => 120
	i32 592146354, ; 97: pt-BR/Microsoft.Maui.Controls.resources.dll => 0x234b6fb2 => 335
	i32 601371474, ; 98: System.IO.IsolatedStorage.dll => 0x23d83352 => 52
	i32 605376203, ; 99: System.IO.Compression.FileSystem => 0x24154ecb => 44
	i32 613668793, ; 100: System.Security.Cryptography.Algorithms => 0x2493d7b9 => 119
	i32 621990341, ; 101: Xamarin.AndroidX.Lifecycle.Runtime.Android.dll => 0x2512d1c5 => 247
	i32 627609679, ; 102: Xamarin.AndroidX.CustomView => 0x2568904f => 228
	i32 627931235, ; 103: nl\Microsoft.Maui.Controls.resources => 0x256d7863 => 333
	i32 639843206, ; 104: Xamarin.AndroidX.Emoji2.ViewsHelper.dll => 0x26233b86 => 234
	i32 643868501, ; 105: System.Net => 0x2660a755 => 81
	i32 662205335, ; 106: System.Text.Encodings.Web.dll => 0x27787397 => 136
	i32 663517072, ; 107: Xamarin.AndroidX.VersionedParcelable => 0x278c7790 => 275
	i32 666292255, ; 108: Xamarin.AndroidX.Arch.Core.Common.dll => 0x27b6d01f => 208
	i32 672442732, ; 109: System.Collections.Concurrent => 0x2814a96c => 8
	i32 679221896, ; 110: Xamarin.KotlinX.AtomicFU => 0x287c1a88 => 309
	i32 683518922, ; 111: System.Net.Security => 0x28bdabca => 73
	i32 688181140, ; 112: ca/Microsoft.Maui.Controls.resources.dll => 0x2904cf94 => 315
	i32 690569205, ; 113: System.Xml.Linq.dll => 0x29293ff5 => 155
	i32 691348768, ; 114: Xamarin.KotlinX.Coroutines.Android.dll => 0x29352520 => 311
	i32 693804605, ; 115: System.Windows => 0x295a9e3d => 154
	i32 699345723, ; 116: System.Reflection.Emit => 0x29af2b3b => 92
	i32 700284507, ; 117: Xamarin.Jetbrains.Annotations => 0x29bd7e5b => 305
	i32 700358131, ; 118: System.IO.Compression.ZipFile => 0x29be9df3 => 45
	i32 706645707, ; 119: ko/Microsoft.Maui.Controls.resources.dll => 0x2a1e8ecb => 330
	i32 709557578, ; 120: de/Microsoft.Maui.Controls.resources.dll => 0x2a4afd4a => 318
	i32 720511267, ; 121: Xamarin.Kotlin.StdLib.Jdk8 => 0x2af22123 => 308
	i32 722857257, ; 122: System.Runtime.Loader.dll => 0x2b15ed29 => 109
	i32 735137430, ; 123: System.Security.SecureString.dll => 0x2bd14e96 => 129
	i32 752232764, ; 124: System.Diagnostics.Contracts.dll => 0x2cd6293c => 25
	i32 755313932, ; 125: Xamarin.Android.Glide.Annotations.dll => 0x2d052d0c => 198
	i32 759454413, ; 126: System.Net.Requests => 0x2d445acd => 72
	i32 762598435, ; 127: System.IO.Pipes.dll => 0x2d745423 => 55
	i32 775507847, ; 128: System.IO.Compression => 0x2e394f87 => 46
	i32 777317022, ; 129: sk\Microsoft.Maui.Controls.resources => 0x2e54ea9e => 339
	i32 782533833, ; 130: Xamarin.Google.AutoValue.Annotations.dll => 0x2ea484c9 => 290
	i32 789151979, ; 131: Microsoft.Extensions.Options => 0x2f0980eb => 187
	i32 790371945, ; 132: Xamarin.AndroidX.CustomView.PoolingContainer.dll => 0x2f1c1e69 => 229
	i32 804715423, ; 133: System.Data.Common => 0x2ff6fb9f => 22
	i32 807930345, ; 134: Xamarin.AndroidX.Lifecycle.LiveData.Core.Ktx.dll => 0x302809e9 => 244
	i32 823281589, ; 135: System.Private.Uri.dll => 0x311247b5 => 86
	i32 830298997, ; 136: System.IO.Compression.Brotli => 0x317d5b75 => 43
	i32 832635846, ; 137: System.Xml.XPath.dll => 0x31a103c6 => 160
	i32 834051424, ; 138: System.Net.Quic => 0x31b69d60 => 71
	i32 843511501, ; 139: Xamarin.AndroidX.Print => 0x3246f6cd => 260
	i32 873119928, ; 140: Microsoft.VisualBasic => 0x340ac0b8 => 3
	i32 877678880, ; 141: System.Globalization.dll => 0x34505120 => 42
	i32 878954865, ; 142: System.Net.Http.Json => 0x3463c971 => 63
	i32 904024072, ; 143: System.ComponentModel.Primitives.dll => 0x35e25008 => 16
	i32 911108515, ; 144: System.IO.MemoryMappedFiles.dll => 0x364e69a3 => 53
	i32 926902833, ; 145: tr/Microsoft.Maui.Controls.resources.dll => 0x373f6a31 => 342
	i32 928116545, ; 146: Xamarin.Google.Guava.ListenableFuture => 0x3751ef41 => 294
	i32 952186615, ; 147: System.Runtime.InteropServices.JavaScript.dll => 0x38c136f7 => 105
	i32 956575887, ; 148: Xamarin.Kotlin.StdLib.Jdk8.dll => 0x3904308f => 308
	i32 966729478, ; 149: Xamarin.Google.Crypto.Tink.Android => 0x399f1f06 => 292
	i32 967690846, ; 150: Xamarin.AndroidX.Lifecycle.Common.dll => 0x39adca5e => 240
	i32 975236339, ; 151: System.Diagnostics.Tracing => 0x3a20ecf3 => 34
	i32 975874589, ; 152: System.Xml.XDocument => 0x3a2aaa1d => 158
	i32 986514023, ; 153: System.Private.DataContractSerialization.dll => 0x3acd0267 => 85
	i32 987214855, ; 154: System.Diagnostics.Tools => 0x3ad7b407 => 32
	i32 992768348, ; 155: System.Collections.dll => 0x3b2c715c => 12
	i32 994442037, ; 156: System.IO.FileSystem => 0x3b45fb35 => 51
	i32 996733531, ; 157: Xamarin.Google.Android.DataTransport.TransportBackendCct => 0x3b68f25b => 286
	i32 1001831731, ; 158: System.IO.UnmanagedMemoryStream.dll => 0x3bb6bd33 => 56
	i32 1012816738, ; 159: Xamarin.AndroidX.SavedState.dll => 0x3c5e5b62 => 264
	i32 1019214401, ; 160: System.Drawing => 0x3cbffa41 => 36
	i32 1028951442, ; 161: Microsoft.Extensions.DependencyInjection.Abstractions => 0x3d548d92 => 183
	i32 1029334545, ; 162: da/Microsoft.Maui.Controls.resources.dll => 0x3d5a6611 => 317
	i32 1031528504, ; 163: Xamarin.Google.ErrorProne.Annotations.dll => 0x3d7be038 => 293
	i32 1035644815, ; 164: Xamarin.AndroidX.AppCompat => 0x3dbaaf8f => 206
	i32 1036536393, ; 165: System.Drawing.Primitives.dll => 0x3dc84a49 => 35
	i32 1044663988, ; 166: System.Linq.Expressions.dll => 0x3e444eb4 => 58
	i32 1052210849, ; 167: Xamarin.AndroidX.Lifecycle.ViewModel.dll => 0x3eb776a1 => 250
	i32 1061503568, ; 168: Xamarin.Google.AutoValue.Annotations => 0x3f454250 => 290
	i32 1067306892, ; 169: GoogleGson => 0x3f9dcf8c => 174
	i32 1082857460, ; 170: System.ComponentModel.TypeConverter => 0x408b17f4 => 17
	i32 1084122840, ; 171: Xamarin.Kotlin.StdLib => 0x409e66d8 => 306
	i32 1098259244, ; 172: System => 0x41761b2c => 164
	i32 1118262833, ; 173: ko\Microsoft.Maui.Controls.resources => 0x42a75631 => 330
	i32 1121599056, ; 174: Xamarin.AndroidX.Lifecycle.Runtime.Ktx.dll => 0x42da3e50 => 248
	i32 1122050967, ; 175: Xamarin.Google.Android.ODML.Image => 0x42e12397 => 289
	i32 1127624469, ; 176: Microsoft.Extensions.Logging.Debug => 0x43362f15 => 186
	i32 1149092582, ; 177: Xamarin.AndroidX.Window => 0x447dc2e6 => 278
	i32 1157931901, ; 178: Microsoft.EntityFrameworkCore.Abstractions => 0x4504a37d => 176
	i32 1168523401, ; 179: pt\Microsoft.Maui.Controls.resources => 0x45a64089 => 336
	i32 1170634674, ; 180: System.Web.dll => 0x45c677b2 => 153
	i32 1175144683, ; 181: Xamarin.AndroidX.VectorDrawable.Animated => 0x460b48eb => 274
	i32 1178241025, ; 182: Xamarin.AndroidX.Navigation.Runtime.dll => 0x463a8801 => 258
	i32 1202000627, ; 183: Microsoft.EntityFrameworkCore.Abstractions.dll => 0x47a512f3 => 176
	i32 1203215381, ; 184: pl/Microsoft.Maui.Controls.resources.dll => 0x47b79c15 => 334
	i32 1203469131, ; 185: Xamarin.Google.MLKit.Common.dll => 0x47bb7b4b => 297
	i32 1204270330, ; 186: Xamarin.AndroidX.Arch.Core.Common => 0x47c7b4fa => 208
	i32 1204575371, ; 187: Microsoft.Extensions.Caching.Memory.dll => 0x47cc5c8b => 179
	i32 1208641965, ; 188: System.Diagnostics.Process => 0x480a69ad => 29
	i32 1219128291, ; 189: System.IO.IsolatedStorage => 0x48aa6be3 => 52
	i32 1234928153, ; 190: nb/Microsoft.Maui.Controls.resources.dll => 0x499b8219 => 332
	i32 1243150071, ; 191: Xamarin.AndroidX.Window.Extensions.Core.Core.dll => 0x4a18f6f7 => 279
	i32 1246548578, ; 192: Xamarin.AndroidX.Collection.Jvm.dll => 0x4a4cd262 => 218
	i32 1253011324, ; 193: Microsoft.Win32.Registry => 0x4aaf6f7c => 5
	i32 1260983243, ; 194: cs\Microsoft.Maui.Controls.resources => 0x4b2913cb => 316
	i32 1264511973, ; 195: Xamarin.AndroidX.Startup.StartupRuntime.dll => 0x4b5eebe5 => 268
	i32 1264890200, ; 196: Xamarin.KotlinX.Coroutines.Core.dll => 0x4b64b158 => 312
	i32 1267360935, ; 197: Xamarin.AndroidX.VectorDrawable => 0x4b8a64a7 => 273
	i32 1273260888, ; 198: Xamarin.AndroidX.Collection.Ktx => 0x4be46b58 => 219
	i32 1275534314, ; 199: Xamarin.KotlinX.Coroutines.Android => 0x4c071bea => 311
	i32 1278448581, ; 200: Xamarin.AndroidX.Annotation.Jvm => 0x4c3393c5 => 205
	i32 1293217323, ; 201: Xamarin.AndroidX.DrawerLayout.dll => 0x4d14ee2b => 231
	i32 1309188875, ; 202: System.Private.DataContractSerialization => 0x4e08a30b => 85
	i32 1322716291, ; 203: Xamarin.AndroidX.Window.dll => 0x4ed70c83 => 278
	i32 1324164729, ; 204: System.Linq => 0x4eed2679 => 61
	i32 1335329327, ; 205: System.Runtime.Serialization.Json.dll => 0x4f97822f => 112
	i32 1351347447, ; 206: Xamarin.GooglePlayServices.MLKit.BarcodeScanning => 0x508becf7 => 302
	i32 1355368438, ; 207: Xamarin.Google.MLKit.BarcodeScanning.Common.dll => 0x50c947f6 => 296
	i32 1358509622, ; 208: Xamarin.Google.MLKit.BarcodeScanning => 0x50f93636 => 295
	i32 1364015309, ; 209: System.IO => 0x514d38cd => 57
	i32 1373134921, ; 210: zh-Hans\Microsoft.Maui.Controls.resources => 0x51d86049 => 346
	i32 1376866003, ; 211: Xamarin.AndroidX.SavedState => 0x52114ed3 => 264
	i32 1379779777, ; 212: System.Resources.ResourceManager => 0x523dc4c1 => 99
	i32 1379897097, ; 213: Xamarin.JavaX.Inject => 0x523f8f09 => 304
	i32 1402170036, ; 214: System.Configuration.dll => 0x53936ab4 => 19
	i32 1406073936, ; 215: Xamarin.AndroidX.CoordinatorLayout => 0x53cefc50 => 224
	i32 1408764838, ; 216: System.Runtime.Serialization.Formatters.dll => 0x53f80ba6 => 111
	i32 1411638395, ; 217: System.Runtime.CompilerServices.Unsafe => 0x5423e47b => 101
	i32 1422545099, ; 218: System.Runtime.CompilerServices.VisualC => 0x54ca50cb => 102
	i32 1430672901, ; 219: ar\Microsoft.Maui.Controls.resources => 0x55465605 => 314
	i32 1434145427, ; 220: System.Runtime.Handles => 0x557b5293 => 104
	i32 1435222561, ; 221: Xamarin.Google.Crypto.Tink.Android.dll => 0x558bc221 => 292
	i32 1437299793, ; 222: Xamarin.AndroidX.Lifecycle.Common.Jvm => 0x55ab7451 => 241
	i32 1439761251, ; 223: System.Net.Quic.dll => 0x55d10363 => 71
	i32 1441095154, ; 224: Xamarin.AndroidX.Lifecycle.ViewModel.Android => 0x55e55df2 => 251
	i32 1452070440, ; 225: System.Formats.Asn1.dll => 0x568cd628 => 38
	i32 1453312822, ; 226: System.Diagnostics.Tools.dll => 0x569fcb36 => 32
	i32 1457743152, ; 227: System.Runtime.Extensions.dll => 0x56e36530 => 103
	i32 1458022317, ; 228: System.Net.Security.dll => 0x56e7a7ad => 73
	i32 1461004990, ; 229: es\Microsoft.Maui.Controls.resources => 0x57152abe => 320
	i32 1461234159, ; 230: System.Collections.Immutable.dll => 0x5718a9ef => 9
	i32 1461719063, ; 231: System.Security.Cryptography.OpenSsl => 0x57201017 => 123
	i32 1462112819, ; 232: System.IO.Compression.dll => 0x57261233 => 46
	i32 1469204771, ; 233: Xamarin.AndroidX.AppCompat.AppCompatResources => 0x57924923 => 207
	i32 1470490898, ; 234: Microsoft.Extensions.Primitives => 0x57a5e912 => 188
	i32 1479771757, ; 235: System.Collections.Immutable => 0x5833866d => 9
	i32 1480492111, ; 236: System.IO.Compression.Brotli.dll => 0x583e844f => 43
	i32 1487239319, ; 237: Microsoft.Win32.Primitives => 0x58a57897 => 4
	i32 1490025113, ; 238: Xamarin.AndroidX.SavedState.SavedState.Ktx.dll => 0x58cffa99 => 265
	i32 1493001747, ; 239: hi/Microsoft.Maui.Controls.resources.dll => 0x58fd6613 => 324
	i32 1514721132, ; 240: el/Microsoft.Maui.Controls.resources.dll => 0x5a48cf6c => 319
	i32 1536373174, ; 241: System.Diagnostics.TextWriterTraceListener => 0x5b9331b6 => 31
	i32 1543031311, ; 242: System.Text.RegularExpressions.dll => 0x5bf8ca0f => 138
	i32 1543355203, ; 243: System.Reflection.Emit.dll => 0x5bfdbb43 => 92
	i32 1550322496, ; 244: System.Reflection.Extensions.dll => 0x5c680b40 => 93
	i32 1551623176, ; 245: sk/Microsoft.Maui.Controls.resources.dll => 0x5c7be408 => 339
	i32 1565862583, ; 246: System.IO.FileSystem.Primitives => 0x5d552ab7 => 49
	i32 1566207040, ; 247: System.Threading.Tasks.Dataflow.dll => 0x5d5a6c40 => 141
	i32 1573704789, ; 248: System.Runtime.Serialization.Json => 0x5dccd455 => 112
	i32 1580037396, ; 249: System.Threading.Overlapped => 0x5e2d7514 => 140
	i32 1582372066, ; 250: Xamarin.AndroidX.DocumentFile.dll => 0x5e5114e2 => 230
	i32 1592978981, ; 251: System.Runtime.Serialization.dll => 0x5ef2ee25 => 115
	i32 1597949149, ; 252: Xamarin.Google.ErrorProne.Annotations => 0x5f3ec4dd => 293
	i32 1601112923, ; 253: System.Xml.Serialization => 0x5f6f0b5b => 157
	i32 1604827217, ; 254: System.Net.WebClient => 0x5fa7b851 => 76
	i32 1618516317, ; 255: System.Net.WebSockets.Client.dll => 0x6078995d => 79
	i32 1622152042, ; 256: Xamarin.AndroidX.Loader.dll => 0x60b0136a => 254
	i32 1622358360, ; 257: System.Dynamic.Runtime => 0x60b33958 => 37
	i32 1624863272, ; 258: Xamarin.AndroidX.ViewPager2 => 0x60d97228 => 277
	i32 1635184631, ; 259: Xamarin.AndroidX.Emoji2.ViewsHelper => 0x6176eff7 => 234
	i32 1636350590, ; 260: Xamarin.AndroidX.CursorAdapter => 0x6188ba7e => 227
	i32 1639515021, ; 261: System.Net.Http.dll => 0x61b9038d => 64
	i32 1639986890, ; 262: System.Text.RegularExpressions => 0x61c036ca => 138
	i32 1641389582, ; 263: System.ComponentModel.EventBasedAsync.dll => 0x61d59e0e => 15
	i32 1657153582, ; 264: System.Runtime => 0x62c6282e => 116
	i32 1658241508, ; 265: Xamarin.AndroidX.Tracing.Tracing.dll => 0x62d6c1e4 => 270
	i32 1658251792, ; 266: Xamarin.Google.Android.Material.dll => 0x62d6ea10 => 288
	i32 1670060433, ; 267: Xamarin.AndroidX.ConstraintLayout => 0x638b1991 => 222
	i32 1675553242, ; 268: System.IO.FileSystem.DriveInfo.dll => 0x63dee9da => 48
	i32 1677501392, ; 269: System.Net.Primitives.dll => 0x63fca3d0 => 70
	i32 1678508291, ; 270: System.Net.WebSockets => 0x640c0103 => 80
	i32 1679769178, ; 271: System.Security.Cryptography => 0x641f3e5a => 126
	i32 1689493916, ; 272: Microsoft.EntityFrameworkCore.dll => 0x64b3a19c => 175
	i32 1691477237, ; 273: System.Reflection.Metadata => 0x64d1e4f5 => 94
	i32 1696967625, ; 274: System.Security.Cryptography.Csp => 0x6525abc9 => 121
	i32 1701541528, ; 275: System.Diagnostics.Debug.dll => 0x656b7698 => 26
	i32 1718006957, ; 276: Xamarin.GooglePlayServices.MLKit.BarcodeScanning.dll => 0x6666b4ad => 302
	i32 1720223769, ; 277: Xamarin.AndroidX.Lifecycle.LiveData.Core.Ktx => 0x66888819 => 244
	i32 1726116996, ; 278: System.Reflection.dll => 0x66e27484 => 97
	i32 1728033016, ; 279: System.Diagnostics.FileVersionInfo.dll => 0x66ffb0f8 => 28
	i32 1729485958, ; 280: Xamarin.AndroidX.CardView.dll => 0x6715dc86 => 216
	i32 1736233607, ; 281: ro/Microsoft.Maui.Controls.resources.dll => 0x677cd287 => 337
	i32 1743415430, ; 282: ca\Microsoft.Maui.Controls.resources => 0x67ea6886 => 315
	i32 1744735666, ; 283: System.Transactions.Local.dll => 0x67fe8db2 => 149
	i32 1746316138, ; 284: Mono.Android.Export => 0x6816ab6a => 169
	i32 1750313021, ; 285: Microsoft.Win32.Primitives.dll => 0x6853a83d => 4
	i32 1758240030, ; 286: System.Resources.Reader.dll => 0x68cc9d1e => 98
	i32 1763938596, ; 287: System.Diagnostics.TraceSource.dll => 0x69239124 => 33
	i32 1765942094, ; 288: System.Reflection.Extensions => 0x6942234e => 93
	i32 1766324549, ; 289: Xamarin.AndroidX.SwipeRefreshLayout => 0x6947f945 => 269
	i32 1770582343, ; 290: Microsoft.Extensions.Logging.dll => 0x6988f147 => 184
	i32 1772434258, ; 291: NekrasovskyAPP => 0x69a53352 => 0
	i32 1776026572, ; 292: System.Core.dll => 0x69dc03cc => 21
	i32 1777075843, ; 293: System.Globalization.Extensions.dll => 0x69ec0683 => 41
	i32 1780572499, ; 294: Mono.Android.Runtime.dll => 0x6a216153 => 170
	i32 1782862114, ; 295: ms\Microsoft.Maui.Controls.resources => 0x6a445122 => 331
	i32 1788241197, ; 296: Xamarin.AndroidX.Fragment => 0x6a96652d => 236
	i32 1793755602, ; 297: he\Microsoft.Maui.Controls.resources => 0x6aea89d2 => 323
	i32 1808609942, ; 298: Xamarin.AndroidX.Loader => 0x6bcd3296 => 254
	i32 1813058853, ; 299: Xamarin.Kotlin.StdLib.dll => 0x6c111525 => 306
	i32 1813201214, ; 300: Xamarin.Google.Android.Material => 0x6c13413e => 288
	i32 1818569960, ; 301: Xamarin.AndroidX.Navigation.UI.dll => 0x6c652ce8 => 259
	i32 1818787751, ; 302: Microsoft.VisualBasic.Core => 0x6c687fa7 => 2
	i32 1824175904, ; 303: System.Text.Encoding.Extensions => 0x6cbab720 => 134
	i32 1824722060, ; 304: System.Runtime.Serialization.Formatters => 0x6cc30c8c => 111
	i32 1828688058, ; 305: Microsoft.Extensions.Logging.Abstractions.dll => 0x6cff90ba => 185
	i32 1842015223, ; 306: uk/Microsoft.Maui.Controls.resources.dll => 0x6dcaebf7 => 343
	i32 1847515442, ; 307: Xamarin.Android.Glide.Annotations => 0x6e1ed932 => 198
	i32 1853025655, ; 308: sv\Microsoft.Maui.Controls.resources => 0x6e72ed77 => 340
	i32 1858542181, ; 309: System.Linq.Expressions => 0x6ec71a65 => 58
	i32 1866818530, ; 310: Xamarin.AndroidX.Camera.Video => 0x6f4563e2 => 214
	i32 1870277092, ; 311: System.Reflection.Primitives => 0x6f7a29e4 => 95
	i32 1875935024, ; 312: fr\Microsoft.Maui.Controls.resources => 0x6fd07f30 => 322
	i32 1876173635, ; 313: Xamarin.Firebase.Encoders.Proto => 0x6fd42343 => 284
	i32 1879696579, ; 314: System.Formats.Tar.dll => 0x7009e4c3 => 39
	i32 1885316902, ; 315: Xamarin.AndroidX.Arch.Core.Runtime.dll => 0x705fa726 => 209
	i32 1888955245, ; 316: System.Diagnostics.Contracts => 0x70972b6d => 25
	i32 1889954781, ; 317: System.Reflection.Metadata.dll => 0x70a66bdd => 94
	i32 1898237753, ; 318: System.Reflection.DispatchProxy => 0x7124cf39 => 89
	i32 1900610850, ; 319: System.Resources.ResourceManager.dll => 0x71490522 => 99
	i32 1908813208, ; 320: Xamarin.GooglePlayServices.Basement => 0x71c62d98 => 301
	i32 1910275211, ; 321: System.Collections.NonGeneric.dll => 0x71dc7c8b => 10
	i32 1939592360, ; 322: System.Private.Xml.Linq => 0x739bd4a8 => 87
	i32 1956758971, ; 323: System.Resources.Writer => 0x74a1c5bb => 100
	i32 1961813231, ; 324: Xamarin.AndroidX.Security.SecurityCrypto.dll => 0x74eee4ef => 266
	i32 1968388702, ; 325: Microsoft.Extensions.Configuration.dll => 0x75533a5e => 180
	i32 1985761444, ; 326: Xamarin.Android.Glide.GifDecoder => 0x765c50a4 => 200
	i32 2003115576, ; 327: el\Microsoft.Maui.Controls.resources => 0x77651e38 => 319
	i32 2011961780, ; 328: System.Buffers.dll => 0x77ec19b4 => 7
	i32 2019465201, ; 329: Xamarin.AndroidX.Lifecycle.ViewModel => 0x785e97f1 => 250
	i32 2025202353, ; 330: ar/Microsoft.Maui.Controls.resources.dll => 0x78b622b1 => 314
	i32 2031763787, ; 331: Xamarin.Android.Glide => 0x791a414b => 197
	i32 2045470958, ; 332: System.Private.Xml => 0x79eb68ee => 88
	i32 2055257422, ; 333: Xamarin.AndroidX.Lifecycle.LiveData.Core.dll => 0x7a80bd4e => 243
	i32 2060060697, ; 334: System.Windows.dll => 0x7aca0819 => 154
	i32 2066184531, ; 335: de\Microsoft.Maui.Controls.resources => 0x7b277953 => 318
	i32 2070888862, ; 336: System.Diagnostics.TraceSource => 0x7b6f419e => 33
	i32 2079903147, ; 337: System.Runtime.dll => 0x7bf8cdab => 116
	i32 2090596640, ; 338: System.Numerics.Vectors => 0x7c9bf920 => 82
	i32 2100944304, ; 339: BarcodeScanning.Native.Maui.dll => 0x7d39ddb0 => 173
	i32 2124230737, ; 340: Xamarin.Google.Android.DataTransport.TransportBackendCct.dll => 0x7e9d3051 => 286
	i32 2127167465, ; 341: System.Console => 0x7ec9ffe9 => 20
	i32 2129483829, ; 342: Xamarin.GooglePlayServices.Base.dll => 0x7eed5835 => 300
	i32 2142473426, ; 343: System.Collections.Specialized => 0x7fb38cd2 => 11
	i32 2143790110, ; 344: System.Xml.XmlSerializer.dll => 0x7fc7a41e => 162
	i32 2146852085, ; 345: Microsoft.VisualBasic.dll => 0x7ff65cf5 => 3
	i32 2159891885, ; 346: Microsoft.Maui => 0x80bd55ad => 192
	i32 2169148018, ; 347: hu\Microsoft.Maui.Controls.resources => 0x814a9272 => 326
	i32 2174878672, ; 348: Xamarin.Firebase.Annotations => 0x81a203d0 => 280
	i32 2181898931, ; 349: Microsoft.Extensions.Options.dll => 0x820d22b3 => 187
	i32 2188559649, ; 350: Xamarin.Google.MLKit.BarcodeScanning.dll => 0x8272c521 => 295
	i32 2192057212, ; 351: Microsoft.Extensions.Logging.Abstractions => 0x82a8237c => 185
	i32 2193016926, ; 352: System.ObjectModel.dll => 0x82b6c85e => 84
	i32 2201107256, ; 353: Xamarin.KotlinX.Coroutines.Core.Jvm.dll => 0x83323b38 => 313
	i32 2201231467, ; 354: System.Net.Http => 0x8334206b => 64
	i32 2207618523, ; 355: it\Microsoft.Maui.Controls.resources => 0x839595db => 328
	i32 2217644978, ; 356: Xamarin.AndroidX.VectorDrawable.Animated.dll => 0x842e93b2 => 274
	i32 2222056684, ; 357: System.Threading.Tasks.Parallel => 0x8471e4ec => 143
	i32 2244775296, ; 358: Xamarin.AndroidX.LocalBroadcastManager => 0x85cc8d80 => 255
	i32 2252106437, ; 359: System.Xml.Serialization.dll => 0x863c6ac5 => 157
	i32 2252897993, ; 360: Microsoft.EntityFrameworkCore => 0x86487ec9 => 175
	i32 2256313426, ; 361: System.Globalization.Extensions => 0x867c9c52 => 41
	i32 2265110946, ; 362: System.Security.AccessControl.dll => 0x8702d9a2 => 117
	i32 2266799131, ; 363: Microsoft.Extensions.Configuration.Abstractions => 0x871c9c1b => 181
	i32 2267999099, ; 364: Xamarin.Android.Glide.DiskLruCache.dll => 0x872eeb7b => 199
	i32 2270573516, ; 365: fr/Microsoft.Maui.Controls.resources.dll => 0x875633cc => 322
	i32 2279755925, ; 366: Xamarin.AndroidX.RecyclerView.dll => 0x87e25095 => 262
	i32 2293034957, ; 367: System.ServiceModel.Web.dll => 0x88acefcd => 131
	i32 2294913272, ; 368: Npgsql => 0x88c998f8 => 195
	i32 2295906218, ; 369: System.Net.Sockets => 0x88d8bfaa => 75
	i32 2298471582, ; 370: System.Net.Mail => 0x88ffe49e => 66
	i32 2303942373, ; 371: nb\Microsoft.Maui.Controls.resources => 0x89535ee5 => 332
	i32 2305521784, ; 372: System.Private.CoreLib.dll => 0x896b7878 => 172
	i32 2315684594, ; 373: Xamarin.AndroidX.Annotation.dll => 0x8a068af2 => 203
	i32 2320631194, ; 374: System.Threading.Tasks.Parallel.dll => 0x8a52059a => 143
	i32 2334995809, ; 375: Npgsql.EntityFrameworkCore.PostgreSQL.dll => 0x8b2d3561 => 196
	i32 2340441535, ; 376: System.Runtime.InteropServices.RuntimeInformation.dll => 0x8b804dbf => 106
	i32 2344264397, ; 377: System.ValueTuple => 0x8bbaa2cd => 151
	i32 2353062107, ; 378: System.Net.Primitives => 0x8c40e0db => 70
	i32 2368005991, ; 379: System.Xml.ReaderWriter.dll => 0x8d24e767 => 156
	i32 2371007202, ; 380: Microsoft.Extensions.Configuration => 0x8d52b2e2 => 180
	i32 2378619854, ; 381: System.Security.Cryptography.Csp.dll => 0x8dc6dbce => 121
	i32 2383496789, ; 382: System.Security.Principal.Windows.dll => 0x8e114655 => 127
	i32 2395872292, ; 383: id\Microsoft.Maui.Controls.resources => 0x8ece1c24 => 327
	i32 2401565422, ; 384: System.Web.HttpUtility => 0x8f24faee => 152
	i32 2403452196, ; 385: Xamarin.AndroidX.Emoji2.dll => 0x8f41c524 => 233
	i32 2418341376, ; 386: Xamarin.AndroidX.Camera.Video.dll => 0x9024f600 => 214
	i32 2421380589, ; 387: System.Threading.Tasks.Dataflow => 0x905355ed => 141
	i32 2423080555, ; 388: Xamarin.AndroidX.Collection.Ktx.dll => 0x906d466b => 219
	i32 2425270691, ; 389: Xamarin.Google.MLKit.BarcodeScanning.Common => 0x908eb1a3 => 296
	i32 2427813419, ; 390: hi\Microsoft.Maui.Controls.resources => 0x90b57e2b => 324
	i32 2435356389, ; 391: System.Console.dll => 0x912896e5 => 20
	i32 2435904999, ; 392: System.ComponentModel.DataAnnotations.dll => 0x9130f5e7 => 14
	i32 2454642406, ; 393: System.Text.Encoding.dll => 0x924edee6 => 135
	i32 2458678730, ; 394: System.Net.Sockets.dll => 0x928c75ca => 75
	i32 2459001652, ; 395: System.Linq.Parallel.dll => 0x92916334 => 59
	i32 2465532216, ; 396: Xamarin.AndroidX.ConstraintLayout.Core.dll => 0x92f50938 => 223
	i32 2471841756, ; 397: netstandard.dll => 0x93554fdc => 167
	i32 2475788418, ; 398: Java.Interop.dll => 0x93918882 => 168
	i32 2480646305, ; 399: Microsoft.Maui.Controls => 0x93dba8a1 => 190
	i32 2483903535, ; 400: System.ComponentModel.EventBasedAsync => 0x940d5c2f => 15
	i32 2484371297, ; 401: System.Net.ServicePoint => 0x94147f61 => 74
	i32 2490993605, ; 402: System.AppContext.dll => 0x94798bc5 => 6
	i32 2501346920, ; 403: System.Data.DataSetExtensions => 0x95178668 => 23
	i32 2505896520, ; 404: Xamarin.AndroidX.Lifecycle.Runtime.dll => 0x955cf248 => 246
	i32 2522472828, ; 405: Xamarin.Android.Glide.dll => 0x9659e17c => 197
	i32 2538310050, ; 406: System.Reflection.Emit.Lightweight.dll => 0x974b89a2 => 91
	i32 2550873716, ; 407: hr\Microsoft.Maui.Controls.resources => 0x980b3e74 => 325
	i32 2562349572, ; 408: Microsoft.CSharp => 0x98ba5a04 => 1
	i32 2570120770, ; 409: System.Text.Encodings.Web => 0x9930ee42 => 136
	i32 2577256205, ; 410: Xamarin.AndroidX.Lifecycle.Runtime.Ktx.Android => 0x999dcf0d => 249
	i32 2581783588, ; 411: Xamarin.AndroidX.Lifecycle.Runtime.Ktx => 0x99e2e424 => 248
	i32 2581819634, ; 412: Xamarin.AndroidX.VectorDrawable.dll => 0x99e370f2 => 273
	i32 2585220780, ; 413: System.Text.Encoding.Extensions.dll => 0x9a1756ac => 134
	i32 2585805581, ; 414: System.Net.Ping => 0x9a20430d => 69
	i32 2589602615, ; 415: System.Threading.ThreadPool => 0x9a5a3337 => 146
	i32 2593496499, ; 416: pl\Microsoft.Maui.Controls.resources => 0x9a959db3 => 334
	i32 2605712449, ; 417: Xamarin.KotlinX.Coroutines.Core.Jvm => 0x9b500441 => 313
	i32 2615233544, ; 418: Xamarin.AndroidX.Fragment.Ktx => 0x9be14c08 => 237
	i32 2616218305, ; 419: Microsoft.Extensions.Logging.Debug.dll => 0x9bf052c1 => 186
	i32 2617129537, ; 420: System.Private.Xml.dll => 0x9bfe3a41 => 88
	i32 2618712057, ; 421: System.Reflection.TypeExtensions.dll => 0x9c165ff9 => 96
	i32 2620111890, ; 422: Xamarin.Firebase.Encoders.dll => 0x9c2bbc12 => 282
	i32 2620871830, ; 423: Xamarin.AndroidX.CursorAdapter.dll => 0x9c375496 => 227
	i32 2624644809, ; 424: Xamarin.AndroidX.DynamicAnimation => 0x9c70e6c9 => 232
	i32 2626831493, ; 425: ja\Microsoft.Maui.Controls.resources => 0x9c924485 => 329
	i32 2627185994, ; 426: System.Diagnostics.TextWriterTraceListener.dll => 0x9c97ad4a => 31
	i32 2629843544, ; 427: System.IO.Compression.ZipFile.dll => 0x9cc03a58 => 45
	i32 2633051222, ; 428: Xamarin.AndroidX.Lifecycle.LiveData => 0x9cf12c56 => 242
	i32 2634653062, ; 429: Microsoft.EntityFrameworkCore.Relational.dll => 0x9d099d86 => 177
	i32 2639764100, ; 430: Xamarin.Firebase.Encoders => 0x9d579a84 => 282
	i32 2663391936, ; 431: Xamarin.Android.Glide.DiskLruCache => 0x9ec022c0 => 199
	i32 2663698177, ; 432: System.Runtime.Loader => 0x9ec4cf01 => 109
	i32 2664396074, ; 433: System.Xml.XDocument.dll => 0x9ecf752a => 158
	i32 2665622720, ; 434: System.Drawing.Primitives => 0x9ee22cc0 => 35
	i32 2671474046, ; 435: Xamarin.KotlinX.Coroutines.Core => 0x9f3b757e => 312
	i32 2676780864, ; 436: System.Data.Common.dll => 0x9f8c6f40 => 22
	i32 2686887180, ; 437: System.Runtime.Serialization.Xml.dll => 0xa026a50c => 114
	i32 2693849962, ; 438: System.IO.dll => 0xa090e36a => 57
	i32 2701096212, ; 439: Xamarin.AndroidX.Tracing.Tracing => 0xa0ff7514 => 270
	i32 2715334215, ; 440: System.Threading.Tasks.dll => 0xa1d8b647 => 144
	i32 2717744543, ; 441: System.Security.Claims => 0xa1fd7d9f => 118
	i32 2719963679, ; 442: System.Security.Cryptography.Cng.dll => 0xa21f5a1f => 120
	i32 2724373263, ; 443: System.Runtime.Numerics.dll => 0xa262a30f => 110
	i32 2732626843, ; 444: Xamarin.AndroidX.Activity => 0xa2e0939b => 201
	i32 2735172069, ; 445: System.Threading.Channels => 0xa30769e5 => 139
	i32 2737747696, ; 446: Xamarin.AndroidX.AppCompat.AppCompatResources.dll => 0xa32eb6f0 => 207
	i32 2740948882, ; 447: System.IO.Pipes.AccessControl => 0xa35f8f92 => 54
	i32 2748088231, ; 448: System.Runtime.InteropServices.JavaScript => 0xa3cc7fa7 => 105
	i32 2752995522, ; 449: pt-BR\Microsoft.Maui.Controls.resources => 0xa41760c2 => 335
	i32 2758225723, ; 450: Microsoft.Maui.Controls.Xaml => 0xa4672f3b => 191
	i32 2764765095, ; 451: Microsoft.Maui.dll => 0xa4caf7a7 => 192
	i32 2765824710, ; 452: System.Text.Encoding.CodePages.dll => 0xa4db22c6 => 133
	i32 2766642685, ; 453: Xamarin.AndroidX.Lifecycle.ViewModel.Android.dll => 0xa4e79dfd => 251
	i32 2770495804, ; 454: Xamarin.Jetbrains.Annotations.dll => 0xa522693c => 305
	i32 2778768386, ; 455: Xamarin.AndroidX.ViewPager.dll => 0xa5a0a402 => 276
	i32 2779977773, ; 456: Xamarin.AndroidX.ResourceInspection.Annotation.dll => 0xa5b3182d => 263
	i32 2780199943, ; 457: Xamarin.AndroidX.Lifecycle.Common.Jvm.dll => 0xa5b67c07 => 241
	i32 2785988530, ; 458: th\Microsoft.Maui.Controls.resources => 0xa60ecfb2 => 341
	i32 2788224221, ; 459: Xamarin.AndroidX.Fragment.Ktx.dll => 0xa630ecdd => 237
	i32 2801831435, ; 460: Microsoft.Maui.Graphics => 0xa7008e0b => 194
	i32 2803228030, ; 461: System.Xml.XPath.XDocument.dll => 0xa715dd7e => 159
	i32 2804607052, ; 462: Xamarin.Firebase.Components.dll => 0xa72ae84c => 281
	i32 2806116107, ; 463: es/Microsoft.Maui.Controls.resources.dll => 0xa741ef0b => 320
	i32 2810250172, ; 464: Xamarin.AndroidX.CoordinatorLayout.dll => 0xa78103bc => 224
	i32 2819470561, ; 465: System.Xml.dll => 0xa80db4e1 => 163
	i32 2821205001, ; 466: System.ServiceProcess.dll => 0xa8282c09 => 132
	i32 2821294376, ; 467: Xamarin.AndroidX.ResourceInspection.Annotation => 0xa8298928 => 263
	i32 2824502124, ; 468: System.Xml.XmlDocument => 0xa85a7b6c => 161
	i32 2828186339, ; 469: Xamarin.AndroidX.Concurrent.Futures.Ktx.dll => 0xa892b2e3 => 221
	i32 2831556043, ; 470: nl/Microsoft.Maui.Controls.resources.dll => 0xa8c61dcb => 333
	i32 2838993487, ; 471: Xamarin.AndroidX.Lifecycle.ViewModel.Ktx.dll => 0xa9379a4f => 252
	i32 2847418871, ; 472: Xamarin.GooglePlayServices.Base => 0xa9b829f7 => 300
	i32 2847789619, ; 473: Microsoft.EntityFrameworkCore.Relational => 0xa9bdd233 => 177
	i32 2849599387, ; 474: System.Threading.Overlapped.dll => 0xa9d96f9b => 140
	i32 2853208004, ; 475: Xamarin.AndroidX.ViewPager => 0xaa107fc4 => 276
	i32 2855708567, ; 476: Xamarin.AndroidX.Transition => 0xaa36a797 => 272
	i32 2861098320, ; 477: Mono.Android.Export.dll => 0xaa88e550 => 169
	i32 2861189240, ; 478: Microsoft.Maui.Essentials => 0xaa8a4878 => 193
	i32 2868099152, ; 479: Xamarin.Google.MLKit.Vision.Common.dll => 0xaaf3b850 => 298
	i32 2870099610, ; 480: Xamarin.AndroidX.Activity.Ktx.dll => 0xab123e9a => 202
	i32 2875164099, ; 481: Jsr305Binding.dll => 0xab5f85c3 => 291
	i32 2875220617, ; 482: System.Globalization.Calendars.dll => 0xab606289 => 40
	i32 2884993177, ; 483: Xamarin.AndroidX.ExifInterface => 0xabf58099 => 235
	i32 2887636118, ; 484: System.Net.dll => 0xac1dd496 => 81
	i32 2899753641, ; 485: System.IO.UnmanagedMemoryStream => 0xacd6baa9 => 56
	i32 2900621748, ; 486: System.Dynamic.Runtime.dll => 0xace3f9b4 => 37
	i32 2901442782, ; 487: System.Reflection => 0xacf080de => 97
	i32 2905242038, ; 488: mscorlib.dll => 0xad2a79b6 => 166
	i32 2909740682, ; 489: System.Private.CoreLib => 0xad6f1e8a => 172
	i32 2916838712, ; 490: Xamarin.AndroidX.ViewPager2.dll => 0xaddb6d38 => 277
	i32 2919462931, ; 491: System.Numerics.Vectors.dll => 0xae037813 => 82
	i32 2921128767, ; 492: Xamarin.AndroidX.Annotation.Experimental.dll => 0xae1ce33f => 204
	i32 2936416060, ; 493: System.Resources.Reader => 0xaf06273c => 98
	i32 2940926066, ; 494: System.Diagnostics.StackTrace.dll => 0xaf4af872 => 30
	i32 2942453041, ; 495: System.Xml.XPath.XDocument => 0xaf624531 => 159
	i32 2959614098, ; 496: System.ComponentModel.dll => 0xb0682092 => 18
	i32 2965157864, ; 497: Xamarin.AndroidX.Camera.View => 0xb0bcb7e8 => 215
	i32 2968338931, ; 498: System.Security.Principal.Windows => 0xb0ed41f3 => 127
	i32 2972252294, ; 499: System.Security.Cryptography.Algorithms.dll => 0xb128f886 => 119
	i32 2978675010, ; 500: Xamarin.AndroidX.DrawerLayout => 0xb18af942 => 231
	i32 2987532451, ; 501: Xamarin.AndroidX.Security.SecurityCrypto => 0xb21220a3 => 266
	i32 2991449226, ; 502: Xamarin.AndroidX.Camera.Core => 0xb24de48a => 212
	i32 2996846495, ; 503: Xamarin.AndroidX.Lifecycle.Process.dll => 0xb2a03f9f => 245
	i32 3000842441, ; 504: Xamarin.AndroidX.Camera.View.dll => 0xb2dd38c9 => 215
	i32 3016983068, ; 505: Xamarin.AndroidX.Startup.StartupRuntime => 0xb3d3821c => 268
	i32 3023353419, ; 506: WindowsBase.dll => 0xb434b64b => 165
	i32 3024354802, ; 507: Xamarin.AndroidX.Legacy.Support.Core.Utils => 0xb443fdf2 => 239
	i32 3038032645, ; 508: _Microsoft.Android.Resource.Designer.dll => 0xb514b305 => 348
	i32 3047751430, ; 509: Xamarin.AndroidX.Camera.Core.dll => 0xb5a8ff06 => 212
	i32 3056245963, ; 510: Xamarin.AndroidX.SavedState.SavedState.Ktx => 0xb62a9ccb => 265
	i32 3057625584, ; 511: Xamarin.AndroidX.Navigation.Common => 0xb63fa9f0 => 256
	i32 3058099980, ; 512: Xamarin.GooglePlayServices.Tasks => 0xb646e70c => 303
	i32 3059408633, ; 513: Mono.Android.Runtime => 0xb65adef9 => 170
	i32 3059793426, ; 514: System.ComponentModel.Primitives => 0xb660be12 => 16
	i32 3069363400, ; 515: Microsoft.Extensions.Caching.Abstractions.dll => 0xb6f2c4c8 => 178
	i32 3075834255, ; 516: System.Threading.Tasks => 0xb755818f => 144
	i32 3077302341, ; 517: hu/Microsoft.Maui.Controls.resources.dll => 0xb76be845 => 326
	i32 3090735792, ; 518: System.Security.Cryptography.X509Certificates.dll => 0xb838e2b0 => 125
	i32 3099732863, ; 519: System.Security.Claims.dll => 0xb8c22b7f => 118
	i32 3103600923, ; 520: System.Formats.Asn1 => 0xb8fd311b => 38
	i32 3111772706, ; 521: System.Runtime.Serialization => 0xb979e222 => 115
	i32 3121463068, ; 522: System.IO.FileSystem.AccessControl.dll => 0xba0dbf1c => 47
	i32 3124832203, ; 523: System.Threading.Tasks.Extensions => 0xba4127cb => 142
	i32 3132293585, ; 524: System.Security.AccessControl => 0xbab301d1 => 117
	i32 3147165239, ; 525: System.Diagnostics.Tracing.dll => 0xbb95ee37 => 34
	i32 3148237826, ; 526: GoogleGson.dll => 0xbba64c02 => 174
	i32 3155362983, ; 527: Xamarin.Google.Android.DataTransport.TransportApi => 0xbc1304a7 => 285
	i32 3159123045, ; 528: System.Reflection.Primitives.dll => 0xbc4c6465 => 95
	i32 3160747431, ; 529: System.IO.MemoryMappedFiles => 0xbc652da7 => 53
	i32 3178803400, ; 530: Xamarin.AndroidX.Navigation.Fragment.dll => 0xbd78b0c8 => 257
	i32 3192346100, ; 531: System.Security.SecureString => 0xbe4755f4 => 129
	i32 3193515020, ; 532: System.Web => 0xbe592c0c => 153
	i32 3195844289, ; 533: Microsoft.Extensions.Caching.Abstractions => 0xbe7cb6c1 => 178
	i32 3204380047, ; 534: System.Data.dll => 0xbefef58f => 24
	i32 3209718065, ; 535: System.Xml.XmlDocument.dll => 0xbf506931 => 161
	i32 3211777861, ; 536: Xamarin.AndroidX.DocumentFile => 0xbf6fd745 => 230
	i32 3220365878, ; 537: System.Threading => 0xbff2e236 => 148
	i32 3226221578, ; 538: System.Runtime.Handles.dll => 0xc04c3c0a => 104
	i32 3230466174, ; 539: Xamarin.GooglePlayServices.Basement.dll => 0xc08d007e => 301
	i32 3251039220, ; 540: System.Reflection.DispatchProxy.dll => 0xc1c6ebf4 => 89
	i32 3258312781, ; 541: Xamarin.AndroidX.CardView => 0xc235e84d => 216
	i32 3265493905, ; 542: System.Linq.Queryable.dll => 0xc2a37b91 => 60
	i32 3265893370, ; 543: System.Threading.Tasks.Extensions.dll => 0xc2a993fa => 142
	i32 3277815716, ; 544: System.Resources.Writer.dll => 0xc35f7fa4 => 100
	i32 3279906254, ; 545: Microsoft.Win32.Registry.dll => 0xc37f65ce => 5
	i32 3280506390, ; 546: System.ComponentModel.Annotations.dll => 0xc3888e16 => 13
	i32 3290767353, ; 547: System.Security.Cryptography.Encoding => 0xc4251ff9 => 122
	i32 3299363146, ; 548: System.Text.Encoding => 0xc4a8494a => 135
	i32 3303498502, ; 549: System.Diagnostics.FileVersionInfo => 0xc4e76306 => 28
	i32 3305363605, ; 550: fi\Microsoft.Maui.Controls.resources => 0xc503d895 => 321
	i32 3316684772, ; 551: System.Net.Requests.dll => 0xc5b097e4 => 72
	i32 3317135071, ; 552: Xamarin.AndroidX.CustomView.dll => 0xc5b776df => 228
	i32 3317144872, ; 553: System.Data => 0xc5b79d28 => 24
	i32 3340431453, ; 554: Xamarin.AndroidX.Arch.Core.Runtime => 0xc71af05d => 209
	i32 3345895724, ; 555: Xamarin.AndroidX.ProfileInstaller.ProfileInstaller.dll => 0xc76e512c => 261
	i32 3346324047, ; 556: Xamarin.AndroidX.Navigation.Runtime => 0xc774da4f => 258
	i32 3357674450, ; 557: ru\Microsoft.Maui.Controls.resources => 0xc8220bd2 => 338
	i32 3358260929, ; 558: System.Text.Json => 0xc82afec1 => 137
	i32 3359991071, ; 559: Xamarin.AndroidX.Tracing.Tracing.Ktx.dll => 0xc845651f => 271
	i32 3362336904, ; 560: Xamarin.AndroidX.Activity.Ktx => 0xc8693088 => 202
	i32 3362522851, ; 561: Xamarin.AndroidX.Core => 0xc86c06e3 => 225
	i32 3366347497, ; 562: Java.Interop => 0xc8a662e9 => 168
	i32 3371992681, ; 563: Xamarin.Firebase.Encoders.Proto.dll => 0xc8fc8669 => 284
	i32 3374999561, ; 564: Xamarin.AndroidX.RecyclerView => 0xc92a6809 => 262
	i32 3381016424, ; 565: da\Microsoft.Maui.Controls.resources => 0xc9863768 => 317
	i32 3383578424, ; 566: Xamarin.Firebase.Encoders.JSON => 0xc9ad4f38 => 283
	i32 3395150330, ; 567: System.Runtime.CompilerServices.Unsafe.dll => 0xca5de1fa => 101
	i32 3403906625, ; 568: System.Security.Cryptography.OpenSsl.dll => 0xcae37e41 => 123
	i32 3405233483, ; 569: Xamarin.AndroidX.CustomView.PoolingContainer => 0xcaf7bd4b => 229
	i32 3411362516, ; 570: Xamarin.Google.MLKit.Vision.Interfaces => 0xcb5542d4 => 299
	i32 3413944578, ; 571: Xamarin.AndroidX.Camera.Camera2.dll => 0xcb7ca902 => 211
	i32 3421910702, ; 572: Xamarin.AndroidX.Camera.Camera2 => 0xcbf636ae => 211
	i32 3428513518, ; 573: Microsoft.Extensions.DependencyInjection.dll => 0xcc5af6ee => 182
	i32 3429136800, ; 574: System.Xml => 0xcc6479a0 => 163
	i32 3430777524, ; 575: netstandard => 0xcc7d82b4 => 167
	i32 3441283291, ; 576: Xamarin.AndroidX.DynamicAnimation.dll => 0xcd1dd0db => 232
	i32 3445260447, ; 577: System.Formats.Tar => 0xcd5a809f => 39
	i32 3452344032, ; 578: Microsoft.Maui.Controls.Compatibility.dll => 0xcdc696e0 => 189
	i32 3463511458, ; 579: hr/Microsoft.Maui.Controls.resources.dll => 0xce70fda2 => 325
	i32 3471940407, ; 580: System.ComponentModel.TypeConverter.dll => 0xcef19b37 => 17
	i32 3476120550, ; 581: Mono.Android => 0xcf3163e6 => 171
	i32 3479583265, ; 582: ru/Microsoft.Maui.Controls.resources.dll => 0xcf663a21 => 338
	i32 3484440000, ; 583: ro\Microsoft.Maui.Controls.resources => 0xcfb055c0 => 337
	i32 3485117614, ; 584: System.Text.Json.dll => 0xcfbaacae => 137
	i32 3486566296, ; 585: System.Transactions => 0xcfd0c798 => 150
	i32 3493954962, ; 586: Xamarin.AndroidX.Concurrent.Futures.dll => 0xd0418592 => 220
	i32 3509114376, ; 587: System.Xml.Linq => 0xd128d608 => 155
	i32 3515174580, ; 588: System.Security.dll => 0xd1854eb4 => 130
	i32 3530912306, ; 589: System.Configuration => 0xd2757232 => 19
	i32 3539954161, ; 590: System.Net.HttpListener => 0xd2ff69f1 => 65
	i32 3560100363, ; 591: System.Threading.Timer => 0xd432d20b => 147
	i32 3570554715, ; 592: System.IO.FileSystem.AccessControl => 0xd4d2575b => 47
	i32 3580758918, ; 593: zh-HK\Microsoft.Maui.Controls.resources => 0xd56e0b86 => 345
	i32 3597029428, ; 594: Xamarin.Android.Glide.GifDecoder.dll => 0xd6665034 => 200
	i32 3598340787, ; 595: System.Net.WebSockets.Client => 0xd67a52b3 => 79
	i32 3608519521, ; 596: System.Linq.dll => 0xd715a361 => 61
	i32 3624195450, ; 597: System.Runtime.InteropServices.RuntimeInformation => 0xd804d57a => 106
	i32 3626429363, ; 598: Xamarin.Google.MLKit.Common => 0xd826ebb3 => 297
	i32 3627220390, ; 599: Xamarin.AndroidX.Print.dll => 0xd832fda6 => 260
	i32 3633644679, ; 600: Xamarin.AndroidX.Annotation.Experimental => 0xd8950487 => 204
	i32 3638274909, ; 601: System.IO.FileSystem.Primitives.dll => 0xd8dbab5d => 49
	i32 3641597786, ; 602: Xamarin.AndroidX.Lifecycle.LiveData.Core => 0xd90e5f5a => 243
	i32 3643446276, ; 603: tr\Microsoft.Maui.Controls.resources => 0xd92a9404 => 342
	i32 3643854240, ; 604: Xamarin.AndroidX.Navigation.Fragment => 0xd930cda0 => 257
	i32 3645089577, ; 605: System.ComponentModel.DataAnnotations => 0xd943a729 => 14
	i32 3657292374, ; 606: Microsoft.Extensions.Configuration.Abstractions.dll => 0xd9fdda56 => 181
	i32 3660523487, ; 607: System.Net.NetworkInformation => 0xda2f27df => 68
	i32 3672681054, ; 608: Mono.Android.dll => 0xdae8aa5e => 171
	i32 3676461095, ; 609: Xamarin.AndroidX.Camera.Lifecycle => 0xdb225827 => 213
	i32 3682565725, ; 610: Xamarin.AndroidX.Browser => 0xdb7f7e5d => 210
	i32 3684561358, ; 611: Xamarin.AndroidX.Concurrent.Futures => 0xdb9df1ce => 220
	i32 3697841164, ; 612: zh-Hant/Microsoft.Maui.Controls.resources.dll => 0xdc68940c => 347
	i32 3700866549, ; 613: System.Net.WebProxy.dll => 0xdc96bdf5 => 78
	i32 3706696989, ; 614: Xamarin.AndroidX.Core.Core.Ktx.dll => 0xdcefb51d => 226
	i32 3716563718, ; 615: System.Runtime.Intrinsics => 0xdd864306 => 108
	i32 3718780102, ; 616: Xamarin.AndroidX.Annotation => 0xdda814c6 => 203
	i32 3724971120, ; 617: Xamarin.AndroidX.Navigation.Common.dll => 0xde068c70 => 256
	i32 3732100267, ; 618: System.Net.NameResolution => 0xde7354ab => 67
	i32 3737834244, ; 619: System.Net.Http.Json.dll => 0xdecad304 => 63
	i32 3748608112, ; 620: System.Diagnostics.DiagnosticSource => 0xdf6f3870 => 27
	i32 3751444290, ; 621: System.Xml.XPath => 0xdf9a7f42 => 160
	i32 3764085317, ; 622: Xamarin.AndroidX.Lifecycle.Runtime.Ktx.Android.dll => 0xe05b6245 => 249
	i32 3786282454, ; 623: Xamarin.AndroidX.Collection => 0xe1ae15d6 => 217
	i32 3792276235, ; 624: System.Collections.NonGeneric => 0xe2098b0b => 10
	i32 3800979733, ; 625: Microsoft.Maui.Controls.Compatibility => 0xe28e5915 => 189
	i32 3802395368, ; 626: System.Collections.Specialized.dll => 0xe2a3f2e8 => 11
	i32 3819260425, ; 627: System.Net.WebProxy => 0xe3a54a09 => 78
	i32 3823082795, ; 628: System.Security.Cryptography.dll => 0xe3df9d2b => 126
	i32 3829621856, ; 629: System.Numerics.dll => 0xe4436460 => 83
	i32 3841636137, ; 630: Microsoft.Extensions.DependencyInjection.Abstractions.dll => 0xe4fab729 => 183
	i32 3844307129, ; 631: System.Net.Mail.dll => 0xe52378b9 => 66
	i32 3849253459, ; 632: System.Runtime.InteropServices.dll => 0xe56ef253 => 107
	i32 3870376305, ; 633: System.Net.HttpListener.dll => 0xe6b14171 => 65
	i32 3873536506, ; 634: System.Security.Principal => 0xe6e179fa => 128
	i32 3875112723, ; 635: System.Security.Cryptography.Encoding.dll => 0xe6f98713 => 122
	i32 3885497537, ; 636: System.Net.WebHeaderCollection.dll => 0xe797fcc1 => 77
	i32 3885922214, ; 637: Xamarin.AndroidX.Transition.dll => 0xe79e77a6 => 272
	i32 3888767677, ; 638: Xamarin.AndroidX.ProfileInstaller.ProfileInstaller => 0xe7c9e2bd => 261
	i32 3889960447, ; 639: zh-Hans/Microsoft.Maui.Controls.resources.dll => 0xe7dc15ff => 346
	i32 3896106733, ; 640: System.Collections.Concurrent.dll => 0xe839deed => 8
	i32 3896760992, ; 641: Xamarin.AndroidX.Core.dll => 0xe843daa0 => 225
	i32 3901907137, ; 642: Microsoft.VisualBasic.Core.dll => 0xe89260c1 => 2
	i32 3910130544, ; 643: Xamarin.AndroidX.Collection.Jvm => 0xe90fdb70 => 218
	i32 3920810846, ; 644: System.IO.Compression.FileSystem.dll => 0xe9b2d35e => 44
	i32 3921031405, ; 645: Xamarin.AndroidX.VersionedParcelable.dll => 0xe9b630ed => 275
	i32 3928044579, ; 646: System.Xml.ReaderWriter => 0xea213423 => 156
	i32 3930554604, ; 647: System.Security.Principal.dll => 0xea4780ec => 128
	i32 3931092270, ; 648: Xamarin.AndroidX.Navigation.UI => 0xea4fb52e => 259
	i32 3934056515, ; 649: Xamarin.JavaX.Inject.dll => 0xea7cf043 => 304
	i32 3945713374, ; 650: System.Data.DataSetExtensions.dll => 0xeb2ecede => 23
	i32 3953953790, ; 651: System.Text.Encoding.CodePages => 0xebac8bfe => 133
	i32 3955647286, ; 652: Xamarin.AndroidX.AppCompat.dll => 0xebc66336 => 206
	i32 3956287295, ; 653: BarcodeScanning.Native.Maui => 0xebd0273f => 173
	i32 3959773229, ; 654: Xamarin.AndroidX.Lifecycle.Process => 0xec05582d => 245
	i32 3970018735, ; 655: Xamarin.GooglePlayServices.Tasks.dll => 0xeca1adaf => 303
	i32 3980434154, ; 656: th/Microsoft.Maui.Controls.resources.dll => 0xed409aea => 341
	i32 3987592930, ; 657: he/Microsoft.Maui.Controls.resources.dll => 0xedadd6e2 => 323
	i32 4003436829, ; 658: System.Diagnostics.Process.dll => 0xee9f991d => 29
	i32 4015948917, ; 659: Xamarin.AndroidX.Annotation.Jvm.dll => 0xef5e8475 => 205
	i32 4025784931, ; 660: System.Memory => 0xeff49a63 => 62
	i32 4026433800, ; 661: NekrasovskyAPP.dll => 0xeffe8108 => 0
	i32 4046471985, ; 662: Microsoft.Maui.Controls.Xaml.dll => 0xf1304331 => 191
	i32 4054681211, ; 663: System.Reflection.Emit.ILGeneration => 0xf1ad867b => 90
	i32 4068434129, ; 664: System.Private.Xml.Linq.dll => 0xf27f60d1 => 87
	i32 4073602200, ; 665: System.Threading.dll => 0xf2ce3c98 => 148
	i32 4094352644, ; 666: Microsoft.Maui.Essentials.dll => 0xf40add04 => 193
	i32 4099507663, ; 667: System.Drawing.dll => 0xf45985cf => 36
	i32 4100113165, ; 668: System.Private.Uri => 0xf462c30d => 86
	i32 4101236366, ; 669: Npgsql.EntityFrameworkCore.PostgreSQL => 0xf473e68e => 196
	i32 4101593132, ; 670: Xamarin.AndroidX.Emoji2 => 0xf479582c => 233
	i32 4101842092, ; 671: Microsoft.Extensions.Caching.Memory => 0xf47d24ac => 179
	i32 4102112229, ; 672: pt/Microsoft.Maui.Controls.resources.dll => 0xf48143e5 => 336
	i32 4125707920, ; 673: ms/Microsoft.Maui.Controls.resources.dll => 0xf5e94e90 => 331
	i32 4126470640, ; 674: Microsoft.Extensions.DependencyInjection => 0xf5f4f1f0 => 182
	i32 4127667938, ; 675: System.IO.FileSystem.Watcher => 0xf60736e2 => 50
	i32 4130442656, ; 676: System.AppContext => 0xf6318da0 => 6
	i32 4147896353, ; 677: System.Reflection.Emit.ILGeneration.dll => 0xf73be021 => 90
	i32 4150914736, ; 678: uk\Microsoft.Maui.Controls.resources => 0xf769eeb0 => 343
	i32 4151237749, ; 679: System.Core => 0xf76edc75 => 21
	i32 4159265925, ; 680: System.Xml.XmlSerializer => 0xf7e95c85 => 162
	i32 4161255271, ; 681: System.Reflection.TypeExtensions => 0xf807b767 => 96
	i32 4164802419, ; 682: System.IO.FileSystem.Watcher.dll => 0xf83dd773 => 50
	i32 4181436372, ; 683: System.Runtime.Serialization.Primitives => 0xf93ba7d4 => 113
	i32 4182413190, ; 684: Xamarin.AndroidX.Lifecycle.ViewModelSavedState.dll => 0xf94a8f86 => 253
	i32 4185676441, ; 685: System.Security => 0xf97c5a99 => 130
	i32 4192648326, ; 686: Xamarin.Firebase.Encoders.JSON.dll => 0xf9e6bc86 => 283
	i32 4196529839, ; 687: System.Net.WebClient.dll => 0xfa21f6af => 76
	i32 4213026141, ; 688: System.Diagnostics.DiagnosticSource.dll => 0xfb1dad5d => 27
	i32 4228543782, ; 689: Xamarin.KotlinX.AtomicFU.Jvm.dll => 0xfc0a7526 => 310
	i32 4256097574, ; 690: Xamarin.AndroidX.Core.Core.Ktx => 0xfdaee526 => 226
	i32 4258378803, ; 691: Xamarin.AndroidX.Lifecycle.ViewModel.Ktx => 0xfdd1b433 => 252
	i32 4260525087, ; 692: System.Buffers => 0xfdf2741f => 7
	i32 4271975918, ; 693: Microsoft.Maui.Controls.dll => 0xfea12dee => 190
	i32 4274976490, ; 694: System.Runtime.Numerics => 0xfecef6ea => 110
	i32 4284549794, ; 695: Xamarin.Firebase.Components => 0xff610aa2 => 281
	i32 4292120959, ; 696: Xamarin.AndroidX.Lifecycle.ViewModelSavedState => 0xffd4917f => 253
	i32 4294763496 ; 697: Xamarin.AndroidX.ExifInterface.dll => 0xfffce3e8 => 235
], align 4

@assembly_image_cache_indices = dso_local local_unnamed_addr constant [698 x i32] [
	i32 68, ; 0
	i32 67, ; 1
	i32 108, ; 2
	i32 299, ; 3
	i32 195, ; 4
	i32 289, ; 5
	i32 246, ; 6
	i32 294, ; 7
	i32 48, ; 8
	i32 80, ; 9
	i32 213, ; 10
	i32 145, ; 11
	i32 310, ; 12
	i32 221, ; 13
	i32 30, ; 14
	i32 347, ; 15
	i32 124, ; 16
	i32 194, ; 17
	i32 102, ; 18
	i32 267, ; 19
	i32 280, ; 20
	i32 107, ; 21
	i32 267, ; 22
	i32 139, ; 23
	i32 307, ; 24
	i32 77, ; 25
	i32 124, ; 26
	i32 13, ; 27
	i32 217, ; 28
	i32 309, ; 29
	i32 132, ; 30
	i32 269, ; 31
	i32 151, ; 32
	i32 344, ; 33
	i32 345, ; 34
	i32 18, ; 35
	i32 210, ; 36
	i32 271, ; 37
	i32 26, ; 38
	i32 239, ; 39
	i32 1, ; 40
	i32 59, ; 41
	i32 42, ; 42
	i32 91, ; 43
	i32 222, ; 44
	i32 147, ; 45
	i32 242, ; 46
	i32 238, ; 47
	i32 316, ; 48
	i32 54, ; 49
	i32 69, ; 50
	i32 344, ; 51
	i32 201, ; 52
	i32 83, ; 53
	i32 329, ; 54
	i32 240, ; 55
	i32 328, ; 56
	i32 131, ; 57
	i32 55, ; 58
	i32 149, ; 59
	i32 74, ; 60
	i32 145, ; 61
	i32 62, ; 62
	i32 146, ; 63
	i32 348, ; 64
	i32 165, ; 65
	i32 247, ; 66
	i32 340, ; 67
	i32 223, ; 68
	i32 12, ; 69
	i32 236, ; 70
	i32 125, ; 71
	i32 152, ; 72
	i32 113, ; 73
	i32 166, ; 74
	i32 164, ; 75
	i32 238, ; 76
	i32 287, ; 77
	i32 255, ; 78
	i32 287, ; 79
	i32 84, ; 80
	i32 327, ; 81
	i32 321, ; 82
	i32 285, ; 83
	i32 188, ; 84
	i32 298, ; 85
	i32 150, ; 86
	i32 307, ; 87
	i32 60, ; 88
	i32 184, ; 89
	i32 51, ; 90
	i32 103, ; 91
	i32 114, ; 92
	i32 40, ; 93
	i32 291, ; 94
	i32 279, ; 95
	i32 120, ; 96
	i32 335, ; 97
	i32 52, ; 98
	i32 44, ; 99
	i32 119, ; 100
	i32 247, ; 101
	i32 228, ; 102
	i32 333, ; 103
	i32 234, ; 104
	i32 81, ; 105
	i32 136, ; 106
	i32 275, ; 107
	i32 208, ; 108
	i32 8, ; 109
	i32 309, ; 110
	i32 73, ; 111
	i32 315, ; 112
	i32 155, ; 113
	i32 311, ; 114
	i32 154, ; 115
	i32 92, ; 116
	i32 305, ; 117
	i32 45, ; 118
	i32 330, ; 119
	i32 318, ; 120
	i32 308, ; 121
	i32 109, ; 122
	i32 129, ; 123
	i32 25, ; 124
	i32 198, ; 125
	i32 72, ; 126
	i32 55, ; 127
	i32 46, ; 128
	i32 339, ; 129
	i32 290, ; 130
	i32 187, ; 131
	i32 229, ; 132
	i32 22, ; 133
	i32 244, ; 134
	i32 86, ; 135
	i32 43, ; 136
	i32 160, ; 137
	i32 71, ; 138
	i32 260, ; 139
	i32 3, ; 140
	i32 42, ; 141
	i32 63, ; 142
	i32 16, ; 143
	i32 53, ; 144
	i32 342, ; 145
	i32 294, ; 146
	i32 105, ; 147
	i32 308, ; 148
	i32 292, ; 149
	i32 240, ; 150
	i32 34, ; 151
	i32 158, ; 152
	i32 85, ; 153
	i32 32, ; 154
	i32 12, ; 155
	i32 51, ; 156
	i32 286, ; 157
	i32 56, ; 158
	i32 264, ; 159
	i32 36, ; 160
	i32 183, ; 161
	i32 317, ; 162
	i32 293, ; 163
	i32 206, ; 164
	i32 35, ; 165
	i32 58, ; 166
	i32 250, ; 167
	i32 290, ; 168
	i32 174, ; 169
	i32 17, ; 170
	i32 306, ; 171
	i32 164, ; 172
	i32 330, ; 173
	i32 248, ; 174
	i32 289, ; 175
	i32 186, ; 176
	i32 278, ; 177
	i32 176, ; 178
	i32 336, ; 179
	i32 153, ; 180
	i32 274, ; 181
	i32 258, ; 182
	i32 176, ; 183
	i32 334, ; 184
	i32 297, ; 185
	i32 208, ; 186
	i32 179, ; 187
	i32 29, ; 188
	i32 52, ; 189
	i32 332, ; 190
	i32 279, ; 191
	i32 218, ; 192
	i32 5, ; 193
	i32 316, ; 194
	i32 268, ; 195
	i32 312, ; 196
	i32 273, ; 197
	i32 219, ; 198
	i32 311, ; 199
	i32 205, ; 200
	i32 231, ; 201
	i32 85, ; 202
	i32 278, ; 203
	i32 61, ; 204
	i32 112, ; 205
	i32 302, ; 206
	i32 296, ; 207
	i32 295, ; 208
	i32 57, ; 209
	i32 346, ; 210
	i32 264, ; 211
	i32 99, ; 212
	i32 304, ; 213
	i32 19, ; 214
	i32 224, ; 215
	i32 111, ; 216
	i32 101, ; 217
	i32 102, ; 218
	i32 314, ; 219
	i32 104, ; 220
	i32 292, ; 221
	i32 241, ; 222
	i32 71, ; 223
	i32 251, ; 224
	i32 38, ; 225
	i32 32, ; 226
	i32 103, ; 227
	i32 73, ; 228
	i32 320, ; 229
	i32 9, ; 230
	i32 123, ; 231
	i32 46, ; 232
	i32 207, ; 233
	i32 188, ; 234
	i32 9, ; 235
	i32 43, ; 236
	i32 4, ; 237
	i32 265, ; 238
	i32 324, ; 239
	i32 319, ; 240
	i32 31, ; 241
	i32 138, ; 242
	i32 92, ; 243
	i32 93, ; 244
	i32 339, ; 245
	i32 49, ; 246
	i32 141, ; 247
	i32 112, ; 248
	i32 140, ; 249
	i32 230, ; 250
	i32 115, ; 251
	i32 293, ; 252
	i32 157, ; 253
	i32 76, ; 254
	i32 79, ; 255
	i32 254, ; 256
	i32 37, ; 257
	i32 277, ; 258
	i32 234, ; 259
	i32 227, ; 260
	i32 64, ; 261
	i32 138, ; 262
	i32 15, ; 263
	i32 116, ; 264
	i32 270, ; 265
	i32 288, ; 266
	i32 222, ; 267
	i32 48, ; 268
	i32 70, ; 269
	i32 80, ; 270
	i32 126, ; 271
	i32 175, ; 272
	i32 94, ; 273
	i32 121, ; 274
	i32 26, ; 275
	i32 302, ; 276
	i32 244, ; 277
	i32 97, ; 278
	i32 28, ; 279
	i32 216, ; 280
	i32 337, ; 281
	i32 315, ; 282
	i32 149, ; 283
	i32 169, ; 284
	i32 4, ; 285
	i32 98, ; 286
	i32 33, ; 287
	i32 93, ; 288
	i32 269, ; 289
	i32 184, ; 290
	i32 0, ; 291
	i32 21, ; 292
	i32 41, ; 293
	i32 170, ; 294
	i32 331, ; 295
	i32 236, ; 296
	i32 323, ; 297
	i32 254, ; 298
	i32 306, ; 299
	i32 288, ; 300
	i32 259, ; 301
	i32 2, ; 302
	i32 134, ; 303
	i32 111, ; 304
	i32 185, ; 305
	i32 343, ; 306
	i32 198, ; 307
	i32 340, ; 308
	i32 58, ; 309
	i32 214, ; 310
	i32 95, ; 311
	i32 322, ; 312
	i32 284, ; 313
	i32 39, ; 314
	i32 209, ; 315
	i32 25, ; 316
	i32 94, ; 317
	i32 89, ; 318
	i32 99, ; 319
	i32 301, ; 320
	i32 10, ; 321
	i32 87, ; 322
	i32 100, ; 323
	i32 266, ; 324
	i32 180, ; 325
	i32 200, ; 326
	i32 319, ; 327
	i32 7, ; 328
	i32 250, ; 329
	i32 314, ; 330
	i32 197, ; 331
	i32 88, ; 332
	i32 243, ; 333
	i32 154, ; 334
	i32 318, ; 335
	i32 33, ; 336
	i32 116, ; 337
	i32 82, ; 338
	i32 173, ; 339
	i32 286, ; 340
	i32 20, ; 341
	i32 300, ; 342
	i32 11, ; 343
	i32 162, ; 344
	i32 3, ; 345
	i32 192, ; 346
	i32 326, ; 347
	i32 280, ; 348
	i32 187, ; 349
	i32 295, ; 350
	i32 185, ; 351
	i32 84, ; 352
	i32 313, ; 353
	i32 64, ; 354
	i32 328, ; 355
	i32 274, ; 356
	i32 143, ; 357
	i32 255, ; 358
	i32 157, ; 359
	i32 175, ; 360
	i32 41, ; 361
	i32 117, ; 362
	i32 181, ; 363
	i32 199, ; 364
	i32 322, ; 365
	i32 262, ; 366
	i32 131, ; 367
	i32 195, ; 368
	i32 75, ; 369
	i32 66, ; 370
	i32 332, ; 371
	i32 172, ; 372
	i32 203, ; 373
	i32 143, ; 374
	i32 196, ; 375
	i32 106, ; 376
	i32 151, ; 377
	i32 70, ; 378
	i32 156, ; 379
	i32 180, ; 380
	i32 121, ; 381
	i32 127, ; 382
	i32 327, ; 383
	i32 152, ; 384
	i32 233, ; 385
	i32 214, ; 386
	i32 141, ; 387
	i32 219, ; 388
	i32 296, ; 389
	i32 324, ; 390
	i32 20, ; 391
	i32 14, ; 392
	i32 135, ; 393
	i32 75, ; 394
	i32 59, ; 395
	i32 223, ; 396
	i32 167, ; 397
	i32 168, ; 398
	i32 190, ; 399
	i32 15, ; 400
	i32 74, ; 401
	i32 6, ; 402
	i32 23, ; 403
	i32 246, ; 404
	i32 197, ; 405
	i32 91, ; 406
	i32 325, ; 407
	i32 1, ; 408
	i32 136, ; 409
	i32 249, ; 410
	i32 248, ; 411
	i32 273, ; 412
	i32 134, ; 413
	i32 69, ; 414
	i32 146, ; 415
	i32 334, ; 416
	i32 313, ; 417
	i32 237, ; 418
	i32 186, ; 419
	i32 88, ; 420
	i32 96, ; 421
	i32 282, ; 422
	i32 227, ; 423
	i32 232, ; 424
	i32 329, ; 425
	i32 31, ; 426
	i32 45, ; 427
	i32 242, ; 428
	i32 177, ; 429
	i32 282, ; 430
	i32 199, ; 431
	i32 109, ; 432
	i32 158, ; 433
	i32 35, ; 434
	i32 312, ; 435
	i32 22, ; 436
	i32 114, ; 437
	i32 57, ; 438
	i32 270, ; 439
	i32 144, ; 440
	i32 118, ; 441
	i32 120, ; 442
	i32 110, ; 443
	i32 201, ; 444
	i32 139, ; 445
	i32 207, ; 446
	i32 54, ; 447
	i32 105, ; 448
	i32 335, ; 449
	i32 191, ; 450
	i32 192, ; 451
	i32 133, ; 452
	i32 251, ; 453
	i32 305, ; 454
	i32 276, ; 455
	i32 263, ; 456
	i32 241, ; 457
	i32 341, ; 458
	i32 237, ; 459
	i32 194, ; 460
	i32 159, ; 461
	i32 281, ; 462
	i32 320, ; 463
	i32 224, ; 464
	i32 163, ; 465
	i32 132, ; 466
	i32 263, ; 467
	i32 161, ; 468
	i32 221, ; 469
	i32 333, ; 470
	i32 252, ; 471
	i32 300, ; 472
	i32 177, ; 473
	i32 140, ; 474
	i32 276, ; 475
	i32 272, ; 476
	i32 169, ; 477
	i32 193, ; 478
	i32 298, ; 479
	i32 202, ; 480
	i32 291, ; 481
	i32 40, ; 482
	i32 235, ; 483
	i32 81, ; 484
	i32 56, ; 485
	i32 37, ; 486
	i32 97, ; 487
	i32 166, ; 488
	i32 172, ; 489
	i32 277, ; 490
	i32 82, ; 491
	i32 204, ; 492
	i32 98, ; 493
	i32 30, ; 494
	i32 159, ; 495
	i32 18, ; 496
	i32 215, ; 497
	i32 127, ; 498
	i32 119, ; 499
	i32 231, ; 500
	i32 266, ; 501
	i32 212, ; 502
	i32 245, ; 503
	i32 215, ; 504
	i32 268, ; 505
	i32 165, ; 506
	i32 239, ; 507
	i32 348, ; 508
	i32 212, ; 509
	i32 265, ; 510
	i32 256, ; 511
	i32 303, ; 512
	i32 170, ; 513
	i32 16, ; 514
	i32 178, ; 515
	i32 144, ; 516
	i32 326, ; 517
	i32 125, ; 518
	i32 118, ; 519
	i32 38, ; 520
	i32 115, ; 521
	i32 47, ; 522
	i32 142, ; 523
	i32 117, ; 524
	i32 34, ; 525
	i32 174, ; 526
	i32 285, ; 527
	i32 95, ; 528
	i32 53, ; 529
	i32 257, ; 530
	i32 129, ; 531
	i32 153, ; 532
	i32 178, ; 533
	i32 24, ; 534
	i32 161, ; 535
	i32 230, ; 536
	i32 148, ; 537
	i32 104, ; 538
	i32 301, ; 539
	i32 89, ; 540
	i32 216, ; 541
	i32 60, ; 542
	i32 142, ; 543
	i32 100, ; 544
	i32 5, ; 545
	i32 13, ; 546
	i32 122, ; 547
	i32 135, ; 548
	i32 28, ; 549
	i32 321, ; 550
	i32 72, ; 551
	i32 228, ; 552
	i32 24, ; 553
	i32 209, ; 554
	i32 261, ; 555
	i32 258, ; 556
	i32 338, ; 557
	i32 137, ; 558
	i32 271, ; 559
	i32 202, ; 560
	i32 225, ; 561
	i32 168, ; 562
	i32 284, ; 563
	i32 262, ; 564
	i32 317, ; 565
	i32 283, ; 566
	i32 101, ; 567
	i32 123, ; 568
	i32 229, ; 569
	i32 299, ; 570
	i32 211, ; 571
	i32 211, ; 572
	i32 182, ; 573
	i32 163, ; 574
	i32 167, ; 575
	i32 232, ; 576
	i32 39, ; 577
	i32 189, ; 578
	i32 325, ; 579
	i32 17, ; 580
	i32 171, ; 581
	i32 338, ; 582
	i32 337, ; 583
	i32 137, ; 584
	i32 150, ; 585
	i32 220, ; 586
	i32 155, ; 587
	i32 130, ; 588
	i32 19, ; 589
	i32 65, ; 590
	i32 147, ; 591
	i32 47, ; 592
	i32 345, ; 593
	i32 200, ; 594
	i32 79, ; 595
	i32 61, ; 596
	i32 106, ; 597
	i32 297, ; 598
	i32 260, ; 599
	i32 204, ; 600
	i32 49, ; 601
	i32 243, ; 602
	i32 342, ; 603
	i32 257, ; 604
	i32 14, ; 605
	i32 181, ; 606
	i32 68, ; 607
	i32 171, ; 608
	i32 213, ; 609
	i32 210, ; 610
	i32 220, ; 611
	i32 347, ; 612
	i32 78, ; 613
	i32 226, ; 614
	i32 108, ; 615
	i32 203, ; 616
	i32 256, ; 617
	i32 67, ; 618
	i32 63, ; 619
	i32 27, ; 620
	i32 160, ; 621
	i32 249, ; 622
	i32 217, ; 623
	i32 10, ; 624
	i32 189, ; 625
	i32 11, ; 626
	i32 78, ; 627
	i32 126, ; 628
	i32 83, ; 629
	i32 183, ; 630
	i32 66, ; 631
	i32 107, ; 632
	i32 65, ; 633
	i32 128, ; 634
	i32 122, ; 635
	i32 77, ; 636
	i32 272, ; 637
	i32 261, ; 638
	i32 346, ; 639
	i32 8, ; 640
	i32 225, ; 641
	i32 2, ; 642
	i32 218, ; 643
	i32 44, ; 644
	i32 275, ; 645
	i32 156, ; 646
	i32 128, ; 647
	i32 259, ; 648
	i32 304, ; 649
	i32 23, ; 650
	i32 133, ; 651
	i32 206, ; 652
	i32 173, ; 653
	i32 245, ; 654
	i32 303, ; 655
	i32 341, ; 656
	i32 323, ; 657
	i32 29, ; 658
	i32 205, ; 659
	i32 62, ; 660
	i32 0, ; 661
	i32 191, ; 662
	i32 90, ; 663
	i32 87, ; 664
	i32 148, ; 665
	i32 193, ; 666
	i32 36, ; 667
	i32 86, ; 668
	i32 196, ; 669
	i32 233, ; 670
	i32 179, ; 671
	i32 336, ; 672
	i32 331, ; 673
	i32 182, ; 674
	i32 50, ; 675
	i32 6, ; 676
	i32 90, ; 677
	i32 343, ; 678
	i32 21, ; 679
	i32 162, ; 680
	i32 96, ; 681
	i32 50, ; 682
	i32 113, ; 683
	i32 253, ; 684
	i32 130, ; 685
	i32 283, ; 686
	i32 76, ; 687
	i32 27, ; 688
	i32 310, ; 689
	i32 226, ; 690
	i32 252, ; 691
	i32 7, ; 692
	i32 190, ; 693
	i32 110, ; 694
	i32 281, ; 695
	i32 253, ; 696
	i32 235 ; 697
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

; Function attributes: "min-legal-vector-width"="0" mustprogress nofree norecurse nosync "no-trapping-math"="true" nounwind "stack-protector-buffer-size"="8" uwtable willreturn
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

; Function attributes: noreturn "no-trapping-math"="true" nounwind "stack-protector-buffer-size"="8"
declare void @abort() local_unnamed_addr #2

; Function attributes: nofree nounwind
declare noundef i32 @puts(ptr noundef) local_unnamed_addr #1
attributes #0 = { "min-legal-vector-width"="0" mustprogress nofree norecurse nosync "no-trapping-math"="true" nounwind "stack-protector-buffer-size"="8" "stackrealign" "target-cpu"="i686" "target-features"="+cx8,+mmx,+sse,+sse2,+sse3,+ssse3,+x87" "tune-cpu"="generic" uwtable willreturn }
attributes #1 = { nofree nounwind }
attributes #2 = { noreturn "no-trapping-math"="true" nounwind "stack-protector-buffer-size"="8" "stackrealign" "target-cpu"="i686" "target-features"="+cx8,+mmx,+sse,+sse2,+sse3,+ssse3,+x87" "tune-cpu"="generic" }

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

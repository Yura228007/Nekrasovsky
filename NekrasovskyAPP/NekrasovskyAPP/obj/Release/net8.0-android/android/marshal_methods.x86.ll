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

@assembly_image_cache = dso_local local_unnamed_addr global [358 x ptr] zeroinitializer, align 4

; Each entry maps hash of an assembly name to an index into the `assembly_image_cache` array
@assembly_image_cache_hashes = dso_local local_unnamed_addr constant [716 x i32] [
	i32 2616222, ; 0: System.Net.NetworkInformation.dll => 0x27eb9e => 68
	i32 10166715, ; 1: System.Net.NameResolution.dll => 0x9b21bb => 67
	i32 15721112, ; 2: System.Runtime.Intrinsics.dll => 0xefe298 => 108
	i32 20206211, ; 3: Xamarin.Google.MLKit.Vision.Interfaces.dll => 0x1345283 => 308
	i32 28873261, ; 4: Npgsql.dll => 0x1b8922d => 203
	i32 30793855, ; 5: Xamarin.Google.Android.ODML.Image.dll => 0x1d5e07f => 298
	i32 32687329, ; 6: Xamarin.AndroidX.Lifecycle.Runtime => 0x1f2c4e1 => 255
	i32 34715100, ; 7: Xamarin.Google.Guava.ListenableFuture.dll => 0x211b5dc => 303
	i32 34839235, ; 8: System.IO.FileSystem.DriveInfo => 0x2139ac3 => 48
	i32 39485524, ; 9: System.Net.WebSockets.dll => 0x25a8054 => 80
	i32 40744412, ; 10: Xamarin.AndroidX.Camera.Lifecycle.dll => 0x26db5dc => 222
	i32 42639949, ; 11: System.Threading.Thread => 0x28aa24d => 145
	i32 45981941, ; 12: Xamarin.KotlinX.AtomicFU.Jvm => 0x2bda0f5 => 319
	i32 52581868, ; 13: Xamarin.AndroidX.Concurrent.Futures.Ktx => 0x32255ec => 230
	i32 66541672, ; 14: System.Diagnostics.StackTrace => 0x3f75868 => 30
	i32 67008169, ; 15: zh-Hant\Microsoft.Maui.Controls.resources => 0x3fe76a9 => 356
	i32 68219467, ; 16: System.Security.Cryptography.Primitives => 0x410f24b => 124
	i32 72070932, ; 17: Microsoft.Maui.Graphics.dll => 0x44bb714 => 202
	i32 82292897, ; 18: System.Runtime.CompilerServices.VisualC.dll => 0x4e7b0a1 => 102
	i32 101534019, ; 19: Xamarin.AndroidX.SlidingPaneLayout => 0x60d4943 => 276
	i32 103834273, ; 20: Xamarin.Firebase.Annotations.dll => 0x63062a1 => 289
	i32 117431740, ; 21: System.Runtime.InteropServices => 0x6ffddbc => 107
	i32 120558881, ; 22: Xamarin.AndroidX.SlidingPaneLayout.dll => 0x72f9521 => 276
	i32 122350210, ; 23: System.Threading.Channels.dll => 0x74aea82 => 139
	i32 134690465, ; 24: Xamarin.Kotlin.StdLib.Jdk7.dll => 0x80736a1 => 316
	i32 142721839, ; 25: System.Net.WebHeaderCollection => 0x881c32f => 77
	i32 149972175, ; 26: System.Security.Cryptography.Primitives.dll => 0x8f064cf => 124
	i32 159306688, ; 27: System.ComponentModel.Annotations => 0x97ed3c0 => 13
	i32 165246403, ; 28: Xamarin.AndroidX.Collection.dll => 0x9d975c3 => 226
	i32 166070894, ; 29: Xamarin.KotlinX.AtomicFU.dll => 0x9e60a6e => 318
	i32 176265551, ; 30: System.ServiceProcess => 0xa81994f => 132
	i32 182336117, ; 31: Xamarin.AndroidX.SwipeRefreshLayout.dll => 0xade3a75 => 278
	i32 184328833, ; 32: System.ValueTuple.dll => 0xafca281 => 151
	i32 195452805, ; 33: vi/Microsoft.Maui.Controls.resources.dll => 0xba65f85 => 353
	i32 199333315, ; 34: zh-HK/Microsoft.Maui.Controls.resources.dll => 0xbe195c3 => 354
	i32 205061960, ; 35: System.ComponentModel => 0xc38ff48 => 18
	i32 209399409, ; 36: Xamarin.AndroidX.Browser.dll => 0xc7b2e71 => 219
	i32 218154787, ; 37: Xamarin.AndroidX.Tracing.Tracing.Ktx => 0xd00c723 => 280
	i32 220171995, ; 38: System.Diagnostics.Debug => 0xd1f8edb => 26
	i32 221063263, ; 39: Microsoft.AspNetCore.Http.Connections.Client => 0xd2d285f => 176
	i32 230216969, ; 40: Xamarin.AndroidX.Legacy.Support.Core.Utils.dll => 0xdb8d509 => 248
	i32 230752869, ; 41: Microsoft.CSharp.dll => 0xdc10265 => 1
	i32 231409092, ; 42: System.Linq.Parallel => 0xdcb05c4 => 59
	i32 231814094, ; 43: System.Globalization => 0xdd133ce => 42
	i32 246610117, ; 44: System.Reflection.Emit.Lightweight => 0xeb2f8c5 => 91
	i32 261689757, ; 45: Xamarin.AndroidX.ConstraintLayout.dll => 0xf99119d => 231
	i32 276479776, ; 46: System.Threading.Timer.dll => 0x107abf20 => 147
	i32 278686392, ; 47: Xamarin.AndroidX.Lifecycle.LiveData.dll => 0x109c6ab8 => 251
	i32 280482487, ; 48: Xamarin.AndroidX.Interpolator => 0x10b7d2b7 => 247
	i32 280992041, ; 49: cs/Microsoft.Maui.Controls.resources.dll => 0x10bf9929 => 325
	i32 291076382, ; 50: System.IO.Pipes.AccessControl.dll => 0x1159791e => 54
	i32 298918909, ; 51: System.Net.Ping.dll => 0x11d123fd => 69
	i32 317674968, ; 52: vi\Microsoft.Maui.Controls.resources => 0x12ef55d8 => 353
	i32 318968648, ; 53: Xamarin.AndroidX.Activity.dll => 0x13031348 => 210
	i32 321597661, ; 54: System.Numerics => 0x132b30dd => 83
	i32 336156722, ; 55: ja/Microsoft.Maui.Controls.resources.dll => 0x14095832 => 338
	i32 342366114, ; 56: Xamarin.AndroidX.Lifecycle.Common => 0x146817a2 => 249
	i32 348048101, ; 57: Microsoft.AspNetCore.Http.Connections.Common.dll => 0x14becae5 => 177
	i32 356389973, ; 58: it/Microsoft.Maui.Controls.resources.dll => 0x153e1455 => 337
	i32 360082299, ; 59: System.ServiceModel.Web => 0x15766b7b => 131
	i32 367780167, ; 60: System.IO.Pipes => 0x15ebe147 => 55
	i32 374914964, ; 61: System.Transactions.Local => 0x1658bf94 => 149
	i32 375677976, ; 62: System.Net.ServicePoint.dll => 0x16646418 => 74
	i32 379916513, ; 63: System.Threading.Thread.dll => 0x16a510e1 => 145
	i32 385762202, ; 64: System.Memory.dll => 0x16fe439a => 62
	i32 392610295, ; 65: System.Threading.ThreadPool.dll => 0x1766c1f7 => 146
	i32 395744057, ; 66: _Microsoft.Android.Resource.Designer => 0x17969339 => 357
	i32 403441872, ; 67: WindowsBase => 0x180c08d0 => 165
	i32 425531652, ; 68: Xamarin.AndroidX.Lifecycle.Runtime.Android => 0x195d1904 => 256
	i32 435591531, ; 69: sv/Microsoft.Maui.Controls.resources.dll => 0x19f6996b => 349
	i32 441335492, ; 70: Xamarin.AndroidX.ConstraintLayout.Core => 0x1a4e3ec4 => 232
	i32 442565967, ; 71: System.Collections => 0x1a61054f => 12
	i32 450948140, ; 72: Xamarin.AndroidX.Fragment.dll => 0x1ae0ec2c => 245
	i32 451504562, ; 73: System.Security.Cryptography.X509Certificates => 0x1ae969b2 => 125
	i32 456227837, ; 74: System.Web.HttpUtility.dll => 0x1b317bfd => 152
	i32 458494020, ; 75: Microsoft.AspNetCore.SignalR.Common.dll => 0x1b541044 => 180
	i32 459347974, ; 76: System.Runtime.Serialization.Primitives.dll => 0x1b611806 => 113
	i32 465846621, ; 77: mscorlib => 0x1bc4415d => 166
	i32 469710990, ; 78: System.dll => 0x1bff388e => 164
	i32 476646585, ; 79: Xamarin.AndroidX.Interpolator.dll => 0x1c690cb9 => 247
	i32 485140951, ; 80: Xamarin.Google.Android.DataTransport.TransportRuntime => 0x1ceaa9d7 => 296
	i32 486930444, ; 81: Xamarin.AndroidX.LocalBroadcastManager.dll => 0x1d05f80c => 264
	i32 495452658, ; 82: Xamarin.Google.Android.DataTransport.TransportRuntime.dll => 0x1d8801f2 => 296
	i32 498788369, ; 83: System.ObjectModel => 0x1dbae811 => 84
	i32 500358224, ; 84: id/Microsoft.Maui.Controls.resources.dll => 0x1dd2dc50 => 336
	i32 503918385, ; 85: fi/Microsoft.Maui.Controls.resources.dll => 0x1e092f31 => 330
	i32 507148113, ; 86: Xamarin.Google.Android.DataTransport.TransportApi.dll => 0x1e3a7751 => 294
	i32 513247710, ; 87: Microsoft.Extensions.Primitives.dll => 0x1e9789de => 196
	i32 513617146, ; 88: Xamarin.Google.MLKit.Vision.Common => 0x1e9d2cfa => 307
	i32 526420162, ; 89: System.Transactions.dll => 0x1f6088c2 => 150
	i32 527452488, ; 90: Xamarin.Kotlin.StdLib.Jdk7 => 0x1f704948 => 316
	i32 530272170, ; 91: System.Linq.Queryable => 0x1f9b4faa => 60
	i32 539058512, ; 92: Microsoft.Extensions.Logging => 0x20216150 => 192
	i32 540030774, ; 93: System.IO.FileSystem.dll => 0x20303736 => 51
	i32 545304856, ; 94: System.Runtime.Extensions => 0x2080b118 => 103
	i32 546455878, ; 95: System.Runtime.Serialization.Xml => 0x20924146 => 114
	i32 549171840, ; 96: System.Globalization.Calendars => 0x20bbb280 => 40
	i32 557405415, ; 97: Jsr305Binding => 0x213954e7 => 300
	i32 569601784, ; 98: Xamarin.AndroidX.Window.Extensions.Core.Core => 0x21f36ef8 => 288
	i32 577335427, ; 99: System.Security.Cryptography.Cng => 0x22697083 => 120
	i32 592146354, ; 100: pt-BR/Microsoft.Maui.Controls.resources.dll => 0x234b6fb2 => 344
	i32 601371474, ; 101: System.IO.IsolatedStorage.dll => 0x23d83352 => 52
	i32 605376203, ; 102: System.IO.Compression.FileSystem => 0x24154ecb => 44
	i32 613668793, ; 103: System.Security.Cryptography.Algorithms => 0x2493d7b9 => 119
	i32 621990341, ; 104: Xamarin.AndroidX.Lifecycle.Runtime.Android.dll => 0x2512d1c5 => 256
	i32 627609679, ; 105: Xamarin.AndroidX.CustomView => 0x2568904f => 237
	i32 627931235, ; 106: nl\Microsoft.Maui.Controls.resources => 0x256d7863 => 342
	i32 639843206, ; 107: Xamarin.AndroidX.Emoji2.ViewsHelper.dll => 0x26233b86 => 243
	i32 643868501, ; 108: System.Net => 0x2660a755 => 81
	i32 662205335, ; 109: System.Text.Encodings.Web.dll => 0x27787397 => 136
	i32 663517072, ; 110: Xamarin.AndroidX.VersionedParcelable => 0x278c7790 => 284
	i32 666292255, ; 111: Xamarin.AndroidX.Arch.Core.Common.dll => 0x27b6d01f => 217
	i32 672442732, ; 112: System.Collections.Concurrent => 0x2814a96c => 8
	i32 679221896, ; 113: Xamarin.KotlinX.AtomicFU => 0x287c1a88 => 318
	i32 683518922, ; 114: System.Net.Security => 0x28bdabca => 73
	i32 688181140, ; 115: ca/Microsoft.Maui.Controls.resources.dll => 0x2904cf94 => 324
	i32 690569205, ; 116: System.Xml.Linq.dll => 0x29293ff5 => 155
	i32 691348768, ; 117: Xamarin.KotlinX.Coroutines.Android.dll => 0x29352520 => 320
	i32 693804605, ; 118: System.Windows => 0x295a9e3d => 154
	i32 699345723, ; 119: System.Reflection.Emit => 0x29af2b3b => 92
	i32 700284507, ; 120: Xamarin.Jetbrains.Annotations => 0x29bd7e5b => 314
	i32 700358131, ; 121: System.IO.Compression.ZipFile => 0x29be9df3 => 45
	i32 706645707, ; 122: ko/Microsoft.Maui.Controls.resources.dll => 0x2a1e8ecb => 339
	i32 709557578, ; 123: de/Microsoft.Maui.Controls.resources.dll => 0x2a4afd4a => 327
	i32 720511267, ; 124: Xamarin.Kotlin.StdLib.Jdk8 => 0x2af22123 => 317
	i32 722857257, ; 125: System.Runtime.Loader.dll => 0x2b15ed29 => 109
	i32 735137430, ; 126: System.Security.SecureString.dll => 0x2bd14e96 => 129
	i32 752232764, ; 127: System.Diagnostics.Contracts.dll => 0x2cd6293c => 25
	i32 755313932, ; 128: Xamarin.Android.Glide.Annotations.dll => 0x2d052d0c => 207
	i32 759454413, ; 129: System.Net.Requests => 0x2d445acd => 72
	i32 762598435, ; 130: System.IO.Pipes.dll => 0x2d745423 => 55
	i32 775507847, ; 131: System.IO.Compression => 0x2e394f87 => 46
	i32 777317022, ; 132: sk\Microsoft.Maui.Controls.resources => 0x2e54ea9e => 348
	i32 782533833, ; 133: Xamarin.Google.AutoValue.Annotations.dll => 0x2ea484c9 => 299
	i32 789151979, ; 134: Microsoft.Extensions.Options => 0x2f0980eb => 195
	i32 790371945, ; 135: Xamarin.AndroidX.CustomView.PoolingContainer.dll => 0x2f1c1e69 => 238
	i32 804715423, ; 136: System.Data.Common => 0x2ff6fb9f => 22
	i32 807930345, ; 137: Xamarin.AndroidX.Lifecycle.LiveData.Core.Ktx.dll => 0x302809e9 => 253
	i32 823281589, ; 138: System.Private.Uri.dll => 0x311247b5 => 86
	i32 830298997, ; 139: System.IO.Compression.Brotli => 0x317d5b75 => 43
	i32 832635846, ; 140: System.Xml.XPath.dll => 0x31a103c6 => 160
	i32 832711436, ; 141: Microsoft.AspNetCore.SignalR.Protocols.Json.dll => 0x31a22b0c => 181
	i32 834051424, ; 142: System.Net.Quic => 0x31b69d60 => 71
	i32 843511501, ; 143: Xamarin.AndroidX.Print => 0x3246f6cd => 269
	i32 873119928, ; 144: Microsoft.VisualBasic => 0x340ac0b8 => 3
	i32 877678880, ; 145: System.Globalization.dll => 0x34505120 => 42
	i32 878954865, ; 146: System.Net.Http.Json => 0x3463c971 => 63
	i32 904024072, ; 147: System.ComponentModel.Primitives.dll => 0x35e25008 => 16
	i32 911108515, ; 148: System.IO.MemoryMappedFiles.dll => 0x364e69a3 => 53
	i32 926902833, ; 149: tr/Microsoft.Maui.Controls.resources.dll => 0x373f6a31 => 351
	i32 928116545, ; 150: Xamarin.Google.Guava.ListenableFuture => 0x3751ef41 => 303
	i32 952186615, ; 151: System.Runtime.InteropServices.JavaScript.dll => 0x38c136f7 => 105
	i32 956575887, ; 152: Xamarin.Kotlin.StdLib.Jdk8.dll => 0x3904308f => 317
	i32 966729478, ; 153: Xamarin.Google.Crypto.Tink.Android => 0x399f1f06 => 301
	i32 967690846, ; 154: Xamarin.AndroidX.Lifecycle.Common.dll => 0x39adca5e => 249
	i32 975236339, ; 155: System.Diagnostics.Tracing => 0x3a20ecf3 => 34
	i32 975874589, ; 156: System.Xml.XDocument => 0x3a2aaa1d => 158
	i32 986514023, ; 157: System.Private.DataContractSerialization.dll => 0x3acd0267 => 85
	i32 987214855, ; 158: System.Diagnostics.Tools => 0x3ad7b407 => 32
	i32 992768348, ; 159: System.Collections.dll => 0x3b2c715c => 12
	i32 994442037, ; 160: System.IO.FileSystem => 0x3b45fb35 => 51
	i32 996733531, ; 161: Xamarin.Google.Android.DataTransport.TransportBackendCct => 0x3b68f25b => 295
	i32 1001831731, ; 162: System.IO.UnmanagedMemoryStream.dll => 0x3bb6bd33 => 56
	i32 1012816738, ; 163: Xamarin.AndroidX.SavedState.dll => 0x3c5e5b62 => 273
	i32 1019214401, ; 164: System.Drawing => 0x3cbffa41 => 36
	i32 1028951442, ; 165: Microsoft.Extensions.DependencyInjection.Abstractions => 0x3d548d92 => 190
	i32 1029334545, ; 166: da/Microsoft.Maui.Controls.resources.dll => 0x3d5a6611 => 326
	i32 1031528504, ; 167: Xamarin.Google.ErrorProne.Annotations.dll => 0x3d7be038 => 302
	i32 1035644815, ; 168: Xamarin.AndroidX.AppCompat => 0x3dbaaf8f => 215
	i32 1036536393, ; 169: System.Drawing.Primitives.dll => 0x3dc84a49 => 35
	i32 1044663988, ; 170: System.Linq.Expressions.dll => 0x3e444eb4 => 58
	i32 1052210849, ; 171: Xamarin.AndroidX.Lifecycle.ViewModel.dll => 0x3eb776a1 => 259
	i32 1058641855, ; 172: Microsoft.AspNetCore.Http.Connections.Common => 0x3f1997bf => 177
	i32 1061503568, ; 173: Xamarin.Google.AutoValue.Annotations => 0x3f454250 => 299
	i32 1067306892, ; 174: GoogleGson => 0x3f9dcf8c => 174
	i32 1082857460, ; 175: System.ComponentModel.TypeConverter => 0x408b17f4 => 17
	i32 1084122840, ; 176: Xamarin.Kotlin.StdLib => 0x409e66d8 => 315
	i32 1098259244, ; 177: System => 0x41761b2c => 164
	i32 1118262833, ; 178: ko\Microsoft.Maui.Controls.resources => 0x42a75631 => 339
	i32 1121599056, ; 179: Xamarin.AndroidX.Lifecycle.Runtime.Ktx.dll => 0x42da3e50 => 257
	i32 1122050967, ; 180: Xamarin.Google.Android.ODML.Image => 0x42e12397 => 298
	i32 1127624469, ; 181: Microsoft.Extensions.Logging.Debug => 0x43362f15 => 194
	i32 1149092582, ; 182: Xamarin.AndroidX.Window => 0x447dc2e6 => 287
	i32 1157931901, ; 183: Microsoft.EntityFrameworkCore.Abstractions => 0x4504a37d => 183
	i32 1168523401, ; 184: pt\Microsoft.Maui.Controls.resources => 0x45a64089 => 345
	i32 1170634674, ; 185: System.Web.dll => 0x45c677b2 => 153
	i32 1175144683, ; 186: Xamarin.AndroidX.VectorDrawable.Animated => 0x460b48eb => 283
	i32 1178241025, ; 187: Xamarin.AndroidX.Navigation.Runtime.dll => 0x463a8801 => 267
	i32 1202000627, ; 188: Microsoft.EntityFrameworkCore.Abstractions.dll => 0x47a512f3 => 183
	i32 1203215381, ; 189: pl/Microsoft.Maui.Controls.resources.dll => 0x47b79c15 => 343
	i32 1203469131, ; 190: Xamarin.Google.MLKit.Common.dll => 0x47bb7b4b => 306
	i32 1204270330, ; 191: Xamarin.AndroidX.Arch.Core.Common => 0x47c7b4fa => 217
	i32 1204575371, ; 192: Microsoft.Extensions.Caching.Memory.dll => 0x47cc5c8b => 186
	i32 1208641965, ; 193: System.Diagnostics.Process => 0x480a69ad => 29
	i32 1219128291, ; 194: System.IO.IsolatedStorage => 0x48aa6be3 => 52
	i32 1233093933, ; 195: Microsoft.AspNetCore.SignalR.Client.Core.dll => 0x497f852d => 179
	i32 1234928153, ; 196: nb/Microsoft.Maui.Controls.resources.dll => 0x499b8219 => 341
	i32 1243150071, ; 197: Xamarin.AndroidX.Window.Extensions.Core.Core.dll => 0x4a18f6f7 => 288
	i32 1246548578, ; 198: Xamarin.AndroidX.Collection.Jvm.dll => 0x4a4cd262 => 227
	i32 1253011324, ; 199: Microsoft.Win32.Registry => 0x4aaf6f7c => 5
	i32 1260983243, ; 200: cs\Microsoft.Maui.Controls.resources => 0x4b2913cb => 325
	i32 1264511973, ; 201: Xamarin.AndroidX.Startup.StartupRuntime.dll => 0x4b5eebe5 => 277
	i32 1264890200, ; 202: Xamarin.KotlinX.Coroutines.Core.dll => 0x4b64b158 => 321
	i32 1267360935, ; 203: Xamarin.AndroidX.VectorDrawable => 0x4b8a64a7 => 282
	i32 1273260888, ; 204: Xamarin.AndroidX.Collection.Ktx => 0x4be46b58 => 228
	i32 1275534314, ; 205: Xamarin.KotlinX.Coroutines.Android => 0x4c071bea => 320
	i32 1278448581, ; 206: Xamarin.AndroidX.Annotation.Jvm => 0x4c3393c5 => 214
	i32 1293217323, ; 207: Xamarin.AndroidX.DrawerLayout.dll => 0x4d14ee2b => 240
	i32 1309188875, ; 208: System.Private.DataContractSerialization => 0x4e08a30b => 85
	i32 1322716291, ; 209: Xamarin.AndroidX.Window.dll => 0x4ed70c83 => 287
	i32 1324164729, ; 210: System.Linq => 0x4eed2679 => 61
	i32 1335329327, ; 211: System.Runtime.Serialization.Json.dll => 0x4f97822f => 112
	i32 1351347447, ; 212: Xamarin.GooglePlayServices.MLKit.BarcodeScanning => 0x508becf7 => 311
	i32 1355368438, ; 213: Xamarin.Google.MLKit.BarcodeScanning.Common.dll => 0x50c947f6 => 305
	i32 1358509622, ; 214: Xamarin.Google.MLKit.BarcodeScanning => 0x50f93636 => 304
	i32 1364015309, ; 215: System.IO => 0x514d38cd => 57
	i32 1373134921, ; 216: zh-Hans\Microsoft.Maui.Controls.resources => 0x51d86049 => 355
	i32 1376866003, ; 217: Xamarin.AndroidX.SavedState => 0x52114ed3 => 273
	i32 1379779777, ; 218: System.Resources.ResourceManager => 0x523dc4c1 => 99
	i32 1379897097, ; 219: Xamarin.JavaX.Inject => 0x523f8f09 => 313
	i32 1402170036, ; 220: System.Configuration.dll => 0x53936ab4 => 19
	i32 1406073936, ; 221: Xamarin.AndroidX.CoordinatorLayout => 0x53cefc50 => 233
	i32 1408764838, ; 222: System.Runtime.Serialization.Formatters.dll => 0x53f80ba6 => 111
	i32 1411638395, ; 223: System.Runtime.CompilerServices.Unsafe => 0x5423e47b => 101
	i32 1414043276, ; 224: Microsoft.AspNetCore.Connections.Abstractions.dll => 0x5448968c => 175
	i32 1422545099, ; 225: System.Runtime.CompilerServices.VisualC => 0x54ca50cb => 102
	i32 1430672901, ; 226: ar\Microsoft.Maui.Controls.resources => 0x55465605 => 323
	i32 1434145427, ; 227: System.Runtime.Handles => 0x557b5293 => 104
	i32 1435222561, ; 228: Xamarin.Google.Crypto.Tink.Android.dll => 0x558bc221 => 301
	i32 1437299793, ; 229: Xamarin.AndroidX.Lifecycle.Common.Jvm => 0x55ab7451 => 250
	i32 1439761251, ; 230: System.Net.Quic.dll => 0x55d10363 => 71
	i32 1441095154, ; 231: Xamarin.AndroidX.Lifecycle.ViewModel.Android => 0x55e55df2 => 260
	i32 1452070440, ; 232: System.Formats.Asn1.dll => 0x568cd628 => 38
	i32 1453312822, ; 233: System.Diagnostics.Tools.dll => 0x569fcb36 => 32
	i32 1457743152, ; 234: System.Runtime.Extensions.dll => 0x56e36530 => 103
	i32 1458022317, ; 235: System.Net.Security.dll => 0x56e7a7ad => 73
	i32 1461004990, ; 236: es\Microsoft.Maui.Controls.resources => 0x57152abe => 329
	i32 1461234159, ; 237: System.Collections.Immutable.dll => 0x5718a9ef => 9
	i32 1461719063, ; 238: System.Security.Cryptography.OpenSsl => 0x57201017 => 123
	i32 1462112819, ; 239: System.IO.Compression.dll => 0x57261233 => 46
	i32 1469204771, ; 240: Xamarin.AndroidX.AppCompat.AppCompatResources => 0x57924923 => 216
	i32 1470490898, ; 241: Microsoft.Extensions.Primitives => 0x57a5e912 => 196
	i32 1479771757, ; 242: System.Collections.Immutable => 0x5833866d => 9
	i32 1480492111, ; 243: System.IO.Compression.Brotli.dll => 0x583e844f => 43
	i32 1487239319, ; 244: Microsoft.Win32.Primitives => 0x58a57897 => 4
	i32 1490025113, ; 245: Xamarin.AndroidX.SavedState.SavedState.Ktx.dll => 0x58cffa99 => 274
	i32 1493001747, ; 246: hi/Microsoft.Maui.Controls.resources.dll => 0x58fd6613 => 333
	i32 1514721132, ; 247: el/Microsoft.Maui.Controls.resources.dll => 0x5a48cf6c => 328
	i32 1536373174, ; 248: System.Diagnostics.TextWriterTraceListener => 0x5b9331b6 => 31
	i32 1543031311, ; 249: System.Text.RegularExpressions.dll => 0x5bf8ca0f => 138
	i32 1543355203, ; 250: System.Reflection.Emit.dll => 0x5bfdbb43 => 92
	i32 1550322496, ; 251: System.Reflection.Extensions.dll => 0x5c680b40 => 93
	i32 1551623176, ; 252: sk/Microsoft.Maui.Controls.resources.dll => 0x5c7be408 => 348
	i32 1565862583, ; 253: System.IO.FileSystem.Primitives => 0x5d552ab7 => 49
	i32 1566207040, ; 254: System.Threading.Tasks.Dataflow.dll => 0x5d5a6c40 => 141
	i32 1573704789, ; 255: System.Runtime.Serialization.Json => 0x5dccd455 => 112
	i32 1580037396, ; 256: System.Threading.Overlapped => 0x5e2d7514 => 140
	i32 1582372066, ; 257: Xamarin.AndroidX.DocumentFile.dll => 0x5e5114e2 => 239
	i32 1592978981, ; 258: System.Runtime.Serialization.dll => 0x5ef2ee25 => 115
	i32 1597949149, ; 259: Xamarin.Google.ErrorProne.Annotations => 0x5f3ec4dd => 302
	i32 1601112923, ; 260: System.Xml.Serialization => 0x5f6f0b5b => 157
	i32 1604827217, ; 261: System.Net.WebClient => 0x5fa7b851 => 76
	i32 1618516317, ; 262: System.Net.WebSockets.Client.dll => 0x6078995d => 79
	i32 1622152042, ; 263: Xamarin.AndroidX.Loader.dll => 0x60b0136a => 263
	i32 1622358360, ; 264: System.Dynamic.Runtime => 0x60b33958 => 37
	i32 1624863272, ; 265: Xamarin.AndroidX.ViewPager2 => 0x60d97228 => 286
	i32 1635184631, ; 266: Xamarin.AndroidX.Emoji2.ViewsHelper => 0x6176eff7 => 243
	i32 1636350590, ; 267: Xamarin.AndroidX.CursorAdapter => 0x6188ba7e => 236
	i32 1639515021, ; 268: System.Net.Http.dll => 0x61b9038d => 64
	i32 1639986890, ; 269: System.Text.RegularExpressions => 0x61c036ca => 138
	i32 1641389582, ; 270: System.ComponentModel.EventBasedAsync.dll => 0x61d59e0e => 15
	i32 1657153582, ; 271: System.Runtime => 0x62c6282e => 116
	i32 1658241508, ; 272: Xamarin.AndroidX.Tracing.Tracing.dll => 0x62d6c1e4 => 279
	i32 1658251792, ; 273: Xamarin.Google.Android.Material.dll => 0x62d6ea10 => 297
	i32 1670060433, ; 274: Xamarin.AndroidX.ConstraintLayout => 0x638b1991 => 231
	i32 1675553242, ; 275: System.IO.FileSystem.DriveInfo.dll => 0x63dee9da => 48
	i32 1677501392, ; 276: System.Net.Primitives.dll => 0x63fca3d0 => 70
	i32 1678508291, ; 277: System.Net.WebSockets => 0x640c0103 => 80
	i32 1679769178, ; 278: System.Security.Cryptography => 0x641f3e5a => 126
	i32 1689493916, ; 279: Microsoft.EntityFrameworkCore.dll => 0x64b3a19c => 182
	i32 1691477237, ; 280: System.Reflection.Metadata => 0x64d1e4f5 => 94
	i32 1696967625, ; 281: System.Security.Cryptography.Csp => 0x6525abc9 => 121
	i32 1701541528, ; 282: System.Diagnostics.Debug.dll => 0x656b7698 => 26
	i32 1718006957, ; 283: Xamarin.GooglePlayServices.MLKit.BarcodeScanning.dll => 0x6666b4ad => 311
	i32 1720223769, ; 284: Xamarin.AndroidX.Lifecycle.LiveData.Core.Ktx => 0x66888819 => 253
	i32 1726116996, ; 285: System.Reflection.dll => 0x66e27484 => 97
	i32 1728033016, ; 286: System.Diagnostics.FileVersionInfo.dll => 0x66ffb0f8 => 28
	i32 1729485958, ; 287: Xamarin.AndroidX.CardView.dll => 0x6715dc86 => 225
	i32 1736233607, ; 288: ro/Microsoft.Maui.Controls.resources.dll => 0x677cd287 => 346
	i32 1743415430, ; 289: ca\Microsoft.Maui.Controls.resources => 0x67ea6886 => 324
	i32 1744735666, ; 290: System.Transactions.Local.dll => 0x67fe8db2 => 149
	i32 1746115085, ; 291: System.IO.Pipelines.dll => 0x68139a0d => 205
	i32 1746316138, ; 292: Mono.Android.Export => 0x6816ab6a => 169
	i32 1750313021, ; 293: Microsoft.Win32.Primitives.dll => 0x6853a83d => 4
	i32 1758240030, ; 294: System.Resources.Reader.dll => 0x68cc9d1e => 98
	i32 1763938596, ; 295: System.Diagnostics.TraceSource.dll => 0x69239124 => 33
	i32 1765942094, ; 296: System.Reflection.Extensions => 0x6942234e => 93
	i32 1766324549, ; 297: Xamarin.AndroidX.SwipeRefreshLayout => 0x6947f945 => 278
	i32 1770582343, ; 298: Microsoft.Extensions.Logging.dll => 0x6988f147 => 192
	i32 1772434258, ; 299: NekrasovskyAPP => 0x69a53352 => 0
	i32 1776026572, ; 300: System.Core.dll => 0x69dc03cc => 21
	i32 1777075843, ; 301: System.Globalization.Extensions.dll => 0x69ec0683 => 41
	i32 1780572499, ; 302: Mono.Android.Runtime.dll => 0x6a216153 => 170
	i32 1782862114, ; 303: ms\Microsoft.Maui.Controls.resources => 0x6a445122 => 340
	i32 1788241197, ; 304: Xamarin.AndroidX.Fragment => 0x6a96652d => 245
	i32 1793755602, ; 305: he\Microsoft.Maui.Controls.resources => 0x6aea89d2 => 332
	i32 1808609942, ; 306: Xamarin.AndroidX.Loader => 0x6bcd3296 => 263
	i32 1813058853, ; 307: Xamarin.Kotlin.StdLib.dll => 0x6c111525 => 315
	i32 1813201214, ; 308: Xamarin.Google.Android.Material => 0x6c13413e => 297
	i32 1818569960, ; 309: Xamarin.AndroidX.Navigation.UI.dll => 0x6c652ce8 => 268
	i32 1818787751, ; 310: Microsoft.VisualBasic.Core => 0x6c687fa7 => 2
	i32 1824175904, ; 311: System.Text.Encoding.Extensions => 0x6cbab720 => 134
	i32 1824722060, ; 312: System.Runtime.Serialization.Formatters => 0x6cc30c8c => 111
	i32 1828688058, ; 313: Microsoft.Extensions.Logging.Abstractions.dll => 0x6cff90ba => 193
	i32 1842015223, ; 314: uk/Microsoft.Maui.Controls.resources.dll => 0x6dcaebf7 => 352
	i32 1847515442, ; 315: Xamarin.Android.Glide.Annotations => 0x6e1ed932 => 207
	i32 1853025655, ; 316: sv\Microsoft.Maui.Controls.resources => 0x6e72ed77 => 349
	i32 1858542181, ; 317: System.Linq.Expressions => 0x6ec71a65 => 58
	i32 1866818530, ; 318: Xamarin.AndroidX.Camera.Video => 0x6f4563e2 => 223
	i32 1870277092, ; 319: System.Reflection.Primitives => 0x6f7a29e4 => 95
	i32 1875935024, ; 320: fr\Microsoft.Maui.Controls.resources => 0x6fd07f30 => 331
	i32 1876173635, ; 321: Xamarin.Firebase.Encoders.Proto => 0x6fd42343 => 293
	i32 1879696579, ; 322: System.Formats.Tar.dll => 0x7009e4c3 => 39
	i32 1885316902, ; 323: Xamarin.AndroidX.Arch.Core.Runtime.dll => 0x705fa726 => 218
	i32 1888955245, ; 324: System.Diagnostics.Contracts => 0x70972b6d => 25
	i32 1889954781, ; 325: System.Reflection.Metadata.dll => 0x70a66bdd => 94
	i32 1898237753, ; 326: System.Reflection.DispatchProxy => 0x7124cf39 => 89
	i32 1900610850, ; 327: System.Resources.ResourceManager.dll => 0x71490522 => 99
	i32 1908813208, ; 328: Xamarin.GooglePlayServices.Basement => 0x71c62d98 => 310
	i32 1910275211, ; 329: System.Collections.NonGeneric.dll => 0x71dc7c8b => 10
	i32 1939592360, ; 330: System.Private.Xml.Linq => 0x739bd4a8 => 87
	i32 1945717188, ; 331: Microsoft.AspNetCore.SignalR.Client.Core => 0x73f949c4 => 179
	i32 1956758971, ; 332: System.Resources.Writer => 0x74a1c5bb => 100
	i32 1961813231, ; 333: Xamarin.AndroidX.Security.SecurityCrypto.dll => 0x74eee4ef => 275
	i32 1967334205, ; 334: Microsoft.AspNetCore.SignalR.Common => 0x7543233d => 180
	i32 1968388702, ; 335: Microsoft.Extensions.Configuration.dll => 0x75533a5e => 187
	i32 1985761444, ; 336: Xamarin.Android.Glide.GifDecoder => 0x765c50a4 => 209
	i32 2003115576, ; 337: el\Microsoft.Maui.Controls.resources => 0x77651e38 => 328
	i32 2011961780, ; 338: System.Buffers.dll => 0x77ec19b4 => 7
	i32 2019465201, ; 339: Xamarin.AndroidX.Lifecycle.ViewModel => 0x785e97f1 => 259
	i32 2025202353, ; 340: ar/Microsoft.Maui.Controls.resources.dll => 0x78b622b1 => 323
	i32 2031763787, ; 341: Xamarin.Android.Glide => 0x791a414b => 206
	i32 2045470958, ; 342: System.Private.Xml => 0x79eb68ee => 88
	i32 2055257422, ; 343: Xamarin.AndroidX.Lifecycle.LiveData.Core.dll => 0x7a80bd4e => 252
	i32 2060060697, ; 344: System.Windows.dll => 0x7aca0819 => 154
	i32 2066184531, ; 345: de\Microsoft.Maui.Controls.resources => 0x7b277953 => 327
	i32 2070888862, ; 346: System.Diagnostics.TraceSource => 0x7b6f419e => 33
	i32 2079903147, ; 347: System.Runtime.dll => 0x7bf8cdab => 116
	i32 2090596640, ; 348: System.Numerics.Vectors => 0x7c9bf920 => 82
	i32 2100944304, ; 349: BarcodeScanning.Native.Maui.dll => 0x7d39ddb0 => 173
	i32 2124230737, ; 350: Xamarin.Google.Android.DataTransport.TransportBackendCct.dll => 0x7e9d3051 => 295
	i32 2127167465, ; 351: System.Console => 0x7ec9ffe9 => 20
	i32 2129483829, ; 352: Xamarin.GooglePlayServices.Base.dll => 0x7eed5835 => 309
	i32 2142473426, ; 353: System.Collections.Specialized => 0x7fb38cd2 => 11
	i32 2143790110, ; 354: System.Xml.XmlSerializer.dll => 0x7fc7a41e => 162
	i32 2146852085, ; 355: Microsoft.VisualBasic.dll => 0x7ff65cf5 => 3
	i32 2159891885, ; 356: Microsoft.Maui => 0x80bd55ad => 200
	i32 2169148018, ; 357: hu\Microsoft.Maui.Controls.resources => 0x814a9272 => 335
	i32 2174878672, ; 358: Xamarin.Firebase.Annotations => 0x81a203d0 => 289
	i32 2181898931, ; 359: Microsoft.Extensions.Options.dll => 0x820d22b3 => 195
	i32 2188559649, ; 360: Xamarin.Google.MLKit.BarcodeScanning.dll => 0x8272c521 => 304
	i32 2192057212, ; 361: Microsoft.Extensions.Logging.Abstractions => 0x82a8237c => 193
	i32 2193016926, ; 362: System.ObjectModel.dll => 0x82b6c85e => 84
	i32 2201107256, ; 363: Xamarin.KotlinX.Coroutines.Core.Jvm.dll => 0x83323b38 => 322
	i32 2201231467, ; 364: System.Net.Http => 0x8334206b => 64
	i32 2207618523, ; 365: it\Microsoft.Maui.Controls.resources => 0x839595db => 337
	i32 2217644978, ; 366: Xamarin.AndroidX.VectorDrawable.Animated.dll => 0x842e93b2 => 283
	i32 2222056684, ; 367: System.Threading.Tasks.Parallel => 0x8471e4ec => 143
	i32 2229158877, ; 368: Microsoft.Extensions.Features.dll => 0x84de43dd => 191
	i32 2244775296, ; 369: Xamarin.AndroidX.LocalBroadcastManager => 0x85cc8d80 => 264
	i32 2252106437, ; 370: System.Xml.Serialization.dll => 0x863c6ac5 => 157
	i32 2252897993, ; 371: Microsoft.EntityFrameworkCore => 0x86487ec9 => 182
	i32 2256313426, ; 372: System.Globalization.Extensions => 0x867c9c52 => 41
	i32 2265110946, ; 373: System.Security.AccessControl.dll => 0x8702d9a2 => 117
	i32 2266799131, ; 374: Microsoft.Extensions.Configuration.Abstractions => 0x871c9c1b => 188
	i32 2267999099, ; 375: Xamarin.Android.Glide.DiskLruCache.dll => 0x872eeb7b => 208
	i32 2270573516, ; 376: fr/Microsoft.Maui.Controls.resources.dll => 0x875633cc => 331
	i32 2279755925, ; 377: Xamarin.AndroidX.RecyclerView.dll => 0x87e25095 => 271
	i32 2293034957, ; 378: System.ServiceModel.Web.dll => 0x88acefcd => 131
	i32 2294913272, ; 379: Npgsql => 0x88c998f8 => 203
	i32 2295906218, ; 380: System.Net.Sockets => 0x88d8bfaa => 75
	i32 2298471582, ; 381: System.Net.Mail => 0x88ffe49e => 66
	i32 2303942373, ; 382: nb\Microsoft.Maui.Controls.resources => 0x89535ee5 => 341
	i32 2305521784, ; 383: System.Private.CoreLib.dll => 0x896b7878 => 172
	i32 2315684594, ; 384: Xamarin.AndroidX.Annotation.dll => 0x8a068af2 => 212
	i32 2319144366, ; 385: Microsoft.AspNetCore.SignalR.Client => 0x8a3b55ae => 178
	i32 2320631194, ; 386: System.Threading.Tasks.Parallel.dll => 0x8a52059a => 143
	i32 2334995809, ; 387: Npgsql.EntityFrameworkCore.PostgreSQL.dll => 0x8b2d3561 => 204
	i32 2340441535, ; 388: System.Runtime.InteropServices.RuntimeInformation.dll => 0x8b804dbf => 106
	i32 2344264397, ; 389: System.ValueTuple => 0x8bbaa2cd => 151
	i32 2353062107, ; 390: System.Net.Primitives => 0x8c40e0db => 70
	i32 2368005991, ; 391: System.Xml.ReaderWriter.dll => 0x8d24e767 => 156
	i32 2371007202, ; 392: Microsoft.Extensions.Configuration => 0x8d52b2e2 => 187
	i32 2378619854, ; 393: System.Security.Cryptography.Csp.dll => 0x8dc6dbce => 121
	i32 2383496789, ; 394: System.Security.Principal.Windows.dll => 0x8e114655 => 127
	i32 2395872292, ; 395: id\Microsoft.Maui.Controls.resources => 0x8ece1c24 => 336
	i32 2401565422, ; 396: System.Web.HttpUtility => 0x8f24faee => 152
	i32 2403452196, ; 397: Xamarin.AndroidX.Emoji2.dll => 0x8f41c524 => 242
	i32 2418341376, ; 398: Xamarin.AndroidX.Camera.Video.dll => 0x9024f600 => 223
	i32 2421380589, ; 399: System.Threading.Tasks.Dataflow => 0x905355ed => 141
	i32 2423080555, ; 400: Xamarin.AndroidX.Collection.Ktx.dll => 0x906d466b => 228
	i32 2425270691, ; 401: Xamarin.Google.MLKit.BarcodeScanning.Common => 0x908eb1a3 => 305
	i32 2427813419, ; 402: hi\Microsoft.Maui.Controls.resources => 0x90b57e2b => 333
	i32 2435356389, ; 403: System.Console.dll => 0x912896e5 => 20
	i32 2435904999, ; 404: System.ComponentModel.DataAnnotations.dll => 0x9130f5e7 => 14
	i32 2454642406, ; 405: System.Text.Encoding.dll => 0x924edee6 => 135
	i32 2458678730, ; 406: System.Net.Sockets.dll => 0x928c75ca => 75
	i32 2459001652, ; 407: System.Linq.Parallel.dll => 0x92916334 => 59
	i32 2465532216, ; 408: Xamarin.AndroidX.ConstraintLayout.Core.dll => 0x92f50938 => 232
	i32 2471841756, ; 409: netstandard.dll => 0x93554fdc => 167
	i32 2475788418, ; 410: Java.Interop.dll => 0x93918882 => 168
	i32 2480646305, ; 411: Microsoft.Maui.Controls => 0x93dba8a1 => 198
	i32 2483903535, ; 412: System.ComponentModel.EventBasedAsync => 0x940d5c2f => 15
	i32 2484371297, ; 413: System.Net.ServicePoint => 0x94147f61 => 74
	i32 2490993605, ; 414: System.AppContext.dll => 0x94798bc5 => 6
	i32 2501346920, ; 415: System.Data.DataSetExtensions => 0x95178668 => 23
	i32 2505896520, ; 416: Xamarin.AndroidX.Lifecycle.Runtime.dll => 0x955cf248 => 255
	i32 2522472828, ; 417: Xamarin.Android.Glide.dll => 0x9659e17c => 206
	i32 2538310050, ; 418: System.Reflection.Emit.Lightweight.dll => 0x974b89a2 => 91
	i32 2550873716, ; 419: hr\Microsoft.Maui.Controls.resources => 0x980b3e74 => 334
	i32 2562349572, ; 420: Microsoft.CSharp => 0x98ba5a04 => 1
	i32 2570120770, ; 421: System.Text.Encodings.Web => 0x9930ee42 => 136
	i32 2577256205, ; 422: Xamarin.AndroidX.Lifecycle.Runtime.Ktx.Android => 0x999dcf0d => 258
	i32 2581783588, ; 423: Xamarin.AndroidX.Lifecycle.Runtime.Ktx => 0x99e2e424 => 257
	i32 2581819634, ; 424: Xamarin.AndroidX.VectorDrawable.dll => 0x99e370f2 => 282
	i32 2585220780, ; 425: System.Text.Encoding.Extensions.dll => 0x9a1756ac => 134
	i32 2585805581, ; 426: System.Net.Ping => 0x9a20430d => 69
	i32 2589602615, ; 427: System.Threading.ThreadPool => 0x9a5a3337 => 146
	i32 2593496499, ; 428: pl\Microsoft.Maui.Controls.resources => 0x9a959db3 => 343
	i32 2605712449, ; 429: Xamarin.KotlinX.Coroutines.Core.Jvm => 0x9b500441 => 322
	i32 2615233544, ; 430: Xamarin.AndroidX.Fragment.Ktx => 0x9be14c08 => 246
	i32 2616218305, ; 431: Microsoft.Extensions.Logging.Debug.dll => 0x9bf052c1 => 194
	i32 2617129537, ; 432: System.Private.Xml.dll => 0x9bfe3a41 => 88
	i32 2618712057, ; 433: System.Reflection.TypeExtensions.dll => 0x9c165ff9 => 96
	i32 2620111890, ; 434: Xamarin.Firebase.Encoders.dll => 0x9c2bbc12 => 291
	i32 2620871830, ; 435: Xamarin.AndroidX.CursorAdapter.dll => 0x9c375496 => 236
	i32 2624644809, ; 436: Xamarin.AndroidX.DynamicAnimation => 0x9c70e6c9 => 241
	i32 2626831493, ; 437: ja\Microsoft.Maui.Controls.resources => 0x9c924485 => 338
	i32 2627185994, ; 438: System.Diagnostics.TextWriterTraceListener.dll => 0x9c97ad4a => 31
	i32 2629843544, ; 439: System.IO.Compression.ZipFile.dll => 0x9cc03a58 => 45
	i32 2633051222, ; 440: Xamarin.AndroidX.Lifecycle.LiveData => 0x9cf12c56 => 251
	i32 2634653062, ; 441: Microsoft.EntityFrameworkCore.Relational.dll => 0x9d099d86 => 184
	i32 2637500010, ; 442: Microsoft.Extensions.Features => 0x9d350e6a => 191
	i32 2639764100, ; 443: Xamarin.Firebase.Encoders => 0x9d579a84 => 291
	i32 2663391936, ; 444: Xamarin.Android.Glide.DiskLruCache => 0x9ec022c0 => 208
	i32 2663698177, ; 445: System.Runtime.Loader => 0x9ec4cf01 => 109
	i32 2664396074, ; 446: System.Xml.XDocument.dll => 0x9ecf752a => 158
	i32 2665622720, ; 447: System.Drawing.Primitives => 0x9ee22cc0 => 35
	i32 2671474046, ; 448: Xamarin.KotlinX.Coroutines.Core => 0x9f3b757e => 321
	i32 2676780864, ; 449: System.Data.Common.dll => 0x9f8c6f40 => 22
	i32 2686887180, ; 450: System.Runtime.Serialization.Xml.dll => 0xa026a50c => 114
	i32 2693849962, ; 451: System.IO.dll => 0xa090e36a => 57
	i32 2701096212, ; 452: Xamarin.AndroidX.Tracing.Tracing => 0xa0ff7514 => 279
	i32 2715334215, ; 453: System.Threading.Tasks.dll => 0xa1d8b647 => 144
	i32 2717744543, ; 454: System.Security.Claims => 0xa1fd7d9f => 118
	i32 2719963679, ; 455: System.Security.Cryptography.Cng.dll => 0xa21f5a1f => 120
	i32 2724373263, ; 456: System.Runtime.Numerics.dll => 0xa262a30f => 110
	i32 2732626843, ; 457: Xamarin.AndroidX.Activity => 0xa2e0939b => 210
	i32 2735172069, ; 458: System.Threading.Channels => 0xa30769e5 => 139
	i32 2737747696, ; 459: Xamarin.AndroidX.AppCompat.AppCompatResources.dll => 0xa32eb6f0 => 216
	i32 2740948882, ; 460: System.IO.Pipes.AccessControl => 0xa35f8f92 => 54
	i32 2748088231, ; 461: System.Runtime.InteropServices.JavaScript => 0xa3cc7fa7 => 105
	i32 2752995522, ; 462: pt-BR\Microsoft.Maui.Controls.resources => 0xa41760c2 => 344
	i32 2758225723, ; 463: Microsoft.Maui.Controls.Xaml => 0xa4672f3b => 199
	i32 2764765095, ; 464: Microsoft.Maui.dll => 0xa4caf7a7 => 200
	i32 2765824710, ; 465: System.Text.Encoding.CodePages.dll => 0xa4db22c6 => 133
	i32 2766642685, ; 466: Xamarin.AndroidX.Lifecycle.ViewModel.Android.dll => 0xa4e79dfd => 260
	i32 2770495804, ; 467: Xamarin.Jetbrains.Annotations.dll => 0xa522693c => 314
	i32 2778768386, ; 468: Xamarin.AndroidX.ViewPager.dll => 0xa5a0a402 => 285
	i32 2779977773, ; 469: Xamarin.AndroidX.ResourceInspection.Annotation.dll => 0xa5b3182d => 272
	i32 2780199943, ; 470: Xamarin.AndroidX.Lifecycle.Common.Jvm.dll => 0xa5b67c07 => 250
	i32 2785988530, ; 471: th\Microsoft.Maui.Controls.resources => 0xa60ecfb2 => 350
	i32 2788224221, ; 472: Xamarin.AndroidX.Fragment.Ktx.dll => 0xa630ecdd => 246
	i32 2801831435, ; 473: Microsoft.Maui.Graphics => 0xa7008e0b => 202
	i32 2803228030, ; 474: System.Xml.XPath.XDocument.dll => 0xa715dd7e => 159
	i32 2804607052, ; 475: Xamarin.Firebase.Components.dll => 0xa72ae84c => 290
	i32 2806116107, ; 476: es/Microsoft.Maui.Controls.resources.dll => 0xa741ef0b => 329
	i32 2810250172, ; 477: Xamarin.AndroidX.CoordinatorLayout.dll => 0xa78103bc => 233
	i32 2819470561, ; 478: System.Xml.dll => 0xa80db4e1 => 163
	i32 2821205001, ; 479: System.ServiceProcess.dll => 0xa8282c09 => 132
	i32 2821294376, ; 480: Xamarin.AndroidX.ResourceInspection.Annotation => 0xa8298928 => 272
	i32 2824502124, ; 481: System.Xml.XmlDocument => 0xa85a7b6c => 161
	i32 2828186339, ; 482: Xamarin.AndroidX.Concurrent.Futures.Ktx.dll => 0xa892b2e3 => 230
	i32 2831556043, ; 483: nl/Microsoft.Maui.Controls.resources.dll => 0xa8c61dcb => 342
	i32 2838993487, ; 484: Xamarin.AndroidX.Lifecycle.ViewModel.Ktx.dll => 0xa9379a4f => 261
	i32 2847418871, ; 485: Xamarin.GooglePlayServices.Base => 0xa9b829f7 => 309
	i32 2847789619, ; 486: Microsoft.EntityFrameworkCore.Relational => 0xa9bdd233 => 184
	i32 2849599387, ; 487: System.Threading.Overlapped.dll => 0xa9d96f9b => 140
	i32 2853208004, ; 488: Xamarin.AndroidX.ViewPager => 0xaa107fc4 => 285
	i32 2855708567, ; 489: Xamarin.AndroidX.Transition => 0xaa36a797 => 281
	i32 2861098320, ; 490: Mono.Android.Export.dll => 0xaa88e550 => 169
	i32 2861189240, ; 491: Microsoft.Maui.Essentials => 0xaa8a4878 => 201
	i32 2868099152, ; 492: Xamarin.Google.MLKit.Vision.Common.dll => 0xaaf3b850 => 307
	i32 2870099610, ; 493: Xamarin.AndroidX.Activity.Ktx.dll => 0xab123e9a => 211
	i32 2875164099, ; 494: Jsr305Binding.dll => 0xab5f85c3 => 300
	i32 2875220617, ; 495: System.Globalization.Calendars.dll => 0xab606289 => 40
	i32 2875347124, ; 496: Microsoft.AspNetCore.Http.Connections.Client.dll => 0xab6250b4 => 176
	i32 2884993177, ; 497: Xamarin.AndroidX.ExifInterface => 0xabf58099 => 244
	i32 2887636118, ; 498: System.Net.dll => 0xac1dd496 => 81
	i32 2899753641, ; 499: System.IO.UnmanagedMemoryStream => 0xacd6baa9 => 56
	i32 2900621748, ; 500: System.Dynamic.Runtime.dll => 0xace3f9b4 => 37
	i32 2901442782, ; 501: System.Reflection => 0xacf080de => 97
	i32 2905242038, ; 502: mscorlib.dll => 0xad2a79b6 => 166
	i32 2909740682, ; 503: System.Private.CoreLib => 0xad6f1e8a => 172
	i32 2916838712, ; 504: Xamarin.AndroidX.ViewPager2.dll => 0xaddb6d38 => 286
	i32 2919462931, ; 505: System.Numerics.Vectors.dll => 0xae037813 => 82
	i32 2921128767, ; 506: Xamarin.AndroidX.Annotation.Experimental.dll => 0xae1ce33f => 213
	i32 2936416060, ; 507: System.Resources.Reader => 0xaf06273c => 98
	i32 2940926066, ; 508: System.Diagnostics.StackTrace.dll => 0xaf4af872 => 30
	i32 2942453041, ; 509: System.Xml.XPath.XDocument => 0xaf624531 => 159
	i32 2959614098, ; 510: System.ComponentModel.dll => 0xb0682092 => 18
	i32 2965157864, ; 511: Xamarin.AndroidX.Camera.View => 0xb0bcb7e8 => 224
	i32 2968338931, ; 512: System.Security.Principal.Windows => 0xb0ed41f3 => 127
	i32 2972252294, ; 513: System.Security.Cryptography.Algorithms.dll => 0xb128f886 => 119
	i32 2978675010, ; 514: Xamarin.AndroidX.DrawerLayout => 0xb18af942 => 240
	i32 2987532451, ; 515: Xamarin.AndroidX.Security.SecurityCrypto => 0xb21220a3 => 275
	i32 2991449226, ; 516: Xamarin.AndroidX.Camera.Core => 0xb24de48a => 221
	i32 2996846495, ; 517: Xamarin.AndroidX.Lifecycle.Process.dll => 0xb2a03f9f => 254
	i32 3000842441, ; 518: Xamarin.AndroidX.Camera.View.dll => 0xb2dd38c9 => 224
	i32 3016983068, ; 519: Xamarin.AndroidX.Startup.StartupRuntime => 0xb3d3821c => 277
	i32 3023353419, ; 520: WindowsBase.dll => 0xb434b64b => 165
	i32 3024354802, ; 521: Xamarin.AndroidX.Legacy.Support.Core.Utils => 0xb443fdf2 => 248
	i32 3038032645, ; 522: _Microsoft.Android.Resource.Designer.dll => 0xb514b305 => 357
	i32 3047751430, ; 523: Xamarin.AndroidX.Camera.Core.dll => 0xb5a8ff06 => 221
	i32 3056245963, ; 524: Xamarin.AndroidX.SavedState.SavedState.Ktx => 0xb62a9ccb => 274
	i32 3057625584, ; 525: Xamarin.AndroidX.Navigation.Common => 0xb63fa9f0 => 265
	i32 3058099980, ; 526: Xamarin.GooglePlayServices.Tasks => 0xb646e70c => 312
	i32 3059408633, ; 527: Mono.Android.Runtime => 0xb65adef9 => 170
	i32 3059793426, ; 528: System.ComponentModel.Primitives => 0xb660be12 => 16
	i32 3069363400, ; 529: Microsoft.Extensions.Caching.Abstractions.dll => 0xb6f2c4c8 => 185
	i32 3075834255, ; 530: System.Threading.Tasks => 0xb755818f => 144
	i32 3077302341, ; 531: hu/Microsoft.Maui.Controls.resources.dll => 0xb76be845 => 335
	i32 3090735792, ; 532: System.Security.Cryptography.X509Certificates.dll => 0xb838e2b0 => 125
	i32 3099732863, ; 533: System.Security.Claims.dll => 0xb8c22b7f => 118
	i32 3103600923, ; 534: System.Formats.Asn1 => 0xb8fd311b => 38
	i32 3111772706, ; 535: System.Runtime.Serialization => 0xb979e222 => 115
	i32 3121463068, ; 536: System.IO.FileSystem.AccessControl.dll => 0xba0dbf1c => 47
	i32 3124832203, ; 537: System.Threading.Tasks.Extensions => 0xba4127cb => 142
	i32 3132293585, ; 538: System.Security.AccessControl => 0xbab301d1 => 117
	i32 3147165239, ; 539: System.Diagnostics.Tracing.dll => 0xbb95ee37 => 34
	i32 3148237826, ; 540: GoogleGson.dll => 0xbba64c02 => 174
	i32 3155362983, ; 541: Xamarin.Google.Android.DataTransport.TransportApi => 0xbc1304a7 => 294
	i32 3159123045, ; 542: System.Reflection.Primitives.dll => 0xbc4c6465 => 95
	i32 3160747431, ; 543: System.IO.MemoryMappedFiles => 0xbc652da7 => 53
	i32 3178803400, ; 544: Xamarin.AndroidX.Navigation.Fragment.dll => 0xbd78b0c8 => 266
	i32 3192346100, ; 545: System.Security.SecureString => 0xbe4755f4 => 129
	i32 3193515020, ; 546: System.Web => 0xbe592c0c => 153
	i32 3195844289, ; 547: Microsoft.Extensions.Caching.Abstractions => 0xbe7cb6c1 => 185
	i32 3204380047, ; 548: System.Data.dll => 0xbefef58f => 24
	i32 3209718065, ; 549: System.Xml.XmlDocument.dll => 0xbf506931 => 161
	i32 3211777861, ; 550: Xamarin.AndroidX.DocumentFile => 0xbf6fd745 => 239
	i32 3220365878, ; 551: System.Threading => 0xbff2e236 => 148
	i32 3226221578, ; 552: System.Runtime.Handles.dll => 0xc04c3c0a => 104
	i32 3230466174, ; 553: Xamarin.GooglePlayServices.Basement.dll => 0xc08d007e => 310
	i32 3251039220, ; 554: System.Reflection.DispatchProxy.dll => 0xc1c6ebf4 => 89
	i32 3258312781, ; 555: Xamarin.AndroidX.CardView => 0xc235e84d => 225
	i32 3265493905, ; 556: System.Linq.Queryable.dll => 0xc2a37b91 => 60
	i32 3265893370, ; 557: System.Threading.Tasks.Extensions.dll => 0xc2a993fa => 142
	i32 3277815716, ; 558: System.Resources.Writer.dll => 0xc35f7fa4 => 100
	i32 3279906254, ; 559: Microsoft.Win32.Registry.dll => 0xc37f65ce => 5
	i32 3280506390, ; 560: System.ComponentModel.Annotations.dll => 0xc3888e16 => 13
	i32 3290767353, ; 561: System.Security.Cryptography.Encoding => 0xc4251ff9 => 122
	i32 3299363146, ; 562: System.Text.Encoding => 0xc4a8494a => 135
	i32 3303498502, ; 563: System.Diagnostics.FileVersionInfo => 0xc4e76306 => 28
	i32 3305363605, ; 564: fi\Microsoft.Maui.Controls.resources => 0xc503d895 => 330
	i32 3316684772, ; 565: System.Net.Requests.dll => 0xc5b097e4 => 72
	i32 3317135071, ; 566: Xamarin.AndroidX.CustomView.dll => 0xc5b776df => 237
	i32 3317144872, ; 567: System.Data => 0xc5b79d28 => 24
	i32 3340431453, ; 568: Xamarin.AndroidX.Arch.Core.Runtime => 0xc71af05d => 218
	i32 3345895724, ; 569: Xamarin.AndroidX.ProfileInstaller.ProfileInstaller.dll => 0xc76e512c => 270
	i32 3346324047, ; 570: Xamarin.AndroidX.Navigation.Runtime => 0xc774da4f => 267
	i32 3357674450, ; 571: ru\Microsoft.Maui.Controls.resources => 0xc8220bd2 => 347
	i32 3358260929, ; 572: System.Text.Json => 0xc82afec1 => 137
	i32 3359991071, ; 573: Xamarin.AndroidX.Tracing.Tracing.Ktx.dll => 0xc845651f => 280
	i32 3362336904, ; 574: Xamarin.AndroidX.Activity.Ktx => 0xc8693088 => 211
	i32 3362522851, ; 575: Xamarin.AndroidX.Core => 0xc86c06e3 => 234
	i32 3366347497, ; 576: Java.Interop => 0xc8a662e9 => 168
	i32 3371992681, ; 577: Xamarin.Firebase.Encoders.Proto.dll => 0xc8fc8669 => 293
	i32 3374999561, ; 578: Xamarin.AndroidX.RecyclerView => 0xc92a6809 => 271
	i32 3381016424, ; 579: da\Microsoft.Maui.Controls.resources => 0xc9863768 => 326
	i32 3383578424, ; 580: Xamarin.Firebase.Encoders.JSON => 0xc9ad4f38 => 292
	i32 3395150330, ; 581: System.Runtime.CompilerServices.Unsafe.dll => 0xca5de1fa => 101
	i32 3403906625, ; 582: System.Security.Cryptography.OpenSsl.dll => 0xcae37e41 => 123
	i32 3405233483, ; 583: Xamarin.AndroidX.CustomView.PoolingContainer => 0xcaf7bd4b => 238
	i32 3411362516, ; 584: Xamarin.Google.MLKit.Vision.Interfaces => 0xcb5542d4 => 308
	i32 3413944578, ; 585: Xamarin.AndroidX.Camera.Camera2.dll => 0xcb7ca902 => 220
	i32 3421910702, ; 586: Xamarin.AndroidX.Camera.Camera2 => 0xcbf636ae => 220
	i32 3428513518, ; 587: Microsoft.Extensions.DependencyInjection.dll => 0xcc5af6ee => 189
	i32 3429136800, ; 588: System.Xml => 0xcc6479a0 => 163
	i32 3430777524, ; 589: netstandard => 0xcc7d82b4 => 167
	i32 3441283291, ; 590: Xamarin.AndroidX.DynamicAnimation.dll => 0xcd1dd0db => 241
	i32 3445260447, ; 591: System.Formats.Tar => 0xcd5a809f => 39
	i32 3452344032, ; 592: Microsoft.Maui.Controls.Compatibility.dll => 0xcdc696e0 => 197
	i32 3463511458, ; 593: hr/Microsoft.Maui.Controls.resources.dll => 0xce70fda2 => 334
	i32 3466904072, ; 594: Microsoft.AspNetCore.SignalR.Client.dll => 0xcea4c208 => 178
	i32 3471940407, ; 595: System.ComponentModel.TypeConverter.dll => 0xcef19b37 => 17
	i32 3476120550, ; 596: Mono.Android => 0xcf3163e6 => 171
	i32 3479583265, ; 597: ru/Microsoft.Maui.Controls.resources.dll => 0xcf663a21 => 347
	i32 3484440000, ; 598: ro\Microsoft.Maui.Controls.resources => 0xcfb055c0 => 346
	i32 3485117614, ; 599: System.Text.Json.dll => 0xcfbaacae => 137
	i32 3486566296, ; 600: System.Transactions => 0xcfd0c798 => 150
	i32 3493954962, ; 601: Xamarin.AndroidX.Concurrent.Futures.dll => 0xd0418592 => 229
	i32 3509114376, ; 602: System.Xml.Linq => 0xd128d608 => 155
	i32 3515174580, ; 603: System.Security.dll => 0xd1854eb4 => 130
	i32 3530912306, ; 604: System.Configuration => 0xd2757232 => 19
	i32 3539954161, ; 605: System.Net.HttpListener => 0xd2ff69f1 => 65
	i32 3560100363, ; 606: System.Threading.Timer => 0xd432d20b => 147
	i32 3570554715, ; 607: System.IO.FileSystem.AccessControl => 0xd4d2575b => 47
	i32 3580758918, ; 608: zh-HK\Microsoft.Maui.Controls.resources => 0xd56e0b86 => 354
	i32 3597029428, ; 609: Xamarin.Android.Glide.GifDecoder.dll => 0xd6665034 => 209
	i32 3598340787, ; 610: System.Net.WebSockets.Client => 0xd67a52b3 => 79
	i32 3608519521, ; 611: System.Linq.dll => 0xd715a361 => 61
	i32 3624195450, ; 612: System.Runtime.InteropServices.RuntimeInformation => 0xd804d57a => 106
	i32 3626429363, ; 613: Xamarin.Google.MLKit.Common => 0xd826ebb3 => 306
	i32 3627220390, ; 614: Xamarin.AndroidX.Print.dll => 0xd832fda6 => 269
	i32 3633644679, ; 615: Xamarin.AndroidX.Annotation.Experimental => 0xd8950487 => 213
	i32 3638274909, ; 616: System.IO.FileSystem.Primitives.dll => 0xd8dbab5d => 49
	i32 3641597786, ; 617: Xamarin.AndroidX.Lifecycle.LiveData.Core => 0xd90e5f5a => 252
	i32 3643446276, ; 618: tr\Microsoft.Maui.Controls.resources => 0xd92a9404 => 351
	i32 3643854240, ; 619: Xamarin.AndroidX.Navigation.Fragment => 0xd930cda0 => 266
	i32 3645089577, ; 620: System.ComponentModel.DataAnnotations => 0xd943a729 => 14
	i32 3657292374, ; 621: Microsoft.Extensions.Configuration.Abstractions.dll => 0xd9fdda56 => 188
	i32 3660523487, ; 622: System.Net.NetworkInformation => 0xda2f27df => 68
	i32 3672681054, ; 623: Mono.Android.dll => 0xdae8aa5e => 171
	i32 3676461095, ; 624: Xamarin.AndroidX.Camera.Lifecycle => 0xdb225827 => 222
	i32 3682565725, ; 625: Xamarin.AndroidX.Browser => 0xdb7f7e5d => 219
	i32 3684561358, ; 626: Xamarin.AndroidX.Concurrent.Futures => 0xdb9df1ce => 229
	i32 3691870036, ; 627: Microsoft.AspNetCore.SignalR.Protocols.Json => 0xdc0d7754 => 181
	i32 3697841164, ; 628: zh-Hant/Microsoft.Maui.Controls.resources.dll => 0xdc68940c => 356
	i32 3700866549, ; 629: System.Net.WebProxy.dll => 0xdc96bdf5 => 78
	i32 3706696989, ; 630: Xamarin.AndroidX.Core.Core.Ktx.dll => 0xdcefb51d => 235
	i32 3716563718, ; 631: System.Runtime.Intrinsics => 0xdd864306 => 108
	i32 3718780102, ; 632: Xamarin.AndroidX.Annotation => 0xdda814c6 => 212
	i32 3724971120, ; 633: Xamarin.AndroidX.Navigation.Common.dll => 0xde068c70 => 265
	i32 3732100267, ; 634: System.Net.NameResolution => 0xde7354ab => 67
	i32 3737834244, ; 635: System.Net.Http.Json.dll => 0xdecad304 => 63
	i32 3748608112, ; 636: System.Diagnostics.DiagnosticSource => 0xdf6f3870 => 27
	i32 3751444290, ; 637: System.Xml.XPath => 0xdf9a7f42 => 160
	i32 3764085317, ; 638: Xamarin.AndroidX.Lifecycle.Runtime.Ktx.Android.dll => 0xe05b6245 => 258
	i32 3786282454, ; 639: Xamarin.AndroidX.Collection => 0xe1ae15d6 => 226
	i32 3787005001, ; 640: Microsoft.AspNetCore.Connections.Abstractions => 0xe1b91c49 => 175
	i32 3792276235, ; 641: System.Collections.NonGeneric => 0xe2098b0b => 10
	i32 3800979733, ; 642: Microsoft.Maui.Controls.Compatibility => 0xe28e5915 => 197
	i32 3802395368, ; 643: System.Collections.Specialized.dll => 0xe2a3f2e8 => 11
	i32 3819260425, ; 644: System.Net.WebProxy => 0xe3a54a09 => 78
	i32 3823082795, ; 645: System.Security.Cryptography.dll => 0xe3df9d2b => 126
	i32 3829621856, ; 646: System.Numerics.dll => 0xe4436460 => 83
	i32 3841636137, ; 647: Microsoft.Extensions.DependencyInjection.Abstractions.dll => 0xe4fab729 => 190
	i32 3844307129, ; 648: System.Net.Mail.dll => 0xe52378b9 => 66
	i32 3849253459, ; 649: System.Runtime.InteropServices.dll => 0xe56ef253 => 107
	i32 3870376305, ; 650: System.Net.HttpListener.dll => 0xe6b14171 => 65
	i32 3873536506, ; 651: System.Security.Principal => 0xe6e179fa => 128
	i32 3875112723, ; 652: System.Security.Cryptography.Encoding.dll => 0xe6f98713 => 122
	i32 3885497537, ; 653: System.Net.WebHeaderCollection.dll => 0xe797fcc1 => 77
	i32 3885922214, ; 654: Xamarin.AndroidX.Transition.dll => 0xe79e77a6 => 281
	i32 3888767677, ; 655: Xamarin.AndroidX.ProfileInstaller.ProfileInstaller => 0xe7c9e2bd => 270
	i32 3889960447, ; 656: zh-Hans/Microsoft.Maui.Controls.resources.dll => 0xe7dc15ff => 355
	i32 3896106733, ; 657: System.Collections.Concurrent.dll => 0xe839deed => 8
	i32 3896760992, ; 658: Xamarin.AndroidX.Core.dll => 0xe843daa0 => 234
	i32 3901907137, ; 659: Microsoft.VisualBasic.Core.dll => 0xe89260c1 => 2
	i32 3910130544, ; 660: Xamarin.AndroidX.Collection.Jvm => 0xe90fdb70 => 227
	i32 3920810846, ; 661: System.IO.Compression.FileSystem.dll => 0xe9b2d35e => 44
	i32 3921031405, ; 662: Xamarin.AndroidX.VersionedParcelable.dll => 0xe9b630ed => 284
	i32 3928044579, ; 663: System.Xml.ReaderWriter => 0xea213423 => 156
	i32 3930554604, ; 664: System.Security.Principal.dll => 0xea4780ec => 128
	i32 3931092270, ; 665: Xamarin.AndroidX.Navigation.UI => 0xea4fb52e => 268
	i32 3934056515, ; 666: Xamarin.JavaX.Inject.dll => 0xea7cf043 => 313
	i32 3945713374, ; 667: System.Data.DataSetExtensions.dll => 0xeb2ecede => 23
	i32 3953953790, ; 668: System.Text.Encoding.CodePages => 0xebac8bfe => 133
	i32 3955647286, ; 669: Xamarin.AndroidX.AppCompat.dll => 0xebc66336 => 215
	i32 3956287295, ; 670: BarcodeScanning.Native.Maui => 0xebd0273f => 173
	i32 3959773229, ; 671: Xamarin.AndroidX.Lifecycle.Process => 0xec05582d => 254
	i32 3970018735, ; 672: Xamarin.GooglePlayServices.Tasks.dll => 0xeca1adaf => 312
	i32 3980434154, ; 673: th/Microsoft.Maui.Controls.resources.dll => 0xed409aea => 350
	i32 3987592930, ; 674: he/Microsoft.Maui.Controls.resources.dll => 0xedadd6e2 => 332
	i32 4003436829, ; 675: System.Diagnostics.Process.dll => 0xee9f991d => 29
	i32 4015948917, ; 676: Xamarin.AndroidX.Annotation.Jvm.dll => 0xef5e8475 => 214
	i32 4023392905, ; 677: System.IO.Pipelines => 0xefd01a89 => 205
	i32 4025784931, ; 678: System.Memory => 0xeff49a63 => 62
	i32 4026433800, ; 679: NekrasovskyAPP.dll => 0xeffe8108 => 0
	i32 4046471985, ; 680: Microsoft.Maui.Controls.Xaml.dll => 0xf1304331 => 199
	i32 4054681211, ; 681: System.Reflection.Emit.ILGeneration => 0xf1ad867b => 90
	i32 4068434129, ; 682: System.Private.Xml.Linq.dll => 0xf27f60d1 => 87
	i32 4073602200, ; 683: System.Threading.dll => 0xf2ce3c98 => 148
	i32 4094352644, ; 684: Microsoft.Maui.Essentials.dll => 0xf40add04 => 201
	i32 4099507663, ; 685: System.Drawing.dll => 0xf45985cf => 36
	i32 4100113165, ; 686: System.Private.Uri => 0xf462c30d => 86
	i32 4101236366, ; 687: Npgsql.EntityFrameworkCore.PostgreSQL => 0xf473e68e => 204
	i32 4101593132, ; 688: Xamarin.AndroidX.Emoji2 => 0xf479582c => 242
	i32 4101842092, ; 689: Microsoft.Extensions.Caching.Memory => 0xf47d24ac => 186
	i32 4102112229, ; 690: pt/Microsoft.Maui.Controls.resources.dll => 0xf48143e5 => 345
	i32 4125707920, ; 691: ms/Microsoft.Maui.Controls.resources.dll => 0xf5e94e90 => 340
	i32 4126470640, ; 692: Microsoft.Extensions.DependencyInjection => 0xf5f4f1f0 => 189
	i32 4127667938, ; 693: System.IO.FileSystem.Watcher => 0xf60736e2 => 50
	i32 4130442656, ; 694: System.AppContext => 0xf6318da0 => 6
	i32 4147896353, ; 695: System.Reflection.Emit.ILGeneration.dll => 0xf73be021 => 90
	i32 4150914736, ; 696: uk\Microsoft.Maui.Controls.resources => 0xf769eeb0 => 352
	i32 4151237749, ; 697: System.Core => 0xf76edc75 => 21
	i32 4159265925, ; 698: System.Xml.XmlSerializer => 0xf7e95c85 => 162
	i32 4161255271, ; 699: System.Reflection.TypeExtensions => 0xf807b767 => 96
	i32 4164802419, ; 700: System.IO.FileSystem.Watcher.dll => 0xf83dd773 => 50
	i32 4181436372, ; 701: System.Runtime.Serialization.Primitives => 0xf93ba7d4 => 113
	i32 4182413190, ; 702: Xamarin.AndroidX.Lifecycle.ViewModelSavedState.dll => 0xf94a8f86 => 262
	i32 4185676441, ; 703: System.Security => 0xf97c5a99 => 130
	i32 4192648326, ; 704: Xamarin.Firebase.Encoders.JSON.dll => 0xf9e6bc86 => 292
	i32 4196529839, ; 705: System.Net.WebClient.dll => 0xfa21f6af => 76
	i32 4213026141, ; 706: System.Diagnostics.DiagnosticSource.dll => 0xfb1dad5d => 27
	i32 4228543782, ; 707: Xamarin.KotlinX.AtomicFU.Jvm.dll => 0xfc0a7526 => 319
	i32 4256097574, ; 708: Xamarin.AndroidX.Core.Core.Ktx => 0xfdaee526 => 235
	i32 4258378803, ; 709: Xamarin.AndroidX.Lifecycle.ViewModel.Ktx => 0xfdd1b433 => 261
	i32 4260525087, ; 710: System.Buffers => 0xfdf2741f => 7
	i32 4271975918, ; 711: Microsoft.Maui.Controls.dll => 0xfea12dee => 198
	i32 4274976490, ; 712: System.Runtime.Numerics => 0xfecef6ea => 110
	i32 4284549794, ; 713: Xamarin.Firebase.Components => 0xff610aa2 => 290
	i32 4292120959, ; 714: Xamarin.AndroidX.Lifecycle.ViewModelSavedState => 0xffd4917f => 262
	i32 4294763496 ; 715: Xamarin.AndroidX.ExifInterface.dll => 0xfffce3e8 => 244
], align 4

@assembly_image_cache_indices = dso_local local_unnamed_addr constant [716 x i32] [
	i32 68, ; 0
	i32 67, ; 1
	i32 108, ; 2
	i32 308, ; 3
	i32 203, ; 4
	i32 298, ; 5
	i32 255, ; 6
	i32 303, ; 7
	i32 48, ; 8
	i32 80, ; 9
	i32 222, ; 10
	i32 145, ; 11
	i32 319, ; 12
	i32 230, ; 13
	i32 30, ; 14
	i32 356, ; 15
	i32 124, ; 16
	i32 202, ; 17
	i32 102, ; 18
	i32 276, ; 19
	i32 289, ; 20
	i32 107, ; 21
	i32 276, ; 22
	i32 139, ; 23
	i32 316, ; 24
	i32 77, ; 25
	i32 124, ; 26
	i32 13, ; 27
	i32 226, ; 28
	i32 318, ; 29
	i32 132, ; 30
	i32 278, ; 31
	i32 151, ; 32
	i32 353, ; 33
	i32 354, ; 34
	i32 18, ; 35
	i32 219, ; 36
	i32 280, ; 37
	i32 26, ; 38
	i32 176, ; 39
	i32 248, ; 40
	i32 1, ; 41
	i32 59, ; 42
	i32 42, ; 43
	i32 91, ; 44
	i32 231, ; 45
	i32 147, ; 46
	i32 251, ; 47
	i32 247, ; 48
	i32 325, ; 49
	i32 54, ; 50
	i32 69, ; 51
	i32 353, ; 52
	i32 210, ; 53
	i32 83, ; 54
	i32 338, ; 55
	i32 249, ; 56
	i32 177, ; 57
	i32 337, ; 58
	i32 131, ; 59
	i32 55, ; 60
	i32 149, ; 61
	i32 74, ; 62
	i32 145, ; 63
	i32 62, ; 64
	i32 146, ; 65
	i32 357, ; 66
	i32 165, ; 67
	i32 256, ; 68
	i32 349, ; 69
	i32 232, ; 70
	i32 12, ; 71
	i32 245, ; 72
	i32 125, ; 73
	i32 152, ; 74
	i32 180, ; 75
	i32 113, ; 76
	i32 166, ; 77
	i32 164, ; 78
	i32 247, ; 79
	i32 296, ; 80
	i32 264, ; 81
	i32 296, ; 82
	i32 84, ; 83
	i32 336, ; 84
	i32 330, ; 85
	i32 294, ; 86
	i32 196, ; 87
	i32 307, ; 88
	i32 150, ; 89
	i32 316, ; 90
	i32 60, ; 91
	i32 192, ; 92
	i32 51, ; 93
	i32 103, ; 94
	i32 114, ; 95
	i32 40, ; 96
	i32 300, ; 97
	i32 288, ; 98
	i32 120, ; 99
	i32 344, ; 100
	i32 52, ; 101
	i32 44, ; 102
	i32 119, ; 103
	i32 256, ; 104
	i32 237, ; 105
	i32 342, ; 106
	i32 243, ; 107
	i32 81, ; 108
	i32 136, ; 109
	i32 284, ; 110
	i32 217, ; 111
	i32 8, ; 112
	i32 318, ; 113
	i32 73, ; 114
	i32 324, ; 115
	i32 155, ; 116
	i32 320, ; 117
	i32 154, ; 118
	i32 92, ; 119
	i32 314, ; 120
	i32 45, ; 121
	i32 339, ; 122
	i32 327, ; 123
	i32 317, ; 124
	i32 109, ; 125
	i32 129, ; 126
	i32 25, ; 127
	i32 207, ; 128
	i32 72, ; 129
	i32 55, ; 130
	i32 46, ; 131
	i32 348, ; 132
	i32 299, ; 133
	i32 195, ; 134
	i32 238, ; 135
	i32 22, ; 136
	i32 253, ; 137
	i32 86, ; 138
	i32 43, ; 139
	i32 160, ; 140
	i32 181, ; 141
	i32 71, ; 142
	i32 269, ; 143
	i32 3, ; 144
	i32 42, ; 145
	i32 63, ; 146
	i32 16, ; 147
	i32 53, ; 148
	i32 351, ; 149
	i32 303, ; 150
	i32 105, ; 151
	i32 317, ; 152
	i32 301, ; 153
	i32 249, ; 154
	i32 34, ; 155
	i32 158, ; 156
	i32 85, ; 157
	i32 32, ; 158
	i32 12, ; 159
	i32 51, ; 160
	i32 295, ; 161
	i32 56, ; 162
	i32 273, ; 163
	i32 36, ; 164
	i32 190, ; 165
	i32 326, ; 166
	i32 302, ; 167
	i32 215, ; 168
	i32 35, ; 169
	i32 58, ; 170
	i32 259, ; 171
	i32 177, ; 172
	i32 299, ; 173
	i32 174, ; 174
	i32 17, ; 175
	i32 315, ; 176
	i32 164, ; 177
	i32 339, ; 178
	i32 257, ; 179
	i32 298, ; 180
	i32 194, ; 181
	i32 287, ; 182
	i32 183, ; 183
	i32 345, ; 184
	i32 153, ; 185
	i32 283, ; 186
	i32 267, ; 187
	i32 183, ; 188
	i32 343, ; 189
	i32 306, ; 190
	i32 217, ; 191
	i32 186, ; 192
	i32 29, ; 193
	i32 52, ; 194
	i32 179, ; 195
	i32 341, ; 196
	i32 288, ; 197
	i32 227, ; 198
	i32 5, ; 199
	i32 325, ; 200
	i32 277, ; 201
	i32 321, ; 202
	i32 282, ; 203
	i32 228, ; 204
	i32 320, ; 205
	i32 214, ; 206
	i32 240, ; 207
	i32 85, ; 208
	i32 287, ; 209
	i32 61, ; 210
	i32 112, ; 211
	i32 311, ; 212
	i32 305, ; 213
	i32 304, ; 214
	i32 57, ; 215
	i32 355, ; 216
	i32 273, ; 217
	i32 99, ; 218
	i32 313, ; 219
	i32 19, ; 220
	i32 233, ; 221
	i32 111, ; 222
	i32 101, ; 223
	i32 175, ; 224
	i32 102, ; 225
	i32 323, ; 226
	i32 104, ; 227
	i32 301, ; 228
	i32 250, ; 229
	i32 71, ; 230
	i32 260, ; 231
	i32 38, ; 232
	i32 32, ; 233
	i32 103, ; 234
	i32 73, ; 235
	i32 329, ; 236
	i32 9, ; 237
	i32 123, ; 238
	i32 46, ; 239
	i32 216, ; 240
	i32 196, ; 241
	i32 9, ; 242
	i32 43, ; 243
	i32 4, ; 244
	i32 274, ; 245
	i32 333, ; 246
	i32 328, ; 247
	i32 31, ; 248
	i32 138, ; 249
	i32 92, ; 250
	i32 93, ; 251
	i32 348, ; 252
	i32 49, ; 253
	i32 141, ; 254
	i32 112, ; 255
	i32 140, ; 256
	i32 239, ; 257
	i32 115, ; 258
	i32 302, ; 259
	i32 157, ; 260
	i32 76, ; 261
	i32 79, ; 262
	i32 263, ; 263
	i32 37, ; 264
	i32 286, ; 265
	i32 243, ; 266
	i32 236, ; 267
	i32 64, ; 268
	i32 138, ; 269
	i32 15, ; 270
	i32 116, ; 271
	i32 279, ; 272
	i32 297, ; 273
	i32 231, ; 274
	i32 48, ; 275
	i32 70, ; 276
	i32 80, ; 277
	i32 126, ; 278
	i32 182, ; 279
	i32 94, ; 280
	i32 121, ; 281
	i32 26, ; 282
	i32 311, ; 283
	i32 253, ; 284
	i32 97, ; 285
	i32 28, ; 286
	i32 225, ; 287
	i32 346, ; 288
	i32 324, ; 289
	i32 149, ; 290
	i32 205, ; 291
	i32 169, ; 292
	i32 4, ; 293
	i32 98, ; 294
	i32 33, ; 295
	i32 93, ; 296
	i32 278, ; 297
	i32 192, ; 298
	i32 0, ; 299
	i32 21, ; 300
	i32 41, ; 301
	i32 170, ; 302
	i32 340, ; 303
	i32 245, ; 304
	i32 332, ; 305
	i32 263, ; 306
	i32 315, ; 307
	i32 297, ; 308
	i32 268, ; 309
	i32 2, ; 310
	i32 134, ; 311
	i32 111, ; 312
	i32 193, ; 313
	i32 352, ; 314
	i32 207, ; 315
	i32 349, ; 316
	i32 58, ; 317
	i32 223, ; 318
	i32 95, ; 319
	i32 331, ; 320
	i32 293, ; 321
	i32 39, ; 322
	i32 218, ; 323
	i32 25, ; 324
	i32 94, ; 325
	i32 89, ; 326
	i32 99, ; 327
	i32 310, ; 328
	i32 10, ; 329
	i32 87, ; 330
	i32 179, ; 331
	i32 100, ; 332
	i32 275, ; 333
	i32 180, ; 334
	i32 187, ; 335
	i32 209, ; 336
	i32 328, ; 337
	i32 7, ; 338
	i32 259, ; 339
	i32 323, ; 340
	i32 206, ; 341
	i32 88, ; 342
	i32 252, ; 343
	i32 154, ; 344
	i32 327, ; 345
	i32 33, ; 346
	i32 116, ; 347
	i32 82, ; 348
	i32 173, ; 349
	i32 295, ; 350
	i32 20, ; 351
	i32 309, ; 352
	i32 11, ; 353
	i32 162, ; 354
	i32 3, ; 355
	i32 200, ; 356
	i32 335, ; 357
	i32 289, ; 358
	i32 195, ; 359
	i32 304, ; 360
	i32 193, ; 361
	i32 84, ; 362
	i32 322, ; 363
	i32 64, ; 364
	i32 337, ; 365
	i32 283, ; 366
	i32 143, ; 367
	i32 191, ; 368
	i32 264, ; 369
	i32 157, ; 370
	i32 182, ; 371
	i32 41, ; 372
	i32 117, ; 373
	i32 188, ; 374
	i32 208, ; 375
	i32 331, ; 376
	i32 271, ; 377
	i32 131, ; 378
	i32 203, ; 379
	i32 75, ; 380
	i32 66, ; 381
	i32 341, ; 382
	i32 172, ; 383
	i32 212, ; 384
	i32 178, ; 385
	i32 143, ; 386
	i32 204, ; 387
	i32 106, ; 388
	i32 151, ; 389
	i32 70, ; 390
	i32 156, ; 391
	i32 187, ; 392
	i32 121, ; 393
	i32 127, ; 394
	i32 336, ; 395
	i32 152, ; 396
	i32 242, ; 397
	i32 223, ; 398
	i32 141, ; 399
	i32 228, ; 400
	i32 305, ; 401
	i32 333, ; 402
	i32 20, ; 403
	i32 14, ; 404
	i32 135, ; 405
	i32 75, ; 406
	i32 59, ; 407
	i32 232, ; 408
	i32 167, ; 409
	i32 168, ; 410
	i32 198, ; 411
	i32 15, ; 412
	i32 74, ; 413
	i32 6, ; 414
	i32 23, ; 415
	i32 255, ; 416
	i32 206, ; 417
	i32 91, ; 418
	i32 334, ; 419
	i32 1, ; 420
	i32 136, ; 421
	i32 258, ; 422
	i32 257, ; 423
	i32 282, ; 424
	i32 134, ; 425
	i32 69, ; 426
	i32 146, ; 427
	i32 343, ; 428
	i32 322, ; 429
	i32 246, ; 430
	i32 194, ; 431
	i32 88, ; 432
	i32 96, ; 433
	i32 291, ; 434
	i32 236, ; 435
	i32 241, ; 436
	i32 338, ; 437
	i32 31, ; 438
	i32 45, ; 439
	i32 251, ; 440
	i32 184, ; 441
	i32 191, ; 442
	i32 291, ; 443
	i32 208, ; 444
	i32 109, ; 445
	i32 158, ; 446
	i32 35, ; 447
	i32 321, ; 448
	i32 22, ; 449
	i32 114, ; 450
	i32 57, ; 451
	i32 279, ; 452
	i32 144, ; 453
	i32 118, ; 454
	i32 120, ; 455
	i32 110, ; 456
	i32 210, ; 457
	i32 139, ; 458
	i32 216, ; 459
	i32 54, ; 460
	i32 105, ; 461
	i32 344, ; 462
	i32 199, ; 463
	i32 200, ; 464
	i32 133, ; 465
	i32 260, ; 466
	i32 314, ; 467
	i32 285, ; 468
	i32 272, ; 469
	i32 250, ; 470
	i32 350, ; 471
	i32 246, ; 472
	i32 202, ; 473
	i32 159, ; 474
	i32 290, ; 475
	i32 329, ; 476
	i32 233, ; 477
	i32 163, ; 478
	i32 132, ; 479
	i32 272, ; 480
	i32 161, ; 481
	i32 230, ; 482
	i32 342, ; 483
	i32 261, ; 484
	i32 309, ; 485
	i32 184, ; 486
	i32 140, ; 487
	i32 285, ; 488
	i32 281, ; 489
	i32 169, ; 490
	i32 201, ; 491
	i32 307, ; 492
	i32 211, ; 493
	i32 300, ; 494
	i32 40, ; 495
	i32 176, ; 496
	i32 244, ; 497
	i32 81, ; 498
	i32 56, ; 499
	i32 37, ; 500
	i32 97, ; 501
	i32 166, ; 502
	i32 172, ; 503
	i32 286, ; 504
	i32 82, ; 505
	i32 213, ; 506
	i32 98, ; 507
	i32 30, ; 508
	i32 159, ; 509
	i32 18, ; 510
	i32 224, ; 511
	i32 127, ; 512
	i32 119, ; 513
	i32 240, ; 514
	i32 275, ; 515
	i32 221, ; 516
	i32 254, ; 517
	i32 224, ; 518
	i32 277, ; 519
	i32 165, ; 520
	i32 248, ; 521
	i32 357, ; 522
	i32 221, ; 523
	i32 274, ; 524
	i32 265, ; 525
	i32 312, ; 526
	i32 170, ; 527
	i32 16, ; 528
	i32 185, ; 529
	i32 144, ; 530
	i32 335, ; 531
	i32 125, ; 532
	i32 118, ; 533
	i32 38, ; 534
	i32 115, ; 535
	i32 47, ; 536
	i32 142, ; 537
	i32 117, ; 538
	i32 34, ; 539
	i32 174, ; 540
	i32 294, ; 541
	i32 95, ; 542
	i32 53, ; 543
	i32 266, ; 544
	i32 129, ; 545
	i32 153, ; 546
	i32 185, ; 547
	i32 24, ; 548
	i32 161, ; 549
	i32 239, ; 550
	i32 148, ; 551
	i32 104, ; 552
	i32 310, ; 553
	i32 89, ; 554
	i32 225, ; 555
	i32 60, ; 556
	i32 142, ; 557
	i32 100, ; 558
	i32 5, ; 559
	i32 13, ; 560
	i32 122, ; 561
	i32 135, ; 562
	i32 28, ; 563
	i32 330, ; 564
	i32 72, ; 565
	i32 237, ; 566
	i32 24, ; 567
	i32 218, ; 568
	i32 270, ; 569
	i32 267, ; 570
	i32 347, ; 571
	i32 137, ; 572
	i32 280, ; 573
	i32 211, ; 574
	i32 234, ; 575
	i32 168, ; 576
	i32 293, ; 577
	i32 271, ; 578
	i32 326, ; 579
	i32 292, ; 580
	i32 101, ; 581
	i32 123, ; 582
	i32 238, ; 583
	i32 308, ; 584
	i32 220, ; 585
	i32 220, ; 586
	i32 189, ; 587
	i32 163, ; 588
	i32 167, ; 589
	i32 241, ; 590
	i32 39, ; 591
	i32 197, ; 592
	i32 334, ; 593
	i32 178, ; 594
	i32 17, ; 595
	i32 171, ; 596
	i32 347, ; 597
	i32 346, ; 598
	i32 137, ; 599
	i32 150, ; 600
	i32 229, ; 601
	i32 155, ; 602
	i32 130, ; 603
	i32 19, ; 604
	i32 65, ; 605
	i32 147, ; 606
	i32 47, ; 607
	i32 354, ; 608
	i32 209, ; 609
	i32 79, ; 610
	i32 61, ; 611
	i32 106, ; 612
	i32 306, ; 613
	i32 269, ; 614
	i32 213, ; 615
	i32 49, ; 616
	i32 252, ; 617
	i32 351, ; 618
	i32 266, ; 619
	i32 14, ; 620
	i32 188, ; 621
	i32 68, ; 622
	i32 171, ; 623
	i32 222, ; 624
	i32 219, ; 625
	i32 229, ; 626
	i32 181, ; 627
	i32 356, ; 628
	i32 78, ; 629
	i32 235, ; 630
	i32 108, ; 631
	i32 212, ; 632
	i32 265, ; 633
	i32 67, ; 634
	i32 63, ; 635
	i32 27, ; 636
	i32 160, ; 637
	i32 258, ; 638
	i32 226, ; 639
	i32 175, ; 640
	i32 10, ; 641
	i32 197, ; 642
	i32 11, ; 643
	i32 78, ; 644
	i32 126, ; 645
	i32 83, ; 646
	i32 190, ; 647
	i32 66, ; 648
	i32 107, ; 649
	i32 65, ; 650
	i32 128, ; 651
	i32 122, ; 652
	i32 77, ; 653
	i32 281, ; 654
	i32 270, ; 655
	i32 355, ; 656
	i32 8, ; 657
	i32 234, ; 658
	i32 2, ; 659
	i32 227, ; 660
	i32 44, ; 661
	i32 284, ; 662
	i32 156, ; 663
	i32 128, ; 664
	i32 268, ; 665
	i32 313, ; 666
	i32 23, ; 667
	i32 133, ; 668
	i32 215, ; 669
	i32 173, ; 670
	i32 254, ; 671
	i32 312, ; 672
	i32 350, ; 673
	i32 332, ; 674
	i32 29, ; 675
	i32 214, ; 676
	i32 205, ; 677
	i32 62, ; 678
	i32 0, ; 679
	i32 199, ; 680
	i32 90, ; 681
	i32 87, ; 682
	i32 148, ; 683
	i32 201, ; 684
	i32 36, ; 685
	i32 86, ; 686
	i32 204, ; 687
	i32 242, ; 688
	i32 186, ; 689
	i32 345, ; 690
	i32 340, ; 691
	i32 189, ; 692
	i32 50, ; 693
	i32 6, ; 694
	i32 90, ; 695
	i32 352, ; 696
	i32 21, ; 697
	i32 162, ; 698
	i32 96, ; 699
	i32 50, ; 700
	i32 113, ; 701
	i32 262, ; 702
	i32 130, ; 703
	i32 292, ; 704
	i32 76, ; 705
	i32 27, ; 706
	i32 319, ; 707
	i32 235, ; 708
	i32 261, ; 709
	i32 7, ; 710
	i32 198, ; 711
	i32 110, ; 712
	i32 290, ; 713
	i32 262, ; 714
	i32 244 ; 715
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

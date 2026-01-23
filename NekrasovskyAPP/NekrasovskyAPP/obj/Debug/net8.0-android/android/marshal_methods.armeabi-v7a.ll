; ModuleID = 'marshal_methods.armeabi-v7a.ll'
source_filename = "marshal_methods.armeabi-v7a.ll"
target datalayout = "e-m:e-p:32:32-Fi8-i64:64-v128:64:128-a:0:32-n32-S64"
target triple = "armv7-unknown-linux-android21"

%struct.MarshalMethodName = type {
	i64, ; uint64_t id
	ptr ; char* name
}

%struct.MarshalMethodsManagedClass = type {
	i32, ; uint32_t token
	ptr ; MonoClass klass
}

@assembly_image_cache = dso_local local_unnamed_addr global [362 x ptr] zeroinitializer, align 4

; Each entry maps hash of an assembly name to an index into the `assembly_image_cache` array
@assembly_image_cache_hashes = dso_local local_unnamed_addr constant [718 x i32] [
	i32 2616222, ; 0: System.Net.NetworkInformation.dll => 0x27eb9e => 68
	i32 10166715, ; 1: System.Net.NameResolution.dll => 0x9b21bb => 67
	i32 15721112, ; 2: System.Runtime.Intrinsics.dll => 0xefe298 => 108
	i32 20206211, ; 3: Xamarin.Google.MLKit.Vision.Interfaces.dll => 0x1345283 => 309
	i32 28873261, ; 4: Npgsql.dll => 0x1b8922d => 204
	i32 30793855, ; 5: Xamarin.Google.Android.ODML.Image.dll => 0x1d5e07f => 299
	i32 32687329, ; 6: Xamarin.AndroidX.Lifecycle.Runtime => 0x1f2c4e1 => 256
	i32 34715100, ; 7: Xamarin.Google.Guava.ListenableFuture.dll => 0x211b5dc => 304
	i32 34839235, ; 8: System.IO.FileSystem.DriveInfo => 0x2139ac3 => 48
	i32 39485524, ; 9: System.Net.WebSockets.dll => 0x25a8054 => 80
	i32 40744412, ; 10: Xamarin.AndroidX.Camera.Lifecycle.dll => 0x26db5dc => 223
	i32 42639949, ; 11: System.Threading.Thread => 0x28aa24d => 145
	i32 45981941, ; 12: Xamarin.KotlinX.AtomicFU.Jvm => 0x2bda0f5 => 320
	i32 52581868, ; 13: Xamarin.AndroidX.Concurrent.Futures.Ktx => 0x32255ec => 231
	i32 66541672, ; 14: System.Diagnostics.StackTrace => 0x3f75868 => 30
	i32 67008169, ; 15: zh-Hant\Microsoft.Maui.Controls.resources => 0x3fe76a9 => 357
	i32 68219467, ; 16: System.Security.Cryptography.Primitives => 0x410f24b => 124
	i32 72070932, ; 17: Microsoft.Maui.Graphics.dll => 0x44bb714 => 203
	i32 82292897, ; 18: System.Runtime.CompilerServices.VisualC.dll => 0x4e7b0a1 => 102
	i32 101534019, ; 19: Xamarin.AndroidX.SlidingPaneLayout => 0x60d4943 => 277
	i32 103834273, ; 20: Xamarin.Firebase.Annotations.dll => 0x63062a1 => 290
	i32 117431740, ; 21: System.Runtime.InteropServices => 0x6ffddbc => 107
	i32 120558881, ; 22: Xamarin.AndroidX.SlidingPaneLayout.dll => 0x72f9521 => 277
	i32 122350210, ; 23: System.Threading.Channels.dll => 0x74aea82 => 139
	i32 134690465, ; 24: Xamarin.Kotlin.StdLib.Jdk7.dll => 0x80736a1 => 317
	i32 142721839, ; 25: System.Net.WebHeaderCollection => 0x881c32f => 77
	i32 149972175, ; 26: System.Security.Cryptography.Primitives.dll => 0x8f064cf => 124
	i32 159306688, ; 27: System.ComponentModel.Annotations => 0x97ed3c0 => 13
	i32 165246403, ; 28: Xamarin.AndroidX.Collection.dll => 0x9d975c3 => 227
	i32 166070894, ; 29: Xamarin.KotlinX.AtomicFU.dll => 0x9e60a6e => 319
	i32 176265551, ; 30: System.ServiceProcess => 0xa81994f => 132
	i32 182336117, ; 31: Xamarin.AndroidX.SwipeRefreshLayout.dll => 0xade3a75 => 279
	i32 184328833, ; 32: System.ValueTuple.dll => 0xafca281 => 151
	i32 195452805, ; 33: vi/Microsoft.Maui.Controls.resources.dll => 0xba65f85 => 354
	i32 199333315, ; 34: zh-HK/Microsoft.Maui.Controls.resources.dll => 0xbe195c3 => 355
	i32 205061960, ; 35: System.ComponentModel => 0xc38ff48 => 18
	i32 209399409, ; 36: Xamarin.AndroidX.Browser.dll => 0xc7b2e71 => 220
	i32 218154787, ; 37: Xamarin.AndroidX.Tracing.Tracing.Ktx => 0xd00c723 => 281
	i32 220171995, ; 38: System.Diagnostics.Debug => 0xd1f8edb => 26
	i32 221063263, ; 39: Microsoft.AspNetCore.Http.Connections.Client => 0xd2d285f => 177
	i32 230216969, ; 40: Xamarin.AndroidX.Legacy.Support.Core.Utils.dll => 0xdb8d509 => 249
	i32 230752869, ; 41: Microsoft.CSharp.dll => 0xdc10265 => 1
	i32 231409092, ; 42: System.Linq.Parallel => 0xdcb05c4 => 59
	i32 231814094, ; 43: System.Globalization => 0xdd133ce => 42
	i32 246610117, ; 44: System.Reflection.Emit.Lightweight => 0xeb2f8c5 => 91
	i32 261689757, ; 45: Xamarin.AndroidX.ConstraintLayout.dll => 0xf99119d => 232
	i32 276479776, ; 46: System.Threading.Timer.dll => 0x107abf20 => 147
	i32 278686392, ; 47: Xamarin.AndroidX.Lifecycle.LiveData.dll => 0x109c6ab8 => 252
	i32 280482487, ; 48: Xamarin.AndroidX.Interpolator => 0x10b7d2b7 => 248
	i32 280992041, ; 49: cs/Microsoft.Maui.Controls.resources.dll => 0x10bf9929 => 326
	i32 291076382, ; 50: System.IO.Pipes.AccessControl.dll => 0x1159791e => 54
	i32 298918909, ; 51: System.Net.Ping.dll => 0x11d123fd => 69
	i32 317674968, ; 52: vi\Microsoft.Maui.Controls.resources => 0x12ef55d8 => 354
	i32 318968648, ; 53: Xamarin.AndroidX.Activity.dll => 0x13031348 => 211
	i32 321597661, ; 54: System.Numerics => 0x132b30dd => 83
	i32 336156722, ; 55: ja/Microsoft.Maui.Controls.resources.dll => 0x14095832 => 339
	i32 342366114, ; 56: Xamarin.AndroidX.Lifecycle.Common => 0x146817a2 => 250
	i32 348048101, ; 57: Microsoft.AspNetCore.Http.Connections.Common.dll => 0x14becae5 => 178
	i32 356389973, ; 58: it/Microsoft.Maui.Controls.resources.dll => 0x153e1455 => 338
	i32 360082299, ; 59: System.ServiceModel.Web => 0x15766b7b => 131
	i32 367780167, ; 60: System.IO.Pipes => 0x15ebe147 => 55
	i32 374914964, ; 61: System.Transactions.Local => 0x1658bf94 => 149
	i32 375677976, ; 62: System.Net.ServicePoint.dll => 0x16646418 => 74
	i32 379916513, ; 63: System.Threading.Thread.dll => 0x16a510e1 => 145
	i32 385762202, ; 64: System.Memory.dll => 0x16fe439a => 62
	i32 392610295, ; 65: System.Threading.ThreadPool.dll => 0x1766c1f7 => 146
	i32 395744057, ; 66: _Microsoft.Android.Resource.Designer => 0x17969339 => 358
	i32 403441872, ; 67: WindowsBase => 0x180c08d0 => 165
	i32 425531652, ; 68: Xamarin.AndroidX.Lifecycle.Runtime.Android => 0x195d1904 => 257
	i32 435591531, ; 69: sv/Microsoft.Maui.Controls.resources.dll => 0x19f6996b => 350
	i32 441335492, ; 70: Xamarin.AndroidX.ConstraintLayout.Core => 0x1a4e3ec4 => 233
	i32 442565967, ; 71: System.Collections => 0x1a61054f => 12
	i32 450948140, ; 72: Xamarin.AndroidX.Fragment.dll => 0x1ae0ec2c => 246
	i32 451504562, ; 73: System.Security.Cryptography.X509Certificates => 0x1ae969b2 => 125
	i32 456227837, ; 74: System.Web.HttpUtility.dll => 0x1b317bfd => 152
	i32 458494020, ; 75: Microsoft.AspNetCore.SignalR.Common.dll => 0x1b541044 => 181
	i32 459347974, ; 76: System.Runtime.Serialization.Primitives.dll => 0x1b611806 => 113
	i32 465846621, ; 77: mscorlib => 0x1bc4415d => 166
	i32 469710990, ; 78: System.dll => 0x1bff388e => 164
	i32 476646585, ; 79: Xamarin.AndroidX.Interpolator.dll => 0x1c690cb9 => 248
	i32 485140951, ; 80: Xamarin.Google.Android.DataTransport.TransportRuntime => 0x1ceaa9d7 => 297
	i32 486930444, ; 81: Xamarin.AndroidX.LocalBroadcastManager.dll => 0x1d05f80c => 265
	i32 495452658, ; 82: Xamarin.Google.Android.DataTransport.TransportRuntime.dll => 0x1d8801f2 => 297
	i32 498788369, ; 83: System.ObjectModel => 0x1dbae811 => 84
	i32 500358224, ; 84: id/Microsoft.Maui.Controls.resources.dll => 0x1dd2dc50 => 337
	i32 503918385, ; 85: fi/Microsoft.Maui.Controls.resources.dll => 0x1e092f31 => 331
	i32 507148113, ; 86: Xamarin.Google.Android.DataTransport.TransportApi.dll => 0x1e3a7751 => 295
	i32 513247710, ; 87: Microsoft.Extensions.Primitives.dll => 0x1e9789de => 197
	i32 513617146, ; 88: Xamarin.Google.MLKit.Vision.Common => 0x1e9d2cfa => 308
	i32 526420162, ; 89: System.Transactions.dll => 0x1f6088c2 => 150
	i32 527452488, ; 90: Xamarin.Kotlin.StdLib.Jdk7 => 0x1f704948 => 317
	i32 530272170, ; 91: System.Linq.Queryable => 0x1f9b4faa => 60
	i32 539058512, ; 92: Microsoft.Extensions.Logging => 0x20216150 => 193
	i32 540030774, ; 93: System.IO.FileSystem.dll => 0x20303736 => 51
	i32 545304856, ; 94: System.Runtime.Extensions => 0x2080b118 => 103
	i32 546455878, ; 95: System.Runtime.Serialization.Xml => 0x20924146 => 114
	i32 549171840, ; 96: System.Globalization.Calendars => 0x20bbb280 => 40
	i32 557405415, ; 97: Jsr305Binding => 0x213954e7 => 301
	i32 569601784, ; 98: Xamarin.AndroidX.Window.Extensions.Core.Core => 0x21f36ef8 => 289
	i32 577335427, ; 99: System.Security.Cryptography.Cng => 0x22697083 => 120
	i32 592146354, ; 100: pt-BR/Microsoft.Maui.Controls.resources.dll => 0x234b6fb2 => 345
	i32 601371474, ; 101: System.IO.IsolatedStorage.dll => 0x23d83352 => 52
	i32 605376203, ; 102: System.IO.Compression.FileSystem => 0x24154ecb => 44
	i32 613668793, ; 103: System.Security.Cryptography.Algorithms => 0x2493d7b9 => 119
	i32 621990341, ; 104: Xamarin.AndroidX.Lifecycle.Runtime.Android.dll => 0x2512d1c5 => 257
	i32 627609679, ; 105: Xamarin.AndroidX.CustomView => 0x2568904f => 238
	i32 627931235, ; 106: nl\Microsoft.Maui.Controls.resources => 0x256d7863 => 343
	i32 639843206, ; 107: Xamarin.AndroidX.Emoji2.ViewsHelper.dll => 0x26233b86 => 244
	i32 643868501, ; 108: System.Net => 0x2660a755 => 81
	i32 662205335, ; 109: System.Text.Encodings.Web.dll => 0x27787397 => 136
	i32 663517072, ; 110: Xamarin.AndroidX.VersionedParcelable => 0x278c7790 => 285
	i32 666292255, ; 111: Xamarin.AndroidX.Arch.Core.Common.dll => 0x27b6d01f => 218
	i32 672442732, ; 112: System.Collections.Concurrent => 0x2814a96c => 8
	i32 679221896, ; 113: Xamarin.KotlinX.AtomicFU => 0x287c1a88 => 319
	i32 683518922, ; 114: System.Net.Security => 0x28bdabca => 73
	i32 688181140, ; 115: ca/Microsoft.Maui.Controls.resources.dll => 0x2904cf94 => 325
	i32 690569205, ; 116: System.Xml.Linq.dll => 0x29293ff5 => 155
	i32 691348768, ; 117: Xamarin.KotlinX.Coroutines.Android.dll => 0x29352520 => 321
	i32 693804605, ; 118: System.Windows => 0x295a9e3d => 154
	i32 699345723, ; 119: System.Reflection.Emit => 0x29af2b3b => 92
	i32 700284507, ; 120: Xamarin.Jetbrains.Annotations => 0x29bd7e5b => 315
	i32 700358131, ; 121: System.IO.Compression.ZipFile => 0x29be9df3 => 45
	i32 706645707, ; 122: ko/Microsoft.Maui.Controls.resources.dll => 0x2a1e8ecb => 340
	i32 709557578, ; 123: de/Microsoft.Maui.Controls.resources.dll => 0x2a4afd4a => 328
	i32 720511267, ; 124: Xamarin.Kotlin.StdLib.Jdk8 => 0x2af22123 => 318
	i32 722857257, ; 125: System.Runtime.Loader.dll => 0x2b15ed29 => 109
	i32 735137430, ; 126: System.Security.SecureString.dll => 0x2bd14e96 => 129
	i32 752232764, ; 127: System.Diagnostics.Contracts.dll => 0x2cd6293c => 25
	i32 755313932, ; 128: Xamarin.Android.Glide.Annotations.dll => 0x2d052d0c => 208
	i32 759454413, ; 129: System.Net.Requests => 0x2d445acd => 72
	i32 762598435, ; 130: System.IO.Pipes.dll => 0x2d745423 => 55
	i32 775507847, ; 131: System.IO.Compression => 0x2e394f87 => 46
	i32 777317022, ; 132: sk\Microsoft.Maui.Controls.resources => 0x2e54ea9e => 349
	i32 782533833, ; 133: Xamarin.Google.AutoValue.Annotations.dll => 0x2ea484c9 => 300
	i32 789151979, ; 134: Microsoft.Extensions.Options => 0x2f0980eb => 196
	i32 790371945, ; 135: Xamarin.AndroidX.CustomView.PoolingContainer.dll => 0x2f1c1e69 => 239
	i32 804715423, ; 136: System.Data.Common => 0x2ff6fb9f => 22
	i32 807930345, ; 137: Xamarin.AndroidX.Lifecycle.LiveData.Core.Ktx.dll => 0x302809e9 => 254
	i32 823281589, ; 138: System.Private.Uri.dll => 0x311247b5 => 86
	i32 830298997, ; 139: System.IO.Compression.Brotli => 0x317d5b75 => 43
	i32 832635846, ; 140: System.Xml.XPath.dll => 0x31a103c6 => 160
	i32 832711436, ; 141: Microsoft.AspNetCore.SignalR.Protocols.Json.dll => 0x31a22b0c => 182
	i32 834051424, ; 142: System.Net.Quic => 0x31b69d60 => 71
	i32 843511501, ; 143: Xamarin.AndroidX.Print => 0x3246f6cd => 270
	i32 873119928, ; 144: Microsoft.VisualBasic => 0x340ac0b8 => 3
	i32 877678880, ; 145: System.Globalization.dll => 0x34505120 => 42
	i32 878954865, ; 146: System.Net.Http.Json => 0x3463c971 => 63
	i32 904024072, ; 147: System.ComponentModel.Primitives.dll => 0x35e25008 => 16
	i32 911108515, ; 148: System.IO.MemoryMappedFiles.dll => 0x364e69a3 => 53
	i32 926902833, ; 149: tr/Microsoft.Maui.Controls.resources.dll => 0x373f6a31 => 352
	i32 928116545, ; 150: Xamarin.Google.Guava.ListenableFuture => 0x3751ef41 => 304
	i32 952186615, ; 151: System.Runtime.InteropServices.JavaScript.dll => 0x38c136f7 => 105
	i32 956575887, ; 152: Xamarin.Kotlin.StdLib.Jdk8.dll => 0x3904308f => 318
	i32 966729478, ; 153: Xamarin.Google.Crypto.Tink.Android => 0x399f1f06 => 302
	i32 967690846, ; 154: Xamarin.AndroidX.Lifecycle.Common.dll => 0x39adca5e => 250
	i32 975236339, ; 155: System.Diagnostics.Tracing => 0x3a20ecf3 => 34
	i32 975874589, ; 156: System.Xml.XDocument => 0x3a2aaa1d => 158
	i32 986514023, ; 157: System.Private.DataContractSerialization.dll => 0x3acd0267 => 85
	i32 987214855, ; 158: System.Diagnostics.Tools => 0x3ad7b407 => 32
	i32 992768348, ; 159: System.Collections.dll => 0x3b2c715c => 12
	i32 994442037, ; 160: System.IO.FileSystem => 0x3b45fb35 => 51
	i32 996733531, ; 161: Xamarin.Google.Android.DataTransport.TransportBackendCct => 0x3b68f25b => 296
	i32 1001831731, ; 162: System.IO.UnmanagedMemoryStream.dll => 0x3bb6bd33 => 56
	i32 1012816738, ; 163: Xamarin.AndroidX.SavedState.dll => 0x3c5e5b62 => 274
	i32 1019214401, ; 164: System.Drawing => 0x3cbffa41 => 36
	i32 1028951442, ; 165: Microsoft.Extensions.DependencyInjection.Abstractions => 0x3d548d92 => 191
	i32 1029334545, ; 166: da/Microsoft.Maui.Controls.resources.dll => 0x3d5a6611 => 327
	i32 1031528504, ; 167: Xamarin.Google.ErrorProne.Annotations.dll => 0x3d7be038 => 303
	i32 1035644815, ; 168: Xamarin.AndroidX.AppCompat => 0x3dbaaf8f => 216
	i32 1036536393, ; 169: System.Drawing.Primitives.dll => 0x3dc84a49 => 35
	i32 1044663988, ; 170: System.Linq.Expressions.dll => 0x3e444eb4 => 58
	i32 1052210849, ; 171: Xamarin.AndroidX.Lifecycle.ViewModel.dll => 0x3eb776a1 => 260
	i32 1058641855, ; 172: Microsoft.AspNetCore.Http.Connections.Common => 0x3f1997bf => 178
	i32 1061503568, ; 173: Xamarin.Google.AutoValue.Annotations => 0x3f454250 => 300
	i32 1067306892, ; 174: GoogleGson => 0x3f9dcf8c => 175
	i32 1082857460, ; 175: System.ComponentModel.TypeConverter => 0x408b17f4 => 17
	i32 1084122840, ; 176: Xamarin.Kotlin.StdLib => 0x409e66d8 => 316
	i32 1098259244, ; 177: System => 0x41761b2c => 164
	i32 1118262833, ; 178: ko\Microsoft.Maui.Controls.resources => 0x42a75631 => 340
	i32 1121599056, ; 179: Xamarin.AndroidX.Lifecycle.Runtime.Ktx.dll => 0x42da3e50 => 258
	i32 1122050967, ; 180: Xamarin.Google.Android.ODML.Image => 0x42e12397 => 299
	i32 1127624469, ; 181: Microsoft.Extensions.Logging.Debug => 0x43362f15 => 195
	i32 1149092582, ; 182: Xamarin.AndroidX.Window => 0x447dc2e6 => 288
	i32 1157931901, ; 183: Microsoft.EntityFrameworkCore.Abstractions => 0x4504a37d => 184
	i32 1168523401, ; 184: pt\Microsoft.Maui.Controls.resources => 0x45a64089 => 346
	i32 1170634674, ; 185: System.Web.dll => 0x45c677b2 => 153
	i32 1175144683, ; 186: Xamarin.AndroidX.VectorDrawable.Animated => 0x460b48eb => 284
	i32 1178241025, ; 187: Xamarin.AndroidX.Navigation.Runtime.dll => 0x463a8801 => 268
	i32 1202000627, ; 188: Microsoft.EntityFrameworkCore.Abstractions.dll => 0x47a512f3 => 184
	i32 1203215381, ; 189: pl/Microsoft.Maui.Controls.resources.dll => 0x47b79c15 => 344
	i32 1203469131, ; 190: Xamarin.Google.MLKit.Common.dll => 0x47bb7b4b => 307
	i32 1204270330, ; 191: Xamarin.AndroidX.Arch.Core.Common => 0x47c7b4fa => 218
	i32 1204575371, ; 192: Microsoft.Extensions.Caching.Memory.dll => 0x47cc5c8b => 187
	i32 1208641965, ; 193: System.Diagnostics.Process => 0x480a69ad => 29
	i32 1219128291, ; 194: System.IO.IsolatedStorage => 0x48aa6be3 => 52
	i32 1233093933, ; 195: Microsoft.AspNetCore.SignalR.Client.Core.dll => 0x497f852d => 180
	i32 1234928153, ; 196: nb/Microsoft.Maui.Controls.resources.dll => 0x499b8219 => 342
	i32 1243150071, ; 197: Xamarin.AndroidX.Window.Extensions.Core.Core.dll => 0x4a18f6f7 => 289
	i32 1246548578, ; 198: Xamarin.AndroidX.Collection.Jvm.dll => 0x4a4cd262 => 228
	i32 1253011324, ; 199: Microsoft.Win32.Registry => 0x4aaf6f7c => 5
	i32 1260983243, ; 200: cs\Microsoft.Maui.Controls.resources => 0x4b2913cb => 326
	i32 1264511973, ; 201: Xamarin.AndroidX.Startup.StartupRuntime.dll => 0x4b5eebe5 => 278
	i32 1264890200, ; 202: Xamarin.KotlinX.Coroutines.Core.dll => 0x4b64b158 => 322
	i32 1267360935, ; 203: Xamarin.AndroidX.VectorDrawable => 0x4b8a64a7 => 283
	i32 1273260888, ; 204: Xamarin.AndroidX.Collection.Ktx => 0x4be46b58 => 229
	i32 1275534314, ; 205: Xamarin.KotlinX.Coroutines.Android => 0x4c071bea => 321
	i32 1278448581, ; 206: Xamarin.AndroidX.Annotation.Jvm => 0x4c3393c5 => 215
	i32 1293217323, ; 207: Xamarin.AndroidX.DrawerLayout.dll => 0x4d14ee2b => 241
	i32 1309188875, ; 208: System.Private.DataContractSerialization => 0x4e08a30b => 85
	i32 1322716291, ; 209: Xamarin.AndroidX.Window.dll => 0x4ed70c83 => 288
	i32 1324164729, ; 210: System.Linq => 0x4eed2679 => 61
	i32 1335329327, ; 211: System.Runtime.Serialization.Json.dll => 0x4f97822f => 112
	i32 1351347447, ; 212: Xamarin.GooglePlayServices.MLKit.BarcodeScanning => 0x508becf7 => 312
	i32 1355368438, ; 213: Xamarin.Google.MLKit.BarcodeScanning.Common.dll => 0x50c947f6 => 306
	i32 1358509622, ; 214: Xamarin.Google.MLKit.BarcodeScanning => 0x50f93636 => 305
	i32 1364015309, ; 215: System.IO => 0x514d38cd => 57
	i32 1373134921, ; 216: zh-Hans\Microsoft.Maui.Controls.resources => 0x51d86049 => 356
	i32 1376866003, ; 217: Xamarin.AndroidX.SavedState => 0x52114ed3 => 274
	i32 1379779777, ; 218: System.Resources.ResourceManager => 0x523dc4c1 => 99
	i32 1379897097, ; 219: Xamarin.JavaX.Inject => 0x523f8f09 => 314
	i32 1402170036, ; 220: System.Configuration.dll => 0x53936ab4 => 19
	i32 1406073936, ; 221: Xamarin.AndroidX.CoordinatorLayout => 0x53cefc50 => 234
	i32 1408764838, ; 222: System.Runtime.Serialization.Formatters.dll => 0x53f80ba6 => 111
	i32 1411638395, ; 223: System.Runtime.CompilerServices.Unsafe => 0x5423e47b => 101
	i32 1414043276, ; 224: Microsoft.AspNetCore.Connections.Abstractions.dll => 0x5448968c => 176
	i32 1422545099, ; 225: System.Runtime.CompilerServices.VisualC => 0x54ca50cb => 102
	i32 1430672901, ; 226: ar\Microsoft.Maui.Controls.resources => 0x55465605 => 324
	i32 1434145427, ; 227: System.Runtime.Handles => 0x557b5293 => 104
	i32 1435222561, ; 228: Xamarin.Google.Crypto.Tink.Android.dll => 0x558bc221 => 302
	i32 1437299793, ; 229: Xamarin.AndroidX.Lifecycle.Common.Jvm => 0x55ab7451 => 251
	i32 1439761251, ; 230: System.Net.Quic.dll => 0x55d10363 => 71
	i32 1441095154, ; 231: Xamarin.AndroidX.Lifecycle.ViewModel.Android => 0x55e55df2 => 261
	i32 1452070440, ; 232: System.Formats.Asn1.dll => 0x568cd628 => 38
	i32 1453312822, ; 233: System.Diagnostics.Tools.dll => 0x569fcb36 => 32
	i32 1457743152, ; 234: System.Runtime.Extensions.dll => 0x56e36530 => 103
	i32 1458022317, ; 235: System.Net.Security.dll => 0x56e7a7ad => 73
	i32 1461004990, ; 236: es\Microsoft.Maui.Controls.resources => 0x57152abe => 330
	i32 1461234159, ; 237: System.Collections.Immutable.dll => 0x5718a9ef => 9
	i32 1461719063, ; 238: System.Security.Cryptography.OpenSsl => 0x57201017 => 123
	i32 1462112819, ; 239: System.IO.Compression.dll => 0x57261233 => 46
	i32 1469204771, ; 240: Xamarin.AndroidX.AppCompat.AppCompatResources => 0x57924923 => 217
	i32 1470490898, ; 241: Microsoft.Extensions.Primitives => 0x57a5e912 => 197
	i32 1479771757, ; 242: System.Collections.Immutable => 0x5833866d => 9
	i32 1480492111, ; 243: System.IO.Compression.Brotli.dll => 0x583e844f => 43
	i32 1487239319, ; 244: Microsoft.Win32.Primitives => 0x58a57897 => 4
	i32 1490025113, ; 245: Xamarin.AndroidX.SavedState.SavedState.Ktx.dll => 0x58cffa99 => 275
	i32 1493001747, ; 246: hi/Microsoft.Maui.Controls.resources.dll => 0x58fd6613 => 334
	i32 1501494919, ; 247: AppDynamics.Agent => 0x597efe87 => 173
	i32 1514721132, ; 248: el/Microsoft.Maui.Controls.resources.dll => 0x5a48cf6c => 329
	i32 1536373174, ; 249: System.Diagnostics.TextWriterTraceListener => 0x5b9331b6 => 31
	i32 1543031311, ; 250: System.Text.RegularExpressions.dll => 0x5bf8ca0f => 138
	i32 1543355203, ; 251: System.Reflection.Emit.dll => 0x5bfdbb43 => 92
	i32 1550322496, ; 252: System.Reflection.Extensions.dll => 0x5c680b40 => 93
	i32 1551623176, ; 253: sk/Microsoft.Maui.Controls.resources.dll => 0x5c7be408 => 349
	i32 1565862583, ; 254: System.IO.FileSystem.Primitives => 0x5d552ab7 => 49
	i32 1566207040, ; 255: System.Threading.Tasks.Dataflow.dll => 0x5d5a6c40 => 141
	i32 1573704789, ; 256: System.Runtime.Serialization.Json => 0x5dccd455 => 112
	i32 1580037396, ; 257: System.Threading.Overlapped => 0x5e2d7514 => 140
	i32 1582372066, ; 258: Xamarin.AndroidX.DocumentFile.dll => 0x5e5114e2 => 240
	i32 1592978981, ; 259: System.Runtime.Serialization.dll => 0x5ef2ee25 => 115
	i32 1597949149, ; 260: Xamarin.Google.ErrorProne.Annotations => 0x5f3ec4dd => 303
	i32 1601112923, ; 261: System.Xml.Serialization => 0x5f6f0b5b => 157
	i32 1604827217, ; 262: System.Net.WebClient => 0x5fa7b851 => 76
	i32 1618516317, ; 263: System.Net.WebSockets.Client.dll => 0x6078995d => 79
	i32 1622152042, ; 264: Xamarin.AndroidX.Loader.dll => 0x60b0136a => 264
	i32 1622358360, ; 265: System.Dynamic.Runtime => 0x60b33958 => 37
	i32 1624863272, ; 266: Xamarin.AndroidX.ViewPager2 => 0x60d97228 => 287
	i32 1635184631, ; 267: Xamarin.AndroidX.Emoji2.ViewsHelper => 0x6176eff7 => 244
	i32 1636350590, ; 268: Xamarin.AndroidX.CursorAdapter => 0x6188ba7e => 237
	i32 1639515021, ; 269: System.Net.Http.dll => 0x61b9038d => 64
	i32 1639986890, ; 270: System.Text.RegularExpressions => 0x61c036ca => 138
	i32 1641389582, ; 271: System.ComponentModel.EventBasedAsync.dll => 0x61d59e0e => 15
	i32 1657153582, ; 272: System.Runtime => 0x62c6282e => 116
	i32 1658241508, ; 273: Xamarin.AndroidX.Tracing.Tracing.dll => 0x62d6c1e4 => 280
	i32 1658251792, ; 274: Xamarin.Google.Android.Material.dll => 0x62d6ea10 => 298
	i32 1670060433, ; 275: Xamarin.AndroidX.ConstraintLayout => 0x638b1991 => 232
	i32 1675553242, ; 276: System.IO.FileSystem.DriveInfo.dll => 0x63dee9da => 48
	i32 1677501392, ; 277: System.Net.Primitives.dll => 0x63fca3d0 => 70
	i32 1678508291, ; 278: System.Net.WebSockets => 0x640c0103 => 80
	i32 1679769178, ; 279: System.Security.Cryptography => 0x641f3e5a => 126
	i32 1689493916, ; 280: Microsoft.EntityFrameworkCore.dll => 0x64b3a19c => 183
	i32 1691477237, ; 281: System.Reflection.Metadata => 0x64d1e4f5 => 94
	i32 1696967625, ; 282: System.Security.Cryptography.Csp => 0x6525abc9 => 121
	i32 1701541528, ; 283: System.Diagnostics.Debug.dll => 0x656b7698 => 26
	i32 1718006957, ; 284: Xamarin.GooglePlayServices.MLKit.BarcodeScanning.dll => 0x6666b4ad => 312
	i32 1720223769, ; 285: Xamarin.AndroidX.Lifecycle.LiveData.Core.Ktx => 0x66888819 => 254
	i32 1726116996, ; 286: System.Reflection.dll => 0x66e27484 => 97
	i32 1728033016, ; 287: System.Diagnostics.FileVersionInfo.dll => 0x66ffb0f8 => 28
	i32 1729485958, ; 288: Xamarin.AndroidX.CardView.dll => 0x6715dc86 => 226
	i32 1736233607, ; 289: ro/Microsoft.Maui.Controls.resources.dll => 0x677cd287 => 347
	i32 1743415430, ; 290: ca\Microsoft.Maui.Controls.resources => 0x67ea6886 => 325
	i32 1744735666, ; 291: System.Transactions.Local.dll => 0x67fe8db2 => 149
	i32 1746115085, ; 292: System.IO.Pipelines.dll => 0x68139a0d => 206
	i32 1746316138, ; 293: Mono.Android.Export => 0x6816ab6a => 169
	i32 1750313021, ; 294: Microsoft.Win32.Primitives.dll => 0x6853a83d => 4
	i32 1758240030, ; 295: System.Resources.Reader.dll => 0x68cc9d1e => 98
	i32 1763938596, ; 296: System.Diagnostics.TraceSource.dll => 0x69239124 => 33
	i32 1765942094, ; 297: System.Reflection.Extensions => 0x6942234e => 93
	i32 1766324549, ; 298: Xamarin.AndroidX.SwipeRefreshLayout => 0x6947f945 => 279
	i32 1770582343, ; 299: Microsoft.Extensions.Logging.dll => 0x6988f147 => 193
	i32 1772434258, ; 300: NekrasovskyAPP => 0x69a53352 => 0
	i32 1776026572, ; 301: System.Core.dll => 0x69dc03cc => 21
	i32 1777075843, ; 302: System.Globalization.Extensions.dll => 0x69ec0683 => 41
	i32 1780572499, ; 303: Mono.Android.Runtime.dll => 0x6a216153 => 170
	i32 1782862114, ; 304: ms\Microsoft.Maui.Controls.resources => 0x6a445122 => 341
	i32 1788241197, ; 305: Xamarin.AndroidX.Fragment => 0x6a96652d => 246
	i32 1793755602, ; 306: he\Microsoft.Maui.Controls.resources => 0x6aea89d2 => 333
	i32 1808609942, ; 307: Xamarin.AndroidX.Loader => 0x6bcd3296 => 264
	i32 1813058853, ; 308: Xamarin.Kotlin.StdLib.dll => 0x6c111525 => 316
	i32 1813201214, ; 309: Xamarin.Google.Android.Material => 0x6c13413e => 298
	i32 1818569960, ; 310: Xamarin.AndroidX.Navigation.UI.dll => 0x6c652ce8 => 269
	i32 1818787751, ; 311: Microsoft.VisualBasic.Core => 0x6c687fa7 => 2
	i32 1824175904, ; 312: System.Text.Encoding.Extensions => 0x6cbab720 => 134
	i32 1824722060, ; 313: System.Runtime.Serialization.Formatters => 0x6cc30c8c => 111
	i32 1828688058, ; 314: Microsoft.Extensions.Logging.Abstractions.dll => 0x6cff90ba => 194
	i32 1842015223, ; 315: uk/Microsoft.Maui.Controls.resources.dll => 0x6dcaebf7 => 353
	i32 1847515442, ; 316: Xamarin.Android.Glide.Annotations => 0x6e1ed932 => 208
	i32 1853025655, ; 317: sv\Microsoft.Maui.Controls.resources => 0x6e72ed77 => 350
	i32 1858542181, ; 318: System.Linq.Expressions => 0x6ec71a65 => 58
	i32 1866818530, ; 319: Xamarin.AndroidX.Camera.Video => 0x6f4563e2 => 224
	i32 1870277092, ; 320: System.Reflection.Primitives => 0x6f7a29e4 => 95
	i32 1875935024, ; 321: fr\Microsoft.Maui.Controls.resources => 0x6fd07f30 => 332
	i32 1876173635, ; 322: Xamarin.Firebase.Encoders.Proto => 0x6fd42343 => 294
	i32 1879696579, ; 323: System.Formats.Tar.dll => 0x7009e4c3 => 39
	i32 1885316902, ; 324: Xamarin.AndroidX.Arch.Core.Runtime.dll => 0x705fa726 => 219
	i32 1888955245, ; 325: System.Diagnostics.Contracts => 0x70972b6d => 25
	i32 1889954781, ; 326: System.Reflection.Metadata.dll => 0x70a66bdd => 94
	i32 1898237753, ; 327: System.Reflection.DispatchProxy => 0x7124cf39 => 89
	i32 1900610850, ; 328: System.Resources.ResourceManager.dll => 0x71490522 => 99
	i32 1908813208, ; 329: Xamarin.GooglePlayServices.Basement => 0x71c62d98 => 311
	i32 1910275211, ; 330: System.Collections.NonGeneric.dll => 0x71dc7c8b => 10
	i32 1939592360, ; 331: System.Private.Xml.Linq => 0x739bd4a8 => 87
	i32 1945717188, ; 332: Microsoft.AspNetCore.SignalR.Client.Core => 0x73f949c4 => 180
	i32 1956758971, ; 333: System.Resources.Writer => 0x74a1c5bb => 100
	i32 1961813231, ; 334: Xamarin.AndroidX.Security.SecurityCrypto.dll => 0x74eee4ef => 276
	i32 1967334205, ; 335: Microsoft.AspNetCore.SignalR.Common => 0x7543233d => 181
	i32 1968388702, ; 336: Microsoft.Extensions.Configuration.dll => 0x75533a5e => 188
	i32 1985761444, ; 337: Xamarin.Android.Glide.GifDecoder => 0x765c50a4 => 210
	i32 2003115576, ; 338: el\Microsoft.Maui.Controls.resources => 0x77651e38 => 329
	i32 2011961780, ; 339: System.Buffers.dll => 0x77ec19b4 => 7
	i32 2019465201, ; 340: Xamarin.AndroidX.Lifecycle.ViewModel => 0x785e97f1 => 260
	i32 2025202353, ; 341: ar/Microsoft.Maui.Controls.resources.dll => 0x78b622b1 => 324
	i32 2031763787, ; 342: Xamarin.Android.Glide => 0x791a414b => 207
	i32 2045470958, ; 343: System.Private.Xml => 0x79eb68ee => 88
	i32 2055257422, ; 344: Xamarin.AndroidX.Lifecycle.LiveData.Core.dll => 0x7a80bd4e => 253
	i32 2060060697, ; 345: System.Windows.dll => 0x7aca0819 => 154
	i32 2066184531, ; 346: de\Microsoft.Maui.Controls.resources => 0x7b277953 => 328
	i32 2070888862, ; 347: System.Diagnostics.TraceSource => 0x7b6f419e => 33
	i32 2079903147, ; 348: System.Runtime.dll => 0x7bf8cdab => 116
	i32 2090596640, ; 349: System.Numerics.Vectors => 0x7c9bf920 => 82
	i32 2100944304, ; 350: BarcodeScanning.Native.Maui.dll => 0x7d39ddb0 => 174
	i32 2124230737, ; 351: Xamarin.Google.Android.DataTransport.TransportBackendCct.dll => 0x7e9d3051 => 296
	i32 2127167465, ; 352: System.Console => 0x7ec9ffe9 => 20
	i32 2129483829, ; 353: Xamarin.GooglePlayServices.Base.dll => 0x7eed5835 => 310
	i32 2142473426, ; 354: System.Collections.Specialized => 0x7fb38cd2 => 11
	i32 2143790110, ; 355: System.Xml.XmlSerializer.dll => 0x7fc7a41e => 162
	i32 2146852085, ; 356: Microsoft.VisualBasic.dll => 0x7ff65cf5 => 3
	i32 2159891885, ; 357: Microsoft.Maui => 0x80bd55ad => 201
	i32 2169148018, ; 358: hu\Microsoft.Maui.Controls.resources => 0x814a9272 => 336
	i32 2174878672, ; 359: Xamarin.Firebase.Annotations => 0x81a203d0 => 290
	i32 2181898931, ; 360: Microsoft.Extensions.Options.dll => 0x820d22b3 => 196
	i32 2188559649, ; 361: Xamarin.Google.MLKit.BarcodeScanning.dll => 0x8272c521 => 305
	i32 2192057212, ; 362: Microsoft.Extensions.Logging.Abstractions => 0x82a8237c => 194
	i32 2193016926, ; 363: System.ObjectModel.dll => 0x82b6c85e => 84
	i32 2201107256, ; 364: Xamarin.KotlinX.Coroutines.Core.Jvm.dll => 0x83323b38 => 323
	i32 2201231467, ; 365: System.Net.Http => 0x8334206b => 64
	i32 2207618523, ; 366: it\Microsoft.Maui.Controls.resources => 0x839595db => 338
	i32 2217644978, ; 367: Xamarin.AndroidX.VectorDrawable.Animated.dll => 0x842e93b2 => 284
	i32 2222056684, ; 368: System.Threading.Tasks.Parallel => 0x8471e4ec => 143
	i32 2229158877, ; 369: Microsoft.Extensions.Features.dll => 0x84de43dd => 192
	i32 2244775296, ; 370: Xamarin.AndroidX.LocalBroadcastManager => 0x85cc8d80 => 265
	i32 2252106437, ; 371: System.Xml.Serialization.dll => 0x863c6ac5 => 157
	i32 2252897993, ; 372: Microsoft.EntityFrameworkCore => 0x86487ec9 => 183
	i32 2256313426, ; 373: System.Globalization.Extensions => 0x867c9c52 => 41
	i32 2265110946, ; 374: System.Security.AccessControl.dll => 0x8702d9a2 => 117
	i32 2266799131, ; 375: Microsoft.Extensions.Configuration.Abstractions => 0x871c9c1b => 189
	i32 2267999099, ; 376: Xamarin.Android.Glide.DiskLruCache.dll => 0x872eeb7b => 209
	i32 2270573516, ; 377: fr/Microsoft.Maui.Controls.resources.dll => 0x875633cc => 332
	i32 2279755925, ; 378: Xamarin.AndroidX.RecyclerView.dll => 0x87e25095 => 272
	i32 2293034957, ; 379: System.ServiceModel.Web.dll => 0x88acefcd => 131
	i32 2294913272, ; 380: Npgsql => 0x88c998f8 => 204
	i32 2295906218, ; 381: System.Net.Sockets => 0x88d8bfaa => 75
	i32 2298471582, ; 382: System.Net.Mail => 0x88ffe49e => 66
	i32 2303942373, ; 383: nb\Microsoft.Maui.Controls.resources => 0x89535ee5 => 342
	i32 2305521784, ; 384: System.Private.CoreLib.dll => 0x896b7878 => 172
	i32 2315684594, ; 385: Xamarin.AndroidX.Annotation.dll => 0x8a068af2 => 213
	i32 2319144366, ; 386: Microsoft.AspNetCore.SignalR.Client => 0x8a3b55ae => 179
	i32 2320631194, ; 387: System.Threading.Tasks.Parallel.dll => 0x8a52059a => 143
	i32 2334995809, ; 388: Npgsql.EntityFrameworkCore.PostgreSQL.dll => 0x8b2d3561 => 205
	i32 2340441535, ; 389: System.Runtime.InteropServices.RuntimeInformation.dll => 0x8b804dbf => 106
	i32 2344264397, ; 390: System.ValueTuple => 0x8bbaa2cd => 151
	i32 2353062107, ; 391: System.Net.Primitives => 0x8c40e0db => 70
	i32 2368005991, ; 392: System.Xml.ReaderWriter.dll => 0x8d24e767 => 156
	i32 2371007202, ; 393: Microsoft.Extensions.Configuration => 0x8d52b2e2 => 188
	i32 2378619854, ; 394: System.Security.Cryptography.Csp.dll => 0x8dc6dbce => 121
	i32 2383496789, ; 395: System.Security.Principal.Windows.dll => 0x8e114655 => 127
	i32 2395872292, ; 396: id\Microsoft.Maui.Controls.resources => 0x8ece1c24 => 337
	i32 2401565422, ; 397: System.Web.HttpUtility => 0x8f24faee => 152
	i32 2403452196, ; 398: Xamarin.AndroidX.Emoji2.dll => 0x8f41c524 => 243
	i32 2418341376, ; 399: Xamarin.AndroidX.Camera.Video.dll => 0x9024f600 => 224
	i32 2421380589, ; 400: System.Threading.Tasks.Dataflow => 0x905355ed => 141
	i32 2423080555, ; 401: Xamarin.AndroidX.Collection.Ktx.dll => 0x906d466b => 229
	i32 2425270691, ; 402: Xamarin.Google.MLKit.BarcodeScanning.Common => 0x908eb1a3 => 306
	i32 2427813419, ; 403: hi\Microsoft.Maui.Controls.resources => 0x90b57e2b => 334
	i32 2435356389, ; 404: System.Console.dll => 0x912896e5 => 20
	i32 2435904999, ; 405: System.ComponentModel.DataAnnotations.dll => 0x9130f5e7 => 14
	i32 2454642406, ; 406: System.Text.Encoding.dll => 0x924edee6 => 135
	i32 2458678730, ; 407: System.Net.Sockets.dll => 0x928c75ca => 75
	i32 2459001652, ; 408: System.Linq.Parallel.dll => 0x92916334 => 59
	i32 2465532216, ; 409: Xamarin.AndroidX.ConstraintLayout.Core.dll => 0x92f50938 => 233
	i32 2471841756, ; 410: netstandard.dll => 0x93554fdc => 167
	i32 2475788418, ; 411: Java.Interop.dll => 0x93918882 => 168
	i32 2480646305, ; 412: Microsoft.Maui.Controls => 0x93dba8a1 => 199
	i32 2483903535, ; 413: System.ComponentModel.EventBasedAsync => 0x940d5c2f => 15
	i32 2484371297, ; 414: System.Net.ServicePoint => 0x94147f61 => 74
	i32 2490993605, ; 415: System.AppContext.dll => 0x94798bc5 => 6
	i32 2501346920, ; 416: System.Data.DataSetExtensions => 0x95178668 => 23
	i32 2505896520, ; 417: Xamarin.AndroidX.Lifecycle.Runtime.dll => 0x955cf248 => 256
	i32 2522472828, ; 418: Xamarin.Android.Glide.dll => 0x9659e17c => 207
	i32 2538310050, ; 419: System.Reflection.Emit.Lightweight.dll => 0x974b89a2 => 91
	i32 2550873716, ; 420: hr\Microsoft.Maui.Controls.resources => 0x980b3e74 => 335
	i32 2562349572, ; 421: Microsoft.CSharp => 0x98ba5a04 => 1
	i32 2570120770, ; 422: System.Text.Encodings.Web => 0x9930ee42 => 136
	i32 2577256205, ; 423: Xamarin.AndroidX.Lifecycle.Runtime.Ktx.Android => 0x999dcf0d => 259
	i32 2581783588, ; 424: Xamarin.AndroidX.Lifecycle.Runtime.Ktx => 0x99e2e424 => 258
	i32 2581819634, ; 425: Xamarin.AndroidX.VectorDrawable.dll => 0x99e370f2 => 283
	i32 2585220780, ; 426: System.Text.Encoding.Extensions.dll => 0x9a1756ac => 134
	i32 2585805581, ; 427: System.Net.Ping => 0x9a20430d => 69
	i32 2589602615, ; 428: System.Threading.ThreadPool => 0x9a5a3337 => 146
	i32 2593496499, ; 429: pl\Microsoft.Maui.Controls.resources => 0x9a959db3 => 344
	i32 2605712449, ; 430: Xamarin.KotlinX.Coroutines.Core.Jvm => 0x9b500441 => 323
	i32 2615233544, ; 431: Xamarin.AndroidX.Fragment.Ktx => 0x9be14c08 => 247
	i32 2616218305, ; 432: Microsoft.Extensions.Logging.Debug.dll => 0x9bf052c1 => 195
	i32 2617129537, ; 433: System.Private.Xml.dll => 0x9bfe3a41 => 88
	i32 2618712057, ; 434: System.Reflection.TypeExtensions.dll => 0x9c165ff9 => 96
	i32 2620111890, ; 435: Xamarin.Firebase.Encoders.dll => 0x9c2bbc12 => 292
	i32 2620871830, ; 436: Xamarin.AndroidX.CursorAdapter.dll => 0x9c375496 => 237
	i32 2624644809, ; 437: Xamarin.AndroidX.DynamicAnimation => 0x9c70e6c9 => 242
	i32 2626831493, ; 438: ja\Microsoft.Maui.Controls.resources => 0x9c924485 => 339
	i32 2627185994, ; 439: System.Diagnostics.TextWriterTraceListener.dll => 0x9c97ad4a => 31
	i32 2629843544, ; 440: System.IO.Compression.ZipFile.dll => 0x9cc03a58 => 45
	i32 2633051222, ; 441: Xamarin.AndroidX.Lifecycle.LiveData => 0x9cf12c56 => 252
	i32 2634653062, ; 442: Microsoft.EntityFrameworkCore.Relational.dll => 0x9d099d86 => 185
	i32 2637500010, ; 443: Microsoft.Extensions.Features => 0x9d350e6a => 192
	i32 2639764100, ; 444: Xamarin.Firebase.Encoders => 0x9d579a84 => 292
	i32 2663391936, ; 445: Xamarin.Android.Glide.DiskLruCache => 0x9ec022c0 => 209
	i32 2663698177, ; 446: System.Runtime.Loader => 0x9ec4cf01 => 109
	i32 2664396074, ; 447: System.Xml.XDocument.dll => 0x9ecf752a => 158
	i32 2665622720, ; 448: System.Drawing.Primitives => 0x9ee22cc0 => 35
	i32 2671474046, ; 449: Xamarin.KotlinX.Coroutines.Core => 0x9f3b757e => 322
	i32 2676780864, ; 450: System.Data.Common.dll => 0x9f8c6f40 => 22
	i32 2686887180, ; 451: System.Runtime.Serialization.Xml.dll => 0xa026a50c => 114
	i32 2693849962, ; 452: System.IO.dll => 0xa090e36a => 57
	i32 2701096212, ; 453: Xamarin.AndroidX.Tracing.Tracing => 0xa0ff7514 => 280
	i32 2715334215, ; 454: System.Threading.Tasks.dll => 0xa1d8b647 => 144
	i32 2717744543, ; 455: System.Security.Claims => 0xa1fd7d9f => 118
	i32 2719963679, ; 456: System.Security.Cryptography.Cng.dll => 0xa21f5a1f => 120
	i32 2724373263, ; 457: System.Runtime.Numerics.dll => 0xa262a30f => 110
	i32 2732626843, ; 458: Xamarin.AndroidX.Activity => 0xa2e0939b => 211
	i32 2735172069, ; 459: System.Threading.Channels => 0xa30769e5 => 139
	i32 2737747696, ; 460: Xamarin.AndroidX.AppCompat.AppCompatResources.dll => 0xa32eb6f0 => 217
	i32 2740948882, ; 461: System.IO.Pipes.AccessControl => 0xa35f8f92 => 54
	i32 2748088231, ; 462: System.Runtime.InteropServices.JavaScript => 0xa3cc7fa7 => 105
	i32 2752995522, ; 463: pt-BR\Microsoft.Maui.Controls.resources => 0xa41760c2 => 345
	i32 2758225723, ; 464: Microsoft.Maui.Controls.Xaml => 0xa4672f3b => 200
	i32 2764765095, ; 465: Microsoft.Maui.dll => 0xa4caf7a7 => 201
	i32 2765824710, ; 466: System.Text.Encoding.CodePages.dll => 0xa4db22c6 => 133
	i32 2766642685, ; 467: Xamarin.AndroidX.Lifecycle.ViewModel.Android.dll => 0xa4e79dfd => 261
	i32 2770495804, ; 468: Xamarin.Jetbrains.Annotations.dll => 0xa522693c => 315
	i32 2778768386, ; 469: Xamarin.AndroidX.ViewPager.dll => 0xa5a0a402 => 286
	i32 2779977773, ; 470: Xamarin.AndroidX.ResourceInspection.Annotation.dll => 0xa5b3182d => 273
	i32 2780199943, ; 471: Xamarin.AndroidX.Lifecycle.Common.Jvm.dll => 0xa5b67c07 => 251
	i32 2785988530, ; 472: th\Microsoft.Maui.Controls.resources => 0xa60ecfb2 => 351
	i32 2788224221, ; 473: Xamarin.AndroidX.Fragment.Ktx.dll => 0xa630ecdd => 247
	i32 2801831435, ; 474: Microsoft.Maui.Graphics => 0xa7008e0b => 203
	i32 2803228030, ; 475: System.Xml.XPath.XDocument.dll => 0xa715dd7e => 159
	i32 2804607052, ; 476: Xamarin.Firebase.Components.dll => 0xa72ae84c => 291
	i32 2806116107, ; 477: es/Microsoft.Maui.Controls.resources.dll => 0xa741ef0b => 330
	i32 2810250172, ; 478: Xamarin.AndroidX.CoordinatorLayout.dll => 0xa78103bc => 234
	i32 2819470561, ; 479: System.Xml.dll => 0xa80db4e1 => 163
	i32 2821205001, ; 480: System.ServiceProcess.dll => 0xa8282c09 => 132
	i32 2821294376, ; 481: Xamarin.AndroidX.ResourceInspection.Annotation => 0xa8298928 => 273
	i32 2824502124, ; 482: System.Xml.XmlDocument => 0xa85a7b6c => 161
	i32 2828186339, ; 483: Xamarin.AndroidX.Concurrent.Futures.Ktx.dll => 0xa892b2e3 => 231
	i32 2831556043, ; 484: nl/Microsoft.Maui.Controls.resources.dll => 0xa8c61dcb => 343
	i32 2838993487, ; 485: Xamarin.AndroidX.Lifecycle.ViewModel.Ktx.dll => 0xa9379a4f => 262
	i32 2847418871, ; 486: Xamarin.GooglePlayServices.Base => 0xa9b829f7 => 310
	i32 2847789619, ; 487: Microsoft.EntityFrameworkCore.Relational => 0xa9bdd233 => 185
	i32 2849599387, ; 488: System.Threading.Overlapped.dll => 0xa9d96f9b => 140
	i32 2853208004, ; 489: Xamarin.AndroidX.ViewPager => 0xaa107fc4 => 286
	i32 2855708567, ; 490: Xamarin.AndroidX.Transition => 0xaa36a797 => 282
	i32 2861098320, ; 491: Mono.Android.Export.dll => 0xaa88e550 => 169
	i32 2861189240, ; 492: Microsoft.Maui.Essentials => 0xaa8a4878 => 202
	i32 2868099152, ; 493: Xamarin.Google.MLKit.Vision.Common.dll => 0xaaf3b850 => 308
	i32 2870099610, ; 494: Xamarin.AndroidX.Activity.Ktx.dll => 0xab123e9a => 212
	i32 2875164099, ; 495: Jsr305Binding.dll => 0xab5f85c3 => 301
	i32 2875220617, ; 496: System.Globalization.Calendars.dll => 0xab606289 => 40
	i32 2875347124, ; 497: Microsoft.AspNetCore.Http.Connections.Client.dll => 0xab6250b4 => 177
	i32 2884993177, ; 498: Xamarin.AndroidX.ExifInterface => 0xabf58099 => 245
	i32 2887636118, ; 499: System.Net.dll => 0xac1dd496 => 81
	i32 2899753641, ; 500: System.IO.UnmanagedMemoryStream => 0xacd6baa9 => 56
	i32 2900621748, ; 501: System.Dynamic.Runtime.dll => 0xace3f9b4 => 37
	i32 2901442782, ; 502: System.Reflection => 0xacf080de => 97
	i32 2905242038, ; 503: mscorlib.dll => 0xad2a79b6 => 166
	i32 2909740682, ; 504: System.Private.CoreLib => 0xad6f1e8a => 172
	i32 2916838712, ; 505: Xamarin.AndroidX.ViewPager2.dll => 0xaddb6d38 => 287
	i32 2919462931, ; 506: System.Numerics.Vectors.dll => 0xae037813 => 82
	i32 2921128767, ; 507: Xamarin.AndroidX.Annotation.Experimental.dll => 0xae1ce33f => 214
	i32 2936416060, ; 508: System.Resources.Reader => 0xaf06273c => 98
	i32 2940926066, ; 509: System.Diagnostics.StackTrace.dll => 0xaf4af872 => 30
	i32 2942453041, ; 510: System.Xml.XPath.XDocument => 0xaf624531 => 159
	i32 2959614098, ; 511: System.ComponentModel.dll => 0xb0682092 => 18
	i32 2965157864, ; 512: Xamarin.AndroidX.Camera.View => 0xb0bcb7e8 => 225
	i32 2968338931, ; 513: System.Security.Principal.Windows => 0xb0ed41f3 => 127
	i32 2972252294, ; 514: System.Security.Cryptography.Algorithms.dll => 0xb128f886 => 119
	i32 2978675010, ; 515: Xamarin.AndroidX.DrawerLayout => 0xb18af942 => 241
	i32 2987532451, ; 516: Xamarin.AndroidX.Security.SecurityCrypto => 0xb21220a3 => 276
	i32 2991449226, ; 517: Xamarin.AndroidX.Camera.Core => 0xb24de48a => 222
	i32 2996846495, ; 518: Xamarin.AndroidX.Lifecycle.Process.dll => 0xb2a03f9f => 255
	i32 3000842441, ; 519: Xamarin.AndroidX.Camera.View.dll => 0xb2dd38c9 => 225
	i32 3016983068, ; 520: Xamarin.AndroidX.Startup.StartupRuntime => 0xb3d3821c => 278
	i32 3023353419, ; 521: WindowsBase.dll => 0xb434b64b => 165
	i32 3024354802, ; 522: Xamarin.AndroidX.Legacy.Support.Core.Utils => 0xb443fdf2 => 249
	i32 3038032645, ; 523: _Microsoft.Android.Resource.Designer.dll => 0xb514b305 => 358
	i32 3047751430, ; 524: Xamarin.AndroidX.Camera.Core.dll => 0xb5a8ff06 => 222
	i32 3056245963, ; 525: Xamarin.AndroidX.SavedState.SavedState.Ktx => 0xb62a9ccb => 275
	i32 3057625584, ; 526: Xamarin.AndroidX.Navigation.Common => 0xb63fa9f0 => 266
	i32 3058099980, ; 527: Xamarin.GooglePlayServices.Tasks => 0xb646e70c => 313
	i32 3059408633, ; 528: Mono.Android.Runtime => 0xb65adef9 => 170
	i32 3059793426, ; 529: System.ComponentModel.Primitives => 0xb660be12 => 16
	i32 3069363400, ; 530: Microsoft.Extensions.Caching.Abstractions.dll => 0xb6f2c4c8 => 186
	i32 3075834255, ; 531: System.Threading.Tasks => 0xb755818f => 144
	i32 3077302341, ; 532: hu/Microsoft.Maui.Controls.resources.dll => 0xb76be845 => 336
	i32 3090735792, ; 533: System.Security.Cryptography.X509Certificates.dll => 0xb838e2b0 => 125
	i32 3099732863, ; 534: System.Security.Claims.dll => 0xb8c22b7f => 118
	i32 3103600923, ; 535: System.Formats.Asn1 => 0xb8fd311b => 38
	i32 3111772706, ; 536: System.Runtime.Serialization => 0xb979e222 => 115
	i32 3121463068, ; 537: System.IO.FileSystem.AccessControl.dll => 0xba0dbf1c => 47
	i32 3124832203, ; 538: System.Threading.Tasks.Extensions => 0xba4127cb => 142
	i32 3132293585, ; 539: System.Security.AccessControl => 0xbab301d1 => 117
	i32 3147165239, ; 540: System.Diagnostics.Tracing.dll => 0xbb95ee37 => 34
	i32 3148237826, ; 541: GoogleGson.dll => 0xbba64c02 => 175
	i32 3155362983, ; 542: Xamarin.Google.Android.DataTransport.TransportApi => 0xbc1304a7 => 295
	i32 3159123045, ; 543: System.Reflection.Primitives.dll => 0xbc4c6465 => 95
	i32 3160747431, ; 544: System.IO.MemoryMappedFiles => 0xbc652da7 => 53
	i32 3178803400, ; 545: Xamarin.AndroidX.Navigation.Fragment.dll => 0xbd78b0c8 => 267
	i32 3192346100, ; 546: System.Security.SecureString => 0xbe4755f4 => 129
	i32 3193515020, ; 547: System.Web => 0xbe592c0c => 153
	i32 3195844289, ; 548: Microsoft.Extensions.Caching.Abstractions => 0xbe7cb6c1 => 186
	i32 3204380047, ; 549: System.Data.dll => 0xbefef58f => 24
	i32 3209718065, ; 550: System.Xml.XmlDocument.dll => 0xbf506931 => 161
	i32 3211777861, ; 551: Xamarin.AndroidX.DocumentFile => 0xbf6fd745 => 240
	i32 3220365878, ; 552: System.Threading => 0xbff2e236 => 148
	i32 3226221578, ; 553: System.Runtime.Handles.dll => 0xc04c3c0a => 104
	i32 3230466174, ; 554: Xamarin.GooglePlayServices.Basement.dll => 0xc08d007e => 311
	i32 3251039220, ; 555: System.Reflection.DispatchProxy.dll => 0xc1c6ebf4 => 89
	i32 3258312781, ; 556: Xamarin.AndroidX.CardView => 0xc235e84d => 226
	i32 3265493905, ; 557: System.Linq.Queryable.dll => 0xc2a37b91 => 60
	i32 3265893370, ; 558: System.Threading.Tasks.Extensions.dll => 0xc2a993fa => 142
	i32 3277815716, ; 559: System.Resources.Writer.dll => 0xc35f7fa4 => 100
	i32 3279906254, ; 560: Microsoft.Win32.Registry.dll => 0xc37f65ce => 5
	i32 3280506390, ; 561: System.ComponentModel.Annotations.dll => 0xc3888e16 => 13
	i32 3290767353, ; 562: System.Security.Cryptography.Encoding => 0xc4251ff9 => 122
	i32 3299363146, ; 563: System.Text.Encoding => 0xc4a8494a => 135
	i32 3303498502, ; 564: System.Diagnostics.FileVersionInfo => 0xc4e76306 => 28
	i32 3305363605, ; 565: fi\Microsoft.Maui.Controls.resources => 0xc503d895 => 331
	i32 3316684772, ; 566: System.Net.Requests.dll => 0xc5b097e4 => 72
	i32 3317135071, ; 567: Xamarin.AndroidX.CustomView.dll => 0xc5b776df => 238
	i32 3317144872, ; 568: System.Data => 0xc5b79d28 => 24
	i32 3340431453, ; 569: Xamarin.AndroidX.Arch.Core.Runtime => 0xc71af05d => 219
	i32 3345895724, ; 570: Xamarin.AndroidX.ProfileInstaller.ProfileInstaller.dll => 0xc76e512c => 271
	i32 3346324047, ; 571: Xamarin.AndroidX.Navigation.Runtime => 0xc774da4f => 268
	i32 3354801150, ; 572: AppDynamics.Agent.dll => 0xc7f633fe => 173
	i32 3357674450, ; 573: ru\Microsoft.Maui.Controls.resources => 0xc8220bd2 => 348
	i32 3358260929, ; 574: System.Text.Json => 0xc82afec1 => 137
	i32 3359991071, ; 575: Xamarin.AndroidX.Tracing.Tracing.Ktx.dll => 0xc845651f => 281
	i32 3362336904, ; 576: Xamarin.AndroidX.Activity.Ktx => 0xc8693088 => 212
	i32 3362522851, ; 577: Xamarin.AndroidX.Core => 0xc86c06e3 => 235
	i32 3366347497, ; 578: Java.Interop => 0xc8a662e9 => 168
	i32 3371992681, ; 579: Xamarin.Firebase.Encoders.Proto.dll => 0xc8fc8669 => 294
	i32 3374999561, ; 580: Xamarin.AndroidX.RecyclerView => 0xc92a6809 => 272
	i32 3381016424, ; 581: da\Microsoft.Maui.Controls.resources => 0xc9863768 => 327
	i32 3383578424, ; 582: Xamarin.Firebase.Encoders.JSON => 0xc9ad4f38 => 293
	i32 3395150330, ; 583: System.Runtime.CompilerServices.Unsafe.dll => 0xca5de1fa => 101
	i32 3403906625, ; 584: System.Security.Cryptography.OpenSsl.dll => 0xcae37e41 => 123
	i32 3405233483, ; 585: Xamarin.AndroidX.CustomView.PoolingContainer => 0xcaf7bd4b => 239
	i32 3411362516, ; 586: Xamarin.Google.MLKit.Vision.Interfaces => 0xcb5542d4 => 309
	i32 3413944578, ; 587: Xamarin.AndroidX.Camera.Camera2.dll => 0xcb7ca902 => 221
	i32 3421910702, ; 588: Xamarin.AndroidX.Camera.Camera2 => 0xcbf636ae => 221
	i32 3428513518, ; 589: Microsoft.Extensions.DependencyInjection.dll => 0xcc5af6ee => 190
	i32 3429136800, ; 590: System.Xml => 0xcc6479a0 => 163
	i32 3430777524, ; 591: netstandard => 0xcc7d82b4 => 167
	i32 3441283291, ; 592: Xamarin.AndroidX.DynamicAnimation.dll => 0xcd1dd0db => 242
	i32 3445260447, ; 593: System.Formats.Tar => 0xcd5a809f => 39
	i32 3452344032, ; 594: Microsoft.Maui.Controls.Compatibility.dll => 0xcdc696e0 => 198
	i32 3463511458, ; 595: hr/Microsoft.Maui.Controls.resources.dll => 0xce70fda2 => 335
	i32 3466904072, ; 596: Microsoft.AspNetCore.SignalR.Client.dll => 0xcea4c208 => 179
	i32 3471940407, ; 597: System.ComponentModel.TypeConverter.dll => 0xcef19b37 => 17
	i32 3476120550, ; 598: Mono.Android => 0xcf3163e6 => 171
	i32 3479583265, ; 599: ru/Microsoft.Maui.Controls.resources.dll => 0xcf663a21 => 348
	i32 3484440000, ; 600: ro\Microsoft.Maui.Controls.resources => 0xcfb055c0 => 347
	i32 3485117614, ; 601: System.Text.Json.dll => 0xcfbaacae => 137
	i32 3486566296, ; 602: System.Transactions => 0xcfd0c798 => 150
	i32 3493954962, ; 603: Xamarin.AndroidX.Concurrent.Futures.dll => 0xd0418592 => 230
	i32 3509114376, ; 604: System.Xml.Linq => 0xd128d608 => 155
	i32 3515174580, ; 605: System.Security.dll => 0xd1854eb4 => 130
	i32 3530912306, ; 606: System.Configuration => 0xd2757232 => 19
	i32 3539954161, ; 607: System.Net.HttpListener => 0xd2ff69f1 => 65
	i32 3560100363, ; 608: System.Threading.Timer => 0xd432d20b => 147
	i32 3570554715, ; 609: System.IO.FileSystem.AccessControl => 0xd4d2575b => 47
	i32 3580758918, ; 610: zh-HK\Microsoft.Maui.Controls.resources => 0xd56e0b86 => 355
	i32 3597029428, ; 611: Xamarin.Android.Glide.GifDecoder.dll => 0xd6665034 => 210
	i32 3598340787, ; 612: System.Net.WebSockets.Client => 0xd67a52b3 => 79
	i32 3608519521, ; 613: System.Linq.dll => 0xd715a361 => 61
	i32 3624195450, ; 614: System.Runtime.InteropServices.RuntimeInformation => 0xd804d57a => 106
	i32 3626429363, ; 615: Xamarin.Google.MLKit.Common => 0xd826ebb3 => 307
	i32 3627220390, ; 616: Xamarin.AndroidX.Print.dll => 0xd832fda6 => 270
	i32 3633644679, ; 617: Xamarin.AndroidX.Annotation.Experimental => 0xd8950487 => 214
	i32 3638274909, ; 618: System.IO.FileSystem.Primitives.dll => 0xd8dbab5d => 49
	i32 3641597786, ; 619: Xamarin.AndroidX.Lifecycle.LiveData.Core => 0xd90e5f5a => 253
	i32 3643446276, ; 620: tr\Microsoft.Maui.Controls.resources => 0xd92a9404 => 352
	i32 3643854240, ; 621: Xamarin.AndroidX.Navigation.Fragment => 0xd930cda0 => 267
	i32 3645089577, ; 622: System.ComponentModel.DataAnnotations => 0xd943a729 => 14
	i32 3657292374, ; 623: Microsoft.Extensions.Configuration.Abstractions.dll => 0xd9fdda56 => 189
	i32 3660523487, ; 624: System.Net.NetworkInformation => 0xda2f27df => 68
	i32 3672681054, ; 625: Mono.Android.dll => 0xdae8aa5e => 171
	i32 3676461095, ; 626: Xamarin.AndroidX.Camera.Lifecycle => 0xdb225827 => 223
	i32 3682565725, ; 627: Xamarin.AndroidX.Browser => 0xdb7f7e5d => 220
	i32 3684561358, ; 628: Xamarin.AndroidX.Concurrent.Futures => 0xdb9df1ce => 230
	i32 3691870036, ; 629: Microsoft.AspNetCore.SignalR.Protocols.Json => 0xdc0d7754 => 182
	i32 3697841164, ; 630: zh-Hant/Microsoft.Maui.Controls.resources.dll => 0xdc68940c => 357
	i32 3700866549, ; 631: System.Net.WebProxy.dll => 0xdc96bdf5 => 78
	i32 3706696989, ; 632: Xamarin.AndroidX.Core.Core.Ktx.dll => 0xdcefb51d => 236
	i32 3716563718, ; 633: System.Runtime.Intrinsics => 0xdd864306 => 108
	i32 3718780102, ; 634: Xamarin.AndroidX.Annotation => 0xdda814c6 => 213
	i32 3724971120, ; 635: Xamarin.AndroidX.Navigation.Common.dll => 0xde068c70 => 266
	i32 3732100267, ; 636: System.Net.NameResolution => 0xde7354ab => 67
	i32 3737834244, ; 637: System.Net.Http.Json.dll => 0xdecad304 => 63
	i32 3748608112, ; 638: System.Diagnostics.DiagnosticSource => 0xdf6f3870 => 27
	i32 3751444290, ; 639: System.Xml.XPath => 0xdf9a7f42 => 160
	i32 3764085317, ; 640: Xamarin.AndroidX.Lifecycle.Runtime.Ktx.Android.dll => 0xe05b6245 => 259
	i32 3786282454, ; 641: Xamarin.AndroidX.Collection => 0xe1ae15d6 => 227
	i32 3787005001, ; 642: Microsoft.AspNetCore.Connections.Abstractions => 0xe1b91c49 => 176
	i32 3792276235, ; 643: System.Collections.NonGeneric => 0xe2098b0b => 10
	i32 3800979733, ; 644: Microsoft.Maui.Controls.Compatibility => 0xe28e5915 => 198
	i32 3802395368, ; 645: System.Collections.Specialized.dll => 0xe2a3f2e8 => 11
	i32 3819260425, ; 646: System.Net.WebProxy => 0xe3a54a09 => 78
	i32 3823082795, ; 647: System.Security.Cryptography.dll => 0xe3df9d2b => 126
	i32 3829621856, ; 648: System.Numerics.dll => 0xe4436460 => 83
	i32 3841636137, ; 649: Microsoft.Extensions.DependencyInjection.Abstractions.dll => 0xe4fab729 => 191
	i32 3844307129, ; 650: System.Net.Mail.dll => 0xe52378b9 => 66
	i32 3849253459, ; 651: System.Runtime.InteropServices.dll => 0xe56ef253 => 107
	i32 3870376305, ; 652: System.Net.HttpListener.dll => 0xe6b14171 => 65
	i32 3873536506, ; 653: System.Security.Principal => 0xe6e179fa => 128
	i32 3875112723, ; 654: System.Security.Cryptography.Encoding.dll => 0xe6f98713 => 122
	i32 3885497537, ; 655: System.Net.WebHeaderCollection.dll => 0xe797fcc1 => 77
	i32 3885922214, ; 656: Xamarin.AndroidX.Transition.dll => 0xe79e77a6 => 282
	i32 3888767677, ; 657: Xamarin.AndroidX.ProfileInstaller.ProfileInstaller => 0xe7c9e2bd => 271
	i32 3889960447, ; 658: zh-Hans/Microsoft.Maui.Controls.resources.dll => 0xe7dc15ff => 356
	i32 3896106733, ; 659: System.Collections.Concurrent.dll => 0xe839deed => 8
	i32 3896760992, ; 660: Xamarin.AndroidX.Core.dll => 0xe843daa0 => 235
	i32 3901907137, ; 661: Microsoft.VisualBasic.Core.dll => 0xe89260c1 => 2
	i32 3910130544, ; 662: Xamarin.AndroidX.Collection.Jvm => 0xe90fdb70 => 228
	i32 3920810846, ; 663: System.IO.Compression.FileSystem.dll => 0xe9b2d35e => 44
	i32 3921031405, ; 664: Xamarin.AndroidX.VersionedParcelable.dll => 0xe9b630ed => 285
	i32 3928044579, ; 665: System.Xml.ReaderWriter => 0xea213423 => 156
	i32 3930554604, ; 666: System.Security.Principal.dll => 0xea4780ec => 128
	i32 3931092270, ; 667: Xamarin.AndroidX.Navigation.UI => 0xea4fb52e => 269
	i32 3934056515, ; 668: Xamarin.JavaX.Inject.dll => 0xea7cf043 => 314
	i32 3945713374, ; 669: System.Data.DataSetExtensions.dll => 0xeb2ecede => 23
	i32 3953953790, ; 670: System.Text.Encoding.CodePages => 0xebac8bfe => 133
	i32 3955647286, ; 671: Xamarin.AndroidX.AppCompat.dll => 0xebc66336 => 216
	i32 3956287295, ; 672: BarcodeScanning.Native.Maui => 0xebd0273f => 174
	i32 3959773229, ; 673: Xamarin.AndroidX.Lifecycle.Process => 0xec05582d => 255
	i32 3970018735, ; 674: Xamarin.GooglePlayServices.Tasks.dll => 0xeca1adaf => 313
	i32 3980434154, ; 675: th/Microsoft.Maui.Controls.resources.dll => 0xed409aea => 351
	i32 3987592930, ; 676: he/Microsoft.Maui.Controls.resources.dll => 0xedadd6e2 => 333
	i32 4003436829, ; 677: System.Diagnostics.Process.dll => 0xee9f991d => 29
	i32 4015948917, ; 678: Xamarin.AndroidX.Annotation.Jvm.dll => 0xef5e8475 => 215
	i32 4023392905, ; 679: System.IO.Pipelines => 0xefd01a89 => 206
	i32 4025784931, ; 680: System.Memory => 0xeff49a63 => 62
	i32 4026433800, ; 681: NekrasovskyAPP.dll => 0xeffe8108 => 0
	i32 4046471985, ; 682: Microsoft.Maui.Controls.Xaml.dll => 0xf1304331 => 200
	i32 4054681211, ; 683: System.Reflection.Emit.ILGeneration => 0xf1ad867b => 90
	i32 4068434129, ; 684: System.Private.Xml.Linq.dll => 0xf27f60d1 => 87
	i32 4073602200, ; 685: System.Threading.dll => 0xf2ce3c98 => 148
	i32 4094352644, ; 686: Microsoft.Maui.Essentials.dll => 0xf40add04 => 202
	i32 4099507663, ; 687: System.Drawing.dll => 0xf45985cf => 36
	i32 4100113165, ; 688: System.Private.Uri => 0xf462c30d => 86
	i32 4101236366, ; 689: Npgsql.EntityFrameworkCore.PostgreSQL => 0xf473e68e => 205
	i32 4101593132, ; 690: Xamarin.AndroidX.Emoji2 => 0xf479582c => 243
	i32 4101842092, ; 691: Microsoft.Extensions.Caching.Memory => 0xf47d24ac => 187
	i32 4102112229, ; 692: pt/Microsoft.Maui.Controls.resources.dll => 0xf48143e5 => 346
	i32 4125707920, ; 693: ms/Microsoft.Maui.Controls.resources.dll => 0xf5e94e90 => 341
	i32 4126470640, ; 694: Microsoft.Extensions.DependencyInjection => 0xf5f4f1f0 => 190
	i32 4127667938, ; 695: System.IO.FileSystem.Watcher => 0xf60736e2 => 50
	i32 4130442656, ; 696: System.AppContext => 0xf6318da0 => 6
	i32 4147896353, ; 697: System.Reflection.Emit.ILGeneration.dll => 0xf73be021 => 90
	i32 4150914736, ; 698: uk\Microsoft.Maui.Controls.resources => 0xf769eeb0 => 353
	i32 4151237749, ; 699: System.Core => 0xf76edc75 => 21
	i32 4159265925, ; 700: System.Xml.XmlSerializer => 0xf7e95c85 => 162
	i32 4161255271, ; 701: System.Reflection.TypeExtensions => 0xf807b767 => 96
	i32 4164802419, ; 702: System.IO.FileSystem.Watcher.dll => 0xf83dd773 => 50
	i32 4181436372, ; 703: System.Runtime.Serialization.Primitives => 0xf93ba7d4 => 113
	i32 4182413190, ; 704: Xamarin.AndroidX.Lifecycle.ViewModelSavedState.dll => 0xf94a8f86 => 263
	i32 4185676441, ; 705: System.Security => 0xf97c5a99 => 130
	i32 4192648326, ; 706: Xamarin.Firebase.Encoders.JSON.dll => 0xf9e6bc86 => 293
	i32 4196529839, ; 707: System.Net.WebClient.dll => 0xfa21f6af => 76
	i32 4213026141, ; 708: System.Diagnostics.DiagnosticSource.dll => 0xfb1dad5d => 27
	i32 4228543782, ; 709: Xamarin.KotlinX.AtomicFU.Jvm.dll => 0xfc0a7526 => 320
	i32 4256097574, ; 710: Xamarin.AndroidX.Core.Core.Ktx => 0xfdaee526 => 236
	i32 4258378803, ; 711: Xamarin.AndroidX.Lifecycle.ViewModel.Ktx => 0xfdd1b433 => 262
	i32 4260525087, ; 712: System.Buffers => 0xfdf2741f => 7
	i32 4271975918, ; 713: Microsoft.Maui.Controls.dll => 0xfea12dee => 199
	i32 4274976490, ; 714: System.Runtime.Numerics => 0xfecef6ea => 110
	i32 4284549794, ; 715: Xamarin.Firebase.Components => 0xff610aa2 => 291
	i32 4292120959, ; 716: Xamarin.AndroidX.Lifecycle.ViewModelSavedState => 0xffd4917f => 263
	i32 4294763496 ; 717: Xamarin.AndroidX.ExifInterface.dll => 0xfffce3e8 => 245
], align 4

@assembly_image_cache_indices = dso_local local_unnamed_addr constant [718 x i32] [
	i32 68, ; 0
	i32 67, ; 1
	i32 108, ; 2
	i32 309, ; 3
	i32 204, ; 4
	i32 299, ; 5
	i32 256, ; 6
	i32 304, ; 7
	i32 48, ; 8
	i32 80, ; 9
	i32 223, ; 10
	i32 145, ; 11
	i32 320, ; 12
	i32 231, ; 13
	i32 30, ; 14
	i32 357, ; 15
	i32 124, ; 16
	i32 203, ; 17
	i32 102, ; 18
	i32 277, ; 19
	i32 290, ; 20
	i32 107, ; 21
	i32 277, ; 22
	i32 139, ; 23
	i32 317, ; 24
	i32 77, ; 25
	i32 124, ; 26
	i32 13, ; 27
	i32 227, ; 28
	i32 319, ; 29
	i32 132, ; 30
	i32 279, ; 31
	i32 151, ; 32
	i32 354, ; 33
	i32 355, ; 34
	i32 18, ; 35
	i32 220, ; 36
	i32 281, ; 37
	i32 26, ; 38
	i32 177, ; 39
	i32 249, ; 40
	i32 1, ; 41
	i32 59, ; 42
	i32 42, ; 43
	i32 91, ; 44
	i32 232, ; 45
	i32 147, ; 46
	i32 252, ; 47
	i32 248, ; 48
	i32 326, ; 49
	i32 54, ; 50
	i32 69, ; 51
	i32 354, ; 52
	i32 211, ; 53
	i32 83, ; 54
	i32 339, ; 55
	i32 250, ; 56
	i32 178, ; 57
	i32 338, ; 58
	i32 131, ; 59
	i32 55, ; 60
	i32 149, ; 61
	i32 74, ; 62
	i32 145, ; 63
	i32 62, ; 64
	i32 146, ; 65
	i32 358, ; 66
	i32 165, ; 67
	i32 257, ; 68
	i32 350, ; 69
	i32 233, ; 70
	i32 12, ; 71
	i32 246, ; 72
	i32 125, ; 73
	i32 152, ; 74
	i32 181, ; 75
	i32 113, ; 76
	i32 166, ; 77
	i32 164, ; 78
	i32 248, ; 79
	i32 297, ; 80
	i32 265, ; 81
	i32 297, ; 82
	i32 84, ; 83
	i32 337, ; 84
	i32 331, ; 85
	i32 295, ; 86
	i32 197, ; 87
	i32 308, ; 88
	i32 150, ; 89
	i32 317, ; 90
	i32 60, ; 91
	i32 193, ; 92
	i32 51, ; 93
	i32 103, ; 94
	i32 114, ; 95
	i32 40, ; 96
	i32 301, ; 97
	i32 289, ; 98
	i32 120, ; 99
	i32 345, ; 100
	i32 52, ; 101
	i32 44, ; 102
	i32 119, ; 103
	i32 257, ; 104
	i32 238, ; 105
	i32 343, ; 106
	i32 244, ; 107
	i32 81, ; 108
	i32 136, ; 109
	i32 285, ; 110
	i32 218, ; 111
	i32 8, ; 112
	i32 319, ; 113
	i32 73, ; 114
	i32 325, ; 115
	i32 155, ; 116
	i32 321, ; 117
	i32 154, ; 118
	i32 92, ; 119
	i32 315, ; 120
	i32 45, ; 121
	i32 340, ; 122
	i32 328, ; 123
	i32 318, ; 124
	i32 109, ; 125
	i32 129, ; 126
	i32 25, ; 127
	i32 208, ; 128
	i32 72, ; 129
	i32 55, ; 130
	i32 46, ; 131
	i32 349, ; 132
	i32 300, ; 133
	i32 196, ; 134
	i32 239, ; 135
	i32 22, ; 136
	i32 254, ; 137
	i32 86, ; 138
	i32 43, ; 139
	i32 160, ; 140
	i32 182, ; 141
	i32 71, ; 142
	i32 270, ; 143
	i32 3, ; 144
	i32 42, ; 145
	i32 63, ; 146
	i32 16, ; 147
	i32 53, ; 148
	i32 352, ; 149
	i32 304, ; 150
	i32 105, ; 151
	i32 318, ; 152
	i32 302, ; 153
	i32 250, ; 154
	i32 34, ; 155
	i32 158, ; 156
	i32 85, ; 157
	i32 32, ; 158
	i32 12, ; 159
	i32 51, ; 160
	i32 296, ; 161
	i32 56, ; 162
	i32 274, ; 163
	i32 36, ; 164
	i32 191, ; 165
	i32 327, ; 166
	i32 303, ; 167
	i32 216, ; 168
	i32 35, ; 169
	i32 58, ; 170
	i32 260, ; 171
	i32 178, ; 172
	i32 300, ; 173
	i32 175, ; 174
	i32 17, ; 175
	i32 316, ; 176
	i32 164, ; 177
	i32 340, ; 178
	i32 258, ; 179
	i32 299, ; 180
	i32 195, ; 181
	i32 288, ; 182
	i32 184, ; 183
	i32 346, ; 184
	i32 153, ; 185
	i32 284, ; 186
	i32 268, ; 187
	i32 184, ; 188
	i32 344, ; 189
	i32 307, ; 190
	i32 218, ; 191
	i32 187, ; 192
	i32 29, ; 193
	i32 52, ; 194
	i32 180, ; 195
	i32 342, ; 196
	i32 289, ; 197
	i32 228, ; 198
	i32 5, ; 199
	i32 326, ; 200
	i32 278, ; 201
	i32 322, ; 202
	i32 283, ; 203
	i32 229, ; 204
	i32 321, ; 205
	i32 215, ; 206
	i32 241, ; 207
	i32 85, ; 208
	i32 288, ; 209
	i32 61, ; 210
	i32 112, ; 211
	i32 312, ; 212
	i32 306, ; 213
	i32 305, ; 214
	i32 57, ; 215
	i32 356, ; 216
	i32 274, ; 217
	i32 99, ; 218
	i32 314, ; 219
	i32 19, ; 220
	i32 234, ; 221
	i32 111, ; 222
	i32 101, ; 223
	i32 176, ; 224
	i32 102, ; 225
	i32 324, ; 226
	i32 104, ; 227
	i32 302, ; 228
	i32 251, ; 229
	i32 71, ; 230
	i32 261, ; 231
	i32 38, ; 232
	i32 32, ; 233
	i32 103, ; 234
	i32 73, ; 235
	i32 330, ; 236
	i32 9, ; 237
	i32 123, ; 238
	i32 46, ; 239
	i32 217, ; 240
	i32 197, ; 241
	i32 9, ; 242
	i32 43, ; 243
	i32 4, ; 244
	i32 275, ; 245
	i32 334, ; 246
	i32 173, ; 247
	i32 329, ; 248
	i32 31, ; 249
	i32 138, ; 250
	i32 92, ; 251
	i32 93, ; 252
	i32 349, ; 253
	i32 49, ; 254
	i32 141, ; 255
	i32 112, ; 256
	i32 140, ; 257
	i32 240, ; 258
	i32 115, ; 259
	i32 303, ; 260
	i32 157, ; 261
	i32 76, ; 262
	i32 79, ; 263
	i32 264, ; 264
	i32 37, ; 265
	i32 287, ; 266
	i32 244, ; 267
	i32 237, ; 268
	i32 64, ; 269
	i32 138, ; 270
	i32 15, ; 271
	i32 116, ; 272
	i32 280, ; 273
	i32 298, ; 274
	i32 232, ; 275
	i32 48, ; 276
	i32 70, ; 277
	i32 80, ; 278
	i32 126, ; 279
	i32 183, ; 280
	i32 94, ; 281
	i32 121, ; 282
	i32 26, ; 283
	i32 312, ; 284
	i32 254, ; 285
	i32 97, ; 286
	i32 28, ; 287
	i32 226, ; 288
	i32 347, ; 289
	i32 325, ; 290
	i32 149, ; 291
	i32 206, ; 292
	i32 169, ; 293
	i32 4, ; 294
	i32 98, ; 295
	i32 33, ; 296
	i32 93, ; 297
	i32 279, ; 298
	i32 193, ; 299
	i32 0, ; 300
	i32 21, ; 301
	i32 41, ; 302
	i32 170, ; 303
	i32 341, ; 304
	i32 246, ; 305
	i32 333, ; 306
	i32 264, ; 307
	i32 316, ; 308
	i32 298, ; 309
	i32 269, ; 310
	i32 2, ; 311
	i32 134, ; 312
	i32 111, ; 313
	i32 194, ; 314
	i32 353, ; 315
	i32 208, ; 316
	i32 350, ; 317
	i32 58, ; 318
	i32 224, ; 319
	i32 95, ; 320
	i32 332, ; 321
	i32 294, ; 322
	i32 39, ; 323
	i32 219, ; 324
	i32 25, ; 325
	i32 94, ; 326
	i32 89, ; 327
	i32 99, ; 328
	i32 311, ; 329
	i32 10, ; 330
	i32 87, ; 331
	i32 180, ; 332
	i32 100, ; 333
	i32 276, ; 334
	i32 181, ; 335
	i32 188, ; 336
	i32 210, ; 337
	i32 329, ; 338
	i32 7, ; 339
	i32 260, ; 340
	i32 324, ; 341
	i32 207, ; 342
	i32 88, ; 343
	i32 253, ; 344
	i32 154, ; 345
	i32 328, ; 346
	i32 33, ; 347
	i32 116, ; 348
	i32 82, ; 349
	i32 174, ; 350
	i32 296, ; 351
	i32 20, ; 352
	i32 310, ; 353
	i32 11, ; 354
	i32 162, ; 355
	i32 3, ; 356
	i32 201, ; 357
	i32 336, ; 358
	i32 290, ; 359
	i32 196, ; 360
	i32 305, ; 361
	i32 194, ; 362
	i32 84, ; 363
	i32 323, ; 364
	i32 64, ; 365
	i32 338, ; 366
	i32 284, ; 367
	i32 143, ; 368
	i32 192, ; 369
	i32 265, ; 370
	i32 157, ; 371
	i32 183, ; 372
	i32 41, ; 373
	i32 117, ; 374
	i32 189, ; 375
	i32 209, ; 376
	i32 332, ; 377
	i32 272, ; 378
	i32 131, ; 379
	i32 204, ; 380
	i32 75, ; 381
	i32 66, ; 382
	i32 342, ; 383
	i32 172, ; 384
	i32 213, ; 385
	i32 179, ; 386
	i32 143, ; 387
	i32 205, ; 388
	i32 106, ; 389
	i32 151, ; 390
	i32 70, ; 391
	i32 156, ; 392
	i32 188, ; 393
	i32 121, ; 394
	i32 127, ; 395
	i32 337, ; 396
	i32 152, ; 397
	i32 243, ; 398
	i32 224, ; 399
	i32 141, ; 400
	i32 229, ; 401
	i32 306, ; 402
	i32 334, ; 403
	i32 20, ; 404
	i32 14, ; 405
	i32 135, ; 406
	i32 75, ; 407
	i32 59, ; 408
	i32 233, ; 409
	i32 167, ; 410
	i32 168, ; 411
	i32 199, ; 412
	i32 15, ; 413
	i32 74, ; 414
	i32 6, ; 415
	i32 23, ; 416
	i32 256, ; 417
	i32 207, ; 418
	i32 91, ; 419
	i32 335, ; 420
	i32 1, ; 421
	i32 136, ; 422
	i32 259, ; 423
	i32 258, ; 424
	i32 283, ; 425
	i32 134, ; 426
	i32 69, ; 427
	i32 146, ; 428
	i32 344, ; 429
	i32 323, ; 430
	i32 247, ; 431
	i32 195, ; 432
	i32 88, ; 433
	i32 96, ; 434
	i32 292, ; 435
	i32 237, ; 436
	i32 242, ; 437
	i32 339, ; 438
	i32 31, ; 439
	i32 45, ; 440
	i32 252, ; 441
	i32 185, ; 442
	i32 192, ; 443
	i32 292, ; 444
	i32 209, ; 445
	i32 109, ; 446
	i32 158, ; 447
	i32 35, ; 448
	i32 322, ; 449
	i32 22, ; 450
	i32 114, ; 451
	i32 57, ; 452
	i32 280, ; 453
	i32 144, ; 454
	i32 118, ; 455
	i32 120, ; 456
	i32 110, ; 457
	i32 211, ; 458
	i32 139, ; 459
	i32 217, ; 460
	i32 54, ; 461
	i32 105, ; 462
	i32 345, ; 463
	i32 200, ; 464
	i32 201, ; 465
	i32 133, ; 466
	i32 261, ; 467
	i32 315, ; 468
	i32 286, ; 469
	i32 273, ; 470
	i32 251, ; 471
	i32 351, ; 472
	i32 247, ; 473
	i32 203, ; 474
	i32 159, ; 475
	i32 291, ; 476
	i32 330, ; 477
	i32 234, ; 478
	i32 163, ; 479
	i32 132, ; 480
	i32 273, ; 481
	i32 161, ; 482
	i32 231, ; 483
	i32 343, ; 484
	i32 262, ; 485
	i32 310, ; 486
	i32 185, ; 487
	i32 140, ; 488
	i32 286, ; 489
	i32 282, ; 490
	i32 169, ; 491
	i32 202, ; 492
	i32 308, ; 493
	i32 212, ; 494
	i32 301, ; 495
	i32 40, ; 496
	i32 177, ; 497
	i32 245, ; 498
	i32 81, ; 499
	i32 56, ; 500
	i32 37, ; 501
	i32 97, ; 502
	i32 166, ; 503
	i32 172, ; 504
	i32 287, ; 505
	i32 82, ; 506
	i32 214, ; 507
	i32 98, ; 508
	i32 30, ; 509
	i32 159, ; 510
	i32 18, ; 511
	i32 225, ; 512
	i32 127, ; 513
	i32 119, ; 514
	i32 241, ; 515
	i32 276, ; 516
	i32 222, ; 517
	i32 255, ; 518
	i32 225, ; 519
	i32 278, ; 520
	i32 165, ; 521
	i32 249, ; 522
	i32 358, ; 523
	i32 222, ; 524
	i32 275, ; 525
	i32 266, ; 526
	i32 313, ; 527
	i32 170, ; 528
	i32 16, ; 529
	i32 186, ; 530
	i32 144, ; 531
	i32 336, ; 532
	i32 125, ; 533
	i32 118, ; 534
	i32 38, ; 535
	i32 115, ; 536
	i32 47, ; 537
	i32 142, ; 538
	i32 117, ; 539
	i32 34, ; 540
	i32 175, ; 541
	i32 295, ; 542
	i32 95, ; 543
	i32 53, ; 544
	i32 267, ; 545
	i32 129, ; 546
	i32 153, ; 547
	i32 186, ; 548
	i32 24, ; 549
	i32 161, ; 550
	i32 240, ; 551
	i32 148, ; 552
	i32 104, ; 553
	i32 311, ; 554
	i32 89, ; 555
	i32 226, ; 556
	i32 60, ; 557
	i32 142, ; 558
	i32 100, ; 559
	i32 5, ; 560
	i32 13, ; 561
	i32 122, ; 562
	i32 135, ; 563
	i32 28, ; 564
	i32 331, ; 565
	i32 72, ; 566
	i32 238, ; 567
	i32 24, ; 568
	i32 219, ; 569
	i32 271, ; 570
	i32 268, ; 571
	i32 173, ; 572
	i32 348, ; 573
	i32 137, ; 574
	i32 281, ; 575
	i32 212, ; 576
	i32 235, ; 577
	i32 168, ; 578
	i32 294, ; 579
	i32 272, ; 580
	i32 327, ; 581
	i32 293, ; 582
	i32 101, ; 583
	i32 123, ; 584
	i32 239, ; 585
	i32 309, ; 586
	i32 221, ; 587
	i32 221, ; 588
	i32 190, ; 589
	i32 163, ; 590
	i32 167, ; 591
	i32 242, ; 592
	i32 39, ; 593
	i32 198, ; 594
	i32 335, ; 595
	i32 179, ; 596
	i32 17, ; 597
	i32 171, ; 598
	i32 348, ; 599
	i32 347, ; 600
	i32 137, ; 601
	i32 150, ; 602
	i32 230, ; 603
	i32 155, ; 604
	i32 130, ; 605
	i32 19, ; 606
	i32 65, ; 607
	i32 147, ; 608
	i32 47, ; 609
	i32 355, ; 610
	i32 210, ; 611
	i32 79, ; 612
	i32 61, ; 613
	i32 106, ; 614
	i32 307, ; 615
	i32 270, ; 616
	i32 214, ; 617
	i32 49, ; 618
	i32 253, ; 619
	i32 352, ; 620
	i32 267, ; 621
	i32 14, ; 622
	i32 189, ; 623
	i32 68, ; 624
	i32 171, ; 625
	i32 223, ; 626
	i32 220, ; 627
	i32 230, ; 628
	i32 182, ; 629
	i32 357, ; 630
	i32 78, ; 631
	i32 236, ; 632
	i32 108, ; 633
	i32 213, ; 634
	i32 266, ; 635
	i32 67, ; 636
	i32 63, ; 637
	i32 27, ; 638
	i32 160, ; 639
	i32 259, ; 640
	i32 227, ; 641
	i32 176, ; 642
	i32 10, ; 643
	i32 198, ; 644
	i32 11, ; 645
	i32 78, ; 646
	i32 126, ; 647
	i32 83, ; 648
	i32 191, ; 649
	i32 66, ; 650
	i32 107, ; 651
	i32 65, ; 652
	i32 128, ; 653
	i32 122, ; 654
	i32 77, ; 655
	i32 282, ; 656
	i32 271, ; 657
	i32 356, ; 658
	i32 8, ; 659
	i32 235, ; 660
	i32 2, ; 661
	i32 228, ; 662
	i32 44, ; 663
	i32 285, ; 664
	i32 156, ; 665
	i32 128, ; 666
	i32 269, ; 667
	i32 314, ; 668
	i32 23, ; 669
	i32 133, ; 670
	i32 216, ; 671
	i32 174, ; 672
	i32 255, ; 673
	i32 313, ; 674
	i32 351, ; 675
	i32 333, ; 676
	i32 29, ; 677
	i32 215, ; 678
	i32 206, ; 679
	i32 62, ; 680
	i32 0, ; 681
	i32 200, ; 682
	i32 90, ; 683
	i32 87, ; 684
	i32 148, ; 685
	i32 202, ; 686
	i32 36, ; 687
	i32 86, ; 688
	i32 205, ; 689
	i32 243, ; 690
	i32 187, ; 691
	i32 346, ; 692
	i32 341, ; 693
	i32 190, ; 694
	i32 50, ; 695
	i32 6, ; 696
	i32 90, ; 697
	i32 353, ; 698
	i32 21, ; 699
	i32 162, ; 700
	i32 96, ; 701
	i32 50, ; 702
	i32 113, ; 703
	i32 263, ; 704
	i32 130, ; 705
	i32 293, ; 706
	i32 76, ; 707
	i32 27, ; 708
	i32 320, ; 709
	i32 236, ; 710
	i32 262, ; 711
	i32 7, ; 712
	i32 199, ; 713
	i32 110, ; 714
	i32 291, ; 715
	i32 263, ; 716
	i32 245 ; 717
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
attributes #0 = { "min-legal-vector-width"="0" mustprogress "no-trapping-math"="true" nofree norecurse nosync nounwind "stack-protector-buffer-size"="8" "target-cpu"="generic" "target-features"="+armv7-a,+d32,+dsp,+fp64,+neon,+vfp2,+vfp2sp,+vfp3,+vfp3d16,+vfp3d16sp,+vfp3sp,-aes,-fp-armv8,-fp-armv8d16,-fp-armv8d16sp,-fp-armv8sp,-fp16,-fp16fml,-fullfp16,-sha2,-thumb-mode,-vfp4,-vfp4d16,-vfp4d16sp,-vfp4sp" uwtable willreturn }
attributes #1 = { nofree nounwind }
attributes #2 = { "no-trapping-math"="true" noreturn nounwind "stack-protector-buffer-size"="8" "target-cpu"="generic" "target-features"="+armv7-a,+d32,+dsp,+fp64,+neon,+vfp2,+vfp2sp,+vfp3,+vfp3d16,+vfp3d16sp,+vfp3sp,-aes,-fp-armv8,-fp-armv8d16,-fp-armv8d16sp,-fp-armv8sp,-fp16,-fp16fml,-fullfp16,-sha2,-thumb-mode,-vfp4,-vfp4d16,-vfp4d16sp,-vfp4sp" }

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
!7 = !{i32 1, !"min_enum_size", i32 4}

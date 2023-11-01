// Read S PowerClipper ClipboardEntry.cs
// 2023-09-27 @ 7:43 PM

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Novus.FileTypes;

namespace PowerClipper;

public class ClipboardEntry : IDisposable
{
	private static readonly TextDataFormat[] TextDataFormats =
		(TextDataFormat[]) Enum.GetValuesAsUnderlyingType<TextDataFormat>();

	public string Name { get; private init; }

	public string Format { get; private init; }

	public object? Data { get; private init; }

	public BitmapSource Preview { get; private set; }

	public int Sequence { get;  }

	private ClipboardEntry()
	{
		Sequence = Clipboard2.SequenceNumber;
	}

	public static IEnumerable<ClipboardEntry> Get()
	{
		if (Clipboard.ContainsFileDropList()) {
			var fs = Clipboard.GetFileDropList();

			foreach (var f in fs) {
				var ce = new ClipboardEntry()
				{
					Format = DataFormats.FileDrop,
					Data   = f
				};

				using var fss = File.OpenRead(f);

				if (FileType.Resolve(fss).Type == FileType.MT_IMAGE) {
					ce.Preview = new BitmapImage(new Uri(f, UriKind.Absolute));
				}
				yield return ce;

			}
		}

		if (Clipboard.ContainsImage()) {
			var bs = Clipboard.GetImage();

			var ce = new ClipboardEntry()
			{
				Format = DataFormats.Bitmap,
				Data   = bs,

			};
			ce.Preview = bs;

			yield return ce;
		}

		if (Clipboard.ContainsAudio()) {
			yield return new ClipboardEntry()
			{
				Format = DataFormats.WaveAudio,
				Data   = Clipboard.GetAudioStream()
			};
		}

		foreach (var fmt in TextDataFormats) {
			if (Clipboard.ContainsText(fmt)) {
				yield return new ClipboardEntry()
				{
					Format = fmt.ToString(),
					Data   = Clipboard.GetText(fmt)
				};
			}

		}

	}

	public void Dispose() { }

	public bool IsAudio => Format == DataFormats.WaveAudio;
	public bool IsImage => Format == DataFormats.Bitmap;

	public bool IsText => TextDataFormats.Any(x => x.ToString() == Format);

	public bool IsFileDrop => Format == DataFormats.FileDrop;
}
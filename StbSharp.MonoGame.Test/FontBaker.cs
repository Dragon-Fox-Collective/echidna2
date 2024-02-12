﻿using StbTrueTypeSharp;

namespace StbSharp.MonoGame.Test;

public unsafe class FontBaker
{
	private byte[] _bitmap;
	private StbTrueType.stbtt_pack_context _context;
	private Dictionary<int, GlyphInfo> _glyphs;
	private int bitmapWidth, bitmapHeight;
	
	public void Begin(int width, int height)
	{
		bitmapWidth = width;
		bitmapHeight = height;
		_bitmap = new byte[width * height];
		_context = new StbTrueType.stbtt_pack_context();
		
		fixed (byte* pixelsPtr = _bitmap)
		{
			StbTrueType.stbtt_PackBegin(_context, pixelsPtr, width, height, width, 1, null);
		}
		
		_glyphs = new Dictionary<int, GlyphInfo>();
	}
	
	public void Add(byte[] ttf, float fontPixelHeight, IEnumerable<CharacterRange> characterRanges)
	{
		if (ttf == null || ttf.Length == 0)
			throw new ArgumentNullException(nameof(ttf));
		
		if (fontPixelHeight <= 0)
			throw new ArgumentOutOfRangeException(nameof(fontPixelHeight));
		
		if (characterRanges == null)
			throw new ArgumentNullException(nameof(characterRanges));
		
		if (!characterRanges.Any())
			throw new ArgumentException("characterRanges must have a least one value.");
		
		StbTrueType.stbtt_fontinfo? fontInfo = StbTrueType.CreateFont(ttf, 0);
		if (fontInfo == null)
			throw new Exception("Failed to init font.");
		
		float scaleFactor = StbTrueType.stbtt_ScaleForPixelHeight(fontInfo, fontPixelHeight);
		
		int ascent, descent, lineGap;
		StbTrueType.stbtt_GetFontVMetrics(fontInfo, &ascent, &descent, &lineGap);
		
		foreach (CharacterRange range in characterRanges)
		{
			if (range.Start > range.End)
				continue;
			
			StbTrueType.stbtt_packedchar[] cd = new StbTrueType.stbtt_packedchar[range.End - range.Start + 1];
			fixed (StbTrueType.stbtt_packedchar* chardataPtr = cd)
			{
				StbTrueType.stbtt_PackFontRange(_context, fontInfo.data, 0, fontPixelHeight,
					range.Start,
					range.End - range.Start + 1,
					chardataPtr);
			}
			
			for (int i = 0; i < cd.Length; ++i)
			{
				float yOff = cd[i].yoff;
				yOff += ascent * scaleFactor;
				
				GlyphInfo glyphInfo = new()
				{
					X = cd[i].x0,
					Y = cd[i].y0,
					Width = cd[i].x1 - cd[i].x0,
					Height = cd[i].y1 - cd[i].y0,
					XOffset = (int)cd[i].xoff,
					YOffset = (int)Math.Round(yOff),
					XAdvance = (int)Math.Round(cd[i].xadvance)
				};
				
				_glyphs[i + range.Start] = glyphInfo;
			}
		}
	}
	
	public FontBakerResult End()
	{
		return new FontBakerResult(_glyphs, _bitmap, bitmapWidth, bitmapHeight);
	}
}
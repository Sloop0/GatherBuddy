﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Logging;

namespace GatherBuddy.Plugin;

public static class Functions
{
    // Clamps invalid indices to valid indices.
    public static bool Swap<T>(IList<T> list, int idx1, int idx2)
    {
        idx1 = Math.Clamp(idx1, 0, list.Count);
        idx2 = Math.Clamp(idx2, 0, list.Count);
        if (idx1 == idx2)
            return false;

        (list[idx1], list[idx2]) = (list[idx2], list[idx1]);
        return true;
    }

    public static FileInfo? ObtainSaveFile(string fileName)
    {
        var dir = new DirectoryInfo(Dalamud.PluginInterface.GetPluginConfigDirectory());
        if (dir.Exists)
            return new FileInfo(Path.Combine(dir.FullName, fileName));

        try
        {
            dir.Create();
        }
        catch (Exception e)
        {
            PluginLog.Error($"Could not create save directory at {dir.FullName}:\n{e}");
            return null;
        }

        return new FileInfo(Path.Combine(dir.FullName, fileName));
    }

    public static bool CompareCi(string lhs, string rhs)
        => string.Compare(lhs, rhs, StringComparison.InvariantCultureIgnoreCase) == 0;

    public static bool TryParseBoolean(string text, out bool parsed)
    {
        parsed = false;
        if (text.Length == 1)
        {
            if (text[0] != '1')
                return text[0] == '0';

            parsed = true;
            return true;
        }

        if (!CompareCi(text,       "on") && !CompareCi(text, "true"))
            return CompareCi(text, "off") || CompareCi(text, "false");

        parsed = true;
        return true;
    }

    [DllImport("msvcrt.dll")]
    private static extern unsafe int memcmp(byte* b1, byte* b2, int count);

    public static unsafe bool MemCmpUnchecked(byte* ptr1, byte* ptr2, int count)
        => memcmp(ptr1, ptr2, count) == 0;

    public static unsafe bool MemCmp(byte* ptr1, byte* ptr2, int length1, int length2)
    {
        if (length1 != length2)
            return false;

        if (ptr1 == ptr2 || length1 == 0)
            return true;

        if (ptr1 == null || ptr2 == null)
            return false;

        return memcmp(ptr1, ptr2, length1) == 0;
    }

    public static string CompressedBase64(byte[] data)
    {
        using var compressedStream = new MemoryStream();
        using (var zipStream = new GZipStream(compressedStream, CompressionMode.Compress))
        {
            zipStream.Write(data, 0, data.Length);
        }

        return Convert.ToBase64String(compressedStream.ToArray());
    }

    public static byte[] DecompressedBase64(string compressedBase64)
    {
        var       data             = Convert.FromBase64String(compressedBase64);
        using var compressedStream = new MemoryStream(data);
        using var zipStream        = new GZipStream(compressedStream, CompressionMode.Decompress);
        using var resultStream     = new MemoryStream();
        zipStream.CopyTo(resultStream);
        return resultStream.ToArray();
    }

    public static void Print(SeString message)
    {
        var entry = new XivChatEntry()
        {
            Message = message,
            Name    = SeString.Empty,
            Type    = GatherBuddy.Config.ChatTypeMessage,
        };
        Dalamud.Chat.PrintChat(entry);
    }

    public static void PrintError(SeString message)
    {
        var entry = new XivChatEntry()
        {
            Message = message,
            Name    = SeString.Empty,
            Type    = GatherBuddy.Config.ChatTypeError,
        };
        Dalamud.Chat.PrintChat(entry);
    }

    public static void Print(string message)
        => Print((SeString)message);

    public static void PrintError(string message)
        => PrintError((SeString)message);
}

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Client.UI.Shell;

namespace GatherBuddy.Managers;

public unsafe class MacroManager : IDisposable
{
    public const int NumMacroLines    = 15;
    public const int NumRequiredLines = 3;

    public RaptureShellModule* Module
        => Framework.Instance()->GetUiModule()->GetRaptureShellModule();

    public RaptureMacroModule.Macro* Macro;

    public MacroManager()
    {
        Macro = (RaptureMacroModule.Macro*)Marshal.AllocHGlobal(sizeof(RaptureMacroModule.Macro));
        PrepareMacro(Macro);
    }

    public void Dispose()
    {
        DisposeMacro(Macro);
        Marshal.FreeHGlobal((IntPtr)Macro);
    }

    public static void ClearString(Utf8String* ret)
    {
        ret->BufUsed      = 1;
        ret->IsEmpty      = 1;
        ret->StringLength = 0;
        ret->StringPtr[0] = 0;
    }

    public static void CreateEmptyString(Utf8String* ret)
    {
        ret->BufSize             = 0x40;
        ret->IsUsingInlineBuffer = 1;
        ret->StringPtr           = ret->InlineBuffer;
        ClearString(ret);
    }

    public static void CreateTempString(Utf8String* ret)
    {
        ret->BufSize             = 128;
        ret->IsUsingInlineBuffer = 0;
        ret->StringPtr           = (byte*)Marshal.AllocHGlobal(128);
        ClearString(ret);
    }

    public static void DisposeString(Utf8String* ret)
    {
        if (ret->BufSize == 128)
            Marshal.FreeHGlobal((IntPtr)ret->StringPtr);
        CreateEmptyString(ret);
    }

    public static bool CopyString(string text, Utf8String* ret)
    {
        var bytes = Encoding.UTF8.GetBytes(text);
        if (bytes.Length + 1 >= ret->BufSize)
            return false;

        Marshal.Copy(bytes, 0, (IntPtr)ret->StringPtr, bytes.Length);
        ret->BufUsed                 = bytes.Length + 1;
        ret->StringLength            = bytes.Length;
        ret->StringPtr[bytes.Length] = 0;
        return true;
    }

    public static void PrepareMacro(RaptureMacroModule.Macro* macro)
    {
        CreateEmptyString(&macro->Name);
        for (var i = 0; i < NumRequiredLines; ++i)
            CreateTempString(macro->Line[i]);
        for (var i = NumRequiredLines; i < NumMacroLines; ++i)
            CreateEmptyString(macro->Line[i]);
    }

    public static void DisposeMacro(RaptureMacroModule.Macro* macro)
    {
        for (var i = 0; i < NumRequiredLines; ++i)
            DisposeString(macro->Line[i]);
    }

    public bool ExecuteMacroLines(params string[] lines)
    {
        Debug.Assert(lines.Length <= 3);
        for (var i = 0; i < lines.Length; ++i)
        {
            if (!CopyString(lines[i], Macro->Line[i]))
                return false;
        }

        Module->ExecuteMacro(Macro);
        return true;
    }
}

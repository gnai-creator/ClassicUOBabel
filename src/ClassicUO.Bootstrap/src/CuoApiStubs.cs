// Stub implementations for CUO_API types when cuoapi.dll is not available (Linux builds)
#if LINUX
using System;

namespace CUO_API
{
    // Stub implementations to allow compilation on Linux
    public delegate void OnCastSpell(int index);
    public delegate short OnGetPacketLength(int id);
    public delegate bool OnGetPlayerPosition(out int x, out int y, out int z);
    public delegate void OnGetStaticImage(ushort g, ref ArtInfo info);
    public delegate string OnGetUOFilePath();
    public delegate void OnClientClose();
    public delegate void OnConnected();
    public delegate void OnDisconnected();
    public delegate void OnFocusGained();
    public delegate void OnFocusLost();
    public delegate bool OnHotkey(int key, int mod, bool pressed);
    public delegate void OnInitialize();
    public delegate void OnMouse(int button, int wheel);
    public delegate void OnUpdatePlayerPosition(int x, int y, int z);
    public delegate bool OnPacketSendRecv(ref byte[] data, ref int length);
    public delegate bool RequestMove(int dir, bool run);
    public delegate void OnSetTitle(string title);
    public delegate void OnTick();

    // Stub struct for ArtInfo
    public struct ArtInfo
    {
        public int Graphic;
        public int Hue;
        public int X;
        public int Y;
        public int Z;
    }
}
#endif

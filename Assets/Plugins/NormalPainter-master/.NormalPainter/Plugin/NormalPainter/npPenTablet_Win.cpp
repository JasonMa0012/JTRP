#include "pch.h"
#include "NormalPainter.h"

#ifdef _WIN32
#ifdef npEnablePenTablet
#include <dbghelp.h>
#include <psapi.h>
#pragma comment(lib, "dbghelp.lib")


extern float g_pen_pressure;

struct PenContext
{
    LRESULT(CALLBACK *wndproc)(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam) = nullptr;
};

static std::map<HWND, PenContext> g_pen_contexts;


static void* FindSymbolByName(const char *name)
{
    static std::once_flag s_once;
    std::call_once(s_once, [&]() {
        // set path to main module to symbol search path
        char sympath[MAX_PATH] = "";
        {
            auto ret = ::GetModuleFileNameA(::GetModuleHandleA(nullptr), (LPSTR)sympath, sizeof(sympath));
            for (int i = ret - 1; i > 0; --i) {
                if (sympath[i] == '\\') {
                    sympath[i] = '\0';
                    break;
                }
            }
        }

        DWORD opt = ::SymGetOptions();
        opt |= SYMOPT_DEFERRED_LOADS;
        opt &= ~SYMOPT_UNDNAME;
        ::SymSetOptions(opt);
        ::SymInitialize(::GetCurrentProcess(), sympath, TRUE);
    });

    char buf[sizeof(SYMBOL_INFO) + MAX_SYM_NAME];
    PSYMBOL_INFO sinfo = (PSYMBOL_INFO)buf;
    sinfo->SizeOfStruct = sizeof(SYMBOL_INFO);
    sinfo->MaxNameLen = MAX_SYM_NAME;
    if (::SymFromName(::GetCurrentProcess(), name, sinfo) == FALSE) {
        return nullptr;
    }
    return (void*)sinfo->Address;
}


static HWND(*_GetMainEditorWindow)();

static HWND GetEditorWindow()
{
    static std::once_flag s_once;
    std::call_once(s_once, [&]() {
        (void*&)_GetMainEditorWindow = FindSymbolByName("?GetMainEditorWindow@@YAPEAUHWND__@@XZ");
    });

    if (_GetMainEditorWindow) {
        return _GetMainEditorWindow();
    }
    else {
        return GetActiveWindow();
    }
}


// pointer API is available Windows8 or later.
// so import these manually to avoid link error on pre-Windows8.
BOOL (WINAPI *_GetPointerType)(UINT32 pointerId, POINTER_INPUT_TYPE *pointerType);
BOOL (WINAPI *_GetPointerPenInfo)(UINT32 pointerId, POINTER_PEN_INFO *penInfo);
BOOL (WINAPI *_GetPointerFrameTouchInfo)(UINT32 pointerId, UINT32 *pointerCount, POINTER_TOUCH_INFO *touchInfo);

static bool ImportPointerAPI()
{
    static bool s_result = false;
    static std::once_flag s_once;

    std::call_once(s_once, [&]() {
        auto user32 = ::GetModuleHandleA("user32.dll");
#define Import(Name) (void*&)_##Name = ::GetProcAddress(user32, #Name); if (_##Name == nullptr) { return; }
        Import(GetPointerType);
        Import(GetPointerPenInfo);
        Import(GetPointerFrameTouchInfo);
#undef Import
        s_result = true;
    });
    return s_result;
}

static LRESULT CALLBACK npWndProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam)
{
    auto& ctx = g_pen_contexts[hWnd];

    switch (message)
    {
    case WM_POINTERUP:
    case WM_POINTERDOWN:
    case WM_POINTERUPDATE:
    {
            UINT pid = GET_POINTERID_WPARAM(wParam);
            POINTER_INPUT_TYPE ptype;
            if (_GetPointerType(pid, &ptype)) {
                if (ptype == PT_PEN) {
                    POINTER_PEN_INFO pinfo;
                    if (_GetPointerPenInfo(pid, &pinfo)) {
                        g_pen_pressure = (float)pinfo.pressure / 1024;
                    }
                }
                else if (ptype == PT_TOUCH && IS_POINTER_PRIMARY_WPARAM(wParam)) {
                    POINTER_TOUCH_INFO pinfo[4];
                    UINT pcount = 4;
                    if (_GetPointerFrameTouchInfo(pid, &pcount, pinfo)) {
                        g_pen_pressure = (float)pinfo[0].pressure / 1024;
                    }
                }
                else if (ptype == PT_TOUCHPAD) {
                    // 
                }
            }
            return 0;
        }
    }

    if (ctx.wndproc) {
        return CallWindowProc(ctx.wndproc, hWnd, message, wParam, lParam);
    }
    else {
        return DefWindowProc(hWnd, message, wParam, lParam);
    }
}



BOOL CALLBACK cbEnumWindows(HWND hwnd, LPARAM lParam)
{
    DWORD procId;
    GetWindowThreadProcessId(hwnd, &procId);
    if (procId != GetCurrentProcessId()) { return TRUE; }

    auto& ctx = g_pen_contexts[hwnd];
    if (!ctx.wndproc) {
        (LONG_PTR&)ctx.wndproc = SetWindowLongPtr(hwnd, GWLP_WNDPROC, (LONG_PTR)npWndProc);
    }
    return TRUE;
}

void npInitializePenInput_Win()
{
    if (ImportPointerAPI()) {
        cbEnumWindows(GetEditorWindow(), 0);
        //EnumWindows(cbEnumWindows, 0);
    }
}
#endif // npEnablePenTablet
#endif // _WIN32

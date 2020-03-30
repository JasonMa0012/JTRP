#include "pch.h"
#include "muMisc.h"
#ifdef _WIN32
    #include <dbghelp.h>
    #include <psapi.h>
    #pragma comment(lib, "dbghelp.lib")
#else
    #include <unistd.h>
    #include <sys/mman.h>
#endif

namespace mu {

nanosec Now()
{
    using namespace std::chrono;
    return duration_cast<nanoseconds>(steady_clock::now().time_since_epoch()).count();
}

void Print(const char *fmt, ...)
{
    va_list args;
    va_start(args, fmt);
#ifdef _WIN32
    char buf[1024];
    vsnprintf(buf, sizeof(buf), fmt, args);
    ::OutputDebugStringA(buf);
#else
    vprintf(fmt, args);
#endif
    va_end(args);
}

void Print(const wchar_t *fmt, ...)
{
    va_list args;
    va_start(args, fmt);
#ifdef _WIN32
    wchar_t buf[1024];
    _vsnwprintf(buf, sizeof(buf), fmt, args);
    ::OutputDebugStringW(buf);
#else
    vwprintf(fmt, args);
#endif
    va_end(args);
}

#ifdef _WIN32
std::string ToUTF8(const char *src)
{
    // to UTF-16
    const int wsize = ::MultiByteToWideChar(CP_ACP, 0, (LPCSTR)src, -1, nullptr, 0);
    std::wstring ws;
    ws.resize(wsize);
    ::MultiByteToWideChar(CP_ACP, 0, (LPCSTR)src, -1, (LPWSTR)ws.data(), wsize);

    // to UTF-8
    const int u8size = ::WideCharToMultiByte(CP_UTF8, 0, (LPCWSTR)ws.data(), -1, nullptr, 0, nullptr, nullptr);
    std::string u8s;
    u8s.resize(u8size);
    ::WideCharToMultiByte(CP_UTF8, 0, (LPCWSTR)ws.data(), -1, (LPSTR)u8s.data(), u8size, nullptr, nullptr);
    return u8s;
}
std::string ToUTF8(const std::string& src)
{
    return ToUTF8(src.c_str());
}

std::string ToANSI(const char *src)
{
    // to UTF-16
    const int wsize = ::MultiByteToWideChar(CP_UTF8, 0, (LPCSTR)src, -1, nullptr, 0);
    std::wstring ws;
    ws.resize(wsize);
    ::MultiByteToWideChar(CP_UTF8, 0, (LPCSTR)src, -1, (LPWSTR)ws.data(), wsize);

    // to ANSI
    const int u8size = ::WideCharToMultiByte(CP_ACP, 0, (LPCWSTR)ws.data(), -1, nullptr, 0, nullptr, nullptr);
    std::string u8s;
    u8s.resize(u8size);
    ::WideCharToMultiByte(CP_ACP, 0, (LPCWSTR)ws.data(), -1, (LPSTR)u8s.data(), u8size, nullptr, nullptr);
    return u8s;
}
std::string ToANSI(const std::string& src)
{
    return ToANSI(src.c_str());
}
#endif



void AddDLLSearchPath(const char *v)
{
#if defined(_WIN32)
    #define LIBRARY_PATH "PATH"
#elif defined(__APPLE__)
    #define LIBRARY_PATH "DYLD_LIBRARY_PATH"
#else
    #define LIBRARY_PATH "LD_LIBRARY_PATH"
#endif
#ifdef _WIN32
    std::string path;
    {
        DWORD size = ::GetEnvironmentVariableA(LIBRARY_PATH, nullptr, 0);
        if (size > 0) {
            path.resize(size);
            ::GetEnvironmentVariableA(LIBRARY_PATH, &path[0], (DWORD)path.size());
            path.pop_back(); // delete last '\0'
        }
    }
    if (path.find(v) == std::string::npos) {
        auto pos = path.size();
        path = std::string(v) + ";" + path;
        for (size_t i = pos; i < path.size(); ++i) {
            char& c = path[i];
            if (c == '/') { c = '\\'; }
        }
        ::SetEnvironmentVariableA(LIBRARY_PATH, path.c_str());
    }
#else
    std::string path;
    if (auto path_ = ::getenv(LIBRARY_PATH)) {
        path = path_;
    }
    if (path.find(v) == std::string::npos) {
        if (!path.empty()) { path += ":"; }
        auto pos = path.size();
        path += v;
        for (size_t i = pos; i < path.size(); ++i) {
            char& c = path[i];
            if (c == '\\') { c = '/'; }
        }
        ::setenv(LIBRARY_PATH, path.c_str(), 1);
    }
#endif
#undef LIBRARY_PATH
}



bool ResolveImports(void *module)
{
#ifdef _WIN32
    bool ret = true;
    EnumerateDLLImports((HMODULE)module, [&](const char *dll, const char *funcname, DWORD ordinal, void *&addr) {
        auto mod = ::GetModuleHandleA(dll);
        if (!mod) { mod = ::LoadLibraryA(dll); }
        if (!mod) {
            ret = false; return;
        }
        if (funcname) {
            ForceWrite<void*>(&addr, ::GetProcAddress(mod, funcname));
        }
        else {
            ForceWrite<void*>(&addr, ::GetProcAddress(mod, MAKEINTRESOURCEA(ordinal)));
        }
    });
    return ret;
#else  // _WIN32
    return false;
#endif //_WIN32
}

void* LoadModule(const char *path)
{
#ifdef _WIN32
    return ::LoadLibraryA(path);
#else  // _WIN32
    return nullptr;
#endif //_WIN32
}

void* GetModule(const char *module_name)
{
#ifdef _WIN32
    return ::GetModuleHandleA(module_name);
#else  // _WIN32
    return nullptr;
#endif //_WIN32
}


void InitializeSymbols(const char *path)
{
#ifdef _WIN32
    char tmp[MAX_PATH];
    if (!path) {
        auto ret = ::GetModuleFileNameA(::GetModuleHandleA(nullptr), (LPSTR)tmp, sizeof(tmp));
        for (int i = ret - 1; i > 0; --i) {
            if (tmp[i] == '\\') {
                tmp[i] = '\0';
                break;
            }
        }
        path = tmp;
    }

    DWORD opt = ::SymGetOptions();
    opt |= SYMOPT_DEBUG;
    opt |= SYMOPT_DEFERRED_LOADS;
    opt &= ~SYMOPT_UNDNAME;
    ::SymSetOptions(opt);
    ::SymInitialize(::GetCurrentProcess(), path, TRUE);
#else  // _WIN32
#endif //_WIN32
}

void* FindSymbolByName(const char *name)
{
#ifdef _WIN32
    char buf[sizeof(SYMBOL_INFO) + MAX_SYM_NAME];
    PSYMBOL_INFO sinfo = (PSYMBOL_INFO)buf;
    sinfo->SizeOfStruct = sizeof(SYMBOL_INFO);
    sinfo->MaxNameLen = MAX_SYM_NAME;
    if (::SymFromName(::GetCurrentProcess(), name, sinfo) == FALSE) {
        return nullptr;
    }
    return (void*)sinfo->Address;
#else  // _WIN32
    return nullptr;
#endif //_WIN32
}


#ifdef _WIN32
struct cbEnumSymbolsCtx
{
    const char *name;
    void *ret;
};
BOOL CALLBACK cbEnumSymbols(PCSTR SymbolName, DWORD64 SymbolAddress, ULONG /*SymbolSize*/, PVOID UserContext)
{
    auto ctx = (cbEnumSymbolsCtx*)UserContext;
    if (strcmp(SymbolName, ctx->name) == 0) {
        ctx->ret = (void*)SymbolAddress;
        return FALSE;
    }
    return TRUE;

}

#endif //_WIN32

void* FindSymbolByName(const char *name, const char *module_name)
{
#ifdef _WIN32
    cbEnumSymbolsCtx ctx{ name, nullptr };
    SymEnumerateSymbols64(::GetCurrentProcess(), (ULONG64)GetModuleHandleA(module_name), cbEnumSymbols, &ctx);
    return ctx.ret;
#else  // _WIN32
    return nullptr;
#endif //_WIN32
}

void SetMemoryProtection(void *addr, size_t size, MemoryFlags flags)
{
#ifdef _WIN32
    DWORD flag = 0;
    switch (flags) {
    case MemoryFlags::ReadWrite: flag = PAGE_READWRITE; break;
    case MemoryFlags::ExecuteRead: flag = PAGE_EXECUTE_READ; break;
    case MemoryFlags::ExecuteReadWrite: flag = PAGE_EXECUTE_READWRITE; break;
    }
    DWORD old_flag;
    VirtualProtect(addr, size, flag, &old_flag);
#else
    int flag = 0;
    switch (flags) {
    case MemoryFlags::ReadWrite: flag = PROT_READ | PROT_WRITE; break;
    case MemoryFlags::ExecuteRead: flag = PROT_EXEC | PROT_READ; break;
    case MemoryFlags::ExecuteReadWrite: flag = PROT_EXEC | PROT_READ | PROT_WRITE; break;
    }
    void *page = (void*)((size_t)addr - ((size_t)addr % getpagesize()));
    mprotect(page, size, flag);
#endif
}


} // namespace mu

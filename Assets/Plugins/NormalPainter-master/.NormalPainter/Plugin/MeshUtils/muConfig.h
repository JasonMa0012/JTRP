#pragma once

// available options:
//   muEnablePPL
//   muEnableTBB
//   muEnableISPC
//   muEnableAMP
//   muEnableSymbol

#ifdef _WIN32
    #define muEnablePPL
    #define muEnableISPC
    #define muEnableAMP

    #define muEnableSymbol
#endif


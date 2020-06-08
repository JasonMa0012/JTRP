option(ENABLE_TBB "Use Intel TBB." OFF)
option(ENABLE_HALF "Use half." OFF)

if(ENABLE_ISPC)
    setup_ispc()

    set(MUISPC_OUTDIR ${CMAKE_CURRENT_BINARY_DIR}/ISPC)
    set(MUISPC_HEADERS
        "${CMAKE_CURRENT_SOURCE_DIR}/ispcmath.h"
        "${CMAKE_CURRENT_SOURCE_DIR}/muSIMDConfig.h"
    )
    file(GLOB MUISPC_SOURCES *.ispc)
    add_ispc_targets(SOURCES ${MUISPC_SOURCES} HEADERS ${MUISPC_HEADERS} OUTDIR ${MUISPC_OUTDIR})
    set(MUISPC_OUTPUTS ${_ispc_outputs})
endif()

file(GLOB sources *.cpp *.c *.h)
add_library(MeshUtils STATIC ${sources} ${MUISPC_OUTPUTS})
install(TARGETS MeshUtils DESTINATION .)

if(ENABLE_ISPC)
    add_definitions(-DmuEnableISPC)
    target_include_directories(MeshUtils PUBLIC ${MUISPC_OUTDIR})
endif()
if(ENABLE_TBB)
    find_package(TBB QUIET)
    add_definitions(-DmuEnableTBB)
    include_directories(${TBB_INCLUDE_DIRS})
    list(APPEND EXTERNAL_LIBS ${TBB_LIBRARIES})
endif()
if(ENABLE_HALF)
    find_package(OpenEXR QUIET)
    add_definitions(-DmuEnableHalf)
    include_directories(${OPENEXR_INCLUDE_DIR})
    list(APPEND EXTERNAL_LIBS ${OPENEXR_Half_LIBRARY})
endif()
set(EXTERNAL_LIBS ${EXTERNAL_LIBS} PARENT_SCOPE)

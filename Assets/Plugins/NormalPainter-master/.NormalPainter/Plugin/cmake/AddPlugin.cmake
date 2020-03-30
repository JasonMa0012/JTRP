set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -fPIC")
set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -fPIC")

if (NOT CMAKE_BUILD_TYPE)
    set(CMAKE_BUILD_TYPE "Release" CACHE PATH "" FORCE)
endif()
if (CMAKE_INSTALL_PREFIX_INITIALIZED_TO_DEFAULT)
    set(CMAKE_INSTALL_PREFIX "${CMAKE_BINARY_DIR}/dist" CACHE PATH "" FORCE)
endif()

if(APPLE)
    set(CMAKE_FIND_LIBRARY_SUFFIXES ".a")
    option(ENABLE_OSX_BUNDLE "Build bundle." ON)
    set(CMAKE_MACOSX_RPATH ON)

    if(ENABLE_OSX_BUNDLE)
        set(CMAKE_SKIP_RPATH ON)
    else()
        set(CMAKE_SKIP_RPATH OFF)
        set(CMAKE_INSTALL_RPATH_USE_LINK_PATH ON)
    endif()
elseif(CMAKE_SYSTEM_NAME STREQUAL "Linux")
    option(ENABLE_LINUX_USE_LINK_PATH "" ON)

    if(ENABLE_LINUX_USE_LINK_PATH)
        set(CMAKE_INSTALL_RPATH_USE_LINK_PATH ON)
    endif()
endif()
option(ENABLE_DEPLOY "Copy built binaries to plugins directory." ON)


function(add_plugin name)
    cmake_parse_arguments(arg "" "PLUGINS_DIR" "SOURCES" ${ARGN})

    if(ENABLE_OSX_BUNDLE)
        add_library(${name} MODULE ${arg_SOURCES})
        set_target_properties(${name} PROPERTIES BUNDLE ON)
    else()
        add_library(${name} SHARED ${arg_SOURCES})
    endif()

    if(ENABLE_DEPLOY)
        if(ENABLE_OSX_BUNDLE)
            SET(target_filename "${name}.bundle")
        else()
            SET(target_filename $<TARGET_FILE:${name}>)
        endif()
        add_custom_target("Deploy${name}" ALL
            COMMAND rm -rf ${arg_PLUGINS_DIR}/${target_filename}
            COMMAND cp -r ${target_filename} ${arg_PLUGINS_DIR}
            DEPENDS ${name}
        )
    endif()
endfunction()

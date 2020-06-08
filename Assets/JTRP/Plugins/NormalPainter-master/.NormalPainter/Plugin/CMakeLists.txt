cmake_minimum_required(VERSION 2.8)
project(NormalPainterCore)

set(CMAKE_MODULE_PATH ${CMAKE_SOURCE_DIR}/cmake)
include(ISPC)
include(AddPlugin)

set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -fPIC")
set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -fPIC -std=c++11")
set(CMAKE_SHARED_LINKER_FLAGS "${CMAKE_SHARED_LINKER_FLAGS} -Wl,--no-undefined")
include_directories(${CMAKE_SOURCE_DIR})

add_subdirectory(MeshUtils)
add_subdirectory(NormalPainter)
add_subdirectory(Test)

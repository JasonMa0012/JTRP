file(GLOB sources *.cpp *.h)
set(plugins_dir "${CMAKE_SOURCE_DIR}/../Assets/UTJ/NormalPainter/Runtime/Plugins/x86_64")
add_plugin(NormalPainterCore SOURCES ${sources} PLUGINS_DIR ${plugins_dir})

add_dependencies(NormalPainterCore MeshUtils)
target_link_libraries(NormalPainterCore MeshUtils ${EXTERNAL_LIBS})
install(TARGETS NormalPainterCore DESTINATION .)

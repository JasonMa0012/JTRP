call buildtools.bat

msbuild NormalPainterCore.vcxproj /t:Build /p:Configuration=Master /p:Platform=x64 /m /nologo

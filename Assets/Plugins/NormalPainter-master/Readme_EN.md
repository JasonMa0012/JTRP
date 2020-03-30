# NormalPainter
  
![](https://user-images.githubusercontent.com/1488611/27468607-b3e9e4d0-5825-11e7-954d-fca1a7a50417.gif)


You will need Unity 2017.1 or later, Windows (64 bit) or Mac (**Graphics API of D3D11 and later**). 
(Please note that if your target platform is not standalone, D3D9 will limit functions and may not work properly.) 



## How to Use 

- Import  [NormalPainter.unitypackage](https://github.com/unity3d-jp/NormalPainter/releases/download/20180116/NormalPainter.unitypackage) to your project.
- For Unity 2018.3 and later, you can also import this repository directly. Open Packages/manifest.json of your project in the text editor and add it after "dependencies".
  > "com.utj.normalpainter": "https://github.com/unity3d-jp/NormalPainter.git",
  
  -Window-> Open the tool window in Normal Painter
  -"Add NormalPainter" will pop up on the tool window when you select an object with MeshRenderer or SkinnedMeshRenderer, so you can add it as a component. 

##Functions 
  - Vertices, normal vectors, tangents etc. will become visible. 
  -Paint, rotate, scale normal vectors. (Please reference the video above) 
    -Masking with selected range 
  -Mirror normal vectors (Misc-> Mirroring) 
  -Project normal vectors (Edit-> Projection) 
    -Rays will be projected from each vertex and the vectors of the selected object will be picked up where the rays hit.
  
  -import/ export 
    -Normal vectors<-> vertices color conversion. 
    -Export normal vectors as texture. 
    -Import vector maps as normal vectors of vertices. 
    -Export as .obj file. 
    



## License
[MIT](LICENSE.txt)

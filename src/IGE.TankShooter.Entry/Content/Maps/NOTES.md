Pack textures using https://www.codeandweb.com/texturepacker (seems to be the done thing - Tiled can't pack textures).

Example of loading tiled tmx files in MonoGame.Extended.Tiles: 
https://lioncatdevstudio.blogspot.com/2021/01/tile-map-collisions.html

Gotchas:
* Need alpha version of monogame exetnded due to bug with getTransparent_back() or some nonsense.
* The hack to depend on the DLLS from the content builder pipeline doesn't work because it can't find the alpha versions in ~/.nuget - and neither can I!!! WHERE ARE THEY?!?!
